# 15 — Facebook & LinkedIn Publishers

## Purpose
Implement Facebook Graph API video uploads and LinkedIn Video API integration. Both platforms follow a similar download-upload-delete pattern for videos stored on Google Drive.

---

## PART A: FACEBOOK PUBLISHER

### New Files:
| File | Purpose |
|------|---------|
| `FacebookPublisher.cs` (`Infrastructure/Publishing/Facebook/`) | Publisher — **7 methods** |
| `FacebookModels.cs` (`Domain/Publishing/`) | Models — **2 entities** |

### ENTITY: FacebookUploadResult — 11 Fields
```
├── Id, PlatformPublishJobId, FacebookVideoId, FacebookUrl, PageId, PageName,
├── UploadStatus, Privacy ("EVERYONE"|"FRIENDS"|"SELF"), Description,
├── ScheduledPublishTime, UploadedAt
```

### ENTITY: FacebookCredential — 6 Fields
```
├── Id, AgentKey, AppId, AppSecret, PageAccessToken (encrypted), PageId
```

### CLASS: FacebookPublisher — 7 Methods
1. `UploadAsync()` — Facebook resumable video upload:
   - `POST /{page-id}/videos` with `upload_phase=start`
   - `POST /{upload-session-id}` with video chunks
   - `POST /{upload-session-id}` with `upload_phase=finish`
2. `UploadFromDriveAsync()` — download from Drive → upload → cleanup
3. `GetVideoStatsAsync()` — Graph API insights
4. `SchedulePostAsync()` — scheduled publishing with `scheduled_publish_time`
5. `DeleteVideoAsync()`
6. `RefreshPageTokenAsync()`
7. `ValidateCredentialsAsync()`

### FACEBOOK API CONSTRAINTS
| Max Duration | 240 minutes | Max File Size | 10 GB | Max Description | 63,206 chars |

---

## PART B: LINKEDIN PUBLISHER

### New Files:
| File | Purpose |
|------|---------|
| `LinkedInPublisher.cs` (`Infrastructure/Publishing/LinkedIn/`) | Publisher — **7 methods** |
| `LinkedInModels.cs` (`Domain/Publishing/`) | Models — **2 entities** |

### ENTITY: LinkedInUploadResult — 11 Fields
```
├── Id, PlatformPublishJobId, LinkedInPostUrn, LinkedInUrl, OrganizationId, AuthorUrn,
├── UploadStatus, Visibility ("PUBLIC"|"CONNECTIONS"), Commentary,
├── AssetUrn, UploadedAt
```

### ENTITY: LinkedInCredential — 6 Fields
```
├── Id, AgentKey, ClientId, ClientSecret, AccessToken (encrypted), OrganizationId
```

### CLASS: LinkedInPublisher — 7 Methods
1. `UploadAsync()` — LinkedIn video upload flow:
   - Register upload: `POST /rest/videos?action=initializeUpload`
   - Upload binary: `PUT {uploadUrl}` with video data
   - Create post: `POST /rest/posts` with `media.id={video-urn}`
2. `UploadFromDriveAsync()` — Drive → LinkedIn pipeline
3. `GetPostStatsAsync()` — LinkedIn Analytics API
4. `DeletePostAsync()`
5. `GetOrganizationInfoAsync()`
6. `RefreshAccessTokenAsync()`
7. `ValidateCredentialsAsync()`

### LINKEDIN API CONSTRAINTS
| Max Duration | 15 minutes | Max File Size | 5 GB | Max Description | 3,000 chars |

### LINKEDIN SCOPES
```
r_liteprofile, w_member_social, r_organization_social, w_organization_social
```

---

## REST API ENDPOINTS (Both Platforms)
```
POST   /api/publish/facebook/upload
POST   /api/publish/facebook/upload-from-drive
POST   /api/publish/linkedin/upload
POST   /api/publish/linkedin/upload-from-drive
GET    /api/publish/{platform}/{id}/status
POST   /api/publish/{platform}/auth/url
POST   /api/publish/{platform}/auth/callback
```

## DI REGISTRATION
```csharp
services.AddScoped<IPlatformPublisher, FacebookPublisher>();
services.AddScoped<IPlatformPublisher, LinkedInPublisher>();
```

## ESTIMATED TIME: 4-5 hours each (8-10 total)
