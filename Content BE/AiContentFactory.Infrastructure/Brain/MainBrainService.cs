using AiContentFactory.Application.Brain;
using AiContentFactory.Application.Memory;
using AiContentFactory.Domain.Brain;
using AiContentFactory.Domain.GlobalMemory;
using AiContentFactory.Domain.Pipeline;
using AiContentFactory.Infrastructure.Persistence;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace AiContentFactory.Infrastructure.Brain;

public sealed class MainBrainService : IBrainOrchestrator
{
    private readonly StudioDbContext _dbContext;
    private readonly IGlobalMemoryService _globalMemoryService;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly ILogger<MainBrainService> _logger;
    private readonly BrainOptions _options;

    public MainBrainService(
        StudioDbContext dbContext,
        IGlobalMemoryService globalMemoryService,
        IBackgroundJobClient backgroundJobClient,
        IOptions<BrainOptions> options,
        ILogger<MainBrainService> logger)
    {
        _dbContext = dbContext;
        _globalMemoryService = globalMemoryService;
        _backgroundJobClient = backgroundJobClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task ExecuteTickAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Main Brain Tick Started");

        var state = await GetStateAsync(ct);
        state.CurrentTickNumber++;
        state.LastTickAt = DateTimeOffset.UtcNow;

        if (state.IsCircuitBreakerOpen)
        {
            _logger.LogWarning("Circuit breaker is open. Skipping dispatch.");
            await _dbContext.SaveChangesAsync(ct);
            return;
        }

        var tickLog = new BrainTickLog
        {
            TickNumber = state.CurrentTickNumber,
            StartedAt = DateTimeOffset.UtcNow
        };

        try
        {
            // Sync global memory if needed
            if ((DateTimeOffset.UtcNow - state.LastGlobalMemorySync).TotalSeconds > _options.GlobalMemorySyncIntervalSeconds)
            {
                await SyncGlobalMemoryAsync(ct);
                tickLog.GlobalMemoryRead = true;
            }

            // Get active jobs
            var activeJobs = await _dbContext.PlatformPublishJobs
                .Where(j => j.Status != PublishStatus.Published && j.Status != PublishStatus.Failed)
                .ToListAsync(ct);
                
            // Handle active pipelines (simplified logic for now)
            int dispatched = 0;
            
            // Handle stuck jobs
            await HandleStuckJobsAsync(ct);

            tickLog.JobsDispatched = dispatched;
            tickLog.Notes = "Tick completed successfully.";
            tickLog.CompletedAt = DateTimeOffset.UtcNow;
            tickLog.DurationMs = (long)(tickLog.CompletedAt.Value - tickLog.StartedAt).TotalMilliseconds;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during brain tick.");
            state.Status = BrainStatus.Error;
            state.LastErrorMessage = ex.Message;
            tickLog.Notes = $"Error: {ex.Message}";
            tickLog.JobsFailed++;
        }

        _dbContext.BrainTickLogs.Add(tickLog);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<GlobalMemory> SyncGlobalMemoryAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Syncing Global Memory from Drive...");
        var memory = await _globalMemoryService.LoadAsync(ct);
        
        var state = await GetStateAsync(ct);
        state.GlobalMemoryVersion = memory.Version;
        state.LastGlobalMemorySync = DateTimeOffset.UtcNow;
        
        await _dbContext.SaveChangesAsync(ct);
        return memory;
    }

    public Task DispatchAgentAsync(string agentKey, Guid jobId, CancellationToken ct = default)
    {
        _logger.LogInformation($"Dispatching {agentKey} for job {jobId}");
        
        string queue = agentKey switch
        {
            "script-gen-agent" => "ai",
            "edit-agent" => "ffmpeg",
            "shorts-agent" => "ffmpeg",
            "short-edit-agent" => "ffmpeg",
            "upload-agent" => "upload",
            "trend-agent" => "ai",
            "analytics-agent" => "ai",
            _ => "default"
        };
        
        // TODO: Enqueue actual agent jobs
        _logger.LogInformation($"Would enqueue to Hangfire queue: {queue}");
        
        return Task.CompletedTask;
    }

    public async Task<Dictionary<string, AgentHealthStatus>> CheckAgentHealthAsync(CancellationToken ct = default)
    {
        var state = await GetStateAsync(ct);
        return state.AgentHealthMap;
    }

    public async Task<BrainState> GetStateAsync(CancellationToken ct = default)
    {
        var state = await _dbContext.BrainStates.FirstOrDefaultAsync(ct);
        if (state == null)
        {
            state = new BrainState
            {
                Status = BrainStatus.Idle,
                AgentHealthMap = new Dictionary<string, AgentHealthStatus>()
            };
            _dbContext.BrainStates.Add(state);
            await _dbContext.SaveChangesAsync(ct);
        }
        return state;
    }

    public async Task PauseAsync(CancellationToken ct = default)
    {
        var state = await GetStateAsync(ct);
        state.Status = BrainStatus.Paused;
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task ResumeAsync(CancellationToken ct = default)
    {
        var state = await GetStateAsync(ct);
        state.Status = BrainStatus.Idle;
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task ForceGlobalMemoryRefreshAsync(CancellationToken ct = default)
    {
        await SyncGlobalMemoryAsync(ct);
    }
    
    private Task HandleStuckJobsAsync(CancellationToken ct)
    {
        // Dummy implementation for stuck jobs logic
        return Task.CompletedTask;
    }
}
