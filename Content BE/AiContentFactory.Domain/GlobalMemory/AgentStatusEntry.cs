using AiContentFactory.Domain.Brain;

namespace AiContentFactory.Domain.GlobalMemory;

public sealed class AgentStatusEntry
{
    public string AgentKey { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public AgentHealthStatus Status { get; set; }
    public DateTimeOffset? LastRunAt { get; set; }
    public DateTimeOffset? LastSuccessAt { get; set; }
    public DateTimeOffset? LastErrorAt { get; set; }
    public string? LastErrorMessage { get; set; }
    public long TotalRuns { get; set; }
    public long TotalSuccesses { get; set; }
    public long TotalFailures { get; set; }
    public double AverageRunDurationMs { get; set; }
}
