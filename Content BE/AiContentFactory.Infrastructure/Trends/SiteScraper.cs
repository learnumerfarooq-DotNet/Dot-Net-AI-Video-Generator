using AiContentFactory.Domain.Trends;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AiContentFactory.Infrastructure.Trends;

public sealed class TrendOptions
{
    public string RunCronExpression { get; set; } = "0 0 * * * ?";
    public List<string> Tier1Sites { get; set; } = new();
    public List<string> Tier2Sites { get; set; } = new();
    public List<string> Tier3Sites { get; set; } = new();
    public bool UseOpenRouterFallback { get; set; } = true;
    public int MaxSitesToCheck { get; set; } = 50;
    public List<string> PeakSlotsUtc { get; set; } = new();
}

public sealed class SiteScraper
{
    private readonly TrendOptions _options;
    private readonly ILogger<SiteScraper> _logger;

    public SiteScraper(IOptions<TrendOptions> options, ILogger<SiteScraper> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<List<ScrapeResult>> ScrapeAllAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Starting tiered trend scraping for {Count} sites.", _options.MaxSitesToCheck);
        
        var results = new List<ScrapeResult>();

        // TIER 1
        foreach (var site in _options.Tier1Sites)
        {
            results.Add(await ScrapeSiteAsync(site, 1, ct));
        }

        // TIER 2 (Continue if needed)
        if (results.Count(r => r.Success && r.TopicsFound > 0) < 10)
        {
            foreach (var site in _options.Tier2Sites)
            {
                results.Add(await ScrapeSiteAsync(site, 2, ct));
            }
        }

        // TIER 3
        if (results.Count(r => r.Success && r.TopicsFound > 0) < 20)
        {
            foreach (var site in _options.Tier3Sites)
            {
                results.Add(await ScrapeSiteAsync(site, 3, ct));
            }
        }

        _logger.LogInformation("Scraping completed. Successful scrapes: {Count}", results.Count(r => r.Success));
        return results;
    }

    public async Task<ScrapeResult> ScrapeSiteAsync(string url, int tier, CancellationToken ct = default)
    {
        _logger.LogDebug("Scraping Tier {Tier} site: {Url}", tier, url);
        
        try
        {
            // Simulation of scraping
            await Task.Delay(200, ct);
            
            return new ScrapeResult
            {
                Id = Guid.NewGuid(),
                SiteUrl = url,
                Tier = tier,
                Success = true,
                TopicsFound = 1,
                RawContent = $"Mock content from {url}",
                ScrapedAt = DateTimeOffset.UtcNow,
                DurationMs = 200,
                ResponseCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to scrape {Url}", url);
            return new ScrapeResult
            {
                Id = Guid.NewGuid(),
                SiteUrl = url,
                Tier = tier,
                Success = false,
                ErrorMessage = ex.Message,
                ScrapedAt = DateTimeOffset.UtcNow
            };
        }
    }
}
