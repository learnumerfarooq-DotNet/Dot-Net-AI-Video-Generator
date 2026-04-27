namespace AiContentFactory.Domain.Errors;

public sealed class ErrorLog
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public string AgentKey { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public int AttemptNumber { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}
