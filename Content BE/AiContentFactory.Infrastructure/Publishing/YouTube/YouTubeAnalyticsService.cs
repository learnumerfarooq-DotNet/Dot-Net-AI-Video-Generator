using AiContentFactory.Application.Publishing;
using AiContentFactory.Domain.Analytics;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTubeAnalytics.v2;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AiContentFactory.Infrastructure.Publishing.YouTube;

public class YouTubeAnalyticsService
{
    private readonly YouTubeOAuthManager _oauthManager;
    private readonly YouTubeOptions _options;
    private readonly ILogger<YouTubeAnalyticsService> _logger;

    public YouTubeAnalyticsService(
        YouTubeOAuthManager oauthManager,
        IOptions<YouTubeOptions> options,
        ILogger<YouTubeAnalyticsService> logger)
    {
        _oauthManager = oauthManager;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<VideoAnalytics> GetVideoStatsAsync(string videoId, string agentKey, CancellationToken ct)
    {
        var credential = await _oauthManager.GetCredentialAsync(agentKey, ct);
        
        var youtubeService = new YouTubeService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = _options.ApplicationName
        });

        var videoRequest = youtubeService.Videos.List("statistics");
        videoRequest.Id = videoId;
        var videoResponse = await videoRequest.ExecuteAsync(ct);
        var video = videoResponse.Items.FirstOrDefault();

        if (video == null) throw new Exception("Video not found");

        return new VideoAnalytics
        {
            VideoId = Guid.Empty, // Needs mapping
            Platform = "YouTube",
            Views = (long)(video.Statistics.ViewCount ?? 0),
            Likes = (long)(video.Statistics.LikeCount ?? 0),
            Comments = (long)(video.Statistics.CommentCount ?? 0),
            CollectedAt = DateTimeOffset.UtcNow
        };
    }

    public async Task<double> GetCTRAsync(string videoId, string agentKey, CancellationToken ct)
    {
        var credential = await _oauthManager.GetCredentialAsync(agentKey, ct);
        
        // This was incorrectly creating a new service instance recursively or with wrong type
        var analyticsService = new Google.Apis.YouTubeAnalytics.v2.YouTubeAnalyticsService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = _options.ApplicationName
        });

        // Simplified: Real CTR requires specific dimensions and metrics from Analytics API
        return 0.05; 
    }
}
