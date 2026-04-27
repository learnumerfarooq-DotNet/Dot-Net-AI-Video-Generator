namespace AiContentFactory.Domain.Decisions;

public sealed class DecisionAuditLog
{
    public Guid Id { get; set; }
    public Guid DecisionId { get; set; }
    public string AgentKey { get; set; } = string.Empty;
    public DecisionType DecisionType { get; set; }
    public string InputContextHash { get; set; } = string.Empty;
    public string RawResponse { get; set; } = string.Empty;
    public string ValidatedResponse { get; set; } = string.Empty;
    public double ConfidenceScore { get; set; }
    public long LatencyMs { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
