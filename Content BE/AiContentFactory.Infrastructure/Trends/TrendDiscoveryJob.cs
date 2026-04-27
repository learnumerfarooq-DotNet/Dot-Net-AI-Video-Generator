using AiContentFactory.Application.Agents;
using Microsoft.Extensions.Logging;

namespace AiContentFactory.Infrastructure.Trends;

public sealed class TrendDiscoveryJob
{
    private readonly ITrendAgent _trendAgent;
    private readonly ILogger<TrendDiscoveryJob> _logger;

    public TrendDiscoveryJob(
        ITrendAgent trendAgent,
        ILogger<TrendDiscoveryJob> logger)
    {
        _trendAgent = trendAgent;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("Starting recurring Trend Discovery job.");

        try
        {
            // Discover trends (Scrape -> Analyze -> Schedule -> Persist)
            var result = await _trendAgent.DiscoverTrendsAsync(CancellationToken.None);

            _logger.LogInformation("Trend Discovery job completed successfully. Found {Count} topics.", result.Topics.Count);

            _logger.LogInformation("Trend Discovery job completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Trend Discovery job failed.");
            throw; // Re-throw to let Hangfire handle retries
        }
    }
}
