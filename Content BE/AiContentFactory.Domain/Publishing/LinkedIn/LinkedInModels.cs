namespace AiContentFactory.Domain.Publishing.LinkedIn;

public sealed class LinkedInUploadResult
{
    public Guid Id { get; set; }
    public Guid PlatformPublishJobId { get; set; }
    public string LinkedInPostUrn { get; set; } = string.Empty;
    public string LinkedInUrl { get; set; } = string.Empty;
    public string OrganizationId { get; set; } = string.Empty;
    public string AuthorUrn { get; set; } = string.Empty;
    public string UploadStatus { get; set; } = string.Empty;
    public string Visibility { get; set; } = "PUBLIC";
    public string Commentary { get; set; } = string.Empty;
    public string AssetUrn { get; set; } = string.Empty;
    public DateTimeOffset UploadedAt { get; set; }
}

public sealed class LinkedInCredential
{
    public Guid Id { get; set; }
    public string AgentKey { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string OrganizationId { get; set; } = string.Empty;
}
