namespace AiContentFactory.Domain.Publishing.TikTok;

public sealed class TikTokUploadResult
{
    public Guid Id { get; set; }
    public Guid PlatformPublishJobId { get; set; }
    public string TikTokVideoId { get; set; } = string.Empty;
    public string TikTokUrl { get; set; } = string.Empty;
    public string CreatorId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string UploadStatus { get; set; } = string.Empty;
    public string PrivacyLevel { get; set; } = "public";
    public bool AllowComments { get; set; } = true;
    public bool AllowDuet { get; set; } = false;
    public bool AllowStitch { get; set; } = false;
    public DateTimeOffset UploadedAt { get; set; }
}

public sealed class TikTokCredential
{
    public Guid Id { get; set; }
    public string AgentKey { get; set; } = string.Empty;
    public string ClientKey { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
}
