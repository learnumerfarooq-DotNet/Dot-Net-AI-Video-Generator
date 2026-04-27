using AiContentFactory.Application.Memory;
using AiContentFactory.Application.Studio;
using AiContentFactory.Domain.GlobalMemory;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AiContentFactory.Infrastructure.Memory;

public sealed class GlobalMemoryService : IGlobalMemoryService
{
    private readonly IGoogleDriveService _driveService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<GlobalMemoryService> _logger;

    private const string CacheKey = "global-memory:current";
    private const string DriveFilePath = "/memory/global.json";

    public GlobalMemoryService(IGoogleDriveService driveService, IMemoryCache cache, ILogger<GlobalMemoryService> logger)
    {
        _driveService = driveService;
        _cache = cache;
        _logger = logger;
    }

    public async Task<GlobalMemory> LoadAsync(CancellationToken ct = default)
    {
        if (_cache.TryGetValue(CacheKey, out GlobalMemory? cachedMemory) && cachedMemory != null)
        {
            return cachedMemory;
        }

        try
        {
            // TODO: Use actual Google Drive logic. For now, try to load from a local fallback or return default.
            // var json = await _driveService.DownloadFileContentAsync(DriveFilePath, ct);
            var json = string.Empty; 

            if (string.IsNullOrWhiteSpace(json))
            {
                return await CreateDefaultAsync(ct);
            }

            var memory = JsonSerializer.Deserialize<GlobalMemory>(json) ?? await CreateDefaultAsync(ct);
            
            ValidateMemory(memory);
            
            _cache.Set(CacheKey, memory, TimeSpan.FromSeconds(30));
            return memory;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load global memory from Drive. Returning default.");
            return await CreateDefaultAsync(ct);
        }
    }

    public async Task SaveAsync(GlobalMemory memory, CancellationToken ct = default)
    {
        memory.LastUpdated = DateTimeOffset.UtcNow;
        // Simple version increment
        if (double.TryParse(memory.Version, out var v))
            memory.Version = (v + 0.1).ToString("0.0");

        var json = JsonSerializer.Serialize(memory, new JsonSerializerOptions { WriteIndented = true });

        // TODO: _driveService.UploadFileContentAsync(DriveFilePath, json, ct);
        
        _cache.Set(CacheKey, memory, TimeSpan.FromSeconds(30));
        _logger.LogInformation($"Saved global memory version {memory.Version}");
        
        await Task.CompletedTask;
    }

    public async Task<FolderRegistry> GetFolderRegistryAsync(CancellationToken ct = default)
    {
        var memory = await LoadAsync(ct);
        return memory.FolderRegistry;
    }

    public async Task<TrendAgentConfig> GetTrendConfigAsync(CancellationToken ct = default)
    {
        var memory = await LoadAsync(ct);
        return memory.TrendAgentConfig;
    }

    public async Task<VideoConstraints> GetVideoConstraintsAsync(CancellationToken ct = default)
    {
        var memory = await LoadAsync(ct);
        return memory.VideoConstraints;
    }

    public async Task UpdateAgentStatusAsync(string agentKey, AgentStatusEntry status, CancellationToken ct = default)
    {
        var memory = await LoadAsync(ct);
        memory.AgentStatuses[agentKey] = status;
        await SaveAsync(memory, ct);
    }

    public async Task UpdateScheduleSlotsAsync(List<ScheduleSlot> slots, CancellationToken ct = default)
    {
        var memory = await LoadAsync(ct);
        memory.ScheduleSlots = slots;
        await SaveAsync(memory, ct);
    }

    public async Task UpdateAnalyticsSummaryAsync(AnalyticsSummary summary, CancellationToken ct = default)
    {
        var memory = await LoadAsync(ct);
        memory.AnalyticsSummary = summary;
        await SaveAsync(memory, ct);
    }

    public async Task UpdateErrorSummaryAsync(ErrorSummary summary, CancellationToken ct = default)
    {
        var memory = await LoadAsync(ct);
        memory.ErrorSummary = summary;
        await SaveAsync(memory, ct);
    }

    public async Task<GlobalMemory> ForceRefreshAsync(CancellationToken ct = default)
    {
        _cache.Remove(CacheKey);
        return await LoadAsync(ct);
    }

    private async Task<GlobalMemory> CreateDefaultAsync(CancellationToken ct)
    {
        var memory = new GlobalMemory();
        await SaveAsync(memory, ct);
        return memory;
    }

    private void ValidateMemory(GlobalMemory memory)
    {
        if (memory.FolderRegistry == null) memory.FolderRegistry = new FolderRegistry();
        if (memory.TrendAgentConfig == null) memory.TrendAgentConfig = new TrendAgentConfig();
        if (memory.VideoConstraints == null) memory.VideoConstraints = new VideoConstraints();
        if (memory.AgentStatuses == null) memory.AgentStatuses = new Dictionary<string, AgentStatusEntry>();
        if (memory.ScheduleSlots == null) memory.ScheduleSlots = new List<ScheduleSlot>();
    }
}
