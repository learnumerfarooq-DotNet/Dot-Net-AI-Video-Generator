namespace AiContentFactory.Infrastructure.Agents;

public static class UploadPrompts
{
    public const string SystemPrompt = @"You are an expert social media content optimizer.
Generate SEO-optimized titles, descriptions, and hashtags
for video content across multiple platforms.
Adapt content to each platform's best practices.";

    public const string JsonSchema = @"{
    ""title"": ""catchy, SEO-optimized title"",
    ""description"": ""platform-optimized description with CTAs"",
    ""keywords"": [""seo"", ""keywords""],
    ""hashtags"": [""#tag1"", ""#tag2""],
    ""category"": ""Entertainment"",
    ""isPublic"": true
}";
}
