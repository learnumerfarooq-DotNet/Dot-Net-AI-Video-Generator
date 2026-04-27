namespace AiContentFactory.Domain.Brain;

public enum BrainStatus
{
    Idle = 0,
    Watching = 1,
    Processing = 2,
    Error = 3,
    Paused = 4
}

public enum AgentHealthStatus
{
    Healthy = 0,
    Degraded = 1,
    Failed = 2,
    Disabled = 3
}

public sealed class BrainState
{
    public Guid Id { get; set; }
    public BrainStatus Status { get; set; }
    public long CurrentTickNumber { get; set; }
    public DateTimeOffset LastTickAt { get; set; }
    public DateTimeOffset LastGlobalMemorySync { get; set; }
    public int ActiveJobCount { get; set; }
    public int PendingJobCount { get; set; }
    public int FailedJobCount { get; set; }
    public int CompletedJobCount { get; set; }
    public string? LastErrorMessage { get; set; }
    public Dictionary<string, AgentHealthStatus> AgentHealthMap { get; set; } = new();
    public string GlobalMemoryVersion { get; set; } = string.Empty;
    public bool IsCircuitBreakerOpen { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
