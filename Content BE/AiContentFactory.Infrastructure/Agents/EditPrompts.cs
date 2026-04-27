namespace AiContentFactory.Infrastructure.Agents;

public static class EditPrompts
{
    public const string SystemPrompt = @"You are a professional video editor AI. Given video metadata and a script,
create an edit plan with precise timestamps for segments, captions, and audio adjustments.
Output valid JSON matching the schema exactly.";

    public const string JsonOutputSchema = @"{
    ""segments"": [
        { ""startTime"": 0.0, ""endTime"": 30.0, ""description"": ""Opening"", ""speed"": 1.0, ""transition"": ""fade"" }
    ],
    ""captions"": [
        { ""startTime"": 2.0, ""endTime"": 8.0, ""text"": ""Hook text here"", ""style"": ""bold"", ""position"": ""bottom-center"" }
    ],
    ""audioAdjustments"": [
        { ""startTime"": 0.0, ""endTime"": 5.0, ""volumeMultiplier"": 0.5, ""normalize"": true }
    ],
    ""colorGrading"": {
        ""brightness"": 1.05, ""contrast"": 1.1, ""saturation"": 1.15, ""gamma"": 1.0
    }
}";
}
