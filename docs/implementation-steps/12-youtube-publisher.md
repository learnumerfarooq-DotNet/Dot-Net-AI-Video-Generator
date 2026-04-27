# 12 — YouTube Publisher (Real Implementation)

## Purpose
Replace the simulated YouTube publisher with a real implementation using YouTube Data API v3, OAuth2 credentials, resumable uploads, and proper error handling. Supports both long-form videos and YouTube Shorts.

---

## FILE MAP
### Existing: `YouTubePublisher.cs` (Infrastructure/Publishing/) — replace simulation with real implementation
### New Files:
| File | Purpose |
|------|---------|
| `YouTubeOAuthManager.cs` (`Infrastructure/Publishing/YouTube/`) | OAuth2 token management — **6 methods** |
| `YouTubeUploadService.cs` (`Infrastructure/Publishing/YouTube/`) | Resumable upload — **8 methods** |
| `YouTubeAnalyticsService.cs` (`Infrastructure/Publishing/YouTube/`) | Stats retrieval — **5 methods** |
| `YouTubeModels.cs` (`Domain/Publishing/`) | YouTube-specific models — **3 entities** |
| `YouTubeOptions.cs` (`Application/Publishing/`) | Configuration — **10 fields** |

---

## ENTITY: YouTubeUploadResult — 14 Fields
```
├── Id, PlatformPublishJobId, YouTubeVideoId, YouTubeUrl, ChannelId, ChannelTitle,
├── UploadStatus ("uploaded"|"processing"|"processed"|"failed"), ProcessingStatus,
├── PrivacyStatus, IsShort (bool), ThumbnailUrl, UploadedAt, FileSizeBytes, DurationMs
```

## ENTITY: YouTubeCredential — 8 Fields
```
├── Id, AgentKey, ClientId, ClientSecret, RefreshToken (encrypted), AccessToken (encrypted),
├── TokenExpiresAt, ChannelId
```

## ENTITY: YouTubeVideoDetails — 12 Fields
```
├── Id, YouTubeVideoId, Title, Description, Tags (List), CategoryId, Privacy,
├── ScheduledPublishAt, ThumbnailPath, IsShort, PlaylistId, CreatedAt
```

---

## CLASS: YouTubeOAuthManager — 6 Methods
```csharp
Task<string> GetAccessTokenAsync(string agentKey, CancellationToken ct);  // Refresh if expired
Task<UserCredential> GetCredentialAsync(string agentKey, CancellationToken ct);
Task SaveCredentialAsync(string agentKey, YouTubeCredential cred, CancellationToken ct);
Task<bool> ValidateCredentialAsync(string agentKey, CancellationToken ct);
Task RevokeTokenAsync(string agentKey, CancellationToken ct);
Task<string> GetAuthorizationUrlAsync(string agentKey, string redirectUri);
```

---

## CLASS: YouTubeUploadService — 8 Methods

### Method 1: `UploadVideoAsync(Stream video, YouTubeVideoDetails details, CancellationToken ct)`
- Create `Video` resource with snippet, status, recording details
- Use resumable upload: `VideosResource.InsertMediaUpload`
- Track progress via `ProgressChanged` event
- Support for scheduled publishing: `Status.PublishAt`
- If `IsShort = true`: add `#Shorts` to tags
- Return `YouTubeUploadResult`

### Method 2: `UploadFromDriveAsync(string driveFileId, YouTubeVideoDetails details, CancellationToken ct)`
- Stream directly from Google Drive to YouTube (no temp download)
- Use `IGoogleDriveService.DownloadStreamAsync()` → pipe to YouTube upload

### Method 3: `SetThumbnailAsync(string videoId, Stream thumbnail, CancellationToken ct)`
### Method 4: `UpdateVideoDetailsAsync(string videoId, YouTubeVideoDetails details, CancellationToken ct)`
### Method 5: `DeleteVideoAsync(string videoId, CancellationToken ct)`
### Method 6: `GetVideoStatusAsync(string videoId, CancellationToken ct)` → processing status
### Method 7: `AddToPlaylistAsync(string videoId, string playlistId, CancellationToken ct)`
### Method 8: `SchedulePublishAsync(string videoId, DateTimeOffset publishAt, CancellationToken ct)`

---

## CLASS: YouTubeAnalyticsService — 5 Methods
```csharp
Task<VideoAnalytics> GetVideoStatsAsync(string videoId, CancellationToken ct);
Task<List<VideoAnalytics>> GetChannelStatsAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken ct);
Task<double> GetCTRAsync(string videoId, CancellationToken ct);
Task<double> GetAverageWatchTimeAsync(string videoId, CancellationToken ct);
Task<long> GetSubscriberDeltaAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken ct);
```

---

## NUGET PACKAGES
```
Google.Apis.YouTube.v3
Google.Apis.YouTubeAnalytics.v2
Google.Apis.Auth
```

## YOUTUBE API SCOPES
```
YouTubeService.Scope.YoutubeUpload
YouTubeService.Scope.YoutubeForceSsl
YouTubeService.Scope.YoutubeReadonly
YouTubeAnalyticsService.Scope.YtAnalyticsReadonly
```

## YOUTUBE SHORTS DETECTION
- Duration ≤ 60 seconds AND aspect ratio 9:16 → automatically categorized as Short
- Add `#Shorts` hashtag to title or description

---

## REST API ENDPOINTS
```
POST   /api/publish/youtube/upload              → Upload video
POST   /api/publish/youtube/upload-from-drive   → Stream from Drive
POST   /api/publish/youtube/{videoId}/thumbnail  → Set thumbnail
GET    /api/publish/youtube/{videoId}/status     → Get processing status
GET    /api/publish/youtube/analytics/{videoId}  → Get video analytics
POST   /api/publish/youtube/auth/url            → Get OAuth URL
POST   /api/publish/youtube/auth/callback       → Handle OAuth callback
```

## ESTIMATED TIME: 5-7 hours
