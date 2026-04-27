using AiContentFactory.Application.Pipeline;
using AiContentFactory.Domain.Pipeline;
using Microsoft.Extensions.Logging;

namespace AiContentFactory.Infrastructure.Pipeline;

public sealed class AgentDispatcher : IAgentDispatcher
{
    private readonly ILogger<AgentDispatcher> _logger;

    public AgentDispatcher(ILogger<AgentDispatcher> logger)
    {
        _logger = logger;
    }

    public Task DispatchAsync(Guid jobId, PipelineStageType stage, object payload, CancellationToken ct = default)
    {
        _logger.LogInformation("Dispatching job {JobId} to stage {Stage}", jobId, stage);
        // Implementation will route to specific agent handlers
        return Task.CompletedTask;
    }
}
