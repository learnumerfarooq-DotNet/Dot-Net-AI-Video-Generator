using AiContentFactory.Application.Publishing;
using AiContentFactory.Domain.Publishing;
using AiContentFactory.Domain.Publishing.Facebook;
using AiContentFactory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace AiContentFactory.Infrastructure.Publishing.Facebook;

public class FacebookPublisher : IPlatformPublisher
{
    private readonly FacebookOptions _options;
    private readonly StudioDbContext _dbContext;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<FacebookPublisher> _logger;

    public FacebookPublisher(
        IOptions<FacebookOptions> options,
        StudioDbContext dbContext,
        IHttpClientFactory httpClientFactory,
        ILogger<FacebookPublisher> logger)
    {
        _options = options.Value;
        _dbContext = dbContext;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public string PlatformName => "Facebook";

    public async Task<string> UploadAsync(Stream videoStream, PlatformMetadata metadata, CancellationToken ct = default)
    {
        _logger.LogInformation("Starting Facebook video upload for: {Title}", metadata.Title);

        var agentKey = "upload-agent";
        var cred = await _dbContext.FacebookCredentials.FirstOrDefaultAsync(c => c.AgentKey == agentKey, ct)
                   ?? throw new Exception("Facebook credentials not found");

        var client = _httpClientFactory.CreateClient();

        // Facebook Resumable Upload Flow: Start -> Transfer -> Finish
        // For simplicity in this implementation, we use a single-step upload if possible or a simplified 3-step.
        
        // 1. Start
        var startUrl = $"{_options.GraphApiBaseUrl}/{cred.PageId}/videos";
        var startRequest = new
        {
            upload_phase = "start",
            file_size = videoStream.Length,
            access_token = cred.PageAccessToken
        };

        var startResponse = await client.PostAsJsonAsync(startUrl, startRequest, ct);
        startResponse.EnsureSuccessStatusCode();
        var startData = await startResponse.Content.ReadFromJsonAsync<FacebookUploadSessionResponse>(cancellationToken: ct);
        var uploadSessionId = startData?.UploadSessionId ?? throw new Exception("Failed to start Facebook upload session");

        // 2. Transfer
        var transferUrl = $"{_options.GraphApiBaseUrl}/{uploadSessionId}";
        using var content = new MultipartFormDataContent();
        var streamContent = new StreamContent(videoStream);
        content.Add(new StringContent("transfer"), "upload_phase");
        content.Add(new StringContent("0"), "start_offset");
        content.Add(new StringContent(cred.PageAccessToken), "access_token");
        content.Add(streamContent, "video_file_chunk", "video.mp4");

        var transferResponse = await client.PostAsync(transferUrl, content, ct);
        transferResponse.EnsureSuccessStatusCode();

        // 3. Finish
        var finishRequest = new
        {
            upload_phase = "finish",
            access_token = cred.PageAccessToken,
            title = metadata.Title,
            description = metadata.Description
        };

        var finishResponse = await client.PostAsJsonAsync(transferUrl, finishRequest, ct);
        finishResponse.EnsureSuccessStatusCode();
        var finishData = await finishResponse.Content.ReadFromJsonAsync<FacebookIdResponse>(cancellationToken: ct);

        _logger.LogInformation("Facebook video upload successful. VideoId: {VideoId}", finishData?.Id);

        return finishData?.Id ?? uploadSessionId;
    }

    private class FacebookUploadSessionResponse { public string UploadSessionId { get; set; } = string.Empty; }
    private class FacebookIdResponse { public string Id { get; set; } = string.Empty; }
}
