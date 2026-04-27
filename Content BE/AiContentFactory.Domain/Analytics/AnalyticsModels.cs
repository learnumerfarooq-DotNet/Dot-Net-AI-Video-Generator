namespace AiContentFactory.Domain.Analytics;

public sealed class VideoAnalytics
{
    public Guid Id { get; set; }
    public Guid VideoId { get; set; }
    public string Platform { get; set; } = string.Empty;
    public long Views { get; set; }
    public long Likes { get; set; }
    public long Comments { get; set; }
    public long Shares { get; set; }
    public double CTR { get; set; }
    public double WatchTime { get; set; }
    public double EngagementRate { get; set; }
    public DateTimeOffset CollectedAt { get; set; }
}

public sealed class ViralPattern
{
    public Guid Id { get; set; }
    public string PatternType { get; set; } = string.Empty; // e.g. "HookType", "UploadTime", "Topic"
    public string Description { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public List<Guid> AffectedVideos { get; set; } = new();
    public DateTimeOffset DiscoveredAt { get; set; }
}

public sealed class PlatformPerformanceReport
{
    public string Platform { get; set; } = string.Empty;
    public long TotalViews { get; set; }
    public long TotalEngagement { get; set; }
    public List<Guid> TopVideos { get; set; } = new();
    public string Period { get; set; } = "Daily";
    public DateTimeOffset GeneratedAt { get; set; }
}
