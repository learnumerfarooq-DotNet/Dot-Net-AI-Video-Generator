using AiContentFactory.Domain.Analytics;

namespace AiContentFactory.Application.Agents;

public interface IAnalyticsAgent
{
    Task<AnalyticsReport> CollectDailyAnalyticsAsync(CancellationToken ct);
    Task<List<VideoAnalytics>> CollectPlatformStatsAsync(string platform, CancellationToken ct);
    Task<List<ViralPattern>> DetectPatternsAsync(List<VideoAnalytics> stats, CancellationToken ct);
    Task ApplyFeedbackAsync(AnalyticsReport report, CancellationToken ct);
    Task<AnalyticsReport?> GetLatestReportAsync(CancellationToken ct);
    Task<List<AnalyticsReport>> GetReportHistoryAsync(int days, CancellationToken ct);
    Task<double> CalculateContentScoreAsync(Guid videoId, CancellationToken ct);
    Task UpdateGlobalMemoryWithInsightsAsync(AnalyticsReport report, CancellationToken ct);
}
