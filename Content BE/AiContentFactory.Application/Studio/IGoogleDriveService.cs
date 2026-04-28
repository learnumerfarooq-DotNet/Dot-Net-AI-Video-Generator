namespace AiContentFactory.Application.Studio;

public interface IGoogleDriveService
{
    Task<IReadOnlyList<DriveFileDto>> ListFilesAsync(DriveSettingsDto settings, string? folderId, CancellationToken cancellationToken);
    Task<DriveFileDto?> CreateFolderAsync(DriveSettingsDto settings, string? folderId, string folderName, CancellationToken cancellationToken);
    Task<DriveFileDto?> UploadFileAsync(DriveSettingsDto settings, string? folderId, string fileName, string contentType, Stream fileStream, CancellationToken cancellationToken);
    Task<(Stream Content, string ContentType, string FileName, long Size)?> DownloadFileAsync(DriveSettingsDto settings, string fileId, CancellationToken cancellationToken);
    Task<string> WatchFolderAsync(DriveSettingsDto settings, string folderId, string webhookUrl, CancellationToken cancellationToken);
    Task<(long Used, long Limit)> GetStorageQuotaAsync(DriveSettingsDto settings, CancellationToken cancellationToken);
}
