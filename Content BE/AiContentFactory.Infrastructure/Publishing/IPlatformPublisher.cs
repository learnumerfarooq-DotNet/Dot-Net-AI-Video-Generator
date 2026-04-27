using AiContentFactory.Domain.Publishing;

namespace AiContentFactory.Infrastructure.Publishing;

public interface IPlatformPublisher
{
    string PlatformName { get; }
    Task<string> UploadAsync(Stream videoStream, PlatformMetadata metadata, CancellationToken ct = default);
}
