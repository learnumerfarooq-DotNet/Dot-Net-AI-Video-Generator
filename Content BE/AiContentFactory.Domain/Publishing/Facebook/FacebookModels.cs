namespace AiContentFactory.Domain.Publishing.Facebook;

public sealed class FacebookUploadResult
{
    public Guid Id { get; set; }
    public Guid PlatformPublishJobId { get; set; }
    public string FacebookVideoId { get; set; } = string.Empty;
    public string FacebookUrl { get; set; } = string.Empty;
    public string PageId { get; set; } = string.Empty;
    public string PageName { get; set; } = string.Empty;
    public string UploadStatus { get; set; } = string.Empty;
    public string Privacy { get; set; } = "EVERYONE";
    public string Description { get; set; } = string.Empty;
    public DateTimeOffset? ScheduledPublishTime { get; set; }
    public DateTimeOffset UploadedAt { get; set; }
}

public sealed class FacebookCredential
{
    public Guid Id { get; set; }
    public string AgentKey { get; set; } = string.Empty;
    public string AppId { get; set; } = string.Empty;
    public string AppSecret { get; set; } = string.Empty;
    public string PageAccessToken { get; set; } = string.Empty;
    public string PageId { get; set; } = string.Empty;
}
