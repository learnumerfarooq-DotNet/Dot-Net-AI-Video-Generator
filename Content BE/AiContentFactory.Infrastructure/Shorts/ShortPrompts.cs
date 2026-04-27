namespace AiContentFactory.Infrastructure.Shorts;

public static class ShortPrompts
{
    public const string SystemPrompt = @"You are an expert at identifying viral-worthy moments in videos.
Given video metadata and a script, identify the top 5 most engaging segments
that would work as standalone short-form videos (≤60 seconds each).
Focus on: hooks, surprising moments, emotional peaks, and key takeaways.";

    public const string UserPromptTemplate = @"Analyze this video and identify the best short clips:

Video Duration: {duration} seconds
Scene Changes at: {sceneChanges}
Audio Peaks at: {audioPeaks}
Script Summary: {scriptSummary}

Requirements:
- Each clip must be 15-60 seconds
- Each clip needs a strong hook in the first 3 seconds
- Clips should be self-contained (make sense without context)
- Maximum 5 clips
- Include engagement score prediction (0.0-1.0)

Output JSON format:
{jsonSchema}";

    public const string JsonOutputSchema = @"{
    ""parentVideoId"": ""string"",
    ""shorts"": [
        {
            ""startTime"": 0.0,
            ""endTime"": 45.0,
            ""title"": ""string"",
            ""hook"": ""string"",
            ""rationale"": ""string"",
            ""engagementScore"": 0.85
        }
    ]
}";
}
