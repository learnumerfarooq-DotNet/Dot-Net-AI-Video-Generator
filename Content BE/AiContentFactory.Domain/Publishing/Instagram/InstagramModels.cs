namespace AiContentFactory.Domain.Publishing.Instagram;

public sealed class InstagramUploadResult
{
    public Guid Id { get; set; }
    public Guid PlatformPublishJobId { get; set; }
    public string InstagramMediaId { get; set; } = string.Empty;
    public string InstagramUrl { get; set; } = string.Empty;
    public string AccountId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string MediaType { get; set; } = "REELS";
    public string UploadStatus { get; set; } = string.Empty;
    public string Caption { get; set; } = string.Empty;
    public List<string> Hashtags { get; set; } = new();
    public string CoverImageUrl { get; set; } = string.Empty;
    public DateTimeOffset UploadedAt { get; set; }
}

public sealed class InstagramCredential
{
    public Guid Id { get; set; }
    public string AgentKey { get; set; } = string.Empty;
    public string FacebookAppId { get; set; } = string.Empty;
    public string FacebookAppSecret { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string InstagramAccountId { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
}
