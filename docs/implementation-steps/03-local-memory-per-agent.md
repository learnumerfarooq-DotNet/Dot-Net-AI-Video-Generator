# 03 — Local Memory Per Agent

## Purpose
Each agent in the pipeline carries its own **Local Memory** — a lightweight, agent-specific context that stores preferences, last run state, retry counts, style configurations, and operational parameters. Unlike Global Memory (shared), Local Memory is **private to each agent** and stored both in PostgreSQL (persistent) and as JSON files on Google Drive (backup).

## Architecture Position
```
Main Brain
    ├── Script Gen Agent ── 🗂 Local Memory (last_script_style, tone_config)
    ├── Edit Agent ──────── 🗂 Local Memory (cut_style, caption_template)
    ├── Shorts Agent ─────── 🗂 Local Memory (max_seconds, hook_style)
    ├── Short Edit Agent ─── 🗂 Local Memory (overlay_text, music_pref)
    ├── Trend Agent ──────── 🗂 Local Memory (last_trends, top_keywords)
    ├── Upload Agent ─────── 🗂 Local Memory (platform_tokens, title_template)
    └── Analytics Agent ──── 🗂 Local Memory (video_ids_tracked, last_run)
```

---

## FILE MAP

### Existing Files to MODIFY
| File | Path | Changes |
|------|------|---------|
| `MemoryEntry.cs` | `Domain/Memory/MemoryEntry.cs` | Add AgentKey field, restructure |
| `DbMemoryRepository.cs` | `Infrastructure/Memory/DbMemoryRepository.cs` | Add local memory queries |

### New Files to CREATE
| File | Path | Purpose |
|------|------|---------|
| `AgentLocalMemory.cs` | `Domain/Memory/AgentLocalMemory.cs` | Per-agent memory entity |
| `ILocalMemoryService.cs` | `Application/Memory/ILocalMemoryService.cs` | Interface |
| `LocalMemoryService.cs` | `Infrastructure/Memory/LocalMemoryService.cs` | Implementation |
| `LocalMemoryDriveSync.cs` | `Infrastructure/Memory/LocalMemoryDriveSync.cs` | Drive backup sync |
| `ScriptGenLocalMemory.cs` | `Domain/Memory/AgentMemories/ScriptGenLocalMemory.cs` | Script agent config |
| `EditAgentLocalMemory.cs` | `Domain/Memory/AgentMemories/EditAgentLocalMemory.cs` | Edit agent config |
| `ShortsAgentLocalMemory.cs` | `Domain/Memory/AgentMemories/ShortsAgentLocalMemory.cs` | Shorts config |
| `ShortEditLocalMemory.cs` | `Domain/Memory/AgentMemories/ShortEditLocalMemory.cs` | Short edit config |
| `TrendAgentLocalMemory.cs` | `Domain/Memory/AgentMemories/TrendAgentLocalMemory.cs` | Trend agent config |
| `UploadAgentLocalMemory.cs` | `Domain/Memory/AgentMemories/UploadAgentLocalMemory.cs` | Upload config |
| `AnalyticsAgentLocalMemory.cs` | `Domain/Memory/AgentMemories/AnalyticsAgentLocalMemory.cs` | Analytics config |

---

## ENTITY: AgentLocalMemory (Base)

**File**: `Domain/Memory/AgentLocalMemory.cs`
**Fields**: 12

```
AgentLocalMemory
├── Id                : Guid            (PK)
├── AgentKey          : string          (e.g. "script-gen-agent", unique per agent)
├── AgentDisplayName  : string          (e.g. "Script Gen Agent")
├── ConfigJson        : string          (JSON blob with agent-specific config)
├── LastRunAt         : DateTimeOffset?
├── LastSuccessAt     : DateTimeOffset?
├── LastErrorAt       : DateTimeOffset?
├── LastErrorMessage  : string?
├── RunCount          : long
├── SuccessCount      : long
├── FailureCount      : long
├── CreatedAt         : DateTimeOffset
├── UpdatedAt         : DateTimeOffset
```

---

## TYPED MEMORY: ScriptGenLocalMemory

**File**: `Domain/Memory/AgentMemories/ScriptGenLocalMemory.cs`
**Fields**: 10

```
ScriptGenLocalMemory
├── LastScriptStyle      : string    ("educational", "entertaining", "dramatic")
├── ToneConfig           : string    ("casual", "professional", "energetic")
├── VideoType            : string    ("long", "short")
├── PreferredLanguage    : string    ("en", "ur", etc.)
├── OutputFolder         : string    ("/RAW/scripts/")
├── MaxScriptLength      : int       (default 2000 characters)
├── IncludeCallToAction  : bool      (default true)
├── HookStylePreference  : string    ("question", "statistic", "story")
├── KeywordFocusAreas    : List<string>  (topics to focus on)
├── LastGeneratedScript  : string?   (last script content for reference)
```

---

## TYPED MEMORY: EditAgentLocalMemory

**File**: `Domain/Memory/AgentMemories/EditAgentLocalMemory.cs`
**Fields**: 12

```
EditAgentLocalMemory
├── CutStyle            : string    ("fast-cuts", "smooth", "cinematic")
├── CaptionTemplate     : string    ("default", "bold", "minimal")
├── CaptionPosition     : string    ("bottom-center", "top-left")
├── CaptionFontSize     : int       (default 36)
├── CaptionColor        : string    ("#FFFFFF")
├── InputFolder         : string    ("/RAW/")
├── OutputFolder        : string    ("/Processed/")
├── TransitionType      : string    ("fade", "cut", "dissolve")
├── AudioNormalize      : bool      (default true)
├── LastErrorId         : Guid?     
├── RetryCount          : int
├── PreferredCodec      : string    ("h264", "h265")
```

---

## TYPED MEMORY: ShortsAgentLocalMemory

**File**: `Domain/Memory/AgentMemories/ShortsAgentLocalMemory.cs`
**Fields**: 10

```
ShortsAgentLocalMemory
├── MaxSeconds          : int       (60 — from Global Memory)
├── HookStyle           : string    ("text-overlay", "zoom-in", "countdown")
├── OverlayText         : string?   (default text overlay)
├── InputFolder         : string    ("/Processed/")
├── OutputFolder        : string    ("/Shorts/raw/")
├── MaxShortsPerVideo   : int       (default 5)
├── MinSegmentDuration  : int       (default 15 seconds)
├── PreferFastParts     : bool      (true — select high-energy segments)
├── AspectRatio         : string    ("9:16")
├── OutputResolution    : string    ("1080x1920")
```

---

## TYPED MEMORY: ShortEditLocalMemory

**File**: `Domain/Memory/AgentMemories/ShortEditLocalMemory.cs`
**Fields**: 11

```
ShortEditLocalMemory
├── HookDuration        : int       (3 seconds — intro hook duration)
├── CaptionStyle        : string    ("word-by-word", "sentence", "karaoke")
├── MusicTrackPreference : string   ("trending", "calm", "energetic")
├── MusicVolume         : double    (0.3 — 30% of original)
├── OverlayEmoji        : bool      (true — add emoji overlays)
├── InputFolder         : string    ("/Shorts/raw/")
├── OutputFolder        : string    ("/Shorts/processed/")
├── AddWatermark        : bool      (false)
├── WatermarkText       : string?
├── FontFamily          : string    ("Inter", "Montserrat")
├── TransitionEffect    : string    ("glitch", "slide", "bounce")
```

---

## TYPED MEMORY: TrendAgentLocalMemory

**File**: `Domain/Memory/AgentMemories/TrendAgentLocalMemory.cs`
**Fields**: 11

```
TrendAgentLocalMemory
├── Top50Sites          : List<string>   (copied from Global Memory on init)
├── LastTrends          : List<string>   (last discovered trending topics)
├── TopKeywords         : List<string>   (best-performing keywords)
├── ScheduleSlots       : List<string>   (peak hours from Global Memory)
├── PeakHours           : List<int>      (8, 12, 18, 21)
├── OutputFolder        : string         ("/Scheduler/slots/")
├── LastScrapeAt        : DateTimeOffset?
├── ScrapeSuccessRate   : double         (% of successful scrapes)
├── PreferredNiches     : List<string>   (tech, entertainment, etc.)
├── AvoidedTopics       : List<string>   (topics to skip)
├── FallbackToOpenRouter : bool          (true)
```

---

## TYPED MEMORY: UploadAgentLocalMemory

**File**: `Domain/Memory/AgentMemories/UploadAgentLocalMemory.cs`
**Fields**: 12

```
UploadAgentLocalMemory
├── PlatformTokens       : Dictionary<string, string>  (platform → token)
├── AccountIds           : Dictionary<string, string>  (platform → account ID)
├── TitleTemplate        : string    ("{topic} | {keyword} #shorts")
├── DescriptionTemplate  : string    ("...")
├── HashtagBank          : List<string>  (reusable hashtags)
├── DefaultPrivacy       : string    ("public")
├── DefaultCategory      : string    ("22" — People & Blogs)
├── InputFolder          : string    ("/ReadyToUpload/")
├── LastUploadedIds      : List<Guid>
├── PreferredPlatforms   : List<string>  (["YouTube", "TikTok", "Instagram"])
├── AutoSchedule         : bool      (true — auto-assign to peak slots)
├── MaxUploadsPerDay     : int       (10)
```

---

## TYPED MEMORY: AnalyticsAgentLocalMemory

**File**: `Domain/Memory/AgentMemories/AnalyticsAgentLocalMemory.cs`
**Fields**: 9

```
AnalyticsAgentLocalMemory
├── VideoIdsTracked      : List<Guid>    (videos being monitored)
├── Views                : long          (aggregated views)
├── Likes                : long          (aggregated likes)
├── CTRPerVideo          : Dictionary<Guid, double>  (per-video CTR)
├── OutputFolder         : string        ("/Logs/analytics/")
├── LastRunTimestamp      : DateTimeOffset?
├── CollectionPeriodDays : int           (7 — look back N days)
├── TopPerformers        : List<Guid>    (top 10 by views)
├── AlertThresholds      : Dictionary<string, double>  (e.g. {"low_views": 100})
```

---

## INTERFACE: ILocalMemoryService

**File**: `Application/Memory/ILocalMemoryService.cs`
**Methods**: 10

```csharp
public interface ILocalMemoryService
{
    // Get local memory for a specific agent
    Task<AgentLocalMemory?> GetAsync(string agentKey, CancellationToken ct = default);

    // Get typed config from local memory
    Task<T?> GetConfigAsync<T>(string agentKey, CancellationToken ct = default) where T : class;

    // Save typed config to local memory
    Task SaveConfigAsync<T>(string agentKey, T config, CancellationToken ct = default) where T : class;

    // Update run statistics
    Task RecordRunAsync(string agentKey, bool success, string? errorMessage = null, CancellationToken ct = default);

    // Get all agent local memories
    Task<List<AgentLocalMemory>> GetAllAsync(CancellationToken ct = default);

    // Reset an agent's local memory to defaults
    Task ResetAsync(string agentKey, CancellationToken ct = default);

    // Sync local memory to Drive as backup
    Task SyncToDriveAsync(string agentKey, CancellationToken ct = default);

    // Load local memory from Drive backup
    Task LoadFromDriveAsync(string agentKey, CancellationToken ct = default);

    // Merge global memory settings into local memory
    Task MergeGlobalSettingsAsync(string agentKey, GlobalMemory globalMemory, CancellationToken ct = default);

    // Delete an agent's local memory
    Task DeleteAsync(string agentKey, CancellationToken ct = default);
}
```

---

## CLASS: LocalMemoryService

**File**: `Infrastructure/Memory/LocalMemoryService.cs`
**Methods**: 12
**Dependencies**: `StudioDbContext`, `IGoogleDriveService`, `IStudioWorkspaceStore`, `IMemoryCache`, `ILogger`

### Method 1: `GetAsync(string agentKey, CancellationToken ct)`
- Query `AgentLocalMemory` by `AgentKey` from PostgreSQL
- Return null if not found

### Method 2: `GetConfigAsync<T>(string agentKey, CancellationToken ct)`
- Get `AgentLocalMemory` → deserialize `ConfigJson` to type `T`
- Uses `System.Text.Json.JsonSerializer.Deserialize<T>()`

### Method 3: `SaveConfigAsync<T>(string agentKey, T config, CancellationToken ct)`
- Serialize `config` to JSON
- Upsert `AgentLocalMemory` with new `ConfigJson`
- Set `UpdatedAt = DateTimeOffset.UtcNow`

### Method 4: `RecordRunAsync(string agentKey, bool success, string? errorMessage, CancellationToken ct)`
- Load agent memory → increment `RunCount`
- If success: increment `SuccessCount`, set `LastSuccessAt`
- If failure: increment `FailureCount`, set `LastErrorAt`, `LastErrorMessage`
- Set `LastRunAt = DateTimeOffset.UtcNow`
- Save changes

### Method 5: `GetAllAsync(CancellationToken ct)`
- Return all `AgentLocalMemory` records from database

### Method 6: `ResetAsync(string agentKey, CancellationToken ct)`
- Delete existing record → create new with default `ConfigJson`
- Default config is determined by agent key (factory method)

### Method 7: `SyncToDriveAsync(string agentKey, CancellationToken ct)`
- Get local memory → serialize to JSON
- Upload to Drive at `/memory/local/{agentKey}.json`

### Method 8: `LoadFromDriveAsync(string agentKey, CancellationToken ct)`
- Download `/memory/local/{agentKey}.json` from Drive
- Deserialize and save to PostgreSQL

### Method 9: `MergeGlobalSettingsAsync(string agentKey, GlobalMemory globalMemory, CancellationToken ct)`
- For each agent, copy relevant global settings:
  - Shorts Agent → copy `ShortMaxDurationSeconds` to local `MaxSeconds`
  - Trend Agent → copy `TrendAgentConfig.Tier1Sites` to local `Top50Sites`
  - Upload Agent → copy `PeakUploadSlotsUtc` to local `ScheduleSlots`

### Method 10: `CreateDefaultConfig(string agentKey)` (private)
- Factory method that returns default typed config based on agent key
- Switch on agentKey → return appropriate typed memory object

### Method 11: `InitializeAllAgentMemoriesAsync(CancellationToken ct)`
- Called on startup
- For each known agent key, check if local memory exists
- If not, create default

### Method 12: `CleanupOldRunStatsAsync(int keepDays, CancellationToken ct)`
- Periodically clean up old run statistics

---

## REST API ENDPOINTS

**File**: `Api/Memory/LocalMemoryController.cs`
**Routes**: 6

```
GET    /api/memory/local                   → Get all agent local memories
GET    /api/memory/local/{agentKey}        → Get specific agent memory
PUT    /api/memory/local/{agentKey}        → Update agent memory config
POST   /api/memory/local/{agentKey}/reset  → Reset to defaults
POST   /api/memory/local/{agentKey}/sync   → Sync to Drive
GET    /api/memory/local/{agentKey}/stats  → Get run statistics
```

---

## EF CORE CONFIGURATION

```csharp
// In StudioDbContext.cs — add:
public DbSet<AgentLocalMemory> AgentLocalMemories { get; set; }

// In OnModelCreating:
modelBuilder.Entity<AgentLocalMemory>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.HasIndex(e => e.AgentKey).IsUnique();
    entity.Property(e => e.ConfigJson).HasColumnType("jsonb");
});
```

---

## SEED DATA

On startup, create default local memories for all 11 agents:
```
script-gen-agent → ScriptGenLocalMemory (defaults)
edit-agent → EditAgentLocalMemory (defaults)
shorts-agent → ShortsAgentLocalMemory (defaults)
short-edit-agent → ShortEditLocalMemory (defaults)
trend-agent → TrendAgentLocalMemory (defaults)
upload-agent → UploadAgentLocalMemory (defaults)
analytics-agent → AnalyticsAgentLocalMemory (defaults)
youtube-agent → UploadAgentLocalMemory (YouTube-specific)
tiktok-agent → UploadAgentLocalMemory (TikTok-specific)
instagram-agent → UploadAgentLocalMemory (Instagram-specific)
facebook-agent → UploadAgentLocalMemory (Facebook-specific)
```

---

## ANGULAR INTEGRATION

### Memory Local Component
- Table showing all agents with their local memory status
- Click agent → expand to show typed config fields
- Edit button → inline editing of config JSON
- "Reset to Defaults" button per agent
- "Sync to Drive" button per agent
- Run statistics: total runs, success rate, last run time

### SignalR Events
| Event | Payload | When |
|-------|---------|------|
| `OnLocalMemoryUpdated` | `{ agentKey, configJson }` | When any agent memory changes |
| `OnAgentRunRecorded` | `{ agentKey, success, timestamp }` | When a run is recorded |

---

## TESTING PLAN
1. **Unit Test**: `GetConfigAsync<T>()` — verify deserialization
2. **Unit Test**: `SaveConfigAsync<T>()` — verify serialization + upsert
3. **Unit Test**: `RecordRunAsync()` — verify counter increments
4. **Unit Test**: `MergeGlobalSettingsAsync()` — verify correct fields copied
5. **Integration Test**: Full CRUD cycle with PostgreSQL
6. **Manual Test**: Edit local memory in Angular → verify Drive backup

## ESTIMATED IMPLEMENTATION TIME
- Backend entities: 2 hours
- Service implementation: 3 hours
- API + Angular: 2 hours
- Testing: 1-2 hours
