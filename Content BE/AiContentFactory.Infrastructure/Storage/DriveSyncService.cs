using AiContentFactory.Application.Studio;
using AiContentFactory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AiContentFactory.Infrastructure.Storage;

public sealed class DriveSyncService(
    IServiceProvider serviceProvider,
    ILogger<DriveSyncService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Drive Sync Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SyncDriveAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while syncing Google Drive.");
            }

            // Sync every 10 minutes
            await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
        }
    }

    private async Task SyncDriveAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IStudioWorkspaceStore>();
        var driveService = scope.ServiceProvider.GetRequiredService<IGoogleDriveService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<StudioDbContext>();

        var settings = await store.GetDriveSettingsAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(settings.ClientId) || string.IsNullOrWhiteSpace(settings.RefreshToken))
        {
            logger.LogWarning("Drive Sync skipped: Credentials not configured.");
            return;
        }

        logger.LogInformation("Syncing Google Drive folder: {RootFolderId}", settings.RootFolderId);
        var files = await driveService.ListFilesAsync(settings, null, cancellationToken);

        foreach (var file in files)
        {
            // Only sync video files
            if (!file.Type.StartsWith("video/") && !file.Type.Contains("google-apps.folder")) continue;

            var existingVideo = await dbContext.Videos.FirstOrDefaultAsync(v => v.DriveFileId == file.Id, cancellationToken);
            if (existingVideo == null)
            {
                // New file found in Drive that is not in our DB
                logger.LogInformation("Found new Drive asset: {FileName} ({FileId})", file.Name, file.Id);
                
                DateTimeOffset? createdAt = null;
                if (DateTimeOffset.TryParse(file.Date, out var parsedDate)) createdAt = parsedDate;

                dbContext.Videos.Add(new StudioVideoEntity
                {
                    Id = Guid.NewGuid(),
                    Title = file.Name,
                    Topic = "Discovered from Drive",
                    Format = "Unknown",
                    Stage = "ReadyToUpload", // Assume it's ready if it's in the root or a managed folder
                    DriveFileId = file.Id,
                    CreatedAt = createdAt ?? DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                });
            }
            else
            {
                // Update existing record if name changed
                if (existingVideo.Title != file.Name)
                {
                    existingVideo.Title = file.Name;
                    existingVideo.UpdatedAt = DateTimeOffset.UtcNow;
                }
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Drive Sync completed. Found {Count} assets.", files.Count);
    }
}
