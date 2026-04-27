namespace AiContentFactory.Domain.Pipeline;

public sealed class UploadSchedule
{
    public Guid Id { get; set; }
    public Guid VideoPipelineJobId { get; set; }
    public DateTimeOffset ScheduledTimeUtc { get; set; }
    public string Platforms { get; set; } = string.Empty;
    public string Status { get; set; } = "Scheduled";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
