namespace AiContentFactory.Domain.Pipeline;

public enum PlatformType
{
    YouTube,
    TikTok,
    Instagram,
    Facebook,
    LinkedIn,
    Twitter
}

public enum PublishStatus
{
    Scheduled,
    Uploading,
    Published,
    Failed,
    Cancelled
}

public sealed class PlatformPublishJob
{
    public Guid Id { get; private set; }
    public Guid VideoPipelineJobId { get; private set; }
    public PlatformType Platform { get; private set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Keywords { get; set; } = new();
    public List<string> Hashtags { get; set; } = new();
    public DateTimeOffset? ScheduledTime { get; set; }
    public PublishStatus Status { get; private set; }
    public string? PlatformVideoId { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? PublishedAt { get; private set; }

    public static PlatformPublishJob Create(Guid videoJobId, PlatformType platform)
    {
        return new PlatformPublishJob
        {
            Id = Guid.NewGuid(),
            VideoPipelineJobId = videoJobId,
            Platform = platform,
            Status = PublishStatus.Scheduled,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void MarkPublished(string platformVideoId)
    {
        Status = PublishStatus.Published;
        PlatformVideoId = platformVideoId;
        PublishedAt = DateTimeOffset.UtcNow;
    }

    public void MarkFailed(string error)
    {
        Status = PublishStatus.Failed;
        ErrorMessage = error;
    }
}