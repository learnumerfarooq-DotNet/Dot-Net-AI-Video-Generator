using AiContentFactory.Domain.Common;

namespace AiContentFactory.Domain.Memory;

public sealed class MemorySuggestion : AggregateRoot
{
    public MemoryScope Scope { get; set; }
    public string? AgentName { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public MemorySuggestionStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public MemorySuggestion() { }

    public MemorySuggestion(Guid id, MemoryScope scope, string? agentName, string content, string reason) : base(id)
    {
        Scope = scope;
        AgentName = agentName;
        Content = content;
        Reason = reason;
        Status = MemorySuggestionStatus.Pending;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}
