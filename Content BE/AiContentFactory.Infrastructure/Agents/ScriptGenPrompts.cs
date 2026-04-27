namespace AiContentFactory.Infrastructure.Agents;

public static class ScriptGenPrompts
{
    public const string SystemPrompt = @"You are an expert video script writer for social media content.
You create engaging, viral-worthy scripts optimized for maximum viewer retention.
You must output valid JSON matching the exact schema provided.";

    public const string UserPromptTemplate = @"Generate a video script for the following video:

File: {fileName}
Duration: {duration} seconds
Resolution: {resolution}
Video Type: {videoType}
Style: {style}
Tone: {tone}
Language: {language}

Requirements:
1. Create an attention-grabbing hook (first 3 seconds)
2. Structure the body with clear segments
3. Include a call-to-action
4. Suggest 5-10 SEO keywords
5. Suggest 5-15 hashtags
6. Recommend platforms

Respond in the following JSON format:
{jsonSchema}";

    public const string JsonOutputSchema = @"{
    ""title"": ""string — catchy video title"",
    ""hook"": ""string — opening hook for first 3 seconds"",
    ""introduction"": ""string — scene setting"",
    ""body"": ""string — main content with paragraphs"",
    ""callToAction"": ""string — closing CTA"",
    ""keywords"": [""string array — SEO keywords""],
    ""hashtags"": [""string array — hashtags with #""],
    ""suggestedPlatforms"": [""string array — YouTube, TikTok, etc.""],
    ""estimatedDuration"": 0
}";
}
