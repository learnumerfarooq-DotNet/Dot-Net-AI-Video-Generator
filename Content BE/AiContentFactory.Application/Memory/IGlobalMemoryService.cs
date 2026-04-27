using AiContentFactory.Domain.GlobalMemory;

namespace AiContentFactory.Application.Memory;

public interface IGlobalMemoryService
{
    Task<GlobalMemory> LoadAsync(CancellationToken ct = default);
    Task SaveAsync(GlobalMemory memory, CancellationToken ct = default);
    
    Task<FolderRegistry> GetFolderRegistryAsync(CancellationToken ct = default);
    Task<TrendAgentConfig> GetTrendConfigAsync(CancellationToken ct = default);
    Task<VideoConstraints> GetVideoConstraintsAsync(CancellationToken ct = default);

    Task UpdateAgentStatusAsync(string agentKey, AgentStatusEntry status, CancellationToken ct = default);
    Task UpdateScheduleSlotsAsync(List<ScheduleSlot> slots, CancellationToken ct = default);
    Task UpdateAnalyticsSummaryAsync(AnalyticsSummary summary, CancellationToken ct = default);
    Task UpdateErrorSummaryAsync(ErrorSummary summary, CancellationToken ct = default);

    Task<GlobalMemory> ForceRefreshAsync(CancellationToken ct = default);
}
