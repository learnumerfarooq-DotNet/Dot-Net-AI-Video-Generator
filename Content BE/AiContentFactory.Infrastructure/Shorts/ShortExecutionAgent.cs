using AiContentFactory.Application.Common;
using AiContentFactory.Application.Decisions;
using AiContentFactory.Application.Agents;
using AiContentFactory.Application.Memory;
using AiContentFactory.Application.Pipeline;
using AiContentFactory.Application.Processing;
using AiContentFactory.Application.Studio;
using AiContentFactory.Domain.Events;
using AiContentFactory.Domain.Memory.AgentMemories;
using AiContentFactory.Domain.Pipeline;
using AiContentFactory.Domain.Processing;
using AiContentFactory.Domain.Agents;
using AiContentFactory.Domain.Decisions;
using AiContentFactory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AiContentFactory.Infrastructure.Shorts;

public sealed class ShortExecutionAgent : IShortsAgent
{
    private readonly IDecisionEngine _decisionEngine;
    private readonly AspectRatioConverter _aspectRatio;
    private readonly ShortDurationEnforcer _durationEnforcer;
    private readonly SegmentScorer _scorer;
    private readonly ILocalMemoryService _localMemory;
    private readonly IGlobalMemoryService _globalMemory;
    private readonly IGoogleDriveService _drive;
    private readonly IStudioWorkspaceStore _workspaceStore;
    private readonly IPipelineOrchestrator _orchestrator;
    private readonly ITempStorageManager _tempStorage;
    private readonly IRealtimeEventEmitter _emitter;
    private readonly StudioDbContext _dbContext;
    private readonly ILogger<ShortExecutionAgent> _logger;

    public ShortExecutionAgent(
        IDecisionEngine decisionEngine,
        AspectRatioConverter aspectRatio,
        ShortDurationEnforcer durationEnforcer,
        SegmentScorer scorer,
        ILocalMemoryService localMemory,
        IGlobalMemoryService globalMemory,
        IGoogleDriveService drive,
        IStudioWorkspaceStore workspaceStore,
        IPipelineOrchestrator orchestrator,
        ITempStorageManager tempStorage,
        IRealtimeEventEmitter emitter,
        StudioDbContext dbContext,
        ILogger<ShortExecutionAgent> logger)
    {
        _decisionEngine = decisionEngine;
        _aspectRatio = aspectRatio;
        _durationEnforcer = durationEnforcer;
        _scorer = scorer;
        _localMemory = localMemory;
        _globalMemory = globalMemory;
        _drive = drive;
        _workspaceStore = workspaceStore;
        _orchestrator = orchestrator;
        _tempStorage = tempStorage;
        _emitter = emitter;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<List<ShortClip>> GenerateShortsAsync(Guid jobId, CancellationToken ct = default)
    {
        var job = await _dbContext.VideoPipelineJobs.FindAsync(new object[] { jobId }, ct) ?? throw new Exception("Job not found");
        var script = await _dbContext.ScriptOutputs.FirstOrDefaultAsync(s => s.JobId == jobId, ct);
        var localMemory = await _localMemory.GetConfigAsync<ShortsAgentLocalMemory>("shorts-agent", ct) ?? new ShortsAgentLocalMemory();

        var context = new Dictionary<string, string>
        {
            { "duration", "600" }, // Mock
            { "sceneChanges", "10.5, 45.2, 120.0" },
            { "audioPeaks", "12.0, 48.0" },
            { "scriptSummary", script?.Body ?? "N/A" }
        };

        var decision = await _decisionEngine.MakeDecisionAsync("shorts-agent", DecisionType.ShortGeneration, context, jobId, ct);
        var payload = ParseShortsDecision(decision);

        var clips = new List<ShortClip>();
        int clipNumber = 1;

        foreach (var segment in payload.Shorts.Take(localMemory.MaxShortsPerVideo))
        {
            var duration = segment.EndTime - segment.StartTime;
            if (duration < localMemory.MinSegmentDuration || duration > localMemory.MaxSeconds)
            {
                continue; // Skip invalid
            }

            var clip = new ShortClip
            {
                JobId = jobId,
                ParentVideoFileId = job.DriveFileId,
                ClipNumber = clipNumber++,
                Title = segment.Title,
                Hook = segment.Hook,
                Rationale = segment.Rationale,
                StartTime = segment.StartTime,
                EndTime = segment.EndTime,
                Duration = duration,
                AspectRatio = localMemory.AspectRatio,
                EngagementScore = segment.EngagementScore,
                Status = ShortClipStatus.Planned,
                CreatedAt = DateTimeOffset.UtcNow
            };

            clips.Add(clip);
            _dbContext.ShortClips.Add(clip);
        }

        await _dbContext.SaveChangesAsync(ct);

        var tempDir = _tempStorage.CreateJobDirectory(jobId);
        var sourcePath = Path.Combine(tempDir, "input.mp4"); // Mock source path

        foreach (var clip in clips)
        {
            await ProcessShortClipAsync(clip, sourcePath, ct);
        }

        await _localMemory.RecordRunAsync("shorts-agent", true, null, ct);
        await _orchestrator.TransitionStageAsync(jobId, PipelineStageType.ShortGeneration, ct);
        await _emitter.EmitShortsCreatedAsync(new ShortsCreatedPayload(jobId, clips.Count, 60), ct);

        return clips;
    }

    public async Task ProcessShortClipAsync(ShortClip clip, string sourcePath, CancellationToken ct = default)
    {
        clip.Status = ShortClipStatus.Processing;
        await _dbContext.SaveChangesAsync(ct);

        try
        {
            var tempDir = Path.GetDirectoryName(sourcePath)!;
            var shortName = $"short_{clip.ClipNumber}.mp4";
            var trimPath = Path.Combine(tempDir, "trim_" + shortName);
            var finalPath = Path.Combine(tempDir, "final_" + shortName);

            // 1. Trim
            await _durationEnforcer.TrimToDuration(sourcePath, trimPath, clip.StartTime, clip.EndTime, ct);

            // 2. Convert to 9:16
            await _aspectRatio.ConvertTo916(trimPath, finalPath, ct);

            clip.Status = ShortClipStatus.Ready;
            clip.OutputFileName = shortName;
            clip.ProcessedAt = DateTimeOffset.UtcNow;
            await _dbContext.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            clip.Status = ShortClipStatus.Failed;
            await _dbContext.SaveChangesAsync(ct);
            _logger.LogError(ex, "Failed to process short clip {ClipId}", clip.Id);
        }
    }

    public async Task<List<ShortClip>> GetShortsAsync(Guid jobId, CancellationToken ct = default)
    {
        return await _dbContext.ShortClips.Where(c => c.JobId == jobId).ToListAsync(ct);
    }

    public async Task<List<ShortClip>> RegenerateShortsAsync(Guid jobId, int maxShorts, int minDuration, CancellationToken ct = default)
    {
        // Delete existing planned/failed ones, keep ready ones, or just regenerate all
        // Simplified:
        return await GenerateShortsAsync(jobId, ct);
    }

    public async Task<bool> ValidateShortAsync(ShortClip clip, CancellationToken ct = default)
    {
        return clip.Duration >= 15 && clip.Duration <= 60 && clip.Width == 1080 && clip.Height == 1920;
    }

    public async Task<double> ScoreSegmentAsync(double startTime, double endTime, VideoAnalysisResult analysis, CancellationToken ct = default)
    {
        return await _scorer.ScoreSegmentAsync(startTime, endTime, analysis, ct);
    }

    private ShortDecisionPayload ParseShortsDecision(AgentDecision decision)
    {
        var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return JsonSerializer.Deserialize<ShortDecisionPayload>(decision.ValidatedPayload, opts) 
               ?? new ShortDecisionPayload();
    }
}

public class ShortDecisionPayload
{
    public string ParentVideoId { get; set; } = string.Empty;
    public List<ShortSegment> Shorts { get; set; } = new();
}

public class ShortSegment
{
    public double StartTime { get; set; }
    public double EndTime { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Hook { get; set; } = string.Empty;
    public string Rationale { get; set; } = string.Empty;
    public double EngagementScore { get; set; }
}
