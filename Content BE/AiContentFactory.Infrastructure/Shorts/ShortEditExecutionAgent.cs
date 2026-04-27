using AiContentFactory.Application.Common;
using AiContentFactory.Application.Decisions;
using AiContentFactory.Application.Agents;
using AiContentFactory.Application.Memory;
using AiContentFactory.Application.Pipeline;
using AiContentFactory.Application.Processing;
using AiContentFactory.Application.Studio;
using AiContentFactory.Domain.Events;
using AiContentFactory.Domain.Pipeline;
using AiContentFactory.Domain.Agents;
using AiContentFactory.Domain.Decisions;
using AiContentFactory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AiContentFactory.Infrastructure.Shorts;

public sealed class ShortEditExecutionAgent : IShortEditAgent
{
    private readonly IDecisionEngine _decisionEngine;
    private readonly IFFmpegService _ffmpeg;
    private readonly ILocalMemoryService _localMemory;
    private readonly IGoogleDriveService _drive;
    private readonly IStudioWorkspaceStore _workspaceStore;
    private readonly IPipelineOrchestrator _orchestrator;
    private readonly ITempStorageManager _tempStorage;
    private readonly CaptionRenderer _captionRenderer;
    private readonly MusicOverlayService _musicService;
    private readonly IRealtimeEventEmitter _emitter;
    private readonly StudioDbContext _dbContext;
    private readonly ILogger<ShortEditExecutionAgent> _logger;

    public ShortEditExecutionAgent(
        IDecisionEngine decisionEngine,
        IFFmpegService ffmpeg,
        ILocalMemoryService localMemory,
        IGoogleDriveService drive,
        IStudioWorkspaceStore workspaceStore,
        IPipelineOrchestrator orchestrator,
        ITempStorageManager tempStorage,
        CaptionRenderer captionRenderer,
        MusicOverlayService musicService,
        IRealtimeEventEmitter emitter,
        StudioDbContext dbContext,
        ILogger<ShortEditExecutionAgent> logger)
    {
        _decisionEngine = decisionEngine;
        _ffmpeg = ffmpeg;
        _localMemory = localMemory;
        _drive = drive;
        _workspaceStore = workspaceStore;
        _orchestrator = orchestrator;
        _tempStorage = tempStorage;
        _captionRenderer = captionRenderer;
        _musicService = musicService;
        _emitter = emitter;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<ShortEditPlan> CreateEditPlanAsync(Guid shortClipId, CancellationToken ct = default)
    {
        var clip = await _dbContext.ShortClips.FindAsync(new object[] { shortClipId }, ct) ?? throw new Exception("Clip not found");
        
        var context = new Dictionary<string, string>
        {
            { "clipDuration", clip.Duration.ToString() },
            { "hookStyle", clip.Hook },
            { "captionPreference", clip.Title }
        };

        var decision = await _decisionEngine.MakeDecisionAsync("short-edit-agent", DecisionType.ShortEditing, context, clip.JobId, ct);
        var payload = JsonSerializer.Deserialize<ShortEditPlan>(decision.ValidatedPayload, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) 
                      ?? new ShortEditPlan();

        payload.ShortClipId = shortClipId;
        payload.JobId = clip.JobId;
        payload.Status = EditPlanStatus.Planned;
        payload.CreatedAt = DateTimeOffset.UtcNow;

        _dbContext.ShortEditPlans.Add(payload);
        await _dbContext.SaveChangesAsync(ct);

        return payload;
    }

    public async Task ExecuteEditPlanAsync(Guid jobId, ShortEditPlan plan, CancellationToken ct = default)
    {
        _logger.LogInformation("Executing Short Edit Plan for clip {ClipId}", plan.ShortClipId);

        try
        {
            plan.Status = EditPlanStatus.Executing;
            await _dbContext.SaveChangesAsync(ct);

            var tempDir = _tempStorage.CreateJobDirectory(jobId);
            var clip = await _dbContext.ShortClips.FindAsync(new object[] { plan.ShortClipId }, ct);
            
            // Mock paths
            var inputPath = Path.Combine(tempDir, $"short_{clip?.ClipNumber ?? 1}.mp4");
            var outputPath = Path.Combine(tempDir, $"processed_short_{clip?.ClipNumber ?? 1}.mp4");

            // 1. Render Captions
            await _captionRenderer.RenderCaptionsAsync(inputPath, outputPath, plan.Captions, ct);

            // 2. Add Music
            if (plan.MusicTrack != null)
            {
                var musicOutput = Path.Combine(tempDir, $"music_short_{clip?.ClipNumber ?? 1}.mp4");
                await _musicService.AddMusicAsync(outputPath, musicOutput, plan.MusicTrack, ct);
                outputPath = musicOutput;
            }

            plan.Status = EditPlanStatus.Completed;
            await _dbContext.SaveChangesAsync(ct);

            await _emitter.EmitShortEditCompletedAsync(new ShortEditCompletedPayload(jobId, plan.ShortClipId.ToString(), "drive-output-id"), ct);
        }
        catch (Exception ex)
        {
            plan.Status = EditPlanStatus.Failed;
            await _dbContext.SaveChangesAsync(ct);
            _logger.LogError(ex, "Failed to execute short edit plan");
            throw;
        }
    }

    public async Task ExecuteAsync(Guid jobId, CancellationToken ct = default)
    {
        _logger.LogInformation("Executing Short Editing for job {JobId}", jobId);
        
        var clips = await _dbContext.ShortClips.Where(c => c.JobId == jobId).ToListAsync(ct);
        
        foreach (var clip in clips)
        {
            var plan = await CreateEditPlanAsync(clip.Id, ct);
            await ExecuteEditPlanAsync(jobId, plan, ct);
        }

        await _orchestrator.TransitionStageAsync(jobId, PipelineStageType.ShortEditing, ct);
    }

    public async Task<ShortEditPlan?> GetEditPlanAsync(Guid shortClipId, CancellationToken ct = default)
    {
        return await _dbContext.ShortEditPlans.FirstOrDefaultAsync(p => p.ShortClipId == shortClipId, ct);
    }

    public async Task ReExecuteAsync(Guid shortClipId, CancellationToken ct = default)
    {
        var plan = await GetEditPlanAsync(shortClipId, ct) ?? throw new Exception("No plan found");
        await ExecuteEditPlanAsync(plan.JobId, plan, ct);
    }

    public Task<bool> ValidatePlanAsync(ShortEditPlan plan, CancellationToken ct = default)
    {
        return Task.FromResult(plan.Captions.Any());
    }
}
