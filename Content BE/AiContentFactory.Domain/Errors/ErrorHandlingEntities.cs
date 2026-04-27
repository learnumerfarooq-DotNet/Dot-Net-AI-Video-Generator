namespace AiContentFactory.Domain.Errors;

public sealed class DeadLetterEntry
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public string AgentKey { get; set; } = string.Empty;
    public string Stage { get; set; } = string.Empty;
    public string OriginalError { get; set; } = string.Empty;
    public List<string> AllErrors { get; set; } = new();
    public int RetryAttempts { get; set; }
    public DateTimeOffset FirstFailedAt { get; set; }
    public DateTimeOffset LastFailedAt { get; set; }
    public bool IsResolvable { get; set; }
    public string? ResolutionNotes { get; set; }
    public DateTimeOffset? ArchivedAt { get; set; }
}

public sealed class CircuitBreakerState
{
    public Guid Id { get; set; }
    public string AgentKey { get; set; } = string.Empty;
    public string State { get; set; } = "Closed";
    public int FailureCount { get; set; }
    public int Threshold { get; set; } = 3;
    public int PauseMinutes { get; set; } = 10;
    public DateTimeOffset? LastFailureAt { get; set; }
    public DateTimeOffset? OpenedAt { get; set; }
    public DateTimeOffset? NextRetryAt { get; set; }
}

public sealed class RetryPolicy
{
    public Guid Id { get; set; }
    public string AgentKey { get; set; } = string.Empty;
    public int MaxRetries { get; set; } = 3;
    public List<int> BackoffSeconds { get; set; } = new() { 30, 120, 300 };
    public string BackoffType { get; set; } = "exponential";
    public List<string> RetryOnExceptions { get; set; } = new();
    public List<string> SkipOnExceptions { get; set; } = new();
    public int TimeoutSeconds { get; set; } = 300;
    public bool IsEnabled { get; set; } = true;
    public DateTimeOffset LastUpdated { get; set; }
}
