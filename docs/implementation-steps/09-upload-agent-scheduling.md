# 09 — Upload Agent & Multi-Platform Scheduling

## Purpose
The Upload Agent prepares videos for publishing by attaching AI-generated titles, descriptions, keywords, hashtags, thumbnails, and scheduling them for optimal upload times. It reads from `/ReadyToUpload/` and trend schedule slots, then coordinates with platform-specific publishers.

---

## FILE MAP
### Existing: `UploadAgent.cs` (Infrastructure/Agents/) — enhance 43 → ~200 lines
### New Files:
| File | Purpose |
|------|---------|
| `UploadPackage.cs` (`Domain/Agents/`) | Upload package entity — **22 fields** |
| `IUploadAgent.cs` (`Application/Agents/`) | Interface — **8 methods** |
| `UploadMetadataGenerator.cs` (`Infrastructure/Agents/`) | AI metadata generation |
| `UploadQueueManager.cs` (`Infrastructure/Agents/`) | Queue management |
| `UploadPrompts.cs` (`Infrastructure/Agents/`) | Prompt templates |

---

## ENTITY: UploadPackage — 22 Fields
```
UploadPackage
├── Id                  : Guid
├── JobId               : Guid            (FK → VideoPipelineJob)
├── VideoType           : string          ("long" | "short")
├── SourceDriveFileId   : string          (Drive ID of video to upload)
├── SourceFolder        : string          ("/ReadyToUpload/" or "/Shorts/processed/")
├── Title               : string          (AI-generated platform-optimized title)
├── Description         : string          (AI-generated description)
├── Keywords            : List<string>    (SEO keywords)
├── Hashtags            : List<string>    (#hashtags)
├── Category            : string          (platform category)
├── Privacy             : string          ("public" | "unlisted" | "private")
├── ScheduledTime       : DateTimeOffset? (from Trend Agent schedule)
├── TargetPlatforms     : List<string>    (["YouTube", "TikTok", "Instagram"])
├── PublishJobs         : List<PlatformPublishJob>  (per-platform jobs)
├── ThumbnailDriveFileId : string?        (AI-generated or extracted thumbnail)
├── ThumbnailText       : string?         (text overlay on thumbnail)
├── ScheduleSlotId      : Guid?           (FK → ScheduleSlot from Trend Agent)
├── TrendKeyword        : string?         (trending keyword this targets)
├── Status              : UploadPackageStatus (Preparing/Ready/Publishing/Published/Failed)
├── ConfidenceScore     : double
├── ApprovalRequired    : bool            (manual approval needed?)
├── CreatedAt           : DateTimeOffset
```

**Enum**: `UploadPackageStatus` — 5 values: `Preparing, Ready, Publishing, Published, Failed`

---

## INTERFACE: IUploadAgent — 8 Methods
```csharp
Task<UploadPackage> PrepareUploadAsync(Guid jobId, CancellationToken ct);
Task<UploadPackage> GenerateMetadataAsync(Guid packageId, CancellationToken ct);
Task AssignToScheduleSlotAsync(Guid packageId, Guid slotId, CancellationToken ct);
Task<List<PlatformPublishJob>> CreatePublishJobsAsync(Guid packageId, CancellationToken ct);
Task ExecuteUploadAsync(Guid packageId, CancellationToken ct);
Task<UploadPackage?> GetPackageAsync(Guid packageId, CancellationToken ct);
Task<List<UploadPackage>> GetPendingPackagesAsync(CancellationToken ct);
Task ApprovePackageAsync(Guid packageId, CancellationToken ct);
```

---

## CLASS: UploadMetadataGenerator — 5 Methods

### Method 1: `GenerateTitleAsync(ScriptOutput script, TrendResult trends, string platform)` → AI-optimized title
### Method 2: `GenerateDescriptionAsync(ScriptOutput script, string platform)` → SEO description
### Method 3: `GenerateHashtagsAsync(List<string> keywords, string platform)` → platform-specific hashtags
### Method 4: `SuggestCategoryAsync(ScriptOutput script, string platform)` → best category
### Method 5: `GenerateThumbnailTextAsync(string title)` → thumbnail overlay text

### Platform-specific rules:
- **YouTube**: Title ≤100 chars, Description ≤5000, Tags ≤500 total chars, Category from YouTube list
- **TikTok**: Title ≤150 chars, Max 10 hashtags, Caption ≤2200 chars
- **Instagram Reels**: Caption ≤2200, Max 30 hashtags
- **Facebook**: Title ≤100, Description ≤63206
- **LinkedIn**: Title ≤200, Description ≤3000

---

## CLASS: UploadQueueManager — 6 Methods

### Method 1: `EnqueueUploadAsync(UploadPackage package)` — add to upload queue
### Method 2: `DequeueNextAsync()` — get next scheduled upload
### Method 3: `GetQueueStatusAsync()` — queue depth, next upload time
### Method 4: `RescheduleAsync(Guid packageId, DateTimeOffset newTime)` — move in queue
### Method 5: `CancelUploadAsync(Guid packageId)` — remove from queue
### Method 6: `ProcessQueueAsync()` — process all due uploads (called by Hangfire)

---

## PROMPT TEMPLATE

### System Prompt
```
You are an expert social media content optimizer.
Generate SEO-optimized titles, descriptions, and hashtags
for video content across multiple platforms.
Adapt content to each platform's best practices.
```

### JSON Schema
```json
{
    "title": "string — catchy, SEO-optimized title",
    "description": "string — platform-optimized description with CTAs",
    "keywords": ["string array"],
    "hashtags": ["#string array"],
    "category": "string",
    "isPublic": true
}
```

---

## OPENROUTER MODEL
| Model | OpenRouter tool-use model |
| Features | Platform API tools |

---

## SIGNALR EVENTS
| Event | Payload |
|-------|---------|
| `OnUploadPackageCreated` | `{ packageId, jobId, platformCount }` |
| `OnUploadScheduled` | `{ packageId, scheduledTime, platform }` |
| `OnUploadStarted` | `{ packageId, platform }` |
| `OnUploadComplete` | `{ packageId, platform, platformVideoId }` |

---

## REST API ENDPOINTS
```
POST   /api/agents/upload/prepare/{jobId}        → Prepare upload package
POST   /api/agents/upload/{packageId}/metadata    → Generate metadata
POST   /api/agents/upload/{packageId}/schedule    → Assign schedule slot
POST   /api/agents/upload/{packageId}/execute     → Execute upload
POST   /api/agents/upload/{packageId}/approve     → Manual approval
GET    /api/agents/upload/pending                 → Get pending uploads
GET    /api/agents/upload/queue                   → Get upload queue
```

## EF CORE: `DbSet<UploadPackage>` with JSONB for lists, FK to VideoPipelineJob

## ESTIMATED TIME: 4-6 hours
