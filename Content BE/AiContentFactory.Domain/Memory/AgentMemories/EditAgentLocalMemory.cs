namespace AiContentFactory.Domain.Memory.AgentMemories;

public sealed class EditAgentLocalMemory
{
    public string CutStyle { get; set; } = "smooth";
    public string CaptionTemplate { get; set; } = "default";
    public string CaptionPosition { get; set; } = "bottom-center";
    public int CaptionFontSize { get; set; } = 36;
    public string CaptionColor { get; set; } = "#FFFFFF";
    public string InputFolder { get; set; } = "/RAW/";
    public string OutputFolder { get; set; } = "/Processed/";
    public string TransitionType { get; set; } = "fade";
    public bool AudioNormalize { get; set; } = true;
    public Guid? LastErrorId { get; set; }
    public int RetryCount { get; set; }
    public string PreferredCodec { get; set; } = "h264";
}
