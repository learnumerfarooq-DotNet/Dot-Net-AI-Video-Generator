using AiContentFactory.Application.Pipeline;
using AiContentFactory.Application.Studio;
using AiContentFactory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AiContentFactory.Infrastructure.Pipeline;

public sealed class DriveFolderWatcher : IDriveFolderWatcher
{
    private readonly IGoogleDriveService _driveService;
    private readonly IStudioWorkspaceStore _workspaceStore;
    private readonly StudioDbContext _dbContext;
    private readonly ILogger<DriveFolderWatcher> _logger;

    public DriveFolderWatcher(
        IGoogleDriveService driveService,
        IStudioWorkspaceStore workspaceStore,
        StudioDbContext dbContext,
        ILogger<DriveFolderWatcher> logger)
    {
        _driveService = driveService;
        _workspaceStore = workspaceStore;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<IReadOnlyList<DriveFileChange>> PollForChangesAsync(string folderPath, CancellationToken ct = default)
    {
        _logger.LogInformation("Polling Drive folder: {Path}", folderPath);

        // 1. Get drive settings
        var settings = await _workspaceStore.GetDriveSettingsAsync(ct);
        if (settings == null) return Array.Empty<DriveFileChange>();

        // 2. Find folder ID from path (or use root for now)
        // In a real implementation, we would traverse the path or have a registry.
        // For Step 4, we assume /RAW is a specific folder ID or we look for it.
        var files = await _driveService.ListFilesAsync(settings, settings.RootFolderId, ct);
        var rawFolder = files.FirstOrDefault(f => f.Name.Equals("Raw Video", StringComparison.OrdinalIgnoreCase) && f.Type == "folder");
        
        if (rawFolder == null)
        {
            _logger.LogWarning("RAW folder not found in Drive root.");
            return Array.Empty<DriveFileChange>();
        }

        // 3. List files in RAW folder
        var rawFiles = await _driveService.ListFilesAsync(settings, rawFolder.Id, ct);
        
        // 4. Identify new files (those not already in our VideoPipelineJobs table)
        var knownFileIds = await _dbContext.VideoPipelineJobs
            .Select(j => j.DriveFileId)
            .ToListAsync(ct);

        var newFiles = rawFiles
            .Where(f => f.Type == "video" && !knownFileIds.Contains(f.Id))
            .Select(f => new DriveFileChange(f.Id, f.Name, DateTimeOffset.UtcNow, ChangeType.Created))
            .ToList();

        if (newFiles.Any())
        {
            _logger.LogInformation("Detected {Count} new videos in RAW folder.", newFiles.Count);
        }

        return newFiles;
    }
}
