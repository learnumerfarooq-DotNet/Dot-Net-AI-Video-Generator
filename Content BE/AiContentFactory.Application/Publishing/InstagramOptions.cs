namespace AiContentFactory.Application.Publishing;

public sealed class InstagramOptions
{
    public string AppId { get; set; } = string.Empty;
    public string AppSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
    public string GraphApiBaseUrl { get; set; } = "https://graph.facebook.com/v20.0";
    public string Scopes { get; set; } = "instagram_basic,instagram_content_publish,instagram_manage_insights,pages_show_list";
    public int MaxCaptionLength { get; set; } = 2200;
    public int MaxHashtags { get; set; } = 30;
}
