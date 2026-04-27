using AiContentFactory.Application.Common;
using AiContentFactory.Domain.Events;
using Microsoft.Extensions.Logging;

namespace AiContentFactory.Infrastructure.Errors;

public sealed class FailureMonitor
{
    private readonly IRealtimeEventEmitter _emitter;
    private readonly ILogger<FailureMonitor> _logger;

    public FailureMonitor(IRealtimeEventEmitter emitter, ILogger<FailureMonitor> logger)
    {
        _emitter = emitter;
        _logger = logger;
    }

    public async Task RecordFailureAsync(Guid jobId, string operation, string error, CancellationToken ct = default)
    {
        _logger.LogError("Failure recorded for job {JobId} in {Operation}: {Error}", jobId, operation, error);

        // Alert the UI via SignalR
        await _emitter.EmitJobFailedAsync(new JobFailedPayload(jobId, error, 0, false), ct);

        // In a real implementation, we would track failure rates in memory or Redis
        // to trigger a global alert if the threshold is exceeded.
    }
}
