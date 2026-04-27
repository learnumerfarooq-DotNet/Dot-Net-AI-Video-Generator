using AiContentFactory.Domain.Decisions;

namespace AiContentFactory.Infrastructure.Decisions;

public static class DefaultPrompts
{
    public static List<PromptTemplate> GetDefaults()
    {
        var now = DateTimeOffset.UtcNow;

        return new List<PromptTemplate>
        {
            new()
            {
                Id = Guid.NewGuid(),
                AgentKey = "script-gen-agent",
                DecisionType = DecisionType.ScriptGeneration,
                Version = "1.0",
                IsActive = true,
                CreatedAt = now,
                ActivatedAt = now,
                SystemPrompt = "Expert video script writer for social media. You create engaging, highly-retaining scripts optimized for shorts/reels. Focus on strong hooks, clear pacing, and compelling calls to action.",
                UserPromptTemplate = "Generate a video script based on these parameters:\n- File Name: {fileName}\n- Target Duration: {duration}\n- Style: {style}\n- Tone: {tone}\n\nEnsure the script is optimized for maximum viewer retention.",
                JsonOutputSchema = "{ \"title\": \"string\", \"hook\": \"string\", \"introduction\": \"string\", \"body\": \"string\", \"callToAction\": \"string\", \"keywords\": [\"string\"], \"hashtags\": [\"string\"], \"suggestedPlatforms\": [\"string\"], \"estimatedDuration\": \"number\" }"
            },
            new()
            {
                Id = Guid.NewGuid(),
                AgentKey = "edit-agent",
                DecisionType = DecisionType.VideoEditing,
                Version = "1.0",
                IsActive = true,
                CreatedAt = now,
                ActivatedAt = now,
                SystemPrompt = "Professional video editor AI. You analyze raw footage and script content to create precise editing decisions, including cuts, transitions, captions, and audio adjustments.",
                UserPromptTemplate = "Create an edit plan based on:\n- Video Analysis: {videoAnalysis}\n- Script Content: {scriptContent}\n- Style Preferences: {stylePreferences}",
                JsonOutputSchema = "{ \"segments\": [ { \"startTime\": \"number\", \"endTime\": \"number\", \"action\": \"string\" } ], \"captions\": [ { \"startTime\": \"number\", \"endTime\": \"number\", \"text\": \"string\" } ], \"audioAdjustments\": [ { \"startTime\": \"number\", \"volumeLevel\": \"number\" } ], \"colorGrading\": \"string\" }"
            },
            new()
            {
                Id = Guid.NewGuid(),
                AgentKey = "shorts-agent",
                DecisionType = DecisionType.ShortGeneration,
                Version = "1.0",
                IsActive = true,
                CreatedAt = now,
                ActivatedAt = now,
                SystemPrompt = "Expert at identifying viral moments in long-form videos. You find the most engaging, high-retention segments (15-60s) perfect for TikTok, YouTube Shorts, and Instagram Reels.",
                UserPromptTemplate = "Find viral shorts from this video data:\n- Duration: {duration}\n- Scene Changes: {sceneChanges}\n- Audio Hotspots: {audioHotspots}\n- Script Summary: {scriptSummary}",
                JsonOutputSchema = "{ \"shorts\": [ { \"startTime\": \"number\", \"endTime\": \"number\", \"title\": \"string\", \"hook\": \"string\", \"rationale\": \"string\" } ] }"
            },
            new()
            {
                Id = Guid.NewGuid(),
                AgentKey = "short-edit-agent",
                DecisionType = DecisionType.ShortEditing,
                Version = "1.0",
                IsActive = true,
                CreatedAt = now,
                ActivatedAt = now,
                SystemPrompt = "Short-form video editor specializing in hooks, dynamic captions, and viral pacing. You create edit instructions optimized for vertical video platforms.",
                UserPromptTemplate = "Create an edit plan for a short clip:\n- Clip Duration: {clipDuration}\n- Hook Style: {hookStyle}\n- Caption Preference: {captionPreference}",
                JsonOutputSchema = "{ \"hookOverlay\": \"string\", \"captions\": [ { \"text\": \"string\", \"animation\": \"string\" } ], \"musicTrack\": \"string\", \"emojiOverlays\": [ { \"emoji\": \"string\", \"time\": \"number\" } ] }"
            },
            new()
            {
                Id = Guid.NewGuid(),
                AgentKey = "trend-agent",
                DecisionType = DecisionType.TrendDiscovery,
                Version = "1.0",
                IsActive = true,
                CreatedAt = now,
                ActivatedAt = now,
                SystemPrompt = "Trend analysis AI for video content creators. You analyze scraped social data and historical performance to predict upcoming viral trends and topics.",
                UserPromptTemplate = "Analyze these inputs for content trends:\n- Scraped Data: {scrapedData}\n- Previous Trends: {previousTrends}\n- Current Performance: {currentPerformance}",
                JsonOutputSchema = "{ \"topics\": [ \"string\" ], \"plannedUploads\": [ { \"topic\": \"string\", \"bestDay\": \"string\", \"rationale\": \"string\" } ], \"analysisSummary\": \"string\" }"
            },
            new()
            {
                Id = Guid.NewGuid(),
                AgentKey = "upload-agent",
                DecisionType = DecisionType.UploadMetadata,
                Version = "1.0",
                IsActive = true,
                CreatedAt = now,
                ActivatedAt = now,
                SystemPrompt = "Social media content optimizer. You generate SEO-optimized titles, descriptions, and tags tailored for specific platforms to maximize search visibility and click-through rates.",
                UserPromptTemplate = "Generate upload metadata based on:\n- Script Content: {scriptContent}\n- Trend Keywords: {trendKeywords}\n- Target Platform: {targetPlatform}",
                JsonOutputSchema = "{ \"title\": \"string\", \"description\": \"string\", \"keywords\": [ \"string\" ], \"hashtags\": [ \"string\" ], \"category\": \"string\" }"
            },
            new()
            {
                Id = Guid.NewGuid(),
                AgentKey = "analytics-agent",
                DecisionType = DecisionType.AnalyticsInsight,
                Version = "1.0",
                IsActive = true,
                CreatedAt = now,
                ActivatedAt = now,
                SystemPrompt = "Video analytics expert. You analyze video performance data to identify growth patterns, engagement drops, and actionable recommendations for future content.",
                UserPromptTemplate = "Analyze video performance:\n- Video Stats: {videoStats}\n- Platform Data: {platformData}\n- Historical Performance: {historicalPerformance}",
                JsonOutputSchema = "{ \"patterns\": [ \"string\" ], \"recommendations\": [ \"string\" ], \"contentScore\": \"number\" }"
            },
            new()
            {
                Id = Guid.NewGuid(),
                AgentKey = "main-brain",
                DecisionType = DecisionType.ContentVariation,
                Version = "1.0",
                IsActive = true,
                CreatedAt = now,
                ActivatedAt = now,
                SystemPrompt = "Content strategy AI. You are the Main Brain that dynamically adjusts the global content strategy based on continuous feedback loops, ensuring long-term channel growth.",
                UserPromptTemplate = "Adjust content strategy based on:\n- Current Strategy: {currentStrategy}\n- Performance Feedback: {performance}\n- New Trends: {trends}",
                JsonOutputSchema = "{ \"focusTopics\": [ \"string\" ], \"avoidTopics\": [ \"string\" ], \"contentMix\": \"string\", \"toneAdjustment\": \"string\" }"
            }
        };
    }
}
