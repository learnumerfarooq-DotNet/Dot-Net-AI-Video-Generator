using AiContentFactory.Application.Errors;
using AiContentFactory.Application.Memory;
using AiContentFactory.Application.Studio;
using AiContentFactory.Domain.Errors;
using AiContentFactory.Domain.GlobalMemory;
using AiContentFactory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AiContentFactory.Infrastructure.Errors;

public class ErrorHandlingService : IErrorHandlingService
{
    private readonly StudioDbContext _dbContext;
    private readonly IGoogleDriveService _drive;
    private readonly IGlobalMemoryService _globalMemory;
    private readonly IStudioWorkspaceStore _workspaceStore;
    private readonly ILogger<ErrorHandlingService> _logger;

    public ErrorHandlingService(
        StudioDbContext dbContext,
        IGoogleDriveService drive,
        IGlobalMemoryService globalMemory,
        IStudioWorkspaceStore workspaceStore,
        ILogger<ErrorHandlingService> logger)
    {
        _dbContext = dbContext;
        _drive = drive;
        _globalMemory = globalMemory;
        _workspaceStore = workspaceStore;
        _logger = logger;
    }

    public async Task HandleErrorAsync(Guid jobId, string agentKey, string error, CancellationToken ct)
    {
        _logger.LogError("Handling error for job {JobId}, agent {AgentKey}: {Error}", jobId, agentKey, error);

        var log = new ErrorLog
        {
            Id = Guid.NewGuid(),
            JobId = jobId,
            AgentKey = agentKey,
            Message = error,
            Timestamp = DateTimeOffset.UtcNow
        };

        _dbContext.ErrorLogs.Add(log);
        await _dbContext.SaveChangesAsync(ct);

        // Update Global Memory Error Summary
        var memory = await _globalMemory.LoadAsync(ct);
        memory.ErrorSummary ??= new ErrorSummary();
        memory.ErrorSummary.TotalErrorsLast24h++;
        memory.ErrorSummary.MostCommonError = error;
        memory.ErrorSummary.MostFailedAgent = agentKey;
        memory.ErrorSummary.LastUpdated = DateTimeOffset.UtcNow;
        await _globalMemory.SaveAsync(memory, ct);

        if (await ShouldRetryAsync(jobId, agentKey, ct))
        {
            await RetryJobAsync(jobId, agentKey, ct);
        }
        else
        {
            await MoveToDeadLetterAsync(jobId, "Max retries reached or unrecoverable error", ct);
        }
    }

    public async Task<bool> ShouldRetryAsync(Guid jobId, string agentKey, CancellationToken ct)
    {
        var retryCount = await _dbContext.ErrorLogs.CountAsync(l => l.JobId == jobId && l.AgentKey == agentKey, ct);
        var policy = await _dbContext.RetryPolicies.FirstOrDefaultAsync(p => p.AgentKey == agentKey, ct) 
                     ?? new RetryPolicy { AgentKey = agentKey };

        return retryCount <= policy.MaxRetries;
    }

    public async Task RetryJobAsync(Guid jobId, string agentKey, CancellationToken ct)
    {
        var retryCount = await _dbContext.ErrorLogs.CountAsync(l => l.JobId == jobId && l.AgentKey == agentKey, ct);
        _logger.LogInformation("Retrying job {JobId} for agent {AgentKey}. Attempt {Attempt}", jobId, agentKey, retryCount);

        // Write to Drive /Errors/retry/
        var settings = await _workspaceStore.GetDriveSettingsAsync(ct);
        if (settings != null && !string.IsNullOrEmpty(settings.RootFolderId))
        {
            var errorsFolder = await _drive.CreateFolderAsync(settings, settings.RootFolderId, "Errors", ct);
            var retryFolder = await _drive.CreateFolderAsync(settings, errorsFolder.Id, "retry", ct);
            
            var retryData = new { JobId = jobId, AgentKey = agentKey, Attempt = retryCount, Timestamp = DateTimeOffset.UtcNow };
            using var stream = new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(retryData));
            await _drive.UploadFileAsync(settings, retryFolder.Id, $"{jobId}_attempt_{retryCount}.json", "application/json", stream, ct);
        }
    }

    public async Task MoveToDeadLetterAsync(Guid jobId, string reason, CancellationToken ct)
    {
        _logger.LogWarning("Moving job {JobId} to dead letter queue. Reason: {Reason}", jobId, reason);

        var entry = new DeadLetterEntry
        {
            Id = Guid.NewGuid(),
            JobId = jobId,
            OriginalError = reason,
            FirstFailedAt = DateTimeOffset.UtcNow,
            LastFailedAt = DateTimeOffset.UtcNow,
            IsResolvable = true
        };

        _dbContext.DeadLetterEntries.Add(entry);
        await _dbContext.SaveChangesAsync(ct);

        // Write to Drive /Errors/dead-letter/
        var settings = await _workspaceStore.GetDriveSettingsAsync(ct);
        if (settings != null && !string.IsNullOrEmpty(settings.RootFolderId))
        {
            var errorsFolder = await _drive.CreateFolderAsync(settings, settings.RootFolderId, "Errors", ct);
            var dlFolder = await _drive.CreateFolderAsync(settings, errorsFolder.Id, "dead-letter", ct);
            
            using var stream = new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(entry));
            await _drive.UploadFileAsync(settings, dlFolder.Id, $"{jobId}_dead.json", "application/json", stream, ct);
        }
    }

    public async Task<CircuitBreakerState> GetCircuitStateAsync(string agentKey, CancellationToken ct)
    {
        return await _dbContext.CircuitBreakerStates.FirstOrDefaultAsync(s => s.AgentKey == agentKey, ct)
               ?? new CircuitBreakerState { AgentKey = agentKey };
    }

    public async Task OpenCircuitBreakerAsync(string agentKey, CancellationToken ct)
    {
        var state = await GetCircuitStateAsync(agentKey, ct);
        state.State = "Open";
        state.OpenedAt = DateTimeOffset.UtcNow;
        state.NextRetryAt = DateTimeOffset.UtcNow.AddMinutes(state.PauseMinutes);

        if (state.Id == Guid.Empty) _dbContext.CircuitBreakerStates.Add(state);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task CloseCircuitBreakerAsync(string agentKey, CancellationToken ct)
    {
        var state = await GetCircuitStateAsync(agentKey, ct);
        state.State = "Closed";
        state.FailureCount = 0;
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<List<DeadLetterEntry>> GetDeadLetterQueueAsync(CancellationToken ct)
    {
        return await _dbContext.DeadLetterEntries
            .Where(e => e.ArchivedAt == null)
            .ToListAsync(ct);
    }

    public async Task<ErrorSummary> GetErrorSummaryAsync(CancellationToken ct)
    {
        var memory = await _globalMemory.LoadAsync(ct);
        return memory.ErrorSummary ?? new ErrorSummary { LastUpdated = DateTimeOffset.UtcNow };
    }

    public async Task ResolveDeadLetterAsync(Guid entryId, string resolution, CancellationToken ct)
    {
        var entry = await _dbContext.DeadLetterEntries.FindAsync(new object[] { entryId }, ct) ?? throw new Exception("Entry not found");
        entry.ResolutionNotes = resolution;
        entry.ArchivedAt = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync(ct);
    }
}
