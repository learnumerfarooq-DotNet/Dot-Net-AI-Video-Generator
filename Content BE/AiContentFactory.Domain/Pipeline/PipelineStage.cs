namespace AiContentFactory.Domain.Pipeline;

public sealed class PipelineStage
{
    public Guid Id { get; private set; }
    public Guid JobId { get; private set; }
    public PipelineStageType StageType { get; private set; }
    public StageStatus Status { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    private PipelineStage() { }

    public static PipelineStage Create(Guid jobId, PipelineStageType type, StageStatus status, string? error = null)
    {
        return new PipelineStage
        {
            Id = Guid.NewGuid(),
            JobId = jobId,
            StageType = type,
            Status = status,
            ErrorMessage = error,
            StartedAt = DateTimeOffset.UtcNow,
            CompletedAt = status == StageStatus.Completed || status == StageStatus.Failed
                ? DateTimeOffset.UtcNow
                : null
        };
    }
}