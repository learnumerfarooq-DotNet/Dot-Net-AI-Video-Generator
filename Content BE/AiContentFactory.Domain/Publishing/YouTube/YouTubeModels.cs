namespace AiContentFactory.Domain.Publishing.YouTube;

public sealed class YouTubeUploadResult
{
    public Guid Id { get; set; }
    public Guid PlatformPublishJobId { get; set; }
    public string YouTubeVideoId { get; set; } = string.Empty;
    public string YouTubeUrl { get; set; } = string.Empty;
    public string ChannelId { get; set; } = string.Empty;
    public string ChannelTitle { get; set; } = string.Empty;
    public string UploadStatus { get; set; } = "processing";
    public string ProcessingStatus { get; set; } = string.Empty;
    public string PrivacyStatus { get; set; } = "private";
    public bool IsShort { get; set; }
    public string ThumbnailUrl { get; set; } = string.Empty;
    public DateTimeOffset UploadedAt { get; set; }
    public long FileSizeBytes { get; set; }
    public long DurationMs { get; set; }
}

public sealed class YouTubeCredential
{
    public Guid Id { get; set; }
    public string AgentKey { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public DateTimeOffset TokenExpiresAt { get; set; }
    public string ChannelId { get; set; } = string.Empty;
}
public sealed class YouTubeVideoDetails
{
    public Guid Id { get; set; }
    public string YouTubeVideoId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public string CategoryId { get; set; } = "22"; // People & Blogs
    public string Privacy { get; set; } = "private";
    public DateTimeOffset? ScheduledPublishAt { get; set; }
    public string? ThumbnailPath { get; set; }
    public bool IsShort { get; set; }
    public string? PlaylistId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
