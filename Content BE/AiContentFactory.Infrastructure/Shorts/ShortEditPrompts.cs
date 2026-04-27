namespace AiContentFactory.Infrastructure.Shorts;

public static class ShortEditPrompts
{
    public const string SystemPrompt = @"You are a viral short-form video editor AI.
Given a short clip's metadata (title, hook, rationale), create a high-impact edit plan.
Focus on:
1. A strong attention hook overlay in the first 3 seconds.
2. Dynamic captions (word-by-word style) that are perfectly timed.
3. Relevant background music choice.
4. Strategic emoji overlays to emphasize key points.
Output valid JSON matching the schema exactly.";

    public const string UserPromptTemplate = @"Create an edit plan for this short clip:
Title: {title}
Hook: {hook}
Rationale: {rationale}
Duration: {duration} seconds

Output JSON format:
{jsonSchema}";

    public const string JsonOutputSchema = @"{
    ""hookOverlay"": {
        ""text"": ""Hook text here"",
        ""fontSize"": 72,
        ""fontColor"": ""#FFFFFF"",
        ""backgroundColor"": ""#FF0000"",
        ""animationType"": ""pop"",
        ""durationSeconds"": 3.0
    },
    ""captions"": [
        { ""startTime"": 0.0, ""endTime"": 2.5, ""text"": ""Caption text"", ""style"": ""word-by-word"", ""fontSize"": 48, ""color"": ""#FFFFFF"", ""position"": ""center"" }
    ],
    ""musicTrack"": {
        ""trackName"": ""high_energy_phonk"",
        ""volume"": 0.4,
        ""fadeInSeconds"": 1.0,
        ""fadeOutSeconds"": 2.0,
        ""genre"": ""trending""
    },
    ""emojiOverlays"": [
        { ""emoji"": ""🔥"", ""startTime"": 1.5, ""endTime"": 3.5, ""position"": ""top-right"", ""animationType"": ""bounce"" }
    ],
    ""transitionIn"": ""glitch"",
    ""transitionOut"": ""fade""
}";
}
