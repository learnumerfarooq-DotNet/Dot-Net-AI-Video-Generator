using AiContentFactory.Application.Common;
using AiContentFactory.Application.Pipeline;
using AiContentFactory.Domain.Events;
using AiContentFactory.Domain.Pipeline;
using AiContentFactory.Infrastructure.Persistence;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AiContentFactory.Infrastructure.Pipeline;

public sealed class PipelineOrchestrator : IPipelineOrchestrator
{
    private readonly StudioDbContext _dbContext;
    private readonly IBackgroundJobClient _jobClient;
    private readonly IRealtimeEventEmitter _emitter;
    private readonly ILogger<PipelineOrchestrator> _logger;

    public PipelineOrchestrator(
        StudioDbContext dbContext,
        IBackgroundJobClient jobClient,
        IRealtimeEventEmitter emitter,
        ILogger<PipelineOrchestrator> logger)
    {
        _dbContext = dbContext;
        _jobClient = jobClient;
        _emitter = emitter;
        _logger = logger;
    }

    public async Task<VideoPipelineJob> StartPipelineAsync(string driveFileId, string fileName, CancellationToken ct = default)
    {
        _logger.LogInformation("Starting pipeline for file: {FileName} ({Id})", fileName, driveFileId);

        var job = VideoPipelineJob.Create(driveFileId, fileName);
        _dbContext.VideoPipelineJobs.Add(job);
        
        await _dbContext.SaveChangesAsync(ct);

        // Queue the first stage: Raw Pipeline Handling
        _jobClient.Enqueue<RawPipelineHandler>(h => h.HandleAsync(job.Id, CancellationToken.None));

        await _emitter.EmitJobStartedAsync(new JobStartedPayload(job.Id, fileName, driveFileId), ct);

        return job;
    }

    public async Task TransitionStageAsync(Guid jobId, PipelineStageType stage, CancellationToken ct = default)
    {
        var job = await _dbContext.VideoPipelineJobs
            .Include(j => j.Stages)
            .FirstOrDefaultAsync(j => j.Id == jobId, ct);

        if (job == null) return;

        _logger.LogInformation("Job {JobId} transitioning to {Stage}", jobId, stage);

        // Determine next status based on stage
        var status = stage switch
        {
            PipelineStageType.RawDetection => PipelineStatus.RawDetected,
            PipelineStageType.ScriptGeneration => PipelineStatus.ScriptGenerated,
            PipelineStageType.VideoEditing => PipelineStatus.Edited,
            PipelineStageType.ShortGeneration => PipelineStatus.ShortClipped,
            PipelineStageType.ShortEditing => PipelineStatus.ShortEdited,
            PipelineStageType.TrendDiscovery => PipelineStatus.TrendScheduled,
            PipelineStageType.UploadScheduling => PipelineStatus.ReadyToUpload,
            PipelineStageType.PlatformPublishing => PipelineStatus.Uploading,
            PipelineStageType.AnalyticsCollection => PipelineStatus.Published,
            _ => job.Status
        };

        job.TransitionTo(stage, status);
        await _dbContext.SaveChangesAsync(ct);

        await _emitter.EmitStageCompletedAsync(new StageCompletedPayload(jobId, stage.ToString(), 1.0), ct);

        // Route to next stage handler
        switch (stage)
        {
            case PipelineStageType.RawDetection:
                // Handled by watcher
                break;
            case PipelineStageType.ScriptGeneration:
                _jobClient.Enqueue<Application.Agents.IEditAgent>(h => h.CreateEditPlanAsync(jobId, CancellationToken.None));
                break;
            case PipelineStageType.VideoEditing:
                _jobClient.Enqueue<Application.Agents.IShortsAgent>(h => h.GenerateShortsAsync(jobId, CancellationToken.None));
                break;
            case PipelineStageType.ShortGeneration:
                // This is transitioned to from ShortExecutionAgent which already does the work
                // But if we have a separate ShortEditing agent:
                _jobClient.Enqueue<Application.Agents.IShortEditAgent>(h => h.ExecuteAsync(jobId, CancellationToken.None));
                break;
        }
    }

    public async Task HandleFailureAsync(Guid jobId, string errorMessage, CancellationToken ct = default)
    {
        var job = await _dbContext.VideoPipelineJobs
            .Include(j => j.Stages)
            .FirstOrDefaultAsync(j => j.Id == jobId, ct);

        if (job == null) return;

        _logger.LogError("Job {JobId} failed at stage {Stage}: {Error}", jobId, job.CurrentStage, errorMessage);

        job.MarkFailed(errorMessage);
        await _dbContext.SaveChangesAsync(ct);
        
        await _emitter.EmitJobFailedAsync(new JobFailedPayload(jobId, errorMessage, job.RetryCount, false), ct);

        // Logic for retries or error queue moves
    }

    public async Task<VideoPipelineJob?> GetJobAsync(Guid jobId, CancellationToken ct = default)
    {
        return await _dbContext.VideoPipelineJobs
            .Include(j => j.Stages)
            .Include(j => j.Metadata)
            .FirstOrDefaultAsync(j => j.Id == jobId, ct);
    }

    public async Task<IReadOnlyList<VideoPipelineJob>> GetActiveJobsAsync(CancellationToken ct = default)
    {
        return await _dbContext.VideoPipelineJobs
            .Where(j => j.Status != PipelineStatus.Published && j.Status != PipelineStatus.Failed)
            .OrderByDescending(j => j.UpdatedAt)
            .ToListAsync(ct);
    }
}
