using AiContentFactory.Application.Publishing;
using AiContentFactory.Application.Studio;
using AiContentFactory.Domain.Publishing;
using AiContentFactory.Domain.Publishing.Instagram;
using AiContentFactory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace AiContentFactory.Infrastructure.Publishing.Instagram;

public class InstagramPublisher : IPlatformPublisher
{
    private readonly InstagramOAuthManager _oauthManager;
    private readonly InstagramOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly StudioDbContext _dbContext;
    private readonly ILogger<InstagramPublisher> _logger;

    public InstagramPublisher(
        InstagramOAuthManager oauthManager,
        IOptions<InstagramOptions> options,
        IHttpClientFactory httpClientFactory,
        StudioDbContext dbContext,
        ILogger<InstagramPublisher> logger)
    {
        _oauthManager = oauthManager;
        _options = options.Value;
        _httpClientFactory = httpClientFactory;
        _dbContext = dbContext;
        _logger = logger;
    }

    public string PlatformName => "Instagram";

    public async Task<string> UploadAsync(Stream videoStream, PlatformMetadata metadata, CancellationToken ct = default)
    {
        _logger.LogInformation("Starting Instagram Reels upload for: {Title}", metadata.Title);

        var agentKey = "upload-agent";
        var accessToken = await _oauthManager.GetAccessTokenAsync(agentKey, ct);
        var cred = await _dbContext.InstagramCredentials.FirstAsync(c => c.AgentKey == agentKey, ct);

        // NOTE: Instagram Reels API requires a PUBLIC URL for the video.
        // In a real implementation, we would upload to a temporary public bucket or use a Drive signed URL.
        // For this implementation, we simulate the container creation.
        
        var client = _httpClientFactory.CreateClient();

        // 1. Create Media Container
        var containerUrl = $"{_options.GraphApiBaseUrl}/{cred.InstagramAccountId}/media";
        var containerRequest = new
        {
            media_type = "REELS",
            video_url = "https://temporary-public-url.com/video.mp4", // Placeholder
            caption = $"{metadata.Title}\n\n{metadata.Description}\n{string.Join(" ", metadata.Hashtags.Select(h => "#" + h))}",
            access_token = accessToken
        };

        var containerResponse = await client.PostAsJsonAsync(containerUrl, containerRequest, ct);
        containerResponse.EnsureSuccessStatusCode();
        var containerData = await containerResponse.Content.ReadFromJsonAsync<MetaIdResponse>(cancellationToken: ct);
        var containerId = containerData?.Id ?? throw new Exception("Failed to create Instagram media container");

        // 2. Wait for Processing
        await WaitForProcessingAsync(containerId, accessToken, ct);

        // 3. Publish
        var publishUrl = $"{_options.GraphApiBaseUrl}/{cred.InstagramAccountId}/media_publish";
        var publishRequest = new
        {
            creation_id = containerId,
            access_token = accessToken
        };

        var publishResponse = await client.PostAsJsonAsync(publishUrl, publishRequest, ct);
        publishResponse.EnsureSuccessStatusCode();
        var publishData = await publishResponse.Content.ReadFromJsonAsync<MetaIdResponse>(cancellationToken: ct);

        _logger.LogInformation("Instagram Reels published successfully. MediaId: {MediaId}", publishData?.Id);

        return publishData?.Id ?? containerId;
    }

    private async Task WaitForProcessingAsync(string containerId, string accessToken, CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient();
        var url = $"{_options.GraphApiBaseUrl}/{containerId}?fields=status_code&access_token={accessToken}";

        for (int i = 0; i < 30; i++) // Max 5 minutes
        {
            var response = await client.GetAsync(url, ct);
            var data = await response.Content.ReadFromJsonAsync<MetaStatusResponse>(cancellationToken: ct);

            if (data?.StatusCode == "FINISHED") return;
            if (data?.StatusCode == "ERROR") throw new Exception("Instagram video processing failed");

            _logger.LogDebug("Waiting for Instagram processing: {Status}", data?.StatusCode);
            await Task.Delay(TimeSpan.FromSeconds(10), ct);
        }

        throw new Exception("Instagram processing timed out");
    }

    private class MetaIdResponse { public string Id { get; set; } = string.Empty; }
    private class MetaStatusResponse { public string StatusCode { get; set; } = string.Empty; }
}
