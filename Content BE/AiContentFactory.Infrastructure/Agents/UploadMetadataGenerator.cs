using AiContentFactory.Application.Decisions;
using AiContentFactory.Domain.Agents;
using AiContentFactory.Domain.Decisions;
using System.Text.Json;

namespace AiContentFactory.Infrastructure.Agents;

public class UploadMetadataGenerator
{
    private readonly IDecisionEngine _decisionEngine;

    public UploadMetadataGenerator(IDecisionEngine decisionEngine)
    {
        _decisionEngine = decisionEngine;
    }

    public async Task<UploadPackage> GenerateMetadataAsync(ScriptOutput script, string platform, Guid jobId, CancellationToken ct)
    {
        var context = new Dictionary<string, string>
        {
            { "script", script.Body },
            { "platform", platform },
            { "jsonSchema", UploadPrompts.JsonSchema }
        };

        var decision = await _decisionEngine.MakeDecisionAsync("upload-agent", DecisionType.UploadMetadata, context, jobId, ct);
        
        var result = JsonSerializer.Deserialize<MetadataResult>(decision.ValidatedPayload, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) 
                      ?? new MetadataResult();

        return new UploadPackage
        {
            Title = result.Title,
            Description = result.Description,
            Keywords = result.Keywords,
            Hashtags = result.Hashtags,
            Category = result.Category,
            Privacy = result.IsPublic ? "public" : "private"
        };
    }

    private class MetadataResult
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Keywords { get; set; } = new();
        public List<string> Hashtags { get; set; } = new();
        public string Category { get; set; } = string.Empty;
        public bool IsPublic { get; set; } = true;
    }
}
