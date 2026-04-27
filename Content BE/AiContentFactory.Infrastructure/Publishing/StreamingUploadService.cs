using AiContentFactory.Application.Pipeline;
using AiContentFactory.Application.Studio;
using AiContentFactory.Domain.Publishing;
using Microsoft.Extensions.Logging;

namespace AiContentFactory.Infrastructure.Publishing;

public sealed class StreamingUploadService
{
    private readonly IGoogleDriveService _drive;
    private readonly IStudioWorkspaceStore _workspaceStore;
    private readonly IEnumerable<IPlatformPublisher> _publishers;
    private readonly ILogger<StreamingUploadService> _logger;

    public StreamingUploadService(
        IGoogleDriveService drive,
        IStudioWorkspaceStore workspaceStore,
        IEnumerable<IPlatformPublisher> publishers,
        ILogger<StreamingUploadService> logger)
    {
        _drive = drive;
        _workspaceStore = workspaceStore;
        _publishers = publishers;
        _logger = logger;
    }

    public async Task PublishAsync(string driveFileId, PlatformMetadata metadata, CancellationToken ct = default)
    {
        var publisher = _publishers.FirstOrDefault(p => p.PlatformName.Equals(metadata.Platform, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"No publisher found for platform: {metadata.Platform}");

        _logger.LogInformation("Publishing to {Platform}: {Title}", metadata.Platform, metadata.Title);

        var settings = await _workspaceStore.GetDriveSettingsAsync(ct);
        if (settings == null) throw new InvalidOperationException("No drive settings.");

        // Strategy: STREAM from Drive directly to Platform API (Zero VPS disk usage)
        var driveFile = await _drive.DownloadFileAsync(settings, driveFileId, ct);
        if (driveFile == null) throw new InvalidOperationException("Failed to get stream from Drive.");

        using (var stream = driveFile.Value.Content)
        {
            var platformId = await publisher.UploadAsync(stream, metadata, ct);
            _logger.LogInformation("Successfully published to {Platform}. Platform ID: {Id}", metadata.Platform, platformId);
        }
    }
}
