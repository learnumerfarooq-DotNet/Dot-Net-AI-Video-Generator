namespace AiContentFactory.Domain.Memory.AgentMemories;

public sealed class AnalyticsAgentLocalMemory
{
    public List<Guid> VideoIdsTracked { get; set; } = new();
    public long Views { get; set; }
    public long Likes { get; set; }
    public Dictionary<Guid, double> CTRPerVideo { get; set; } = new();
    public string OutputFolder { get; set; } = "/Logs/analytics/";
    public DateTimeOffset? LastRunTimestamp { get; set; }
    public int CollectionPeriodDays { get; set; } = 7;
    public List<Guid> TopPerformers { get; set; } = new();
    public Dictionary<string, double> AlertThresholds { get; set; } = new();
}
