using AiContentFactory.Domain.Common;

namespace AiContentFactory.Domain.Memory;

public sealed record MemorySuggestion(
    Guid Id,
    MemoryScope Scope,
    string? AgentName,
    string Content,
    string Reason,
    MemorySuggestionStatus Status,
    DateTimeOffset CreatedAt) : AggregateRoot(Id);
