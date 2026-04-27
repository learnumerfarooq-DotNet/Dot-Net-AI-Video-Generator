using AiContentFactory.Domain.Common;

namespace AiContentFactory.Domain.Pipeline;

public enum PipelineStatus
{
    Idle,
    Watching,
    RawDetected,
    ScriptGenerated,
    Edited,
    ShortClipped,
    ShortEdited,
    TrendScheduled,
    ReadyToUpload,
    Uploading,
    Published,
    AnalyticsCollected,
    Failed,
    RetryPending
}

public enum PipelineStageType
{
    RawDetection,
    ScriptGeneration,
    VideoEditing,
    ShortGeneration,
    ShortEditing,
    TrendDiscovery,
    UploadScheduling,
    PlatformPublishing,
    AnalyticsCollection
}

public enum StageStatus
{
    Pending,
    Running,
    Completed,
    Failed,
    Skipped
}

public sealed class VideoPipelineJob : AggregateRoot
{
    public string DriveFileId { get; private set; } = string.Empty;
    public string FileName { get; private set; } = string.Empty;
    public PipelineStatus Status { get; private set; }
    public PipelineStageType CurrentStage { get; private set; }
    public int RetryCount { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    public VideoMetadata? Metadata { get; private set; }
    public List<PipelineStage> Stages { get; private set; } = new();
    public List<PlatformPublishJob> PublishJobs { get; private set; } = new();

    private VideoPipelineJob() { } // EF Core

    public static VideoPipelineJob Create(string driveFileId, string fileName)
    {
        return new VideoPipelineJob
        {
            Id = Guid.NewGuid(),
            DriveFileId = driveFileId,
            FileName = fileName,
            Status = PipelineStatus.RawDetected,
            CurrentStage = PipelineStageType.RawDetection,
            RetryCount = 0,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    public void TransitionTo(PipelineStageType stage, PipelineStatus status)
    {
        CurrentStage = stage;
        Status = status;
        UpdatedAt = DateTimeOffset.UtcNow;

        Stages.Add(PipelineStage.Create(Id, stage, StageStatus.Completed));
    }

    public void MarkFailed(string errorMessage)
    {
        Status = PipelineStatus.Failed;
        RetryCount++;
        UpdatedAt = DateTimeOffset.UtcNow;

        Stages.Add(PipelineStage.Create(Id, CurrentStage, StageStatus.Failed, errorMessage));
    }

    public void MarkCompleted()
    {
        Status = PipelineStatus.AnalyticsCollected;
        CompletedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}