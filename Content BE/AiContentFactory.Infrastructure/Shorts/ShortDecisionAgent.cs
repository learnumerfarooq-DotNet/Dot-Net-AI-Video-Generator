using AiContentFactory.Application.Decisions;
using AiContentFactory.Domain.Decisions;
using Microsoft.Extensions.Logging;

namespace AiContentFactory.Infrastructure.Shorts;

public sealed class ShortDecisionAgent
{
    private readonly IDecisionEngine _decisionEngine;
    private readonly ILogger<ShortDecisionAgent> _logger;

    public ShortDecisionAgent(IDecisionEngine decisionEngine, ILogger<ShortDecisionAgent> logger)
    {
        _decisionEngine = decisionEngine;
        _logger = logger;
    }

    public async Task<AgentDecision> GenerateDecisionAsync(Guid jobId, Dictionary<string, string> context, CancellationToken ct = default)
    {
        _logger.LogInformation("Agent 'shorts-agent' generating decision for job {JobId}", jobId);
        
        return await _decisionEngine.MakeDecisionAsync(
            "shorts-agent", 
            DecisionType.ShortGeneration, 
            context, 
            jobId, 
            ct);
    }
}
