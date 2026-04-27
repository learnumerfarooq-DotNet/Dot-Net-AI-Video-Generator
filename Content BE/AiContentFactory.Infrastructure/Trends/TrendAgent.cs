using AiContentFactory.Application.Agents;
using AiContentFactory.Domain.Trends;
using AiContentFactory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AiContentFactory.Infrastructure.Trends;

public class TrendAgent : ITrendAgent
{
    private readonly SiteScraper _scraper;
    private readonly TrendAnalyzer _analyzer;
    private readonly TrendScheduler _scheduler;
    private readonly StudioDbContext _dbContext;
    private readonly ILogger<TrendAgent> _logger;

    public TrendAgent(
        SiteScraper scraper,
        TrendAnalyzer analyzer,
        TrendScheduler scheduler,
        StudioDbContext dbContext,
        ILogger<TrendAgent> logger)
    {
        _scraper = scraper;
        _analyzer = analyzer;
        _scheduler = scheduler;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<TrendResult> DiscoverTrendsAsync(CancellationToken ct)
    {
        _logger.LogInformation("Discovering trends...");
        
        // 1. Scrape
        var scrapes = await ScrapeAllSitesAsync(ct);
        
        // 2. Analyze
        var result = await AnalyzeTrendsAsync(scrapes, ct);
        
        // 3. Schedule
        result.PlannedUploads = await CreateScheduleSlotsAsync(result, ct);
        
        // 4. Persist
        _dbContext.TrendResults.Add(result);
        await _dbContext.SaveChangesAsync(ct);
        
        return result;
    }

    public async Task<List<ScrapeResult>> ScrapeAllSitesAsync(CancellationToken ct)
    {
        return await _scraper.ScrapeAllAsync(ct);
    }

    public async Task<ScrapeResult> ScrapeSiteAsync(string siteUrl, int tier, CancellationToken ct)
    {
        return await _scraper.ScrapeSiteAsync(siteUrl, tier, ct);
    }

    public async Task<TrendResult> AnalyzeTrendsAsync(List<ScrapeResult> scrapes, CancellationToken ct)
    {
        return await _analyzer.AnalyzeTrendsAsync(scrapes, ct);
    }

    public async Task<List<PlannedUpload>> CreateScheduleSlotsAsync(TrendResult trends, CancellationToken ct)
    {
        return await _scheduler.ScheduleTrendsAsync(trends, ct);
    }

    public async Task<TrendResult?> GetLatestTrendResultAsync(CancellationToken ct)
    {
        return await _dbContext.TrendResults
            .OrderByDescending(t => t.DiscoveredAt)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<TrendResult>> GetTrendHistoryAsync(int days, CancellationToken ct)
    {
        var cutoff = DateTimeOffset.UtcNow.AddDays(-days);
        return await _dbContext.TrendResults
            .Where(t => t.DiscoveredAt >= cutoff)
            .OrderByDescending(t => t.DiscoveredAt)
            .ToListAsync(ct);
    }
}
