using AiContentFactory.Domain.Common;

namespace AiContentFactory.Domain.Memory;

public sealed record MemoryEntry(
    Guid Id,
    MemoryScope Scope,
    string? AgentName,
    string Content,
    IReadOnlyList<string> Tags,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt) : AggregateRoot(Id);
