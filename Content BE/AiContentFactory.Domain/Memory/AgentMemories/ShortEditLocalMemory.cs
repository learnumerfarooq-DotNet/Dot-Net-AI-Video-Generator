namespace AiContentFactory.Domain.Memory.AgentMemories;

public sealed class ShortEditLocalMemory
{
    public int HookDuration { get; set; } = 3;
    public string CaptionStyle { get; set; } = "word-by-word";
    public string MusicTrackPreference { get; set; } = "trending";
    public double MusicVolume { get; set; } = 0.3;
    public bool OverlayEmoji { get; set; } = true;
    public string InputFolder { get; set; } = "/Shorts/raw/";
    public string OutputFolder { get; set; } = "/Shorts/processed/";
    public bool AddWatermark { get; set; } = false;
    public string? WatermarkText { get; set; }
    public string FontFamily { get; set; } = "Inter";
    public string TransitionEffect { get; set; } = "glitch";
}
