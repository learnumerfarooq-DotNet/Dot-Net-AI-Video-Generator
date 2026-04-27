# 02 — Global Memory System

## Purpose
Global Memory is the **shared knowledge base** across all agents. It is stored as `/memory/global.json` on Google Drive and loaded by the Main Brain on every tick. It contains the folder registry, trend agent configuration, video constraints, peak upload slots, and agent status tracking.

## Architecture Position
```
Main Brain ──reads──→ /memory/global.json (Google Drive)
    ↓
    ├── Script Gen Agent (reads folder paths)
    ├── Edit Agent (reads video constraints)
    ├── Shorts Agent (reads short max duration)
    ├── Trend Agent (reads tier sites list)
    ├── Upload Agent (reads peak upload slots)
    └── Analytics Agent (reads + writes feedback)
```

---

## FILE MAP

### Existing Files to MODIFY
| File | Path | Changes Needed |
|------|------|----------------|
| `GlobalMemory.cs` | `Domain/GlobalMemory/GlobalMemory.cs` | Add 12 new fields |
| `DependencyInjection.cs` | `Infrastructure/DependencyInjection.cs` | Register new services |

### New Files to CREATE
| File | Path | Purpose |
|------|------|---------|
| `IGlobalMemoryService.cs` | `Application/Memory/IGlobalMemoryService.cs` | Interface contract |
| `GlobalMemoryService.cs` | `Infrastructure/Memory/GlobalMemoryService.cs` | Drive-backed implementation |
| `GlobalMemorySyncJob.cs` | `Infrastructure/Memory/GlobalMemorySyncJob.cs` | Hangfire sync job |
| `AgentStatusEntry.cs` | `Domain/GlobalMemory/AgentStatusEntry.cs` | Agent status tracking |
| `ScheduleSlot.cs` | `Domain/GlobalMemory/ScheduleSlot.cs` | Upload schedule slot |
| `GlobalMemoryController.cs` | `Api/Memory/GlobalMemoryController.cs` | REST API endpoints |

---

## ENTITY: GlobalMemory (ENHANCED)

**File**: `Domain/GlobalMemory/GlobalMemory.cs`
**Total Fields After Enhancement**: 24 (was 6)

```
GlobalMemory
├── FolderRegistry          : FolderRegistry          (existing — 10 agent-folder mappings)
├── TrendAgentConfig        : TrendAgentConfig         (existing — 5 fields)
├── VideoConstraints        : VideoConstraints          (existing — 12 fields)
├── PeakUploadSlotsUtc      : List<string>             (existing)
├── LastUpdated             : DateTimeOffset            (existing)
├── Version                 : string                   (existing)
│
├── ── NEW FIELDS ──────────────────────────────────────
├── AgentStatuses           : Dictionary<string, AgentStatusEntry>  (NEW — per-agent health)
├── ActivePipelineCount     : int                      (NEW — currently active pipelines)
├── TotalProcessedCount     : long                     (NEW — lifetime processed videos)
├── LastSuccessfulUpload    : DateTimeOffset?           (NEW)
├── LastFailedUpload        : DateTimeOffset?           (NEW)
├── ScheduleSlots           : List<ScheduleSlot>       (NEW — pre-computed upload slots)
├── AnalyticsSummary        : AnalyticsSummary?         (NEW — latest analytics summary)
├── ErrorSummary            : ErrorSummary?             (NEW — recent error counts)
├── SystemHealth            : SystemHealthStatus        (NEW — overall system health)
├── ContentStrategy         : ContentStrategy?          (NEW — AI-generated content strategy)
├── NotificationPreferences : NotificationPreferences?  (NEW)
├── LastBrainTickAt         : DateTimeOffset?           (NEW — when brain last read this)
```

---

## ENTITY: AgentStatusEntry

**File**: `Domain/GlobalMemory/AgentStatusEntry.cs`
**Fields**: 11

```
AgentStatusEntry
├── AgentKey           : string           (e.g. "script-gen-agent")
├── DisplayName        : string           (e.g. "Script Gen Agent")
├── Status             : AgentHealthStatus (Healthy/Degraded/Failed/Disabled)
├── LastRunAt          : DateTimeOffset?
├── LastSuccessAt      : DateTimeOffset?
├── LastErrorAt        : DateTimeOffset?
├── LastErrorMessage   : string?
├── TotalRuns          : long
├── TotalSuccesses     : long
├── TotalFailures      : long
├── AverageRunDurationMs : double
```

---

## ENTITY: ScheduleSlot

**File**: `Domain/GlobalMemory/ScheduleSlot.cs`
**Fields**: 9

```
ScheduleSlot
├── Id                : Guid
├── SlotTime          : DateTimeOffset     (when to upload)
├── Platform          : string             (YouTube/TikTok/Instagram etc.)
├── ContentType       : string             (Long/Short)
├── AssignedJobId     : Guid?              (pipeline job assigned to this slot)
├── Status            : SlotStatus         (Open/Assigned/Completed/Missed)
├── Keywords          : List<string>       (trending keywords for this slot)
├── Priority          : int                (1-10, higher = more important)
├── CreatedAt         : DateTimeOffset
```

**Enum**: `SlotStatus` (4 values)
```
Open = 0
Assigned = 1
Completed = 2
Missed = 3
```

---

## ENTITY: AnalyticsSummary

**File**: `Domain/GlobalMemory/AnalyticsSummary.cs`
**Fields**: 12

```
AnalyticsSummary
├── TotalViews           : long
├── TotalLikes           : long
├── TotalComments        : long
├── TotalShares          : long
├── AverageCTR           : double
├── AverageWatchTime     : double
├── AverageEngagement    : double
├── TopPerformingVideoId : Guid?
├── TopPlatform          : string
├── WeeklyGrowthPercent  : double
├── BestUploadHour       : int    (0-23, UTC)
├── GeneratedAt          : DateTimeOffset
```

---

## ENTITY: ErrorSummary

**File**: `Domain/GlobalMemory/ErrorSummary.cs`
**Fields**: 8

```
ErrorSummary
├── TotalErrorsLast24h   : int
├── TotalErrorsLast7d    : int
├── MostCommonError      : string
├── MostFailedAgent      : string
├── RetryQueueCount      : int
├── DeadLetterCount      : int
├── CircuitBreakerStatus : string  (Open/Closed/HalfOpen)
├── LastUpdated          : DateTimeOffset
```

---

## ENTITY: ContentStrategy

**File**: `Domain/GlobalMemory/ContentStrategy.cs`
**Fields**: 8

```
ContentStrategy
├── FocusTopics          : List<string>     (AI-suggested topics)
├── AvoidTopics          : List<string>     (topics to avoid)
├── PreferredPlatforms   : List<string>     (prioritized platforms)
├── ContentMixRatio      : Dictionary<string, double>  (e.g. {"shorts": 0.6, "long": 0.4})
├── TonePreference       : string           (e.g. "educational", "entertaining")
├── TargetAudience       : string           
├── PostingFrequencyPerDay : int
├── GeneratedAt          : DateTimeOffset
```

---

## ENTITY: NotificationPreferences

**File**: `Domain/GlobalMemory/NotificationPreferences.cs`
**Fields**: 6

```
NotificationPreferences
├── NotifyOnJobComplete   : bool
├── NotifyOnJobFailed     : bool
├── NotifyOnTrendDiscovered : bool
├── NotifyOnCircuitBreaker  : bool
├── NotifyOnAnalyticsReady  : bool
├── WebhookUrl              : string?
```

---

## INTERFACE: IGlobalMemoryService

**File**: `Application/Memory/IGlobalMemoryService.cs`
**Methods**: 10

```csharp
public interface IGlobalMemoryService
{
    // Read global.json from Google Drive
    Task<GlobalMemory> LoadAsync(CancellationToken ct = default);

    // Write updated global.json back to Google Drive
    Task SaveAsync(GlobalMemory memory, CancellationToken ct = default);

    // Get specific section of global memory
    Task<FolderRegistry> GetFolderRegistryAsync(CancellationToken ct = default);
    Task<TrendAgentConfig> GetTrendConfigAsync(CancellationToken ct = default);
    Task<VideoConstraints> GetVideoConstraintsAsync(CancellationToken ct = default);

    // Update specific sections
    Task UpdateAgentStatusAsync(string agentKey, AgentStatusEntry status, CancellationToken ct = default);
    Task UpdateScheduleSlotsAsync(List<ScheduleSlot> slots, CancellationToken ct = default);
    Task UpdateAnalyticsSummaryAsync(AnalyticsSummary summary, CancellationToken ct = default);
    Task UpdateErrorSummaryAsync(ErrorSummary summary, CancellationToken ct = default);

    // Force refresh from Drive
    Task<GlobalMemory> ForceRefreshAsync(CancellationToken ct = default);
}
```

---

## CLASS: GlobalMemoryService

**File**: `Infrastructure/Memory/GlobalMemoryService.cs`
**Methods**: 12
**Dependencies**: `IGoogleDriveService`, `IStudioWorkspaceStore`, `IMemoryCache`, `ILogger`

### Method 1: `LoadAsync(CancellationToken ct)`
- **Purpose**: Download and deserialize `/memory/global.json` from Drive
- **Logic**:
  1. Check `IMemoryCache` for cached copy (TTL: 30 seconds)
  2. If cache miss, use `IGoogleDriveService.DownloadFileAsync()` 
  3. Deserialize JSON to `GlobalMemory` object
  4. Cache the result
  5. Return `GlobalMemory`
  6. If file not found on Drive, create default and upload

### Method 2: `SaveAsync(GlobalMemory memory, CancellationToken ct)`
- **Purpose**: Serialize and upload `GlobalMemory` back to Drive
- **Logic**:
  1. Set `memory.LastUpdated = DateTimeOffset.UtcNow`
  2. Increment `memory.Version`
  3. Serialize to JSON with indented formatting
  4. Use `IGoogleDriveService.UploadFileAsync()` to overwrite `/memory/global.json`
  5. Invalidate memory cache
  6. Log save event

### Method 3: `GetFolderRegistryAsync(CancellationToken ct)`
- **Purpose**: Get just the folder mappings
- **Logic**: Call `LoadAsync()` → return `memory.FolderRegistry`

### Method 4: `GetTrendConfigAsync(CancellationToken ct)`
- **Purpose**: Get trend agent config (tier sites, fallback settings)
- **Logic**: Call `LoadAsync()` → return `memory.TrendAgentConfig`

### Method 5: `GetVideoConstraintsAsync(CancellationToken ct)`
- **Purpose**: Get video constraints (short/long format rules)
- **Logic**: Call `LoadAsync()` → return `memory.VideoConstraints`

### Method 6: `UpdateAgentStatusAsync(string agentKey, AgentStatusEntry status, CancellationToken ct)`
- **Purpose**: Update a single agent's health status in global memory
- **Logic**:
  1. Load current global memory
  2. Set `memory.AgentStatuses[agentKey] = status`
  3. Save back to Drive

### Method 7: `UpdateScheduleSlotsAsync(List<ScheduleSlot> slots, CancellationToken ct)`
- **Purpose**: Replace schedule slots (called by Trend Agent)
- **Logic**:
  1. Load current global memory
  2. Set `memory.ScheduleSlots = slots`
  3. Save back to Drive

### Method 8: `UpdateAnalyticsSummaryAsync(AnalyticsSummary summary, CancellationToken ct)`
- **Purpose**: Update analytics summary (called by Analytics Agent)
- **Logic**: Load → update → save

### Method 9: `UpdateErrorSummaryAsync(ErrorSummary summary, CancellationToken ct)`
- **Purpose**: Update error counts (called by Error handling)
- **Logic**: Load → update → save

### Method 10: `ForceRefreshAsync(CancellationToken ct)`
- **Purpose**: Bypass cache and re-read from Drive
- **Logic**: Invalidate cache → call `LoadAsync()`

### Method 11: `CreateDefaultAsync(CancellationToken ct)` (private)
- **Purpose**: Create a default global.json if none exists on Drive
- **Logic**: Create `GlobalMemory` with all defaults → serialize → upload

### Method 12: `ValidateMemory(GlobalMemory memory)` (private)
- **Purpose**: Validate that global memory has all required fields
- **Logic**: Check FolderRegistry has all 10 mappings, VideoConstraints has valid values, etc.

---

## REST API ENDPOINTS

**File**: `Api/Memory/GlobalMemoryController.cs`
**Routes**: 6

```
GET    /api/memory/global                → Load entire global memory
PUT    /api/memory/global                → Save entire global memory
GET    /api/memory/global/folders        → Get folder registry
GET    /api/memory/global/constraints    → Get video constraints
GET    /api/memory/global/trends-config  → Get trend agent config
POST   /api/memory/global/refresh        → Force refresh from Drive
```

---

## ANGULAR INTEGRATION

### Models (add to `content-factory.models.ts`)
```typescript
export type GlobalMemoryFull = {
    folderRegistry: { agentFolders: Record<string, string> };
    trendAgentConfig: {
        tier1Sites: string[];
        tier2Sites: string[];
        tier3Sites: string[];
        useOpenRouterFallback: boolean;
        maxSitesToCheck: number;
    };
    videoConstraints: {
        shortMaxDurationSeconds: number;
        shortAspectRatio: string;
        shortWidth: number;
        shortHeight: number;
        longMaxDurationSeconds: number;
        longAspectRatio: string;
        longWidth: number;
        longHeight: number;
    };
    peakUploadSlotsUtc: string[];
    agentStatuses: Record<string, AgentStatusEntry>;
    activeePipelineCount: number;
    scheduleSlots: ScheduleSlot[];
    analyticsSummary: AnalyticsSummary | null;
    errorSummary: ErrorSummary | null;
    lastUpdated: string;
    version: string;
};
```

### Service (add to `api.service.ts`)
```typescript
getGlobalMemory(): Observable<GlobalMemoryFull> {
    return this.http.get<GlobalMemoryFull>(`${API_BASE}/api/memory/global`);
}

saveGlobalMemory(memory: GlobalMemoryFull): Observable<void> {
    return this.http.put<void>(`${API_BASE}/api/memory/global`, memory);
}

refreshGlobalMemory(): Observable<GlobalMemoryFull> {
    return this.http.post<GlobalMemoryFull>(`${API_BASE}/api/memory/global/refresh`, {});
}
```

### Memory Global Component Enhancement
- Show folder registry as a table with agent name → Drive path
- Show video constraints for shorts and long-form
- Show trend agent tier sites
- Show agent health statuses with colored indicators
- Show schedule slots timeline
- Add "Force Refresh" button
- Add "Edit" mode for modifying global memory

---

## DRIVE FOLDER STRUCTURE

```
Google Drive Root/
├── memory/
│   └── global.json          ← Global Memory file
├── RAW/
│   └── scripts/             ← Script Gen Agent output
├── Processed/               ← Edit Agent output
├── Shorts/
│   ├── raw/                 ← Shorts Agent output
│   └── processed/           ← Short Edit Agent output
├── Scheduler/
│   ├── main/                ← Long-form schedule JSON
│   └── shorts/              ← Short-form schedule JSON
├── ReadyToUpload/           ← Upload Agent staging
├── Errors/
│   └── retry/               ← Error queue
└── Logs/
    └── analytics/           ← Analytics Agent reports
```

---

## CACHING STRATEGY

| Cache Key | TTL | Invalidation |
|-----------|-----|-------------|
| `global-memory:current` | 30 seconds | On save, on force-refresh |
| `global-memory:folders` | 5 minutes | On folder registry change |
| `global-memory:constraints` | 10 minutes | On constraints change |

---

## DEFAULT global.json CONTENT

```json
{
    "folderRegistry": {
        "agentFolders": {
            "script-gen-agent": "/RAW/scripts/",
            "edit-agent": "/Processed/",
            "shorts-agent": "/Shorts/raw/",
            "short-edit-agent": "/Shorts/processed/",
            "trend-agent": "/Scheduler/slots/",
            "upload-agent": "/ReadyToUpload/",
            "analytics-agent": "/Logs/analytics/",
            "error-queue": "/Errors/retry/",
            "raw-videos": "/RAW/",
            "memory": "/memory/"
        }
    },
    "trendAgentConfig": {
        "tier1Sites": ["youtube.com", "tiktok.com", "trends.google.com", "reddit.com", "twitter.com", "x.com", "instagram.com"],
        "tier2Sites": ["bbc.com", "cnn.com", "reuters.com", "techcrunch.com", "theverge.com", "wired.com"],
        "tier3Sites": ["buzzfeed.com", "mashable.com", "medium.com", "dev.to", "hackernoon.com", "producthunt.com"],
        "useOpenRouterFallback": true,
        "maxSitesToCheck": 50
    },
    "videoConstraints": {
        "shortMaxDurationSeconds": 60,
        "shortAspectRatio": "9:16",
        "shortWidth": 1080,
        "shortHeight": 1920,
        "shortMaxFileMb": 100,
        "shortFormat": "mp4",
        "shortFps": 30,
        "longMaxDurationSeconds": 3600,
        "longAspectRatio": "16:9",
        "longWidth": 1920,
        "longHeight": 1080,
        "longFps": 30,
        "longFormat": "mp4"
    },
    "peakUploadSlotsUtc": ["08:00", "12:00", "18:00", "21:00"],
    "agentStatuses": {},
    "activePipelineCount": 0,
    "totalProcessedCount": 0,
    "scheduleSlots": [],
    "version": "1.0",
    "lastUpdated": "2026-01-01T00:00:00Z"
}
```

---

## DI REGISTRATION

```csharp
services.AddScoped<IGlobalMemoryService, GlobalMemoryService>();
services.AddScoped<GlobalMemorySyncJob>();
```

---

## TESTING PLAN

1. **Unit Test**: `GlobalMemoryService.LoadAsync()` with mocked Drive service
2. **Unit Test**: `GlobalMemoryService.SaveAsync()` verifies serialization format
3. **Unit Test**: `UpdateAgentStatusAsync()` merges correctly
4. **Integration Test**: Full load-modify-save-load cycle with real Drive
5. **Manual Test**: Edit global.json in Drive → verify Angular updates

---

## DEPENDENCIES
- Google.Apis.Drive.v3 (existing)
- Microsoft.Extensions.Caching.Memory (existing)
- System.Text.Json

## ESTIMATED IMPLEMENTATION TIME
- Backend: 3-4 hours
- Frontend: 2-3 hours
- Testing: 1-2 hours
