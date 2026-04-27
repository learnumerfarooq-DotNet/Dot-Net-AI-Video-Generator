# 06 — Shorts Agent Pipeline

## Purpose
The Shorts Agent takes edited long-form videos from `/Processed/` and extracts up to 5 short-form clips (≤60 seconds, 9:16 aspect ratio) suitable for YouTube Shorts, TikTok, and Instagram Reels. It uses AI to identify the most engaging segments, then FFmpeg to clip, resize, and format the shorts. Output goes to `/Shorts/raw/`.

## Architecture Position
```
/Processed/ ──→ Shorts Agent ──→ /Shorts/raw/
                    │
            ┌───────┴───────┐
            │               │
    ShortDurationEnforcer  AspectRatioConverter
            │               │
       FFmpeg (trim)    FFmpeg (9:16 crop/pad)
```

---

## FILE MAP

### Existing Files to MODIFY
| File | Path | Changes |
|------|------|---------|
| `ShortExecutionAgent.cs` | `Infrastructure/Shorts/ShortExecutionAgent.cs` | Enhance — 95 → ~250 lines |
| `ShortDecisionAgent.cs` | `Infrastructure/Shorts/ShortDecisionAgent.cs` | Add prompt template |
| `AspectRatioConverter.cs` | `Infrastructure/Shorts/AspectRatioConverter.cs` | Add padding modes |
| `ShortDurationEnforcer.cs` | `Infrastructure/Shorts/ShortDurationEnforcer.cs` | Add validation |

### New Files to CREATE
| File | Path | Purpose |
|------|------|---------|
| `ShortClip.cs` | `Domain/Agents/ShortClip.cs` | Short clip entity |
| `IShortsAgent.cs` | `Application/Agents/IShortsAgent.cs` | Interface |
| `ShortPrompts.cs` | `Infrastructure/Shorts/ShortPrompts.cs` | Prompt templates |
| `SegmentScorer.cs` | `Infrastructure/Shorts/SegmentScorer.cs` | AI segment scoring |

---

## ENTITY: ShortClip

**File**: `Domain/Agents/ShortClip.cs`
**Fields**: 20

```
ShortClip
├── Id                  : Guid
├── JobId               : Guid            (FK → VideoPipelineJob)
├── ParentVideoFileId   : string          (Drive ID of source video)
├── ClipNumber          : int             (1-5)
├── Title               : string          (AI-generated short title)
├── Hook                : string          (opening hook for first 3 sec)
├── Rationale           : string          (why this segment was chosen)
├── StartTime           : double          (start in source video, seconds)
├── EndTime             : double          (end in source video, seconds)
├── Duration            : double          (actual clip duration)
├── AspectRatio         : string          ("9:16")
├── Width               : int             (1080)
├── Height              : int             (1920)
├── FileSizeBytes       : long
├── OutputFileName      : string
├── DriveFileId         : string?         (Drive ID in /Shorts/raw/)
├── EngagementScore     : double          (AI-predicted engagement 0-1)
├── Status              : ShortClipStatus (Planned/Processing/Ready/Failed)
├── ProcessedAt         : DateTimeOffset?
├── CreatedAt           : DateTimeOffset
```

**Enum**: `ShortClipStatus` (4 values)
```
Planned = 0, Processing = 1, Ready = 2, Failed = 3
```

---

## INTERFACE: IShortsAgent

**File**: `Application/Agents/IShortsAgent.cs`
**Methods**: 6

```csharp
public interface IShortsAgent
{
    // Identify best segments and create short clips
    Task<List<ShortClip>> GenerateShortsAsync(Guid jobId, CancellationToken ct = default);

    // Process a single short clip (trim + resize)
    Task ProcessShortClipAsync(ShortClip clip, string sourcePath, CancellationToken ct = default);

    // Get all shorts for a job
    Task<List<ShortClip>> GetShortsAsync(Guid jobId, CancellationToken ct = default);

    // Regenerate shorts with different parameters
    Task<List<ShortClip>> RegenerateShortsAsync(Guid jobId, int maxShorts, int minDuration, CancellationToken ct = default);

    // Validate short meets platform constraints
    Task<bool> ValidateShortAsync(ShortClip clip, CancellationToken ct = default);

    // Score a segment for engagement potential
    Task<double> ScoreSegmentAsync(double startTime, double endTime, VideoAnalysisResult analysis, CancellationToken ct = default);
}
```

---

## CLASS: ShortExecutionAgent (Enhanced)

**Methods**: 10

### Method 1: `GenerateShortsAsync(Guid jobId, CancellationToken ct)`
- **Logic**:
  1. Load pipeline job and script output
  2. Load local memory: `ShortsAgentLocalMemory`
  3. Load global constraints: `VideoConstraints.ShortMaxDurationSeconds` (60)
  4. Download edited video from `/Processed/` to temp
  5. Analyze video: scene changes, audio peaks, visual energy
  6. Build context for AI: `{ duration, sceneChanges, audioHotspots, script }`
  7. Call Decision Engine: `MakeDecisionAsync("shorts-agent", DecisionType.ShortGeneration, context)`
  8. Parse `ShortDecisionPayload` from decision
  9. Validate each segment: `startTime < endTime`, `duration ≤ 60`, `duration ≥ 15`
  10. Create `ShortClip` entities (max 5)
  11. Save to database
  12. Process each clip: `ProcessShortClipAsync()`
  13. Upload all clips to `/Shorts/raw/` on Drive
  14. Transition pipeline: `TransitionStageAsync(jobId, ShortGeneration)`

### Method 2: `ProcessShortClipAsync(ShortClip clip, string sourcePath, CancellationToken ct)`
- **Logic**:
  1. Generate temp paths: `trim_{clipNumber}.mp4`, `final_{clipNumber}.mp4`
  2. Enforce duration: `ShortDurationEnforcer.TrimToDuration(source, trimPath, start, end)`
     - If duration > 60: trim to 60 seconds from start
     - If duration < 15: extend or skip
  3. Convert aspect ratio: `AspectRatioConverter.ConvertTo916(trimPath, finalPath)`
     - Crop from 16:9 center, or pad with blur background
  4. Validate output: check file exists, check duration ≤ 60, check resolution 1080x1920
  5. Update clip with file size, actual duration
  6. Set `Status = Ready`

### Method 3: `ScoreSegmentAsync()`
- Score based on: audio energy, visual movement, position in video (hooks at start are better)

---

## CLASS: AspectRatioConverter (Enhanced)

**Methods**: 4

### Method 1: `ConvertTo916(string input, string output, CancellationToken ct)`
- FFmpeg command: crop center from 16:9 → 9:16
- Command: `ffmpeg -i {input} -vf "crop=ih*9/16:ih,scale=1080:1920" -c:a copy {output}`

### Method 2: `ConvertTo916WithBlurBackground(string input, string output, CancellationToken ct)`
- Keep original video centered, add blurred version as background
- Complex filter: scale original to fit, scale+blur for background, overlay

### Method 3: `ConvertTo916WithPadding(string input, string output, string padColor, CancellationToken ct)`
- Pad with solid color bars

### Method 4: `GetSourceAspectRatio(string input, CancellationToken ct)` → determines conversion needed

---

## CLASS: ShortDurationEnforcer (Enhanced)

**Methods**: 4

### Method 1: `TrimToDuration(string input, string output, double start, double end, CancellationToken ct)`
- Trim video to exact segment
- Enforce max 60 seconds
- FFmpeg: `ffmpeg -i {input} -ss {start} -to {end} -c:v copy -c:a copy {output}`

### Method 2: `ValidateDuration(string filePath, CancellationToken ct)` → returns actual duration
### Method 3: `EnforceDurationLimit(string input, string output, int maxSeconds, CancellationToken ct)`
### Method 4: `SplitIfTooLong(string input, int maxSeconds, CancellationToken ct)` → splits into multiple clips

---

## PROMPT TEMPLATE FOR SHORT IDENTIFICATION

### System Prompt
```
You are an expert at identifying viral-worthy moments in videos.
Given video metadata and a script, identify the top 5 most engaging segments
that would work as standalone short-form videos (≤60 seconds each).
Focus on: hooks, surprising moments, emotional peaks, and key takeaways.
```

### User Prompt Template
```
Analyze this video and identify the best short clips:

Video Duration: {duration} seconds
Scene Changes at: {sceneChanges}
Audio Peaks at: {audioPeaks}
Script Summary: {scriptSummary}

Requirements:
- Each clip must be 15-60 seconds
- Each clip needs a strong hook in the first 3 seconds
- Clips should be self-contained (make sense without context)
- Maximum 5 clips
- Include engagement score prediction (0.0-1.0)

Output JSON format:
{jsonSchema}
```

### JSON Schema
```json
{
    "parentVideoId": "string",
    "shorts": [
        {
            "startTime": 0.0,
            "endTime": 45.0,
            "title": "string",
            "hook": "string",
            "rationale": "string",
            "engagementScore": 0.85
        }
    ]
}
```

---

## VIDEO CONSTRAINTS (from Global Memory)

| Constraint | Value | Source |
|------------|-------|--------|
| Max Duration | 60 seconds | `VideoConstraints.ShortMaxDurationSeconds` |
| Aspect Ratio | 9:16 | `VideoConstraints.ShortAspectRatio` |
| Width | 1080 | `VideoConstraints.ShortWidth` |
| Height | 1920 | `VideoConstraints.ShortHeight` |
| Max File Size | 100 MB | `VideoConstraints.ShortMaxFileMb` |
| Format | MP4 | `VideoConstraints.ShortFormat` |
| FPS | 30 | `VideoConstraints.ShortFps` |

---

## EF CORE CONFIGURATION

```csharp
public DbSet<ShortClip> ShortClips { get; set; }

modelBuilder.Entity<ShortClip>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.HasIndex(e => e.JobId);
    entity.HasIndex(e => e.Status);
});
```

---

## REST API ENDPOINTS

```
POST   /api/agents/shorts/generate/{jobId}          → Generate shorts
GET    /api/agents/shorts/{jobId}                    → Get all shorts for job
POST   /api/agents/shorts/regenerate/{jobId}         → Regenerate
GET    /api/agents/shorts/clip/{clipId}              → Get specific clip details
POST   /api/agents/shorts/clip/{clipId}/reprocess    → Reprocess a clip
```

---

## SIGNALR EVENTS
| Event | Payload |
|-------|---------|
| `OnShortsIdentified` | `{ jobId, clipCount, totalDuration }` |
| `OnShortClipProcessed` | `{ jobId, clipId, clipNumber, status }` |
| `OnShortsComplete` | `{ jobId, readyCount, failedCount }` |

---

## OPENROUTER MODEL
| Setting | Value |
|---------|-------|
| Model | `meta-llama/llama-3.2-90b-vision-instruct:free` |
| Temperature | 0.6 |
| Max Tokens | 2000 |

---

## DI REGISTRATION
```csharp
services.AddScoped<IShortsAgent, ShortExecutionAgent>();
services.AddScoped<SegmentScorer>();
```

---

## ANGULAR INTEGRATION
- Shorts Agent 1 workspace: shows clip timeline, preview thumbnails
- Each clip card: title, duration, engagement score, status badge
- "Regenerate" button with parameters (max clips, min duration)
- Real-time clip processing progress via SignalR

## TESTING PLAN
1. Unit: `GenerateShortsAsync()` with mocked Decision Engine
2. Unit: `AspectRatioConverter.ConvertTo916()` with test video
3. Unit: `ShortDurationEnforcer.TrimToDuration()` validates ≤60s
4. Integration: Full shorts pipeline with sample video
5. Manual: Upload video → verify shorts in /Shorts/raw/

## ESTIMATED TIME: 5-7 hours
