namespace AiContentFactory.Application.Publishing;

public sealed class FacebookOptions
{
    public string AppId { get; set; } = string.Empty;
    public string AppSecret { get; set; } = string.Empty;
    public string GraphApiBaseUrl { get; set; } = "https://graph.facebook.com/v20.0";
}

public sealed class LinkedInOptions
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.linkedin.com/rest";
}
