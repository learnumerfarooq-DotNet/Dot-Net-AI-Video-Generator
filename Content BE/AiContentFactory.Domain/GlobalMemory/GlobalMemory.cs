namespace AiContentFactory.Domain.GlobalMemory;

public sealed class GlobalMemory
{
    public FolderRegistry FolderRegistry { get; set; } = new();
    public TrendAgentConfig TrendAgentConfig { get; set; } = new();
    public VideoConstraints VideoConstraints { get; set; } = new();
    public List<string> PeakUploadSlotsUtc { get; set; } = new();
    public DateTimeOffset LastUpdated { get; set; }
    public string Version { get; set; } = "1.0";

    // NEW FIELDS
    public Dictionary<string, AgentStatusEntry> AgentStatuses { get; set; } = new();
    public int ActivePipelineCount { get; set; }
    public long TotalProcessedCount { get; set; }
    public DateTimeOffset? LastSuccessfulUpload { get; set; }
    public DateTimeOffset? LastFailedUpload { get; set; }
    public List<ScheduleSlot> ScheduleSlots { get; set; } = new();
    public AnalyticsSummary? AnalyticsSummary { get; set; }
    public ErrorSummary? ErrorSummary { get; set; }
    public SystemHealthStatus SystemHealth { get; set; }
    public ContentStrategy? ContentStrategy { get; set; }
    public NotificationPreferences? NotificationPreferences { get; set; }
    public DateTimeOffset? LastBrainTickAt { get; set; }
}

public sealed class FolderRegistry
{
    public Dictionary<string, string> AgentFolders { get; set; } = new()
    {
        ["script-gen-agent"] = "/RAW/scripts/",
        ["edit-agent"] = "/Processed/",
        ["shorts-agent"] = "/Shorts/raw/",
        ["short-edit-agent"] = "/Shorts/processed/",
        ["trend-agent"] = "/Scheduler/slots/",
        ["upload-agent"] = "/ReadyToUpload/",
        ["analytics-agent"] = "/Logs/analytics/",
        ["error-queue"] = "/Errors/retry/",
        ["raw-videos"] = "/RAW/",
        ["memory"] = "/memory/"
    };
}

public sealed class TrendAgentConfig
{
    public List<string> Tier1Sites { get; set; } = new()
    {
        "youtube.com", "tiktok.com", "google.com/trends", "trends.google.com",
        "reddit.com", "twitter.com", "x.com", "instagram.com"
    };

    public List<string> Tier2Sites { get; set; } = new()
    {
        "bbc.com", "cnn.com", "reuters.com", "techcrunch.com",
        "theverge.com", "wired.com"
    };

    public List<string> Tier3Sites { get; set; } = new()
    {
        "buzzfeed.com", "mashable.com", "medium.com", "dev.to",
        "hackernoon.com", "producthunt.com"
    };

    public bool UseOpenRouterFallback { get; set; } = true;
    public int MaxSitesToCheck { get; set; } = 50;
}

public sealed class VideoConstraints
{
    public int ShortMaxDurationSeconds { get; set; } = 60;
    public string ShortAspectRatio { get; set; } = "9:16";
    public int ShortWidth { get; set; } = 1080;
    public int ShortHeight { get; set; } = 1920;
    public int ShortMaxFileMb { get; set; } = 100;
    public string ShortFormat { get; set; } = "mp4";
    public int ShortFps { get; set; } = 30;

    public int LongMaxDurationSeconds { get; set; } = 3600;
    public string LongAspectRatio { get; set; } = "16:9";
    public int LongWidth { get; set; } = 1920;
    public int LongHeight { get; set; } = 1080;
    public int LongFps { get; set; } = 30;
    public string LongFormat { get; set; } = "mp4";
}