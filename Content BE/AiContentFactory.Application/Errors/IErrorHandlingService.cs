using AiContentFactory.Domain.Errors;
using AiContentFactory.Domain.GlobalMemory;

namespace AiContentFactory.Application.Errors;

public interface IErrorHandlingService
{
    Task HandleErrorAsync(Guid jobId, string agentKey, string error, CancellationToken ct);
    Task<bool> ShouldRetryAsync(Guid jobId, string agentKey, CancellationToken ct);
    Task RetryJobAsync(Guid jobId, string agentKey, CancellationToken ct);
    Task MoveToDeadLetterAsync(Guid jobId, string reason, CancellationToken ct);
    Task<CircuitBreakerState> GetCircuitStateAsync(string agentKey, CancellationToken ct);
    Task OpenCircuitBreakerAsync(string agentKey, CancellationToken ct);
    Task CloseCircuitBreakerAsync(string agentKey, CancellationToken ct);
    Task<List<DeadLetterEntry>> GetDeadLetterQueueAsync(CancellationToken ct);
    Task<ErrorSummary> GetErrorSummaryAsync(CancellationToken ct);
    Task ResolveDeadLetterAsync(Guid entryId, string resolution, CancellationToken ct);
}
