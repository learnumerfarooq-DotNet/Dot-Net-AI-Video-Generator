using AiContentFactory.Domain.Agents;
using AiContentFactory.Domain.Pipeline;

namespace AiContentFactory.Application.Agents;

public interface IScriptGenAgent
{
    // Generate script from video metadata
    Task<ScriptOutput> GenerateScriptAsync(Guid jobId, VideoMetadata metadata, CancellationToken ct = default);

    // Regenerate with different style
    Task<ScriptOutput> RegenerateScriptAsync(Guid jobId, string style, string tone, CancellationToken ct = default);

    // Get the latest script for a job
    Task<ScriptOutput?> GetScriptAsync(Guid jobId, CancellationToken ct = default);

    // Validate script meets quality thresholds
    Task<bool> ValidateScriptAsync(ScriptOutput script, CancellationToken ct = default);
}
