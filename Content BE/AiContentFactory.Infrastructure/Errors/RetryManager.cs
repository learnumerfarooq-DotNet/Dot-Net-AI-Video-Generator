using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;

namespace AiContentFactory.Infrastructure.Errors;

public sealed class ErrorHandlingOptions
{
    public int MaxRetries { get; set; } = 3;
    public List<int> RetryBackoffSeconds { get; set; } = new() { 30, 120, 300 };
    public int CircuitBreakerThreshold { get; set; } = 3;
    public int CircuitBreakerPauseMinutes { get; set; } = 10;
    public int FailureAlertThreshold { get; set; } = 5;
    public bool DeadLetterAfterRetries { get; set; } = true;
}

public sealed class RetryManager
{
    private readonly ErrorHandlingOptions _options;
    private readonly ILogger<RetryManager> _logger;

    public RetryManager(IOptions<ErrorHandlingOptions> options, ILogger<RetryManager> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<T> ExecuteWithRetryAsync<T>(Func<CancellationToken, Task<T>> operation, string operationName, CancellationToken ct = default)
    {
        var retryPolicy = new ResiliencePipelineBuilder<T>()
            .AddRetry(new RetryStrategyOptions<T>
            {
                MaxRetryAttempts = _options.MaxRetries,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                Delay = TimeSpan.FromSeconds(_options.RetryBackoffSeconds.FirstOrDefault()),
                OnRetry = args =>
                {
                    _logger.LogWarning("Retry {Attempt} for {Operation}. Error: {Error}", 
                        args.AttemptNumber, operationName, args.Outcome.Exception?.Message);
                    return default;
                }
            })
            .Build();

        return await retryPolicy.ExecuteAsync(async token => await operation(token), ct);
    }
}
