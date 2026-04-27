namespace AiContentFactory.Domain.Publishing;

public record PlatformMetadata(
    string Platform,
    string Title,
    string Description,
    List<string> Keywords,
    List<string> Hashtags,
    string Privacy = "public",
    List<string>? Tags = null,
    string? PlaylistId = null,
    string Language = "en",
    bool IsShort = false);

public record UploadDecisionPayload(
    Guid VideoId,
    List<PlatformMetadata> Platforms);
