using AiContentFactory.Domain.Pipeline;

namespace AiContentFactory.Domain.Agents;

public enum UploadPackageStatus
{
    Preparing,
    Ready,
    Publishing,
    Published,
    Failed
}

public sealed class UploadPackage
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public string VideoType { get; set; } = "short";
    public string SourceDriveFileId { get; set; } = string.Empty;
    public string SourceFolder { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Keywords { get; set; } = new();
    public List<string> Hashtags { get; set; } = new();
    public string Category { get; set; } = string.Empty;
    public string Privacy { get; set; } = "public";
    public DateTimeOffset? ScheduledTime { get; set; }
    public List<string> TargetPlatforms { get; set; } = new();
    public List<PlatformPublishJob> PublishJobs { get; set; } = new();
    public string? ThumbnailDriveFileId { get; set; }
    public string? ThumbnailText { get; set; }
    public Guid? ScheduleSlotId { get; set; }
    public string? TrendKeyword { get; set; }
    public UploadPackageStatus Status { get; set; }
    public double ConfidenceScore { get; set; }
    public bool ApprovalRequired { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
