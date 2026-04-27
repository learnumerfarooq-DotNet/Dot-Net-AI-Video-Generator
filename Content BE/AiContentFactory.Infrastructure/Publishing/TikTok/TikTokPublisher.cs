using AiContentFactory.Application.Publishing;
using AiContentFactory.Domain.Publishing;
using AiContentFactory.Domain.Publishing.TikTok;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace AiContentFactory.Infrastructure.Publishing.TikTok;

public class TikTokPublisher : IPlatformPublisher
{
    private readonly TikTokOAuthManager _oauthManager;
    private readonly TikTokOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<TikTokPublisher> _logger;

    public TikTokPublisher(
        TikTokOAuthManager oauthManager,
        IOptions<TikTokOptions> options,
        IHttpClientFactory httpClientFactory,
        ILogger<TikTokPublisher> logger)
    {
        _oauthManager = oauthManager;
        _options = options.Value;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public string PlatformName => "TikTok";

    public async Task<string> UploadAsync(Stream videoStream, PlatformMetadata metadata, CancellationToken ct = default)
    {
        _logger.LogInformation("Starting TikTok upload for: {Title}", metadata.Title);

        // 1. Get Access Token
        var agentKey = "upload-agent"; // Default for now
        var accessToken = await _oauthManager.GetAccessTokenAsync(agentKey, ct);

        // 2. Initialize Upload Session
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var initRequest = new
        {
            post_info = new
            {
                title = metadata.Title,
                privacy_level = metadata.Privacy.ToUpper(),
                disable_comment = false,
                disable_duet = true,
                disable_stitch = true
            },
            source_info = new
            {
                source = "FILE_UPLOAD",
                video_size = videoStream.Length,
                chunk_size = videoStream.Length,
                total_chunk_count = 1
            }
        };

        var initResponse = await client.PostAsJsonAsync($"{_options.BaseUrl}/post/publish/inbox/video/init/", initRequest, ct);
        initResponse.EnsureSuccessStatusCode();
        
        var initData = await initResponse.Content.ReadFromJsonAsync<TikTokInitResponse>(cancellationToken: ct);
        if (initData == null || string.IsNullOrEmpty(initData.Data.UploadUrl)) 
            throw new Exception("Failed to initialize TikTok upload session");

        // 3. Upload Video Data
        var uploadRequest = new HttpRequestMessage(HttpMethod.Put, initData.Data.UploadUrl);
        var content = new StreamContent(videoStream);
        content.Headers.ContentType = new MediaTypeHeaderValue("video/mp4");
        content.Headers.ContentRange = new ContentRangeHeaderValue(0, videoStream.Length - 1, videoStream.Length);
        uploadRequest.Content = content;

        var uploadResponse = await client.SendAsync(uploadRequest, ct);
        uploadResponse.EnsureSuccessStatusCode();

        _logger.LogInformation("TikTok upload successful. Metadata: {PublishId}", initData.Data.PublishId);
        
        return initData.Data.PublishId;
    }

    private class TikTokInitResponse
    {
        public TikTokInitData Data { get; set; } = new();
    }

    private class TikTokInitData
    {
        public string PublishId { get; set; } = string.Empty;
        public string UploadUrl { get; set; } = string.Empty;
    }
}
