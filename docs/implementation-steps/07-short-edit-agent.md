# 07 — Short Edit Agent

## Purpose
The Short Edit Agent takes raw short clips from `/Shorts/raw/` and applies final polish: attention hooks (first 3 seconds), captions (word-by-word or sentence-style), background music, emoji overlays, and transitions. Output goes to `/Shorts/processed/` — ready for scheduling and upload.

---

## FILE MAP
### Existing: `ShortEditExecutionAgent.cs` (Infrastructure/Shorts/) — enhance from 96 → ~250 lines
### New Files:
| File | Purpose |
|------|---------|
| `ShortEditPlan.cs` (`Domain/Agents/`) | Edit plan entity — **14 fields** |
| `IShortEditAgent.cs` (`Application/Agents/`) | Interface — **5 methods** |
| `ShortEditPrompts.cs` (`Infrastructure/Shorts/`) | Prompt templates |
| `CaptionRenderer.cs` (`Infrastructure/Shorts/`) | FFmpeg caption rendering |
| `MusicOverlayService.cs` (`Infrastructure/Shorts/`) | Background music mixing |

---

## ENTITY: ShortEditPlan — 14 Fields
```
ShortEditPlan
├── Id                  : Guid
├── ShortClipId         : Guid       (FK → ShortClip)
├── JobId               : Guid       (FK → VideoPipelineJob)
├── HookOverlay         : HookOverlayConfig   (first 3 sec config)
├── Captions            : List<ShortCaption>
├── MusicTrack          : MusicTrackConfig?
├── EmojiOverlays       : List<EmojiOverlay>?
├── TransitionIn        : string     ("glitch", "slide", "bounce", "zoom")
├── TransitionOut       : string     ("fade", "slide-out")
├── Watermark           : WatermarkConfig?
├── OutputDriveFileId   : string?
├── Status              : EditPlanStatus
├── FFmpegCommands      : List<string>
├── CreatedAt           : DateTimeOffset
```

### SUB: HookOverlayConfig — 6 Fields
```
├── Text, FontSize, FontColor, BackgroundColor, AnimationType, DurationSeconds
```

### SUB: ShortCaption — 7 Fields
```
├── StartTime, EndTime, Text, Style ("word-by-word"|"sentence"|"karaoke"), FontSize, Color, Position
```

### SUB: MusicTrackConfig — 5 Fields
```
├── TrackName, Volume (0.0-1.0), FadeInSeconds, FadeOutSeconds, Genre
```

### SUB: EmojiOverlay — 5 Fields
```
├── Emoji, StartTime, EndTime, Position, AnimationType
```

### SUB: WatermarkConfig — 5 Fields
```
├── Text, Position, FontSize, Opacity, Color
```

---

## INTERFACE: IShortEditAgent — 5 Methods
```csharp
Task<ShortEditPlan> CreateEditPlanAsync(Guid shortClipId, CancellationToken ct);
Task ExecuteEditPlanAsync(ShortEditPlan plan, CancellationToken ct);
Task<ShortEditPlan?> GetEditPlanAsync(Guid shortClipId, CancellationToken ct);
Task ReprocessAsync(Guid shortClipId, CancellationToken ct);
Task<List<ShortEditPlan>> GetAllForJobAsync(Guid jobId, CancellationToken ct);
```

---

## CLASS: ShortEditExecutionAgent — 8 Methods

### Method 1: `CreateEditPlanAsync` — AI creates caption timing, hook text, music selection
### Method 2: `ExecuteEditPlanAsync` — FFmpeg pipeline: hook overlay → captions → music → emoji → watermark
### Method 3: `RenderHookOverlay` — FFmpeg drawtext filter for first 3 seconds with animation
### Method 4: `RenderCaptions` — Word-by-word or sentence caption rendering via FFmpeg ASS subtitles
### Method 5: `MixBackgroundMusic` — FFmpeg amix filter to blend music at configured volume
### Method 6: `ApplyEmojiOverlays` — FFmpeg overlay filter for animated emoji
### Method 7: `UploadToShortsProcessed` — Upload final clip to `/Shorts/processed/` on Drive
### Method 8: `HandleFailure` — Error handling + retry logic

---

## CAPTION STYLES
| Style | Description | FFmpeg Approach |
|-------|-------------|-----------------|
| `word-by-word` | Each word appears individually | Multiple drawtext filters with timing |
| `sentence` | Full sentence shown at once | Single drawtext per caption block |
| `karaoke` | Words highlight as spoken | ASS subtitle with `\kf` timing |

---

## OPENROUTER MODEL
| Setting | Value |
|---------|-------|
| Model | `google/gemini-flash-1.5-8b:free` |
| Temperature | 0.5 |

---

## SIGNALR EVENTS
| Event | Payload |
|-------|---------|
| `OnShortEditStarted` | `{ shortClipId, jobId }` |
| `OnShortEditProgress` | `{ shortClipId, stage, percent }` |
| `OnShortEditComplete` | `{ shortClipId, outputFileId }` |

---

## REST API ENDPOINTS
```
POST   /api/agents/short-edit/plan/{shortClipId}
POST   /api/agents/short-edit/execute/{shortClipId}
GET    /api/agents/short-edit/{jobId}/all
POST   /api/agents/short-edit/reprocess/{shortClipId}
```

## EF CORE: `DbSet<ShortEditPlan>` with JSONB columns for sub-entities

## ESTIMATED TIME: 5-6 hours
