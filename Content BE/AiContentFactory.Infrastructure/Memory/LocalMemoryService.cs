using AiContentFactory.Application.Memory;
using AiContentFactory.Application.Studio;
using AiContentFactory.Domain.GlobalMemory;
using AiContentFactory.Domain.Memory;
using AiContentFactory.Domain.Memory.AgentMemories;
using AiContentFactory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AiContentFactory.Infrastructure.Memory;

public sealed class LocalMemoryService : ILocalMemoryService
{
    private readonly StudioDbContext _dbContext;
    private readonly IGoogleDriveService _driveService;
    private readonly ILogger<LocalMemoryService> _logger;

    public LocalMemoryService(
        StudioDbContext dbContext,
        IGoogleDriveService driveService,
        ILogger<LocalMemoryService> logger)
    {
        _dbContext = dbContext;
        _driveService = driveService;
        _logger = logger;
    }

    public async Task<AgentLocalMemory?> GetAsync(string agentKey, CancellationToken ct = default)
    {
        return await _dbContext.AgentLocalMemories.FirstOrDefaultAsync(m => m.AgentKey == agentKey, ct);
    }

    public async Task<T?> GetConfigAsync<T>(string agentKey, CancellationToken ct = default) where T : class
    {
        var memory = await GetAsync(agentKey, ct);
        if (memory == null || string.IsNullOrWhiteSpace(memory.ConfigJson)) return null;

        try
        {
            return JsonSerializer.Deserialize<T>(memory.ConfigJson);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to deserialize ConfigJson for agent {agentKey}");
            return null;
        }
    }

    public async Task SaveConfigAsync<T>(string agentKey, T config, CancellationToken ct = default) where T : class
    {
        var memory = await GetAsync(agentKey, ct);
        var isNew = false;
        if (memory == null)
        {
            memory = new AgentLocalMemory
            {
                AgentKey = agentKey,
                AgentDisplayName = GenerateDisplayName(agentKey),
                CreatedAt = DateTimeOffset.UtcNow
            };
            isNew = true;
        }

        memory.ConfigJson = JsonSerializer.Serialize(config);
        memory.UpdatedAt = DateTimeOffset.UtcNow;

        if (isNew)
        {
            _dbContext.AgentLocalMemories.Add(memory);
        }

        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task RecordRunAsync(string agentKey, bool success, string? errorMessage = null, CancellationToken ct = default)
    {
        var memory = await GetAsync(agentKey, ct);
        if (memory == null) return;

        memory.RunCount++;
        memory.LastRunAt = DateTimeOffset.UtcNow;

        if (success)
        {
            memory.SuccessCount++;
            memory.LastSuccessAt = DateTimeOffset.UtcNow;
        }
        else
        {
            memory.FailureCount++;
            memory.LastErrorAt = DateTimeOffset.UtcNow;
            memory.LastErrorMessage = errorMessage;
        }

        memory.UpdatedAt = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<List<AgentLocalMemory>> GetAllAsync(CancellationToken ct = default)
    {
        return await _dbContext.AgentLocalMemories.ToListAsync(ct);
    }

    public async Task ResetAsync(string agentKey, CancellationToken ct = default)
    {
        var defaultConfig = CreateDefaultConfig(agentKey);
        await SaveConfigAsync(agentKey, defaultConfig, ct);
    }

    public async Task SyncToDriveAsync(string agentKey, CancellationToken ct = default)
    {
        var memory = await GetAsync(agentKey, ct);
        if (memory == null) return;

        var json = JsonSerializer.Serialize(memory, new JsonSerializerOptions { WriteIndented = true });
        // TODO: var path = $"/memory/local/{agentKey}.json";
        // await _driveService.UploadFileContentAsync(path, json, ct);
        
        await Task.CompletedTask;
    }

    public async Task LoadFromDriveAsync(string agentKey, CancellationToken ct = default)
    {
        // TODO: var path = $"/memory/local/{agentKey}.json";
        // var json = await _driveService.DownloadFileContentAsync(path, ct);
        // var memory = JsonSerializer.Deserialize<AgentLocalMemory>(json);
        
        await Task.CompletedTask;
    }

    public async Task MergeGlobalSettingsAsync(string agentKey, GlobalMemory globalMemory, CancellationToken ct = default)
    {
        switch (agentKey)
        {
            case "shorts-agent":
                var shortsConfig = await GetConfigAsync<ShortsAgentLocalMemory>(agentKey, ct) ?? new ShortsAgentLocalMemory();
                shortsConfig.MaxSeconds = globalMemory.VideoConstraints.ShortMaxDurationSeconds;
                shortsConfig.AspectRatio = globalMemory.VideoConstraints.ShortAspectRatio;
                await SaveConfigAsync(agentKey, shortsConfig, ct);
                break;
                
            case "trend-agent":
                var trendConfig = await GetConfigAsync<TrendAgentLocalMemory>(agentKey, ct) ?? new TrendAgentLocalMemory();
                trendConfig.Top50Sites = globalMemory.TrendAgentConfig.Tier1Sites;
                trendConfig.ScheduleSlots = globalMemory.PeakUploadSlotsUtc;
                await SaveConfigAsync(agentKey, trendConfig, ct);
                break;
                
            case "upload-agent":
                var uploadConfig = await GetConfigAsync<UploadAgentLocalMemory>(agentKey, ct) ?? new UploadAgentLocalMemory();
                // Merge Logic for slots, etc.
                await SaveConfigAsync(agentKey, uploadConfig, ct);
                break;
        }
    }

    public async Task DeleteAsync(string agentKey, CancellationToken ct = default)
    {
        var memory = await GetAsync(agentKey, ct);
        if (memory != null)
        {
            _dbContext.AgentLocalMemories.Remove(memory);
            await _dbContext.SaveChangesAsync(ct);
        }
    }

    public async Task InitializeAllAgentMemoriesAsync(CancellationToken ct = default)
    {
        string[] keys = new[] 
        { 
            "script-gen-agent", "edit-agent", "shorts-agent", 
            "short-edit-agent", "trend-agent", "upload-agent", 
            "analytics-agent", "youtube-agent", "tiktok-agent", 
            "instagram-agent", "facebook-agent"
        };

        foreach (var key in keys)
        {
            var exists = await _dbContext.AgentLocalMemories.AnyAsync(m => m.AgentKey == key, ct);
            if (!exists)
            {
                var defaultConfig = CreateDefaultConfig(key);
                await SaveConfigAsync(key, defaultConfig, ct);
            }
        }
    }

    private object CreateDefaultConfig(string agentKey)
    {
        return agentKey switch
        {
            "script-gen-agent" => new ScriptGenLocalMemory(),
            "edit-agent" => new EditAgentLocalMemory(),
            "shorts-agent" => new ShortsAgentLocalMemory(),
            "short-edit-agent" => new ShortEditLocalMemory(),
            "trend-agent" => new TrendAgentLocalMemory(),
            "upload-agent" => new UploadAgentLocalMemory(),
            "analytics-agent" => new AnalyticsAgentLocalMemory(),
            _ => new object()
        };
    }

    private string GenerateDisplayName(string agentKey)
    {
        return string.Join(" ", agentKey.Split('-').Select(w => char.ToUpper(w[0]) + w.Substring(1)));
    }
}
