# 13 — TikTok Publisher

## Purpose
Implement TikTok Content Posting API integration for automated video uploads. TikTok uses a unique flow: download video from Drive → upload to TikTok → delete local copy. Supports scheduled publishing and content management.

---

## FILE MAP
### New Files:
| File | Purpose |
|------|---------|
| `TikTokPublisher.cs` (`Infrastructure/Publishing/TikTok/`) | Publisher — **8 methods** |
| `TikTokOAuthManager.cs` (`Infrastructure/Publishing/TikTok/`) | OAuth — **5 methods** |
| `TikTokModels.cs` (`Domain/Publishing/`) | Models — **3 entities** |
| `TikTokOptions.cs` (`Application/Publishing/`) | Config — **8 fields** |

---

## ENTITY: TikTokUploadResult — 12 Fields
```
├── Id, PlatformPublishJobId, TikTokVideoId, TikTokUrl, CreatorId, Username,
├── UploadStatus, PrivacyLevel ("public"|"friends"|"private"), AllowComments,
├── AllowDuet, AllowStitch, UploadedAt
```

## ENTITY: TikTokCredential — 7 Fields
```
├── Id, AgentKey, ClientKey, ClientSecret, AccessToken (encrypted), RefreshToken (encrypted), ExpiresAt
```

---

## CLASS: TikTokPublisher — 8 Methods

### Method 1: `UploadAsync(Stream video, PlatformMetadata metadata, CancellationToken ct)`
- TikTok uses 3-step process:
  1. Create upload session: `POST /v2/post/publish/inbox/video/init/`
  2. Upload video chunks: `PUT {upload_url}`
  3. Publish: `POST /v2/post/publish/`
- Set: privacy, allow_comments, allow_duet, allow_stitch
- Caption with hashtags (max 2200 chars, max 10 hashtags)

### Method 2: `UploadFromDriveAsync(string driveFileId, PlatformMetadata metadata, CancellationToken ct)`
- Download from Drive to temp → upload to TikTok → delete temp

### Method 3: `GetVideoStatusAsync(string videoId, CancellationToken ct)`
### Method 4: `GetVideoStatsAsync(string videoId, CancellationToken ct)` → views, likes, shares
### Method 5: `DeleteVideoAsync(string videoId, CancellationToken ct)`
### Method 6: `GetCreatorInfoAsync(CancellationToken ct)` → profile info
### Method 7: `RefreshAccessTokenAsync(CancellationToken ct)`
### Method 8: `ValidateCredentialsAsync(CancellationToken ct)`

---

## TIKTOK API ENDPOINTS USED
```
POST   https://open.tiktokapis.com/v2/post/publish/inbox/video/init/
PUT    {upload_url}
POST   https://open.tiktokapis.com/v2/post/publish/
GET    https://open.tiktokapis.com/v2/video/query/
```

## TIKTOK SCOPES
```
video.upload, video.publish, user.info.basic
```

## TIKTOK CONSTRAINTS
| Constraint | Value |
|------------|-------|
| Max Video Duration | 60 seconds (for our use) |
| Max File Size | 4 GB |
| Caption Max Length | 2200 characters |
| Max Hashtags | 10 per post |
| Supported Formats | MP4, WebM |

---

## REST API ENDPOINTS
```
POST   /api/publish/tiktok/upload
POST   /api/publish/tiktok/upload-from-drive
GET    /api/publish/tiktok/{videoId}/status
POST   /api/publish/tiktok/auth/url
POST   /api/publish/tiktok/auth/callback
```

## ESTIMATED TIME: 4-5 hours
