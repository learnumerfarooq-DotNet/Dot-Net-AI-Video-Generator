namespace AiContentFactory.Domain.Memory;

public sealed class AgentLocalMemory
{
    public Guid Id { get; set; }
    public string AgentKey { get; set; } = string.Empty;
    public string AgentDisplayName { get; set; } = string.Empty;
    public string ConfigJson { get; set; } = string.Empty;
    public DateTimeOffset? LastRunAt { get; set; }
    public DateTimeOffset? LastSuccessAt { get; set; }
    public DateTimeOffset? LastErrorAt { get; set; }
    public string? LastErrorMessage { get; set; }
    public long RunCount { get; set; }
    public long SuccessCount { get; set; }
    public long FailureCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
