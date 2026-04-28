using AiContentFactory.Application.Agents;
using AiContentFactory.Application.Pipeline;
using AiContentFactory.Application.Processing;
using AiContentFactory.Application.Studio;
using AiContentFactory.Domain.Pipeline;
using AiContentFactory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AiContentFactory.Infrastructure.Pipeline;

public sealed class RawPipelineHandler
{
    private readonly StudioDbContext _dbContext;
    private readonly IGoogleDriveService _driveService;
    private readonly IStudioWorkspaceStore _workspaceStore;
    private readonly IVideoMetadataExtractor _metadataExtractor;
    private readonly ITempStorageManager _tempStorage;
    private readonly IPipelineOrchestrator _orchestrator;
    private readonly IScriptGenAgent _scriptAgent;
    private readonly ILogger<RawPipelineHandler> _logger;

    public RawPipelineHandler(
        StudioDbContext dbContext,
        IGoogleDriveService driveService,
        IStudioWorkspaceStore workspaceStore,
        IVideoMetadataExtractor metadataExtractor,
        ITempStorageManager tempStorage,
        IPipelineOrchestrator orchestrator,
        IScriptGenAgent scriptAgent,
        ILogger<RawPipelineHandler> logger)
    {
        _dbContext = dbContext;
        _driveService = driveService;
        _workspaceStore = workspaceStore;
        _metadataExtractor = metadataExtractor;
        _tempStorage = tempStorage;
        _orchestrator = orchestrator;
        _scriptAgent = scriptAgent;
        _logger = logger;
    }

    public async Task HandleAsync(Guid jobId, CancellationToken ct)
    {
        var job = await _dbContext.VideoPipelineJobs.FindAsync(new object[] { jobId }, ct);
        if (job == null) return;

        try
        {
            _logger.LogInformation("Processing RAW stage for job {JobId}", jobId);

            // 1. Get Drive Settings
            var settings = await _workspaceStore.GetDriveSettingsAsync(ct);
            if (settings == null) throw new InvalidOperationException("No drive settings found.");

            // 2. Download to Temp
            var tempDir = _tempStorage.CreateJobDirectory(jobId);
            var (stream, contentType, fileName, size) = await _driveService.DownloadFileAsync(settings, job.DriveFileId, ct)
                ?? throw new InvalidOperationException("Failed to download file from Drive.");

            var localPath = Path.Combine(tempDir, fileName);
            using (var fileStream = File.Create(localPath))
            {
                await stream.CopyToAsync(fileStream, ct);
            }

            // 3. Extract Metadata
            var metadata = await _metadataExtractor.ExtractAsync(localPath, ct);
            metadata.JobId = jobId;
            _dbContext.VideoMetadata.Add(metadata);
            await _dbContext.SaveChangesAsync(ct);

            // 4. Generate Script via IScriptGenAgent
            var script = await _scriptAgent.GenerateScriptAsync(jobId, metadata, ct);
            
            // 5. Transition to next stage (Wait, GenerateScriptAsync already does the transition internally!)
            // So we just log.
            
            _logger.LogInformation("RAW stage completed for job {JobId}. Script generated.", jobId);
        }
        catch (Exception ex)
        {
            await _orchestrator.HandleFailureAsync(jobId, ex.Message, ct);
        }
    }
}
