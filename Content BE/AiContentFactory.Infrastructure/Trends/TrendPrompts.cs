namespace AiContentFactory.Infrastructure.Trends;

public static class TrendPrompts
{
    public const string SystemPrompt = @"You are a trend analysis AI for video content creators.
Analyze scraped web data and identify the top trending topics
that would make engaging video content. Rank by relevance and
virality potential. Create an upload schedule for peak hours.";

    public const string JsonSchema = @"{
    ""topics"": [
        { ""keyword"": ""string"", ""source"": ""string"", ""relevanceScore"": 0.9, ""category"": ""string"",
          ""suggestedPlatforms"": [""YouTube""], ""contentType"": ""short"" }
    ],
    ""plannedUploads"": [
        { ""topic"": ""string"", ""scheduledTime"": ""ISO8601"", ""platforms"": [""YouTube"", ""TikTok""] }
    ],
    ""analysisSummary"": ""string"",
    ""validUntil"": ""ISO8601""
}";
}
