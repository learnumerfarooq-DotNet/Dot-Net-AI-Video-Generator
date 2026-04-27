using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;

namespace AiContentFactory.Infrastructure.Errors;

public sealed class CircuitBreakerManager
{
    private readonly ErrorHandlingOptions _options;
    private readonly ILogger<CircuitBreakerManager> _logger;
    private readonly ResiliencePipeline _pipeline;

    public CircuitBreakerManager(IOptions<ErrorHandlingOptions> options, ILogger<CircuitBreakerManager> logger)
    {
        _options = options.Value;
        _logger = logger;

        _pipeline = new ResiliencePipelineBuilder()
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromMinutes(5),
                MinimumThroughput = _options.CircuitBreakerThreshold,
                BreakDuration = TimeSpan.FromMinutes(_options.CircuitBreakerPauseMinutes),
                OnOpened = args =>
                {
                    _logger.LogCritical("Circuit Breaker OPENED for {Duration} minutes due to frequent failures.", _options.CircuitBreakerPauseMinutes);
                    return default;
                },
                OnClosed = args =>
                {
                    _logger.LogInformation("Circuit Breaker CLOSED. Normal operation resumed.");
                    return default;
                }
            })
            .Build();
    }

    public async Task ExecuteAsync(Func<CancellationToken, Task> operation, CancellationToken ct = default)
    {
        await _pipeline.ExecuteAsync(async token => await operation(token), ct);
    }

    public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken ct = default)
    {
        return await _pipeline.ExecuteAsync(async token => await operation(token), ct);
    }
}
