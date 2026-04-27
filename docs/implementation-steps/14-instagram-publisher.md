# 14 — Instagram Reels Publisher

## Purpose
Implement Instagram Content Publishing API for automated Reels uploads. Uses Facebook Graph API underneath. Supports: Reels publishing, carousel posts (future), and Insights retrieval.

---

## FILE MAP
### New Files:
| File | Purpose |
|------|---------|
| `InstagramPublisher.cs` (`Infrastructure/Publishing/Instagram/`) | Publisher — **8 methods** |
| `InstagramOAuthManager.cs` (`Infrastructure/Publishing/Instagram/`) | OAuth — **5 methods** |
| `InstagramModels.cs` (`Domain/Publishing/`) | Models — **3 entities** |
| `InstagramOptions.cs` (`Application/Publishing/`) | Config — **7 fields** |

---

## ENTITY: InstagramUploadResult — 12 Fields
```
├── Id, PlatformPublishJobId, InstagramMediaId, InstagramUrl, AccountId, Username,
├── MediaType ("REELS"|"VIDEO"|"IMAGE"), UploadStatus, Caption, Hashtags (List),
├── CoverImageUrl, UploadedAt
```

## ENTITY: InstagramCredential — 7 Fields
```
├── Id, AgentKey, FacebookAppId, FacebookAppSecret, AccessToken (encrypted), InstagramAccountId, ExpiresAt
```

---

## CLASS: InstagramPublisher — 8 Methods

### Method 1: `UploadAsync(Stream video, PlatformMetadata metadata, CancellationToken ct)`
- Instagram Reels upload flow (via Graph API):
  1. Create media container: `POST /{ig-user-id}/media` with `media_type=REELS`, `video_url`
  2. Check status: `GET /{container-id}?fields=status_code` until `FINISHED`
  3. Publish: `POST /{ig-user-id}/media_publish` with `creation_id={container-id}`
- Caption with hashtags (max 2200 chars, max 30 hashtags)
- Set cover image frame

### Method 2: `UploadFromDriveAsync(string driveFileId, PlatformMetadata metadata, CancellationToken ct)`
- Need publicly accessible URL for video → use Drive sharing link or temp hosting

### Method 3: `GetMediaStatsAsync(string mediaId, CancellationToken ct)` → reach, plays, likes, comments
### Method 4: `GetAccountInsightsAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken ct)`
### Method 5: `DeleteMediaAsync(string mediaId, CancellationToken ct)`
### Method 6: `RefreshLongLivedTokenAsync(CancellationToken ct)`
### Method 7: `ValidateCredentialsAsync(CancellationToken ct)`
### Method 8: `GetMediaStatusAsync(string containerId, CancellationToken ct)`

---

## INSTAGRAM GRAPH API ENDPOINTS
```
POST   https://graph.facebook.com/v20.0/{ig-user-id}/media
GET    https://graph.facebook.com/v20.0/{container-id}?fields=status_code
POST   https://graph.facebook.com/v20.0/{ig-user-id}/media_publish
GET    https://graph.facebook.com/v20.0/{media-id}/insights
```

## INSTAGRAM CONSTRAINTS
| Constraint | Value |
|------------|-------|
| Reels Duration | 3-90 seconds (we use ≤60) |
| Aspect Ratio | 9:16 |
| Max File Size | 1 GB |
| Caption Max | 2200 characters |
| Max Hashtags | 30 |
| Cover Frame | Between 0-video_duration |

## INSTAGRAM PERMISSIONS
```
instagram_basic, instagram_content_publish, instagram_manage_insights, pages_show_list
```

---

## REST API ENDPOINTS
```
POST   /api/publish/instagram/upload
POST   /api/publish/instagram/upload-from-drive
GET    /api/publish/instagram/{mediaId}/status
GET    /api/publish/instagram/{mediaId}/insights
POST   /api/publish/instagram/auth/url
POST   /api/publish/instagram/auth/callback
```

## ESTIMATED TIME: 4-5 hours
