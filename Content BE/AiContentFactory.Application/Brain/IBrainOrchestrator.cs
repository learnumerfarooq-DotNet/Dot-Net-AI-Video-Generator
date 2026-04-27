using AiContentFactory.Domain.Brain;
using AiContentFactory.Domain.GlobalMemory;

namespace AiContentFactory.Application.Brain;

public interface IBrainOrchestrator
{
    // Core tick method — called every N seconds by Hangfire
    Task ExecuteTickAsync(CancellationToken ct = default);

    // Read global memory from Drive and update in-memory state
    Task<GlobalMemory> SyncGlobalMemoryAsync(CancellationToken ct = default);

    // Dispatch a specific agent to process a job
    Task DispatchAgentAsync(string agentKey, Guid jobId, CancellationToken ct = default);

    // Check health of all registered agents
    Task<Dictionary<string, AgentHealthStatus>> CheckAgentHealthAsync(CancellationToken ct = default);

    // Get current brain state
    Task<BrainState> GetStateAsync(CancellationToken ct = default);

    // Pause/Resume the brain
    Task PauseAsync(CancellationToken ct = default);
    Task ResumeAsync(CancellationToken ct = default);

    // Force re-read of global memory
    Task ForceGlobalMemoryRefreshAsync(CancellationToken ct = default);
}
