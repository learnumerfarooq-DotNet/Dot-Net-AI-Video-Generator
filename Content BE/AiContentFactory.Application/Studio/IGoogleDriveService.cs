namespace AiContentFactory.Application.Studio;

public interface IGoogleDriveService
{
    Task<IReadOnlyList<DriveFileDto>> ListFilesAsync(DriveSettingsDto settings, CancellationToken cancellationToken);
    Task<DriveFileDto?> CreateFolderAsync(DriveSettingsDto settings, string folderName, CancellationToken cancellationToken);
}
