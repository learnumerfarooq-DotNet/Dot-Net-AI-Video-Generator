using AiContentFactory.Domain.Trends;

namespace AiContentFactory.Application.Agents;

public interface ITrendAgent
{
    Task<TrendResult> DiscoverTrendsAsync(CancellationToken ct);
    Task<List<ScrapeResult>> ScrapeAllSitesAsync(CancellationToken ct);
    Task<ScrapeResult> ScrapeSiteAsync(string siteUrl, int tier, CancellationToken ct);
    Task<TrendResult> AnalyzeTrendsAsync(List<ScrapeResult> scrapes, CancellationToken ct);
    Task<List<PlannedUpload>> CreateScheduleSlotsAsync(TrendResult trends, CancellationToken ct);
    Task<TrendResult?> GetLatestTrendResultAsync(CancellationToken ct);
    Task<List<TrendResult>> GetTrendHistoryAsync(int days, CancellationToken ct);
}
