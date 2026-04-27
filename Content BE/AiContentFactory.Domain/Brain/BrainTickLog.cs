namespace AiContentFactory.Domain.Brain;

public sealed class BrainTickLog
{
    public Guid Id { get; set; }
    public long TickNumber { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public long DurationMs { get; set; }
    public int JobsDispatched { get; set; }
    public int JobsCompleted { get; set; }
    public int JobsFailed { get; set; }
    public bool GlobalMemoryRead { get; set; }
    public string Notes { get; set; } = string.Empty;
}
