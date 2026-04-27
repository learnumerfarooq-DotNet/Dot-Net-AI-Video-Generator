using AiContentFactory.Domain.Publishing;
using AiContentFactory.Domain.Publishing.YouTube;
using AiContentFactory.Infrastructure.Publishing.YouTube;
using Microsoft.Extensions.Logging;

namespace AiContentFactory.Infrastructure.Publishing;

public sealed class YouTubePublisher : IPlatformPublisher
{
    private readonly YouTubeUploadService _uploadService;
    private readonly ILogger<YouTubePublisher> _logger;

    public YouTubePublisher(YouTubeUploadService uploadService, ILogger<YouTubePublisher> logger)
    {
        _uploadService = uploadService;
        _logger = logger;
    }

    public string PlatformName => "YouTube";

    public async Task<string> UploadAsync(Stream videoStream, PlatformMetadata metadata, CancellationToken ct = default)
    {
        _logger.LogInformation("Starting real YouTube upload for: {Title}", metadata.Title);

        var details = new YouTubeVideoDetails
        {
            Title = metadata.Title,
            Description = metadata.Description,
            Tags = metadata.Tags ?? metadata.Keywords ?? new List<string>(),
            Privacy = metadata.Privacy ?? "private",
            IsShort = metadata.IsShort
        };

        // For IPlatformPublisher compatibility, we need an agentKey. 
        // We'll assume a default or get it from metadata if available.
        var agentKey = "upload-agent"; 

        var result = await _uploadService.UploadVideoAsync(videoStream, details, agentKey, ct);
        
        _logger.LogInformation("YouTube upload successful. VideoId: {VideoId}", result.YouTubeVideoId);
        
        return result.YouTubeVideoId;
    }
}
