using AiContentFactory.Application.Publishing;
using AiContentFactory.Domain.Publishing;
using AiContentFactory.Domain.Publishing.LinkedIn;
using AiContentFactory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace AiContentFactory.Infrastructure.Publishing.LinkedIn;

public class LinkedInPublisher : IPlatformPublisher
{
    private readonly LinkedInOptions _options;
    private readonly StudioDbContext _dbContext;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<LinkedInPublisher> _logger;

    public LinkedInPublisher(
        IOptions<LinkedInOptions> options,
        StudioDbContext dbContext,
        IHttpClientFactory httpClientFactory,
        ILogger<LinkedInPublisher> logger)
    {
        _options = options.Value;
        _dbContext = dbContext;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public string PlatformName => "LinkedIn";

    public async Task<string> UploadAsync(Stream videoStream, PlatformMetadata metadata, CancellationToken ct = default)
    {
        _logger.LogInformation("Starting LinkedIn video upload for: {Title}", metadata.Title);

        var agentKey = "upload-agent";
        var cred = await _dbContext.LinkedInCredentials.FirstOrDefaultAsync(c => c.AgentKey == agentKey, ct)
                   ?? throw new Exception("LinkedIn credentials not found");

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", cred.AccessToken);

        // LinkedIn Flow: Initialize -> Upload -> Create Post
        
        // 1. Initialize
        var initUrl = $"{_options.BaseUrl}/videos?action=initializeUpload";
        var initRequest = new
        {
            initializeUploadRequest = new
            {
                owner = $"urn:li:organization:{cred.OrganizationId}",
                fileSize = videoStream.Length
            }
        };

        var initResponse = await client.PostAsJsonAsync(initUrl, initRequest, ct);
        initResponse.EnsureSuccessStatusCode();
        var initData = await initResponse.Content.ReadFromJsonAsync<LinkedInInitResponse>(cancellationToken: ct);
        var uploadUrl = initData?.Value?.UploadInstructions?.FirstOrDefault()?.UploadUrl ?? throw new Exception("Failed to get LinkedIn upload URL");
        var videoUrn = initData.Value.Video;

        // 2. Upload Binary
        using var binaryContent = new StreamContent(videoStream);
        binaryContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        var uploadResponse = await client.PutAsync(uploadUrl, binaryContent, ct);
        uploadResponse.EnsureSuccessStatusCode();

        // 3. Create Post
        var postUrl = $"{_options.BaseUrl}/posts";
        var postRequest = new
        {
            author = $"urn:li:organization:{cred.OrganizationId}",
            commentary = $"{metadata.Title}\n\n{metadata.Description}",
            visibility = "PUBLIC",
            distribution = new { feedDistribution = "MAIN_FEED" },
            content = new
            {
                media = new
                {
                    id = videoUrn,
                    title = metadata.Title
                }
            },
            lifecycleState = "PUBLISHED"
        };

        var postResponse = await client.PostAsJsonAsync(postUrl, postRequest, ct);
        postResponse.EnsureSuccessStatusCode();

        _logger.LogInformation("LinkedIn post created successfully. VideoUrn: {VideoUrn}", videoUrn);

        return videoUrn;
    }

    private class LinkedInInitResponse
    {
        public LinkedInInitValue Value { get; set; } = new();
    }

    private class LinkedInInitValue
    {
        public string Video { get; set; } = string.Empty;
        public List<LinkedInUploadInstruction> UploadInstructions { get; set; } = new();
    }

    private class LinkedInUploadInstruction { public string UploadUrl { get; set; } = string.Empty; }
}
