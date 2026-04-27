# 05 — Edit Agent + FFmpeg Processing Engine

## Purpose
The Edit Agent takes a RAW video with its generated script and produces a polished, edited video. It uses FFmpeg for all video processing (trim, caption, audio sync, transitions, color grading) guided by AI decisions from the Decision Engine. Output goes to `/Processed/` on Google Drive.

## Architecture Position
```
Script Gen Agent → [ScriptGenerated] → Edit Agent → [Edited] → /Processed/
                                            │
                                    ┌───────┴───────┐
                                    │               │
                              FFmpeg Service    Decision Engine
                                    │               │
                           Local Processing    AI Edit Plan
```

---

## FILE MAP

### Existing Files to MODIFY
| File | Path | Changes |
|------|------|---------|
| `EditExecutionAgent.cs` | `Infrastructure/Agents/EditExecutionAgent.cs` | Complete rewrite — 76 → ~280 lines |
| `IFFmpegService.cs` | `Application/Processing/IFFmpegService.cs` | Add 6 new methods |
| `FFmpegService.cs` | `Infrastructure/Processing/FFmpegService.cs` | Implement new methods |
| `ProcessingContracts.cs` | `Application/Processing/ProcessingContracts.cs` | Add new command types |

### New Files to CREATE
| File | Path | Purpose |
|------|------|---------|
| `EditPlan.cs` | `Domain/Agents/EditPlan.cs` | Edit plan entity |
| `IEditAgent.cs` | `Application/Agents/IEditAgent.cs` | Interface |
| `EditPrompts.cs` | `Infrastructure/Agents/EditPrompts.cs` | Prompt templates |
| `FFmpegCommandBuilder.cs` | `Infrastructure/Processing/FFmpegCommandBuilder.cs` | Fluent command builder |
| `VideoAnalysisResult.cs` | `Domain/Processing/VideoAnalysisResult.cs` | Pre-edit analysis |

---

## ENTITY: EditPlan

**File**: `Domain/Agents/EditPlan.cs`
**Fields**: 18

```
EditPlan
├── Id                  : Guid
├── JobId               : Guid            (FK → VideoPipelineJob)
├── ScriptId            : Guid            (FK → ScriptOutput)
├── Segments            : List<EditSegment>   (trim/cut plan)
├── Captions            : List<EditCaption>   (caption overlays)
├── AudioAdjustments    : List<AudioAdjustment> (volume, normalization)
├── Transitions         : List<TransitionPlan>  (between segments)
├── ColorGrading        : ColorGradingConfig?   (brightness, contrast, saturation)
├── OutputFormat        : string          ("mp4")
├── OutputCodec         : string          ("h264" or "h265")
├── OutputResolution    : string          ("1920x1080")
├── OutputFps           : int             (30)
├── EstimatedOutputSize : long            (bytes)
├── FFmpegCommands      : List<string>    (generated FFmpeg commands)
├── Status              : EditPlanStatus  (Planned/Executing/Completed/Failed)
├── InputDriveFileId    : string
├── OutputDriveFileId   : string?
├── CreatedAt           : DateTimeOffset
```

---

## SUB-ENTITY: EditSegment

**Fields**: 7
```
EditSegment
├── Order       : int
├── StartTime   : double   (seconds)
├── EndTime     : double   (seconds)
├── Description : string
├── Speed       : double   (1.0 = normal, 0.5 = slow-mo, 2.0 = fast)
├── Transition  : string?  ("fade", "cut", "dissolve")
├── KeepAudio   : bool     (true)
```

## SUB-ENTITY: EditCaption

**Fields**: 8
```
EditCaption
├── StartTime   : double
├── EndTime     : double
├── Text        : string
├── Style       : string    ("default", "bold", "minimal")
├── Position    : string    ("bottom-center", "top-left", "center")
├── FontSize    : int       (36)
├── Color       : string    ("#FFFFFF")
├── Background  : string?   ("#000000AA")
```

## SUB-ENTITY: AudioAdjustment

**Fields**: 5
```
AudioAdjustment
├── StartTime     : double
├── EndTime       : double
├── VolumeMultiplier : double  (1.0 = normal)
├── Normalize     : bool
├── FadeIn        : double?   (seconds)
```

## SUB-ENTITY: TransitionPlan

**Fields**: 4
```
TransitionPlan
├── AtTime        : double   (transition point in seconds)
├── Type          : string   ("fade", "dissolve", "wipe", "cut")
├── DurationMs    : int      (transition duration)
├── Direction     : string?  ("left", "right", "up", "down")
```

## SUB-ENTITY: ColorGradingConfig

**Fields**: 6
```
ColorGradingConfig
├── Brightness    : double   (0.0-2.0, default 1.0)
├── Contrast      : double   (0.0-2.0, default 1.0)
├── Saturation    : double   (0.0-2.0, default 1.0)
├── Gamma         : double   (0.0-2.0, default 1.0)
├── Temperature   : int      (Kelvin, 3000-8000, default 5500)
├── LookupTable   : string?  (path to LUT file)
```

---

## INTERFACE: IEditAgent

**File**: `Application/Agents/IEditAgent.cs`
**Methods**: 5

```csharp
public interface IEditAgent
{
    Task<EditPlan> CreateEditPlanAsync(Guid jobId, CancellationToken ct = default);
    Task ExecuteEditPlanAsync(Guid jobId, EditPlan plan, CancellationToken ct = default);
    Task<EditPlan?> GetEditPlanAsync(Guid jobId, CancellationToken ct = default);
    Task ReExecuteAsync(Guid jobId, CancellationToken ct = default);
    Task<VideoAnalysisResult> AnalyzeVideoAsync(string filePath, CancellationToken ct = default);
}
```

---

## ENHANCED IFFmpegService

**File**: `Application/Processing/IFFmpegService.cs`
**Methods**: 10 (was 4)

```csharp
public interface IFFmpegService
{
    // Existing
    Task<FFmpegResult> ExecuteAsync(FFmpegCommand command, CancellationToken ct = default);
    Task<VideoMetadata> ExtractMetadataAsync(string filePath, CancellationToken ct = default);

    // NEW: Segment operations
    Task<FFmpegResult> TrimAsync(string input, string output, double startSec, double endSec, CancellationToken ct = default);
    Task<FFmpegResult> ConcatAsync(List<string> inputs, string output, CancellationToken ct = default);

    // NEW: Caption operations
    Task<FFmpegResult> AddCaptionsAsync(string input, string output, List<EditCaption> captions, CancellationToken ct = default);
    Task<FFmpegResult> BurnSubtitlesAsync(string input, string output, string srtPath, CancellationToken ct = default);

    // NEW: Audio operations
    Task<FFmpegResult> NormalizeAudioAsync(string input, string output, CancellationToken ct = default);
    Task<FFmpegResult> AdjustVolumeAsync(string input, string output, double multiplier, CancellationToken ct = default);

    // NEW: Visual operations
    Task<FFmpegResult> ApplyColorGradingAsync(string input, string output, ColorGradingConfig config, CancellationToken ct = default);
    Task<FFmpegResult> AddTransitionAsync(string clip1, string clip2, string output, string transitionType, int durationMs, CancellationToken ct = default);
}
```

---

## CLASS: FFmpegCommandBuilder

**File**: `Infrastructure/Processing/FFmpegCommandBuilder.cs`
**Methods**: 14 (fluent builder pattern)

```csharp
public class FFmpegCommandBuilder
{
    public FFmpegCommandBuilder Input(string path);
    public FFmpegCommandBuilder Output(string path);
    public FFmpegCommandBuilder Trim(double startSec, double endSec);
    public FFmpegCommandBuilder SetResolution(int width, int height);
    public FFmpegCommandBuilder SetFps(int fps);
    public FFmpegCommandBuilder SetCodec(string codec);
    public FFmpegCommandBuilder SetFormat(string format);
    public FFmpegCommandBuilder AddCaption(string text, double startSec, double endSec, string style);
    public FFmpegCommandBuilder NormalizeAudio();
    public FFmpegCommandBuilder SetVolume(double multiplier);
    public FFmpegCommandBuilder ApplyColorGrading(ColorGradingConfig config);
    public FFmpegCommandBuilder SetSpeed(double speed);
    public FFmpegCommandBuilder OverwriteOutput();
    public string Build();  // Returns full FFmpeg command string
}
```

---

## CLASS: EditExecutionAgent (Enhanced)

**File**: `Infrastructure/Agents/EditExecutionAgent.cs`
**Methods**: 10
**Dependencies**: `IDecisionEngine`, `IFFmpegService`, `ILocalMemoryService`, `IGoogleDriveService`, `IStudioWorkspaceStore`, `IPipelineOrchestrator`, `ITempStorageManager`, `IWorkspaceNotificationService`, `ILogger`

### Method 1: `CreateEditPlanAsync(Guid jobId, CancellationToken ct)`
- **Logic**:
  1. Load pipeline job from database
  2. Load script output for this job
  3. Load local memory: `EditAgentLocalMemory`
  4. Analyze input video: `AnalyzeVideoAsync()`
  5. Build context for Decision Engine with video analysis + script content
  6. Call `DecisionEngine.MakeDecisionAsync("edit-agent", DecisionType.VideoEditing, context, jobId)`
  7. Parse `EditDecisionPayload` from decision
  8. Convert to `EditPlan` entity
  9. Generate FFmpeg commands for each segment, caption, and audio adjustment
  10. Save `EditPlan` to database
  11. Return plan

### Method 2: `ExecuteEditPlanAsync(Guid jobId, EditPlan plan, CancellationToken ct)`
- **Logic**:
  1. Download RAW video from Drive to temp directory
  2. For each segment in plan:
     a. Trim segment: `FFmpegService.TrimAsync()`
     b. Apply speed adjustment if != 1.0
     c. Add captions for this segment
  3. Concatenate all trimmed segments: `FFmpegService.ConcatAsync()`
  4. Apply color grading to full video: `ApplyColorGradingAsync()`
  5. Normalize audio: `NormalizeAudioAsync()`
  6. Upload final output to `/Processed/` on Drive
  7. Clean up temp files
  8. Update plan status to Completed
  9. Transition pipeline: `TransitionStageAsync(jobId, PipelineStageType.VideoEditing)`
  10. Emit SignalR: `OnVideoEdited(jobId)`
  11. Record run: `RecordRunAsync("edit-agent", true)`

### Method 3: `ExecuteAsync(Guid jobId, EditDecisionPayload payload, CancellationToken ct)`
- **Purpose**: Hangfire-callable entry point
- **Logic**:
  1. Call `CreateEditPlanAsync()` to create plan
  2. Call `ExecuteEditPlanAsync()` to execute
  3. On failure: call `HandleFailure()`

### Method 4: `AnalyzeVideoAsync(string filePath, CancellationToken ct)`
- **Purpose**: Pre-edit analysis of source video
- **Returns**: `VideoAnalysisResult` with duration, keyframes, audio levels, scene changes

### Method 5-10: Helper methods for download, upload, cleanup, error handling

---

## ENTITY: VideoAnalysisResult

**File**: `Domain/Processing/VideoAnalysisResult.cs`
**Fields**: 12

```
VideoAnalysisResult
├── Duration          : double    (seconds)
├── Width             : int
├── Height            : int
├── Fps               : double
├── Codec             : string
├── Bitrate           : long      (bps)
├── AudioChannels     : int
├── AudioSampleRate   : int
├── FileSizeBytes     : long
├── SceneChanges      : List<double>   (timestamps of scene changes)
├── AverageVolume     : double
├── PeakVolume        : double
```

---

## PROMPT TEMPLATE FOR EDIT DECISIONS

### System Prompt
```
You are a professional video editor AI. Given video metadata and a script,
create an edit plan with precise timestamps for segments, captions, and audio adjustments.
Output valid JSON matching the schema exactly.
```

### JSON Output Schema
```json
{
    "segments": [
        { "startTime": 0.0, "endTime": 30.0, "description": "Opening", "speed": 1.0, "transition": "fade" }
    ],
    "captions": [
        { "startTime": 2.0, "endTime": 8.0, "text": "Hook text here", "style": "bold", "position": "bottom-center" }
    ],
    "audioAdjustments": [
        { "startTime": 0.0, "endTime": 5.0, "volumeMultiplier": 0.5, "normalize": true }
    ],
    "colorGrading": {
        "brightness": 1.05, "contrast": 1.1, "saturation": 1.15
    }
}
```

---

## OPENROUTER MODEL
| Setting | Value |
|---------|-------|
| Model | `google/gemini-flash-1.5-8b:free` |
| Temperature | 0.5 |
| Max Tokens | 3000 |

---

## EF CORE CONFIGURATION
```csharp
public DbSet<EditPlan> EditPlans { get; set; }
public DbSet<VideoAnalysisResult> VideoAnalysisResults { get; set; }

modelBuilder.Entity<EditPlan>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.HasIndex(e => e.JobId);
    entity.Property(e => e.Segments).HasColumnType("jsonb");
    entity.Property(e => e.Captions).HasColumnType("jsonb");
    entity.Property(e => e.AudioAdjustments).HasColumnType("jsonb");
    entity.Property(e => e.Transitions).HasColumnType("jsonb");
    entity.Property(e => e.ColorGrading).HasColumnType("jsonb");
    entity.Property(e => e.FFmpegCommands).HasColumnType("jsonb");
});
```

---

## ANGULAR INTEGRATION
- Edit Agent workspace shows: video preview, edit plan timeline, FFmpeg command log
- Real-time progress bar via SignalR during FFmpeg processing
- "Re-edit" button to regenerate edit plan with different style

## SIGNALR EVENTS
| Event | Payload |
|-------|---------|
| `OnEditPlanCreated` | `{ jobId, segmentCount, captionCount }` |
| `OnEditProgress` | `{ jobId, stage, percent }` |
| `OnVideoEdited` | `{ jobId, outputFileId, duration }` |

## TESTING PLAN
1. Unit: `CreateEditPlanAsync()` with mocked Decision Engine
2. Unit: `FFmpegCommandBuilder` generates correct commands
3. Integration: Full edit cycle with test video
4. Manual: Upload video → verify edited output in /Processed/

## ESTIMATED TIME: 6-8 hours
