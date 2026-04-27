namespace AiContentFactory.Domain.GlobalMemory;

public sealed class AnalyticsSummary
{
    public long TotalViews { get; set; }
    public long TotalLikes { get; set; }
    public long TotalComments { get; set; }
    public long TotalShares { get; set; }
    public double AverageCTR { get; set; }
    public double AverageWatchTime { get; set; }
    public double AverageEngagement { get; set; }
    public Guid? TopPerformingVideoId { get; set; }
    public string TopPlatform { get; set; } = string.Empty;
    public double WeeklyGrowthPercent { get; set; }
    public int BestUploadHour { get; set; }
    public DateTimeOffset GeneratedAt { get; set; }
}

public sealed class ErrorSummary
{
    public int TotalErrorsLast24h { get; set; }
    public int TotalErrorsLast7d { get; set; }
    public string MostCommonError { get; set; } = string.Empty;
    public string MostFailedAgent { get; set; } = string.Empty;
    public int RetryQueueCount { get; set; }
    public int DeadLetterCount { get; set; }
    public string CircuitBreakerStatus { get; set; } = "Closed";
    public DateTimeOffset LastUpdated { get; set; }
}

public sealed class ContentStrategy
{
    public List<string> FocusTopics { get; set; } = new();
    public List<string> AvoidTopics { get; set; } = new();
    public List<string> PreferredPlatforms { get; set; } = new();
    public Dictionary<string, double> ContentMixRatio { get; set; } = new();
    public string TonePreference { get; set; } = string.Empty;
    public string TargetAudience { get; set; } = string.Empty;
    public int PostingFrequencyPerDay { get; set; }
    public DateTimeOffset GeneratedAt { get; set; }
}

public sealed class NotificationPreferences
{
    public bool NotifyOnJobComplete { get; set; }
    public bool NotifyOnJobFailed { get; set; }
    public bool NotifyOnTrendDiscovered { get; set; }
    public bool NotifyOnCircuitBreaker { get; set; }
    public bool NotifyOnAnalyticsReady { get; set; }
    public string? WebhookUrl { get; set; }
}

public enum SystemHealthStatus
{
    Healthy = 0,
    Degraded = 1,
    Critical = 2
}
