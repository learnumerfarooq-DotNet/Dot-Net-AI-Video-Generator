using AiContentFactory.Application.Common;
using AiContentFactory.Application.Decisions;
using AiContentFactory.Application.Agents;
using AiContentFactory.Application.Memory;
using AiContentFactory.Application.Pipeline;
using AiContentFactory.Application.Studio;
using AiContentFactory.Domain.Agents;
using AiContentFactory.Domain.Decisions;
using AiContentFactory.Domain.Events;
using AiContentFactory.Domain.Memory.AgentMemories;
using AiContentFactory.Domain.Pipeline;
using AiContentFactory.Domain.Processing;
using AiContentFactory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AiContentFactory.Infrastructure.Agents;

public sealed class ScriptGenExecutionAgent : IScriptGenAgent
{
    private readonly IDecisionEngine _decisionEngine;
    private readonly ILocalMemoryService _localMemoryService;
    private readonly IGlobalMemoryService _globalMemoryService;
    private readonly IGoogleDriveService _driveService;
    private readonly IPipelineOrchestrator _orchestrator;
    private readonly IRealtimeEventEmitter _emitter;
    private readonly StudioDbContext _dbContext;
    private readonly ILogger<ScriptGenExecutionAgent> _logger;

    public ScriptGenExecutionAgent(
        IDecisionEngine decisionEngine,
        ILocalMemoryService localMemoryService,
        IGlobalMemoryService globalMemoryService,
        IGoogleDriveService driveService,
        IPipelineOrchestrator orchestrator,
        IRealtimeEventEmitter emitter,
        StudioDbContext dbContext,
        ILogger<ScriptGenExecutionAgent> logger)
    {
        _decisionEngine = decisionEngine;
        _localMemoryService = localMemoryService;
        _globalMemoryService = globalMemoryService;
        _driveService = driveService;
        _orchestrator = orchestrator;
        _emitter = emitter;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<ScriptOutput> GenerateScriptAsync(Guid jobId, VideoMetadata metadata, CancellationToken ct = default)
    {
        _logger.LogInformation("Generating script for job {JobId}", jobId);

        try
        {
            var localMemory = await _localMemoryService.GetConfigAsync<ScriptGenLocalMemory>("script-gen-agent", ct) 
                              ?? new ScriptGenLocalMemory();

            var context = BuildPromptContext(metadata, localMemory);

            var decision = await _decisionEngine.MakeDecisionAsync(
                "script-gen-agent", 
                DecisionType.ScriptGeneration, 
                context, 
                jobId, 
                ct);

            var payload = ParseScriptFromDecision(decision);

            var scriptOutput = new ScriptOutput
            {
                JobId = jobId,
                Title = payload.Title,
                Hook = payload.Hook,
                Introduction = payload.Introduction,
                Body = payload.Body,
                CallToAction = payload.CallToAction,
                Keywords = payload.Keywords,
                Hashtags = payload.Hashtags,
                SuggestedPlatforms = payload.SuggestedPlatforms,
                EstimatedDuration = payload.EstimatedDuration,
                ToneUsed = localMemory.ToneConfig,
                StyleUsed = localMemory.LastScriptStyle,
                ConfidenceScore = decision.ConfidenceScore,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await UploadScriptToDrive(scriptOutput, localMemory.OutputFolder, ct);

            _dbContext.ScriptOutputs.Add(scriptOutput);
            await _dbContext.SaveChangesAsync(ct);

            localMemory.LastGeneratedScript = JsonSerializer.Serialize(payload);
            await _localMemoryService.SaveConfigAsync("script-gen-agent", localMemory, ct);

            await _localMemoryService.RecordRunAsync("script-gen-agent", true, null, ct);

            await _orchestrator.TransitionStageAsync(jobId, PipelineStageType.ScriptGeneration, ct);

            await _emitter.EmitScriptGeneratedAsync(new ScriptGeneratedPayload(jobId, scriptOutput.Title, scriptOutput.ConfidenceScore), ct);
            
            return scriptOutput;
        }
        catch (Exception ex)
        {
            await HandleFailure(jobId, ex, ct);
            throw;
        }
    }

    public async Task<ScriptOutput> RegenerateScriptAsync(Guid jobId, string style, string tone, CancellationToken ct = default)
    {
        var localMemory = await _localMemoryService.GetConfigAsync<ScriptGenLocalMemory>("script-gen-agent", ct) 
                          ?? new ScriptGenLocalMemory();

        localMemory.LastScriptStyle = style;
        localMemory.ToneConfig = tone;
        await _localMemoryService.SaveConfigAsync("script-gen-agent", localMemory, ct);

        // Ideally we invalidate decision cache here for the job, but we'll assume a new request bypasses it or we force generation
        
        var job = await _dbContext.VideoPipelineJobs.FindAsync(new object[] { jobId }, ct);
        if (job == null) throw new InvalidOperationException("Job not found.");
        
        // Mock metadata for regeneration since we don't store raw metadata easily here without another fetch
        var metadata = new VideoMetadata { }; 

        return await GenerateScriptAsync(jobId, metadata, ct);
    }

    public async Task<ScriptOutput?> GetScriptAsync(Guid jobId, CancellationToken ct = default)
    {
        return await _dbContext.ScriptOutputs.OrderByDescending(s => s.CreatedAt).FirstOrDefaultAsync(s => s.JobId == jobId, ct);
    }

    public async Task<bool> ValidateScriptAsync(ScriptOutput script, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(script.Title) || script.Title.Length < 10 || script.Title.Length > 100) return false;
        if (string.IsNullOrWhiteSpace(script.Hook) || script.Hook.Length < 10 || script.Hook.Length > 200) return false;
        if (string.IsNullOrWhiteSpace(script.Body) || script.Body.Length < 100 || script.Body.Length > 5000) return false;
        if (script.Keywords == null || script.Keywords.Count < 3 || script.Keywords.Count > 15) return false;
        if (script.ConfidenceScore < 0.3) return false;

        return await Task.FromResult(true);
    }

    private Dictionary<string, string> BuildPromptContext(VideoMetadata metadata, ScriptGenLocalMemory localMemory)
    {
        return new Dictionary<string, string>
        {
            { "fileName", "Unknown" },
            { "duration", metadata.DurationSeconds.ToString() },
            { "resolution", $"{metadata.Width}x{metadata.Height}" },
            { "audioTracks", "1" },
            { "style", localMemory.LastScriptStyle },
            { "tone", localMemory.ToneConfig },
            { "videoType", localMemory.VideoType },
            { "language", localMemory.PreferredLanguage }
        };
    }

    private ScriptDecisionPayload ParseScriptFromDecision(AgentDecision decision)
    {
        if (string.IsNullOrWhiteSpace(decision.ValidatedPayload))
            throw new Exception("Decision payload is empty.");

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return JsonSerializer.Deserialize<ScriptDecisionPayload>(decision.ValidatedPayload, options) 
               ?? throw new Exception("Failed to parse script payload.");
    }

    private async Task UploadScriptToDrive(ScriptOutput script, string outputFolder, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(script, new JsonSerializerOptions { WriteIndented = true });
        
        // TODO: var path = $"{outputFolder}/{script.JobId}.json";
        // var fileId = await _driveService.UploadFileContentAsync(path, json, ct);
        // script.DriveFileId = fileId;
        
        script.DriveFileId = "mock-drive-id";
        await Task.CompletedTask;
    }

    private async Task HandleFailure(Guid jobId, Exception ex, CancellationToken ct)
    {
        _logger.LogError(ex, "Script generation failed for job {JobId}", jobId);
        await _localMemoryService.RecordRunAsync("script-gen-agent", false, ex.Message, ct);
        // await _orchestrator.HandleFailureAsync(jobId, ex, ct);
    }

    private class ScriptDecisionPayload
    {
        public string Title { get; set; } = string.Empty;
        public string Hook { get; set; } = string.Empty;
        public string Introduction { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string CallToAction { get; set; } = string.Empty;
        public List<string> Keywords { get; set; } = new();
        public List<string> Hashtags { get; set; } = new();
        public List<string> SuggestedPlatforms { get; set; } = new();
        public int EstimatedDuration { get; set; }
    }
}
