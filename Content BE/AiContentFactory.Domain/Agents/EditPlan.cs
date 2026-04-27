namespace AiContentFactory.Domain.Agents;

public enum EditPlanStatus
{
    Planned,
    Executing,
    Completed,
    Failed
}

public sealed class EditPlan
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public Guid ScriptId { get; set; }
    public List<EditSegment> Segments { get; set; } = new();
    public List<EditCaption> Captions { get; set; } = new();
    public List<AudioAdjustment> AudioAdjustments { get; set; } = new();
    public List<TransitionPlan> Transitions { get; set; } = new();
    public ColorGradingConfig? ColorGrading { get; set; }
    public string OutputFormat { get; set; } = "mp4";
    public string OutputCodec { get; set; } = "h264";
    public string OutputResolution { get; set; } = "1920x1080";
    public int OutputFps { get; set; } = 30;
    public long EstimatedOutputSize { get; set; }
    public List<string> FFmpegCommands { get; set; } = new();
    public EditPlanStatus Status { get; set; }
    public string InputDriveFileId { get; set; } = string.Empty;
    public string? OutputDriveFileId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class EditSegment
{
    public int Order { get; set; }
    public double StartTime { get; set; }
    public double EndTime { get; set; }
    public string Description { get; set; } = string.Empty;
    public double Speed { get; set; } = 1.0;
    public string? Transition { get; set; }
    public bool KeepAudio { get; set; } = true;
}

public sealed class EditCaption
{
    public double StartTime { get; set; }
    public double EndTime { get; set; }
    public string Text { get; set; } = string.Empty;
    public string Style { get; set; } = "default";
    public string Position { get; set; } = "bottom-center";
    public int FontSize { get; set; } = 36;
    public string Color { get; set; } = "#FFFFFF";
    public string? Background { get; set; }
}

public sealed class AudioAdjustment
{
    public double StartTime { get; set; }
    public double EndTime { get; set; }
    public double VolumeMultiplier { get; set; } = 1.0;
    public bool Normalize { get; set; }
    public double? FadeIn { get; set; }
}

public sealed class TransitionPlan
{
    public double AtTime { get; set; }
    public string Type { get; set; } = "fade";
    public int DurationMs { get; set; }
    public string? Direction { get; set; }
}

public sealed class ColorGradingConfig
{
    public double Brightness { get; set; } = 1.0;
    public double Contrast { get; set; } = 1.0;
    public double Saturation { get; set; } = 1.0;
    public double Gamma { get; set; } = 1.0;
    public int Temperature { get; set; } = 5500;
    public string? LookupTable { get; set; }
}
