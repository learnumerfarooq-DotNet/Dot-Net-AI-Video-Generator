namespace AiContentFactory.Domain.Pipeline;

public sealed class PipelineError
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public string Stage { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public int RetryCount { get; set; }
    public bool IsPermanent { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
