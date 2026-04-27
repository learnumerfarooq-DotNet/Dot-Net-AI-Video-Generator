using AiContentFactory.Application.Decisions;
using AiContentFactory.Domain.Trends;
using Microsoft.Extensions.Logging;

namespace AiContentFactory.Infrastructure.Trends;

public sealed class TrendDecisionAgent
{
    private readonly IDecisionEngine _decisionEngine;
    private readonly ILogger<TrendDecisionAgent> _logger;

    public TrendDecisionAgent(IDecisionEngine decisionEngine, ILogger<TrendDecisionAgent> logger)
    {
        _decisionEngine = decisionEngine;
        _logger = logger;
    }

    public async Task<TrendDecisionPayload> GenerateTrendDecisionAsync(IReadOnlyList<TrendingTopic> trends, CancellationToken ct = default)
    {
        _logger.LogInformation("Analyzing {Count} trends to generate upload plan.", trends.Count);

        var context = new Dictionary<string, string>
        {
            ["TrendSummary"] = string.Join(", ", trends.Take(10).Select(t => t.Keyword))
        };

        var decision = await _decisionEngine.MakeDecisionAsync(
            "trend-agent",
            AiContentFactory.Domain.Decisions.DecisionType.TrendDiscovery,
            context,
            null,
            ct);

        return await _decisionEngine.ParsePayloadAsync<TrendDecisionPayload>(decision) 
            ?? throw new InvalidOperationException("Failed to generate trend decision");
    }
}
