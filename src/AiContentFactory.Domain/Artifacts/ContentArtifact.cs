namespace AiContentFactory.Domain.Artifacts;

public sealed record ContentArtifact(
    Guid Id,
    string Kind,
    string Title,
    string Body,
    DateTimeOffset CreatedAt);
