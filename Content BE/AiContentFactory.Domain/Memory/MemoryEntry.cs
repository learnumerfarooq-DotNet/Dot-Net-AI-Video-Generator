using AiContentFactory.Domain.Common;

namespace AiContentFactory.Domain.Memory;

public sealed class MemoryEntry : AggregateRoot
{
    public MemoryScope Scope { get; set; }
    public string? AgentName { get; set; }
    public string Content { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public float Score { get; set; }

    public MemoryEntry() { }

    public MemoryEntry(Guid id, MemoryScope scope, string? agentName, string content) : base(id)
    {
        Scope = scope;
        AgentName = agentName;
        Content = content;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
