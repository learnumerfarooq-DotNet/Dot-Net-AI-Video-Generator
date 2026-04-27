namespace AiContentFactory.Domain.Pipeline;

public sealed class VideoMetadata
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public double DurationSeconds { get; set; }
    public string Resolution { get; set; } = string.Empty;
    public string AspectRatio { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public string Codec { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public int Fps { get; set; }
    public string? ThumbnailPath { get; set; }
}