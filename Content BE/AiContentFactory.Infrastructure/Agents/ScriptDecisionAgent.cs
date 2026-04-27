using AiContentFactory.Application.Decisions;
using AiContentFactory.Domain.Decisions;
using Microsoft.Extensions.Logging;

namespace AiContentFactory.Infrastructure.Agents;

public sealed class ScriptDecisionAgent
{
    private readonly IDecisionEngine _decisionEngine;
    private readonly ILogger<ScriptDecisionAgent> _logger;

    public ScriptDecisionAgent(IDecisionEngine decisionEngine, ILogger<ScriptDecisionAgent> logger)
    {
        _decisionEngine = decisionEngine;
        _logger = logger;
    }

    public async Task<AgentDecision> GenerateDecisionAsync(Guid jobId, Dictionary<string, string> context, CancellationToken ct = default)
    {
        _logger.LogInformation("Agent 'script-agent' generating decision for job {JobId}", jobId);
        
        return await _decisionEngine.MakeDecisionAsync(
            "script-agent", 
            DecisionType.ScriptGeneration, 
            context, 
            jobId, 
            ct);
    }
}
