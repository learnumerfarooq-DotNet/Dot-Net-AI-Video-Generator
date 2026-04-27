using AiContentFactory.Domain.Analytics;
using Microsoft.Extensions.Logging;

namespace AiContentFactory.Infrastructure.Analytics;

public class ContentScoreCalculator
{
    public double CalculateScore(VideoAnalytics stats)
    {
        // Simple formula: views + (likes * 2) + (comments * 5) normalized by average
        double score = stats.Views * 0.01 + stats.Likes * 0.1 + stats.Comments * 0.5;
        return Math.Min(1.0, score / 1000.0);
    }
}

public class PlatformStatsCollector
{
    private readonly ILogger<PlatformStatsCollector> _logger;

    public PlatformStatsCollector(ILogger<PlatformStatsCollector> logger)
    {
        _logger = logger;
    }

    public async Task<List<VideoAnalytics>> CollectStatsAsync(string platform, CancellationToken ct)
    {
        _logger.LogInformation("Collecting stats for {Platform}", platform);
        
        // Simulation of API calls
        await Task.Delay(500, ct);
        
        return new List<VideoAnalytics>
        {
            new VideoAnalytics
            {
                Id = Guid.NewGuid(),
                Platform = platform,
                Views = 1250,
                Likes = 45,
                Comments = 12,
                Shares = 5,
                CTR = 0.04,
                WatchTime = 45.5,
                EngagementRate = 0.08,
                CollectedAt = DateTimeOffset.UtcNow
            }
        };
    }
}
