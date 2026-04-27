namespace AiContentFactory.Domain.Pipeline;

public sealed class ViralPattern
{
    public Guid Id { get; set; }
    public string PatternType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public string[] AffectedVideos { get; set; } = [];
    public DateTimeOffset DiscoveredAt { get; set; } = DateTimeOffset.UtcNow;
}
