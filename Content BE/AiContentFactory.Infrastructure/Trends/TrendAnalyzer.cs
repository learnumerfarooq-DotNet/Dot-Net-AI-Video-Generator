using AiContentFactory.Application.Decisions;
using AiContentFactory.Domain.Decisions;
using AiContentFactory.Domain.Trends;
using System.Text.Json;

namespace AiContentFactory.Infrastructure.Trends;

public class TrendAnalyzer
{
    private readonly IDecisionEngine _decisionEngine;

    public TrendAnalyzer(IDecisionEngine decisionEngine)
    {
        _decisionEngine = decisionEngine;
    }

    public async Task<TrendResult> AnalyzeTrendsAsync(List<ScrapeResult> scrapes, CancellationToken ct)
    {
        var context = new Dictionary<string, string>
        {
            { "scrapeData", string.Join("\n", scrapes.Where(s => s.Success).Select(s => $"{s.SiteUrl}: {s.RawContent}")) },
            { "jsonSchema", TrendPrompts.JsonSchema }
        };

        var decision = await _decisionEngine.MakeDecisionAsync("trend-agent", DecisionType.TrendDiscovery, context, Guid.Empty, ct);
        
        var payload = JsonSerializer.Deserialize<TrendResult>(decision.ValidatedPayload, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) 
                      ?? new TrendResult();

        payload.DiscoveredAt = DateTimeOffset.UtcNow;
        payload.TotalSitesScraped = scrapes.Count;
        payload.SuccessfulScrapes = scrapes.Count(s => s.Success);
        payload.FailedScrapes = scrapes.Count(s => !s.Success);

        return payload;
    }

    public List<DiscoveredTopic> DeduplicateTopics(List<DiscoveredTopic> topics)
    {
        return topics.GroupBy(t => t.Keyword.ToLower()).Select(g => g.First()).ToList();
    }
}
