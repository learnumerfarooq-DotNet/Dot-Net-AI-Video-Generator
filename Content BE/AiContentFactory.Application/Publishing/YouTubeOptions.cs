namespace AiContentFactory.Application.Publishing;

public sealed class YouTubeOptions
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
    public string ApplicationName { get; set; } = "AiContentFactory";
    public bool AutoShortsCategorization { get; set; } = true;
    public string DefaultPrivacyStatus { get; set; } = "private";
    public bool EnableResumableUpload { get; set; } = true;
    public int ChunkSizeMb { get; set; } = 1;
    public int MaxRetries { get; set; } = 3;
    public List<string> Scopes { get; set; } = new()
    {
        "https://www.googleapis.com/auth/youtube.upload",
        "https://www.googleapis.com/auth/youtube.force-ssl",
        "https://www.googleapis.com/auth/youtube.readonly",
        "https://www.googleapis.com/auth/yt-analytics.readonly"
    };
}
