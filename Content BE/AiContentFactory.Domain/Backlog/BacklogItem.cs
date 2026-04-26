using AiContentFactory.Domain.Artifacts;
using AiContentFactory.Domain.Common;

namespace AiContentFactory.Domain.Backlog;

public sealed record BacklogItem(
    Guid Id,
    string Topic,
    string Platform,
    string Format,
    BacklogStatus Status,
    IReadOnlyList<ContentArtifact> Artifacts,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt) : AggregateRoot(Id);
