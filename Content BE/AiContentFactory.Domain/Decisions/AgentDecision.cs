namespace AiContentFactory.Domain.Decisions;

public enum DecisionType
{
    ScriptGeneration,
    VideoEditing,
    ShortGeneration,
    ShortEditing,
    TrendDiscovery,
    UploadMetadata,
    AnalyticsInsight,
    ContentVariation
}

public enum DecisionOutcome
{
    Pending,
    Validated,
    Accepted,
    Rejected,
    Modified,
    Executed,
    Failed
}

public sealed class AgentDecision
{
    public Guid Id { get; set; }
    public string AgentKey { get; set; } = string.Empty;
    public DecisionType Type { get; set; }
    public DecisionOutcome Outcome { get; set; }
    public string RawJsonPayload { get; set; } = string.Empty;
    public string ValidatedPayload { get; set; } = string.Empty;
    public double ConfidenceScore { get; set; }
    public string PromptVersion { get; set; } = string.Empty;
    public Guid? JobId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class PromptTemplate
{
    public Guid Id { get; set; }
    public string AgentKey { get; set; } = string.Empty;
    public DecisionType DecisionType { get; set; }
    public string Version { get; set; } = "1.0";
    public string SystemPrompt { get; set; } = string.Empty;
    public string UserPromptTemplate { get; set; } = string.Empty;
    public string JsonOutputSchema { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ActivatedAt { get; set; }
}

public sealed class DecisionValidation
{
    public Guid Id { get; set; }
    public Guid DecisionId { get; set; }
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTimeOffset ValidatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class DecisionCacheEntry
{
    public Guid Id { get; set; }
    public string CacheKey { get; set; } = string.Empty;
    public string JsonPayload { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
}
