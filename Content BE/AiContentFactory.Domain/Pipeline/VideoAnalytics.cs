namespace AiContentFactory.Domain.Pipeline;

public sealed class VideoAnalytics
{
    public Guid Id { get; set; }
    public Guid VideoPipelineJobId { get; set; }
    public string Platform { get; set; } = string.Empty;
    public long Views { get; set; }
    public long Likes { get; set; }
    public long Comments { get; set; }
    public long Shares { get; set; }
    public double EngagementRate { get; set; }
    public DateTimeOffset CollectedAt { get; set; } = DateTimeOffset.UtcNow;
}
