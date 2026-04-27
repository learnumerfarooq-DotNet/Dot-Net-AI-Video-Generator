namespace AiContentFactory.Domain.Trends;

public sealed class TrendResult
{
    public Guid Id { get; set; }
    public DateTimeOffset DiscoveredAt { get; set; }
    public List<DiscoveredTopic> Topics { get; set; } = new();
    public List<PlannedUpload> PlannedUploads { get; set; } = new();
    public string AnalysisSummary { get; set; } = string.Empty;
    public DateTimeOffset ValidUntil { get; set; }
    public int TotalSitesScraped { get; set; }
    public int SuccessfulScrapes { get; set; }
    public int FailedScrapes { get; set; }
    public bool UsedOpenRouterFallback { get; set; }
    public List<string> TopKeywords { get; set; } = new();
    public List<string> TopHashtags { get; set; } = new();
    public double ConfidenceScore { get; set; }
    public string? DriveFileId { get; set; }
}

public sealed class DiscoveredTopic
{
    public string Keyword { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Rationale { get; set; } = string.Empty;
    public double RelevanceScore { get; set; }
    public string SearchVolume { get; set; } = string.Empty;
    public string Competition { get; set; } = "medium";
    public List<string> SuggestedPlatforms { get; set; } = new();
    public string ContentType { get; set; } = "short";
    public int Rank { get; set; }
    public DateTimeOffset DiscoveredAt { get; set; }
}

public sealed class ScrapeResult
{
    public Guid Id { get; set; }
    public string SiteUrl { get; set; } = string.Empty;
    public int Tier { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int TopicsFound { get; set; }
    public string RawContent { get; set; } = string.Empty;
    public DateTimeOffset ScrapedAt { get; set; }
    public long DurationMs { get; set; }
    public int ResponseCode { get; set; }
}
