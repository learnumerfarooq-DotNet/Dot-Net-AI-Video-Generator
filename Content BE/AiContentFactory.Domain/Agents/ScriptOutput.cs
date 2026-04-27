namespace AiContentFactory.Domain.Agents;

public sealed class ScriptOutput
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Hook { get; set; } = string.Empty;
    public string Introduction { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string CallToAction { get; set; } = string.Empty;
    public List<string> Keywords { get; set; } = new();
    public List<string> Hashtags { get; set; } = new();
    public List<string> SuggestedPlatforms { get; set; } = new();
    public int EstimatedDuration { get; set; }
    public string ToneUsed { get; set; } = string.Empty;
    public string StyleUsed { get; set; } = string.Empty;
    public double ConfidenceScore { get; set; }
    public string? DriveFileId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
