namespace AiContentFactory.Domain.Agents;

public enum ShortClipStatus
{
    Planned = 0,
    Processing = 1,
    Ready = 2,
    Failed = 3
}

public sealed class ShortClip
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public string ParentVideoFileId { get; set; } = string.Empty;
    public int ClipNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Hook { get; set; } = string.Empty;
    public string Rationale { get; set; } = string.Empty;
    public double StartTime { get; set; }
    public double EndTime { get; set; }
    public double Duration { get; set; }
    public string AspectRatio { get; set; } = "9:16";
    public int Width { get; set; } = 1080;
    public int Height { get; set; } = 1920;
    public long FileSizeBytes { get; set; }
    public string OutputFileName { get; set; } = string.Empty;
    public string? DriveFileId { get; set; }
    public double EngagementScore { get; set; }
    public ShortClipStatus Status { get; set; }
    public DateTimeOffset? ProcessedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
