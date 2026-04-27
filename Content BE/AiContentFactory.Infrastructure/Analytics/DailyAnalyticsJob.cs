using AiContentFactory.Application.Agents;
using Microsoft.Extensions.Logging;

namespace AiContentFactory.Infrastructure.Analytics;

public sealed class DailyAnalyticsJob
{
    private readonly IAnalyticsAgent _analyticsAgent;
    private readonly ILogger<DailyAnalyticsJob> _logger;

    public DailyAnalyticsJob(
        IAnalyticsAgent analyticsAgent,
        ILogger<DailyAnalyticsJob> logger)
    {
        _analyticsAgent = analyticsAgent;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("Starting Daily Analytics and Feedback Loop job.");

        try
        {
            // Full analytics lifecycle (Collect -> Detect -> Report -> Persist -> Feedback)
            var report = await _analyticsAgent.CollectDailyAnalyticsAsync(CancellationToken.None);

            _logger.LogInformation("Daily Analytics job completed successfully. Analyzed {Count} videos.", report.TotalVideosAnalyzed);

            _logger.LogInformation("Daily Analytics job completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Daily Analytics job failed.");
            throw;
        }
    }
}
