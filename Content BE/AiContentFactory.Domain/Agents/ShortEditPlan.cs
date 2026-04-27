namespace AiContentFactory.Domain.Agents;

public sealed class ShortEditPlan
{
    public Guid Id { get; set; }
    public Guid ShortClipId { get; set; }
    public Guid JobId { get; set; }
    public HookOverlayConfig? HookOverlay { get; set; }
    public List<ShortCaption> Captions { get; set; } = new();
    public MusicTrackConfig? MusicTrack { get; set; }
    public List<EmojiOverlay> EmojiOverlays { get; set; } = new();
    public string TransitionIn { get; set; } = "glitch";
    public string TransitionOut { get; set; } = "fade";
    public WatermarkConfig? Watermark { get; set; }
    public string? OutputDriveFileId { get; set; }
    public EditPlanStatus Status { get; set; }
    public List<string> FFmpegCommands { get; set; } = new();
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class HookOverlayConfig
{
    public string Text { get; set; } = string.Empty;
    public int FontSize { get; set; } = 48;
    public string FontColor { get; set; } = "#FFFFFF";
    public string BackgroundColor { get; set; } = "#FF0000";
    public string AnimationType { get; set; } = "pop";
    public double DurationSeconds { get; set; } = 3.0;
}

public sealed class ShortCaption
{
    public double StartTime { get; set; }
    public double EndTime { get; set; }
    public string Text { get; set; } = string.Empty;
    public string Style { get; set; } = "word-by-word";
    public int FontSize { get; set; } = 36;
    public string Color { get; set; } = "#FFFFFF";
    public string Position { get; set; } = "center";
}

public sealed class MusicTrackConfig
{
    public string TrackName { get; set; } = string.Empty;
    public double Volume { get; set; } = 0.3;
    public double FadeInSeconds { get; set; } = 1.0;
    public double FadeOutSeconds { get; set; } = 1.0;
    public string Genre { get; set; } = "trending";
}

public sealed class EmojiOverlay
{
    public string Emoji { get; set; } = string.Empty;
    public double StartTime { get; set; }
    public double EndTime { get; set; }
    public string Position { get; set; } = "top-right";
    public string AnimationType { get; set; } = "bounce";
}

public sealed class WatermarkConfig
{
    public string Text { get; set; } = string.Empty;
    public string Position { get; set; } = "bottom-right";
    public int FontSize { get; set; } = 24;
    public double Opacity { get; set; } = 0.5;
    public string Color { get; set; } = "#FFFFFF";
}
