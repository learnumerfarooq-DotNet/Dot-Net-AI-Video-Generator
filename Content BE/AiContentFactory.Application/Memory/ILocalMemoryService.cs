using AiContentFactory.Domain.GlobalMemory;
using AiContentFactory.Domain.Memory;

namespace AiContentFactory.Application.Memory;

public interface ILocalMemoryService
{
    // Get local memory for a specific agent
    Task<AgentLocalMemory?> GetAsync(string agentKey, CancellationToken ct = default);

    // Get typed config from local memory
    Task<T?> GetConfigAsync<T>(string agentKey, CancellationToken ct = default) where T : class;

    // Save typed config to local memory
    Task SaveConfigAsync<T>(string agentKey, T config, CancellationToken ct = default) where T : class;

    // Update run statistics
    Task RecordRunAsync(string agentKey, bool success, string? errorMessage = null, CancellationToken ct = default);

    // Get all agent local memories
    Task<List<AgentLocalMemory>> GetAllAsync(CancellationToken ct = default);

    // Reset an agent's local memory to defaults
    Task ResetAsync(string agentKey, CancellationToken ct = default);

    // Sync local memory to Drive as backup
    Task SyncToDriveAsync(string agentKey, CancellationToken ct = default);

    // Load local memory from Drive backup
    Task LoadFromDriveAsync(string agentKey, CancellationToken ct = default);

    // Merge global memory settings into local memory
    Task MergeGlobalSettingsAsync(string agentKey, GlobalMemory globalMemory, CancellationToken ct = default);

    // Delete an agent's local memory
    Task DeleteAsync(string agentKey, CancellationToken ct = default);

    // Initialize defaults on startup
    Task InitializeAllAgentMemoriesAsync(CancellationToken ct = default);
}
