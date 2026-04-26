using System.Text.Json;
using AiContentFactory.Application.Studio;

namespace AiContentFactory.Infrastructure.Tools;

public sealed class ScriptDraftTool : IStudioTool
{
    public string Name => "script_draft";
    public string Description => "Generate a rough draft script for a video based on a specific topic and tone.";

    public string ParametersSchema => @"
    {
        ""type"": ""object"",
        ""properties"": {
            ""topic"": { ""type"": ""string"", ""description"": ""The main subject of the video."" },
            ""durationSeconds"": { ""type"": ""integer"", ""description"": ""Target duration in seconds."" },
            ""tone"": { ""type"": ""string"", ""enum"": [""educational"", ""hype"", ""technical""], ""description"": ""The vibe of the script."" }
        },
        ""required"": [""topic""]
    }";

    public async Task<string> ExecuteAsync(string arguments, CancellationToken cancellationToken)
    {
        await Task.Delay(1500, cancellationToken);
        
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var args = JsonSerializer.Deserialize<JsonElement>(arguments, options);
        var topic = args.GetProperty("topic").GetString();

        return $@"DRAFT SCRIPT: {topic}
Hook: Stop using manual state in Angular.
Point 1: Signals are here to simplify your life.
Point 2: Performance benchmarks show 2x faster updates.
Outro: Check out the repo for the full code.";
    }
}
