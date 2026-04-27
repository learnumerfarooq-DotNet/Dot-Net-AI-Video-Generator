using AiContentFactory.Domain.Analytics;
using Microsoft.Extensions.Logging;

namespace AiContentFactory.Infrastructure.Analytics;

public sealed class AnalyticsOptions
{
    public string RunCronExpression { get; set; } = "0 0 3 * * ?";
    public int RetentionDays { get; set; } = 180;
    public double PatternDetectionThreshold { get; set; } = 0.7;
    public bool AutoGenerateVariations { get; set; } = true;
    public int MaxVariationsPerVideo { get; set; } = 3;
}

public sealed class AnalyticsCollector
{
    private readonly PlatformStatsCollector _statsCollector;
    private readonly ILogger<AnalyticsCollector> _logger;

    public AnalyticsCollector(PlatformStatsCollector statsCollector, ILogger<AnalyticsCollector> logger)
    {
        _statsCollector = statsCollector;
        _logger = logger;
    }

    public async Task<List<VideoAnalytics>> CollectDailyStatsAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Starting daily analytics collection from all platforms.");
        
        var stats = new List<VideoAnalytics>();
        var platforms = new[] { "YouTube", "TikTok", "Instagram", "Facebook", "LinkedIn" };

        foreach (var platform in platforms)
        {
            stats.AddRange(await _statsCollector.CollectStatsAsync(platform, ct));
        }

        _logger.LogInformation("Collected stats for {Count} video-platform pairs.", stats.Count);
        return stats;
    }
}
