using AiContentFactory.Application.Decisions;
using AiContentFactory.Domain.Decisions;

namespace AiContentFactory.Infrastructure.Decisions;

public sealed class AgentDecisionFacade : IAgentDecisionFacade
{
    private readonly IDecisionEngine _engine;

    public AgentDecisionFacade(IDecisionEngine engine)
    {
        _engine = engine;
    }

    public async Task<ScriptDecisionPayload> GetScriptDecisionAsync(Guid jobId, Dictionary<string, string> context, CancellationToken ct = default)
    {
        var decision = await _engine.MakeDecisionAsync("script-agent", DecisionType.ScriptGeneration, context, jobId, ct);
        return await _engine.ParsePayloadAsync<ScriptDecisionPayload>(decision) ?? throw new InvalidOperationException("Failed to generate script decision");
    }

    public async Task<EditDecisionPayload> GetEditDecisionAsync(Guid jobId, Dictionary<string, string> context, CancellationToken ct = default)
    {
        var decision = await _engine.MakeDecisionAsync("edit-agent", DecisionType.VideoEditing, context, jobId, ct);
        return await _engine.ParsePayloadAsync<EditDecisionPayload>(decision) ?? throw new InvalidOperationException("Failed to generate edit decision");
    }

    public async Task<ShortDecisionPayload> GetShortDecisionAsync(Guid jobId, Dictionary<string, string> context, CancellationToken ct = default)
    {
        var decision = await _engine.MakeDecisionAsync("shorts-agent", DecisionType.ShortGeneration, context, jobId, ct);
        return await _engine.ParsePayloadAsync<ShortDecisionPayload>(decision) ?? throw new InvalidOperationException("Failed to generate short decision");
    }

    public async Task<TrendDecisionPayload> GetTrendDecisionAsync(Dictionary<string, string> context, CancellationToken ct = default)
    {
        var decision = await _engine.MakeDecisionAsync("trend-agent", DecisionType.TrendDiscovery, context, null, ct);
        return await _engine.ParsePayloadAsync<TrendDecisionPayload>(decision) ?? throw new InvalidOperationException("Failed to generate trend decision");
    }

    public async Task<UploadDecisionPayload> GetUploadDecisionAsync(Guid jobId, Dictionary<string, string> context, CancellationToken ct = default)
    {
        var decision = await _engine.MakeDecisionAsync("upload-agent", DecisionType.UploadMetadata, context, jobId, ct);
        return await _engine.ParsePayloadAsync<UploadDecisionPayload>(decision) ?? throw new InvalidOperationException("Failed to generate upload decision");
    }
}
