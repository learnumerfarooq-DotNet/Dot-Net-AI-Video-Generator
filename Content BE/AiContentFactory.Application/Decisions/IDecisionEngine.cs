using AiContentFactory.Domain.Decisions;

namespace AiContentFactory.Application.Decisions;

public interface IDecisionEngine
{
    Task<AgentDecision> MakeDecisionAsync(string agentKey, DecisionType type, Dictionary<string, string> context, Guid? jobId = null, CancellationToken ct = default);
    Task<T?> ParsePayloadAsync<T>(AgentDecision decision) where T : class;
}

public interface IDecisionValidator
{
    Task<ValidationResult> ValidateAsync(AgentDecision decision, CancellationToken ct = default);
}

public record ValidationResult(bool IsValid, string? ErrorMessage);

public interface IAgentDecisionFacade
{
    Task<ScriptDecisionPayload> GetScriptDecisionAsync(Guid jobId, Dictionary<string, string> context, CancellationToken ct = default);
    Task<EditDecisionPayload> GetEditDecisionAsync(Guid jobId, Dictionary<string, string> context, CancellationToken ct = default);
    Task<ShortDecisionPayload> GetShortDecisionAsync(Guid jobId, Dictionary<string, string> context, CancellationToken ct = default);
    Task<TrendDecisionPayload> GetTrendDecisionAsync(Dictionary<string, string> context, CancellationToken ct = default);
    Task<UploadDecisionPayload> GetUploadDecisionAsync(Guid jobId, Dictionary<string, string> context, CancellationToken ct = default);
}

public interface IDecisionCache
{
    Task<string?> GetAsync(string key, CancellationToken ct = default);
    Task SetAsync(string key, string jsonPayload, TimeSpan ttl, CancellationToken ct = default);
}
