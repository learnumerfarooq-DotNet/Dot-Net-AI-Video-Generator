# 04 — Script Generation Agent

## Purpose
The Script Gen Agent takes RAW video metadata (file name, duration, resolution, audio tracks) and generates a complete video script using AI. It produces: a hook, structured body, call-to-action, keywords, and suggested title. The script is saved to `/RAW/scripts/` on Google Drive and transitions the pipeline to the next stage.

## Architecture Position
```
/RAW/ (Google Drive) ──→ Main Brain detects new file
    ↓
Script Gen Agent
    ├── Reads: RAW video metadata from Drive
    ├── Uses: OpenRouter (llama-3.1-8b-instruct:free) via Decision Engine
    ├── Reads: Local Memory (last_script_style, tone_config)
    ├── Writes: Script JSON to /RAW/scripts/
    └── Transitions: Pipeline → ScriptGenerated status
```

---

## FILE MAP

### Existing Files to MODIFY
| File | Path | Changes |
|------|------|---------|
| `ScriptAgent.cs` | `Infrastructure/Agents/ScriptAgent.cs` | Complete rewrite — 49 → ~200 lines |
| `ScriptDecisionAgent.cs` | `Infrastructure/Agents/ScriptDecisionAgent.cs` | Enhance with full prompt template |
| `RawPipelineHandler.cs` | `Infrastructure/Pipeline/RawPipelineHandler.cs` | Call Script Gen after RAW detection |

### New Files to CREATE
| File | Path | Purpose |
|------|------|---------|
| `ScriptGenExecutionAgent.cs` | `Infrastructure/Agents/ScriptGenExecutionAgent.cs` | Full execution pipeline |
| `ScriptOutput.cs` | `Domain/Agents/ScriptOutput.cs` | Script output entity |
| `IScriptGenAgent.cs` | `Application/Agents/IScriptGenAgent.cs` | Interface |
| `ScriptGenPrompts.cs` | `Infrastructure/Agents/ScriptGenPrompts.cs` | Prompt templates |

---

## ENTITY: ScriptOutput

**File**: `Domain/Agents/ScriptOutput.cs`
**Fields**: 16

```
ScriptOutput
├── Id                : Guid            (PK)
├── JobId             : Guid            (FK → VideoPipelineJob)
├── Title             : string          (AI-generated video title)
├── Hook              : string          (opening hook — 1-2 sentences)
├── Introduction      : string          (scene setting — 2-3 sentences)
├── Body              : string          (main content — structured paragraphs)
├── CallToAction      : string          (closing CTA — 1-2 sentences)
├── Keywords          : List<string>    (5-10 SEO keywords)
├── Hashtags          : List<string>    (5-15 hashtags)
├── SuggestedPlatforms : List<string>   (which platforms suit this script)
├── EstimatedDuration : int             (estimated video length in seconds)
├── ToneUsed          : string          (tone applied from local memory)
├── StyleUsed         : string          (style applied from local memory)
├── ConfidenceScore   : double          (AI confidence 0.0-1.0)
├── DriveFileId       : string?         (Drive file ID where script is stored)
├── CreatedAt         : DateTimeOffset
```

---

## INTERFACE: IScriptGenAgent

**File**: `Application/Agents/IScriptGenAgent.cs`
**Methods**: 4

```csharp
public interface IScriptGenAgent
{
    // Generate script from video metadata
    Task<ScriptOutput> GenerateScriptAsync(Guid jobId, VideoMetadata metadata, CancellationToken ct = default);

    // Regenerate with different style
    Task<ScriptOutput> RegenerateScriptAsync(Guid jobId, string style, string tone, CancellationToken ct = default);

    // Get the latest script for a job
    Task<ScriptOutput?> GetScriptAsync(Guid jobId, CancellationToken ct = default);

    // Validate script meets quality thresholds
    Task<bool> ValidateScriptAsync(ScriptOutput script, CancellationToken ct = default);
}
```

---

## CLASS: ScriptGenExecutionAgent

**File**: `Infrastructure/Agents/ScriptGenExecutionAgent.cs`
**Methods**: 8
**Dependencies**: `IDecisionEngine`, `ILocalMemoryService`, `IGlobalMemoryService`, `IGoogleDriveService`, `IStudioWorkspaceStore`, `IPipelineOrchestrator`, `IWorkspaceNotificationService`, `ILogger`

### Method 1: `GenerateScriptAsync(Guid jobId, VideoMetadata metadata, CancellationToken ct)`
- **Purpose**: Full script generation pipeline
- **Logic**:
  1. Load local memory: `GetConfigAsync<ScriptGenLocalMemory>("script-gen-agent")`
  2. Load global memory for constraints
  3. Build context dictionary from metadata:
     - `{"fileName": "...", "duration": "...", "resolution": "...", "audioTracks": "..."}`
     - `{"style": localMemory.LastScriptStyle, "tone": localMemory.ToneConfig}`
     - `{"videoType": localMemory.VideoType, "language": localMemory.PreferredLanguage}`
  4. Call `IDecisionEngine.MakeDecisionAsync("script-gen-agent", DecisionType.ScriptGeneration, context, jobId)`
  5. Parse decision payload to `ScriptDecisionPayload`
  6. Create `ScriptOutput` entity from payload
  7. Save to database
  8. Upload script JSON to `/RAW/scripts/{jobId}.json` on Drive
  9. Update local memory with `LastGeneratedScript`
  10. Record run: `RecordRunAsync("script-gen-agent", true)`
  11. Transition pipeline: `TransitionStageAsync(jobId, PipelineStageType.ScriptGeneration)`
  12. Emit SignalR: `OnScriptGenerated(jobId, scriptTitle)`

### Method 2: `RegenerateScriptAsync(Guid jobId, string style, string tone, CancellationToken ct)`
- **Purpose**: Re-generate with different parameters
- **Logic**:
  1. Load existing script for job
  2. Update local memory with new style/tone
  3. Invalidate decision cache for this job
  4. Call `GenerateScriptAsync()` with updated context

### Method 3: `GetScriptAsync(Guid jobId, CancellationToken ct)`
- Query `ScriptOutput` by `JobId` from database

### Method 4: `ValidateScriptAsync(ScriptOutput script, CancellationToken ct)`
- **Validation Rules**:
  - Title length: 10-100 characters
  - Hook length: 10-200 characters
  - Body length: 100-5000 characters
  - Keywords count: 3-15
  - ConfidenceScore >= 0.3 (from DecisionEngine settings)

### Method 5: `BuildPromptContext(VideoMetadata metadata, ScriptGenLocalMemory localMemory)` (private)
- Constructs the context dictionary for the Decision Engine

### Method 6: `ParseScriptFromDecision(AgentDecision decision)` (private)
- Deserializes `decision.ValidatedPayload` into `ScriptDecisionPayload`
- Maps to `ScriptOutput` entity

### Method 7: `UploadScriptToDrive(ScriptOutput script, CancellationToken ct)` (private)
- Serializes script to JSON
- Uploads to `/RAW/scripts/{script.JobId}.json`
- Sets `script.DriveFileId` from upload response

### Method 8: `HandleFailure(Guid jobId, Exception ex, CancellationToken ct)` (private)
- Record failed run
- Call orchestrator `HandleFailureAsync()`
- Check retry count → re-queue or dead-letter

---

## PROMPT TEMPLATE

**File**: `Infrastructure/Agents/ScriptGenPrompts.cs`

### System Prompt
```
You are an expert video script writer for social media content.
You create engaging, viral-worthy scripts optimized for maximum viewer retention.
You must output valid JSON matching the exact schema provided.
```

### User Prompt Template
```
Generate a video script for the following video:

File: {fileName}
Duration: {duration} seconds
Resolution: {resolution}
Video Type: {videoType}
Style: {style}
Tone: {tone}
Language: {language}

Requirements:
1. Create an attention-grabbing hook (first 3 seconds)
2. Structure the body with clear segments
3. Include a call-to-action
4. Suggest 5-10 SEO keywords
5. Suggest 5-15 hashtags
6. Recommend platforms

Respond in the following JSON format:
{jsonSchema}
```

### JSON Output Schema
```json
{
    "title": "string — catchy video title",
    "hook": "string — opening hook for first 3 seconds",
    "introduction": "string — scene setting",
    "body": "string — main content with paragraphs",
    "callToAction": "string — closing CTA",
    "keywords": ["string array — SEO keywords"],
    "hashtags": ["string array — hashtags with #"],
    "suggestedPlatforms": ["string array — YouTube, TikTok, etc."],
    "estimatedDuration": 0
}
```

---

## PROMPT TEMPLATE SEED DATA

```csharp
new PromptTemplate
{
    Id = Guid.NewGuid(),
    AgentKey = "script-gen-agent",
    DecisionType = DecisionType.ScriptGeneration,
    Version = "1.0",
    SystemPrompt = "You are an expert video script writer...",
    UserPromptTemplate = "Generate a video script for...",
    JsonOutputSchema = "{ schema... }",
    IsActive = true,
    CreatedAt = DateTimeOffset.UtcNow,
    ActivatedAt = DateTimeOffset.UtcNow
}
```

---

## EF CORE CONFIGURATION

```csharp
// In StudioDbContext.cs:
public DbSet<ScriptOutput> ScriptOutputs { get; set; }

// In OnModelCreating:
modelBuilder.Entity<ScriptOutput>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.HasIndex(e => e.JobId);
    entity.Property(e => e.Keywords).HasColumnType("jsonb");
    entity.Property(e => e.Hashtags).HasColumnType("jsonb");
    entity.Property(e => e.SuggestedPlatforms).HasColumnType("jsonb");
});
```

---

## REST API ENDPOINTS

```
POST   /api/agents/script-gen/generate/{jobId}     → Generate script
POST   /api/agents/script-gen/regenerate/{jobId}    → Regenerate with new params
GET    /api/agents/script-gen/script/{jobId}        → Get script
POST   /api/agents/script-gen/validate/{jobId}      → Validate script
```

---

## OPENROUTER MODEL

| Setting | Value |
|---------|-------|
| Model | `meta-llama/llama-3.1-8b-instruct:free` |
| Temperature | 0.7 |
| Max Tokens | 2000 |
| Response Format | JSON |

---

## ANGULAR INTEGRATION

### Script Agent Component Enhancement
- Show current script in editor view
- "Generate Script" button triggers API call
- Real-time progress via SignalR
- Style/Tone selector dropdowns
- Preview generated script with syntax highlighting
- "Regenerate" button with style/tone override
- Script history timeline

### SignalR Events
| Event | Payload | When |
|-------|---------|------|
| `OnScriptGenerated` | `{ jobId, title, confidence }` | Script generation complete |
| `OnScriptFailed` | `{ jobId, error }` | Script generation failed |

---

## DI REGISTRATION

```csharp
services.AddScoped<IScriptGenAgent, ScriptGenExecutionAgent>();
```

---

## PIPELINE INTEGRATION

In `RawPipelineHandler.cs`, after downloading RAW video and extracting metadata:
```csharp
// After metadata extraction:
var scriptAgent = scope.ServiceProvider.GetRequiredService<IScriptGenAgent>();
var script = await scriptAgent.GenerateScriptAsync(jobId, metadata, ct);
```

---

## TESTING PLAN
1. **Unit Test**: `GenerateScriptAsync()` with mocked Decision Engine
2. **Unit Test**: `ValidateScriptAsync()` with various script lengths
3. **Unit Test**: `ParseScriptFromDecision()` with valid/invalid JSON
4. **Integration Test**: Full generation cycle with in-memory DB
5. **Manual Test**: Upload video → verify script appears in Drive

## ESTIMATED TIME: 4-5 hours
