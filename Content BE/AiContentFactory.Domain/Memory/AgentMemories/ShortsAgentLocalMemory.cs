namespace AiContentFactory.Domain.Memory.AgentMemories;

public sealed class ShortsAgentLocalMemory
{
    public int MaxSeconds { get; set; } = 60;
    public string HookStyle { get; set; } = "text-overlay";
    public string? OverlayText { get; set; }
    public string InputFolder { get; set; } = "/Processed/";
    public string OutputFolder { get; set; } = "/Shorts/raw/";
    public int MaxShortsPerVideo { get; set; } = 5;
    public int MinSegmentDuration { get; set; } = 15;
    public bool PreferFastParts { get; set; } = true;
    public string AspectRatio { get; set; } = "9:16";
    public string OutputResolution { get; set; } = "1080x1920";
}
