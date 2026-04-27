namespace AiContentFactory.Domain.Processing;

public sealed class VideoAnalysisResult
{
    public Guid Id { get; set; }
    public double Duration { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public double Fps { get; set; }
    public string Codec { get; set; } = string.Empty;
    public long Bitrate { get; set; }
    public int AudioChannels { get; set; }
    public int AudioSampleRate { get; set; }
    public long FileSizeBytes { get; set; }
    public List<double> SceneChanges { get; set; } = new();
    public double AverageVolume { get; set; }
    public double PeakVolume { get; set; }
}
