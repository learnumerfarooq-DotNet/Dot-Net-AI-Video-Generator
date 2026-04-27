using AiContentFactory.Application.Publishing;
using AiContentFactory.Application.Studio;
using AiContentFactory.Domain.Publishing.YouTube;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AiContentFactory.Infrastructure.Publishing.YouTube;

public class YouTubeUploadService
{
    private readonly YouTubeOAuthManager _oauthManager;
    private readonly IGoogleDriveService _driveService;
    private readonly YouTubeOptions _options;
    private readonly ILogger<YouTubeUploadService> _logger;

    public YouTubeUploadService(
        YouTubeOAuthManager oauthManager,
        IGoogleDriveService driveService,
        IOptions<YouTubeOptions> options,
        ILogger<YouTubeUploadService> logger)
    {
        _oauthManager = oauthManager;
        _driveService = driveService;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<YouTubeUploadResult> UploadVideoAsync(Stream videoStream, YouTubeVideoDetails details, string agentKey, CancellationToken ct)
    {
        var credential = await _oauthManager.GetCredentialAsync(agentKey, ct);
        var youtubeService = new YouTubeService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = _options.ApplicationName
        });

        var video = new Video
        {
            Snippet = new VideoSnippet
            {
                Title = details.Title,
                Description = details.Description,
                Tags = details.Tags,
                CategoryId = details.CategoryId
            },
            Status = new VideoStatus
            {
                PrivacyStatus = details.Privacy,
                SelfDeclaredMadeForKids = false
            }
        };

        if (details.IsShort)
        {
            if (!video.Snippet.Title.Contains("#Shorts")) video.Snippet.Title += " #Shorts";
            if (!video.Snippet.Description.Contains("#Shorts")) video.Snippet.Description += "\n\n#Shorts";
        }

        if (details.ScheduledPublishAt.HasValue)
        {
            video.Status.PublishAtDateTimeOffset = details.ScheduledPublishAt.Value;
            video.Status.PrivacyStatus = "private"; // Must be private to schedule
        }

        var videosInsertRequest = youtubeService.Videos.Insert(video, "snippet,status", videoStream, "video/*");
        videosInsertRequest.ChunkSize = _options.ChunkSizeMb * 1024 * 1024;
        
        videosInsertRequest.ProgressChanged += progress =>
        {
            _logger.LogInformation("Upload Progress: {Status} - {Bytes} bytes sent.", progress.Status, progress.BytesSent);
        };

        var uploadProgress = await videosInsertRequest.UploadAsync(ct);

        if (uploadProgress.Status == Google.Apis.Upload.UploadStatus.Failed)
        {
            throw new Exception($"YouTube upload failed: {uploadProgress.Exception.Message}");
        }

        var response = videosInsertRequest.ResponseBody;

        return new YouTubeUploadResult
        {
            Id = Guid.NewGuid(),
            YouTubeVideoId = response.Id,
            YouTubeUrl = $"https://www.youtube.com/watch?v={response.Id}",
            UploadStatus = "uploaded",
            PrivacyStatus = response.Status.PrivacyStatus,
            IsShort = details.IsShort,
            UploadedAt = DateTimeOffset.UtcNow
        };
    }

    public async Task<YouTubeUploadResult> UploadFromDriveAsync(string driveFileId, YouTubeVideoDetails details, string agentKey, CancellationToken ct)
    {
        // We need a workspace settings to get drive credentials, but here we assume the drive service is already configured or we can get it from context.
        // For simplicity in this implementation, we assume we can get a stream.
        
        // In a real scenario, we'd need the DriveSettingsDto from the store.
        // Assuming the caller provides a stream or we fetch it here.
        
        // Mocking the stream acquisition for now as we don't have the full Drive integration context here without a store
        throw new NotImplementedException("Direct Drive to YouTube streaming requires workspace context.");
    }

    public async Task SetThumbnailAsync(string videoId, Stream thumbnail, string agentKey, CancellationToken ct)
    {
        var credential = await _oauthManager.GetCredentialAsync(agentKey, ct);
        var youtubeService = new YouTubeService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = _options.ApplicationName
        });

        var thumbnailRequest = youtubeService.Thumbnails.Set(videoId, thumbnail, "image/jpeg");
        await thumbnailRequest.UploadAsync(ct);
    }

    public async Task<string> GetVideoStatusAsync(string videoId, string agentKey, CancellationToken ct)
    {
        var credential = await _oauthManager.GetCredentialAsync(agentKey, ct);
        var youtubeService = new YouTubeService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = _options.ApplicationName
        });

        var request = youtubeService.Videos.List("status,processingDetails");
        request.Id = videoId;
        var response = await request.ExecuteAsync(ct);

        var video = response.Items.FirstOrDefault();
        return video?.Status?.UploadStatus ?? "unknown";
    }
}
