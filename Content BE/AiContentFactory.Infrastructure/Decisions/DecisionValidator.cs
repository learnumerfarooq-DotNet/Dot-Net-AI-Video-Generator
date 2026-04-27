using System.Text.Json;
using AiContentFactory.Application.Decisions;
using AiContentFactory.Domain.Decisions;

namespace AiContentFactory.Infrastructure.Decisions;

public sealed class DecisionValidator : IDecisionValidator
{
    public Task<ValidationResult> ValidateAsync(AgentDecision decision, CancellationToken ct = default)
    {
        try
        {
            return decision.Type switch
            {
                DecisionType.ScriptGeneration => ValidateScript(decision),
                DecisionType.VideoEditing => ValidateEdit(decision),
                DecisionType.ShortGeneration => ValidateShort(decision),
                DecisionType.TrendDiscovery => ValidateTrend(decision),
                DecisionType.UploadMetadata => ValidateUpload(decision),
                _ => Task.FromResult(new ValidationResult(true, null))
            };
        }
        catch (Exception ex)
        {
            return Task.FromResult(new ValidationResult(false, $"Validation error: {ex.Message}"));
        }
    }

    private Task<ValidationResult> ValidateScript(AgentDecision decision)
    {
        var payload = JsonSerializer.Deserialize<ScriptDecisionPayload>(decision.RawJsonPayload, _options);
        if (payload == null) return Task.FromResult(new ValidationResult(false, "Invalid script payload"));
        
        if (string.IsNullOrWhiteSpace(payload.Title)) return Task.FromResult(new ValidationResult(false, "Title is missing"));
        if (payload.Body.Length < 50) return Task.FromResult(new ValidationResult(false, "Script body is too short"));
        
        return Task.FromResult(new ValidationResult(true, null));
    }

    private Task<ValidationResult> ValidateEdit(AgentDecision decision)
    {
        var payload = JsonSerializer.Deserialize<EditDecisionPayload>(decision.RawJsonPayload, _options);
        if (payload == null) return Task.FromResult(new ValidationResult(false, "Invalid edit payload"));
        
        if (payload.Segments.Count == 0) return Task.FromResult(new ValidationResult(false, "No segments defined"));
        
        return Task.FromResult(new ValidationResult(true, null));
    }

    private Task<ValidationResult> ValidateShort(AgentDecision decision)
    {
        var payload = JsonSerializer.Deserialize<ShortDecisionPayload>(decision.RawJsonPayload, _options);
        if (payload == null) return Task.FromResult(new ValidationResult(false, "Invalid short payload"));
        
        foreach (var s in payload.Shorts)
        {
            if (s.EndTime - s.StartTime > 60) return Task.FromResult(new ValidationResult(false, "Short segment exceeds 60 seconds"));
        }
        
        return Task.FromResult(new ValidationResult(true, null));
    }

    private Task<ValidationResult> ValidateTrend(AgentDecision decision)
    {
        var payload = JsonSerializer.Deserialize<TrendDecisionPayload>(decision.RawJsonPayload, _options);
        if (payload == null) return Task.FromResult(new ValidationResult(false, "Invalid trend payload"));
        
        if (payload.Topics.Count == 0) return Task.FromResult(new ValidationResult(false, "No trending topics found"));
        
        return Task.FromResult(new ValidationResult(true, null));
    }

    private Task<ValidationResult> ValidateUpload(AgentDecision decision)
    {
        var payload = JsonSerializer.Deserialize<UploadDecisionPayload>(decision.RawJsonPayload, _options);
        if (payload == null) return Task.FromResult(new ValidationResult(false, "Invalid upload payload"));
        
        if (payload.Title.Length > 100) return Task.FromResult(new ValidationResult(false, "Title exceeds platform limit (100 chars)"));
        
        return Task.FromResult(new ValidationResult(true, null));
    }

    private static readonly JsonSerializerOptions _options = new() { PropertyNameCaseInsensitive = true };
}
