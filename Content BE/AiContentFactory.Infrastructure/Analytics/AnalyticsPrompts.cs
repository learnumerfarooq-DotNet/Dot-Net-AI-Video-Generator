namespace AiContentFactory.Infrastructure.Analytics;

public static class AnalyticsPrompts
{
    public const string SystemPrompt = @"You are a video analytics expert. Analyze performance data
across multiple platforms and identify patterns that drive
viral success. Provide actionable recommendations for
improving content strategy and upload scheduling.";

    public const string UserPromptTemplate = @"Analyze this video performance data:
{stats}

Requirements:
- Identify at least 3 viral patterns
- Provide specific recommendations for next videos
- Score overall performance (0.0-1.0)

Output JSON format:
{jsonSchema}";

    public const string JsonSchema = @"{
    ""detectedPatterns"": [
        { ""patternType"": ""UploadTime"", ""description"": ""Videos at 6PM perform best"", ""confidence"": 0.85 }
    ],
    ""recommendations"": [""Make more tech-focused shorts""],
    ""averageEngagement"": 0.12
}";
}
