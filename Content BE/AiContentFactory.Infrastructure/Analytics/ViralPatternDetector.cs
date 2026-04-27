using AiContentFactory.Application.Decisions;
using AiContentFactory.Domain.Analytics;
using AiContentFactory.Domain.Decisions;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AiContentFactory.Infrastructure.Analytics;

public sealed class ViralPatternDetector
{
    private readonly IDecisionEngine _decisionEngine;
    private readonly ILogger<ViralPatternDetector> _logger;

    public ViralPatternDetector(IDecisionEngine decisionEngine, ILogger<ViralPatternDetector> logger)
    {
        _decisionEngine = decisionEngine;
        _logger = logger;
    }

    public async Task<List<ViralPattern>> DetectPatternsAsync(List<VideoAnalytics> stats, CancellationToken ct = default)
    {
        _logger.LogInformation("Analyzing performance data for {Count} entries to detect viral patterns.", stats.Count);

        var context = new Dictionary<string, string>
        {
            { "stats", JsonSerializer.Serialize(stats.Take(20)) },
            { "jsonSchema", AnalyticsPrompts.JsonSchema }
        };

        var decision = await _decisionEngine.MakeDecisionAsync("analytics-agent", DecisionType.AnalyticsInsight, context, Guid.Empty, ct);
        
        var result = JsonSerializer.Deserialize<PatternDetectionResult>(decision.ValidatedPayload, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) 
                      ?? new PatternDetectionResult();

        return result.DetectedPatterns.Select(p => new ViralPattern
        {
            Id = Guid.NewGuid(),
            PatternType = p.PatternType,
            Description = p.Description,
            Confidence = p.Confidence,
            DiscoveredAt = DateTimeOffset.UtcNow
        }).ToList();
    }

    private class PatternDetectionResult
    {
        public List<ViralPatternDto> DetectedPatterns { get; set; } = new();
    }

    private class ViralPatternDto
    {
        public string PatternType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Confidence { get; set; }
    }
}
