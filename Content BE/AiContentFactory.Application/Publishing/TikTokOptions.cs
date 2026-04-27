namespace AiContentFactory.Application.Publishing;

public sealed class TikTokOptions
{
    public string ClientKey { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://open.tiktokapis.com/v2";
    public string Scopes { get; set; } = "video.upload,video.publish,user.info.basic";
    public int MaxCaptionLength { get; set; } = 2200;
    public int MaxHashtags { get; set; } = 10;
    public bool AutoRetryOnRateLimit { get; set; } = true;
}
