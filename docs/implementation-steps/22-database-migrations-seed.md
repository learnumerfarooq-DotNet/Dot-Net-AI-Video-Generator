# 22 — Database Migrations, Seed Data & Startup

## Purpose
Define all EF Core migrations, DbSet registrations, seed data, and startup initialization for the complete AI Video Pipeline V2 system. This covers all new entities introduced across files 01-21.

---

## ALL DbSet REGISTRATIONS (StudioDbContext.cs)

### Existing DbSets (keep)
```csharp
public DbSet<MemoryEntry> MemoryEntries { get; set; }
public DbSet<MemorySuggestion> MemorySuggestions { get; set; }
public DbSet<VideoPipelineJob> VideoPipelineJobs { get; set; }
public DbSet<PipelineStage> PipelineStages { get; set; }
public DbSet<PlatformPublishJob> PlatformPublishJobs { get; set; }
public DbSet<AgentDecision> AgentDecisions { get; set; }
public DbSet<PromptTemplate> PromptTemplates { get; set; }
public DbSet<DecisionValidation> DecisionValidations { get; set; }
public DbSet<DecisionCacheEntry> DecisionCacheEntries { get; set; }
public DbSet<ErrorLog> ErrorLogs { get; set; }
public DbSet<VideoAnalytics> VideoAnalyticEntries { get; set; }
public DbSet<ViralPattern> ViralPatterns { get; set; }
```

### NEW DbSets to Add (18)
```csharp
// Brain (File 01)
public DbSet<BrainState> BrainStates { get; set; }
public DbSet<BrainTickLog> BrainTickLogs { get; set; }

// Local Memory (File 03)
public DbSet<AgentLocalMemory> AgentLocalMemories { get; set; }

// Script Gen (File 04)
public DbSet<ScriptOutput> ScriptOutputs { get; set; }

// Edit Agent (File 05)
public DbSet<EditPlan> EditPlans { get; set; }
public DbSet<VideoAnalysisResult> VideoAnalysisResults { get; set; }

// Shorts (File 06)
public DbSet<ShortClip> ShortClips { get; set; }

// Short Edit (File 07)
public DbSet<ShortEditPlan> ShortEditPlans { get; set; }

// Trends (File 08)
public DbSet<TrendResult> TrendResults { get; set; }
public DbSet<ScrapeResult> ScrapeResults { get; set; }

// Upload (File 09)
public DbSet<UploadPackage> UploadPackages { get; set; }

// Analytics (File 10)
public DbSet<AnalyticsReport> AnalyticsReports { get; set; }
public DbSet<PlatformPerformanceReport> PlatformReports { get; set; }

// Error (File 11)
public DbSet<DeadLetterEntry> DeadLetterEntries { get; set; }
public DbSet<CircuitBreakerState> CircuitBreakerStates { get; set; }
public DbSet<RetryPolicy> RetryPolicies { get; set; }

// Decisions (File 16)
public DbSet<DecisionAuditLog> DecisionAuditLogs { get; set; }

// Publishing (Files 12-15)
public DbSet<YouTubeUploadResult> YouTubeUploadResults { get; set; }
```

---

## ENTITY FIELD COUNTS SUMMARY

| Entity | Fields | File |
|--------|--------|------|
| BrainState | 14 | 01 |
| BrainTickLog | 10 | 01 |
| AgentLocalMemory | 12 | 03 |
| ScriptGenLocalMemory | 10 | 03 |
| EditAgentLocalMemory | 12 | 03 |
| ShortsAgentLocalMemory | 10 | 03 |
| ShortEditLocalMemory | 11 | 03 |
| TrendAgentLocalMemory | 11 | 03 |
| UploadAgentLocalMemory | 12 | 03 |
| AnalyticsAgentLocalMemory | 9 | 03 |
| ScriptOutput | 16 | 04 |
| EditPlan | 18 | 05 |
| EditSegment | 7 | 05 |
| EditCaption | 8 | 05 |
| VideoAnalysisResult | 12 | 05 |
| ShortClip | 20 | 06 |
| ShortEditPlan | 14 | 07 |
| TrendResult | 14 | 08 |
| DiscoveredTopic | 10 | 08 |
| ScrapeResult | 10 | 08 |
| UploadPackage | 22 | 09 |
| AnalyticsReport | 16 | 10 |
| RetryPolicy | 10 | 11 |
| CircuitBreakerState | 9 | 11 |
| DeadLetterEntry | 12 | 11 |
| YouTubeUploadResult | 14 | 12 |
| YouTubeCredential | 8 | 12 |
| TikTokUploadResult | 12 | 13 |
| InstagramUploadResult | 12 | 14 |
| FacebookUploadResult | 11 | 15 |
| LinkedInUploadResult | 11 | 15 |
| DecisionAuditLog | 10 | 16 |
| AgentStatusEntry | 11 | 02 |
| ScheduleSlot | 9 | 02 |
| AnalyticsSummary | 12 | 02 |
| ErrorSummary | 8 | 02 |
| ContentStrategy | 8 | 02 |
| **TOTAL** | **~447 fields** | |

---

## JSONB COLUMNS (PostgreSQL)

| Entity | Column | Type |
|--------|--------|------|
| BrainState | AgentHealthMap | jsonb |
| AgentLocalMemory | ConfigJson | jsonb |
| ScriptOutput | Keywords, Hashtags, SuggestedPlatforms | jsonb |
| EditPlan | Segments, Captions, AudioAdjustments, Transitions, ColorGrading, FFmpegCommands | jsonb |
| ShortClip | — (no JSON) | — |
| ShortEditPlan | HookOverlay, Captions, MusicTrack, EmojiOverlays, FFmpegCommands | jsonb |
| TrendResult | Topics, PlannedUploads, TopKeywords, TopHashtags | jsonb |
| UploadPackage | Keywords, Hashtags, TargetPlatforms | jsonb |
| AnalyticsReport | TopPerformingVideos, DetectedPatterns, Recommendations | jsonb |
| DeadLetterEntry | AllErrors | jsonb |
| RetryPolicy | BackoffSeconds, RetryOnExceptions, SkipOnExceptions | jsonb |

---

## INDEXES

```csharp
// High-priority indexes
entity.HasIndex(e => e.Status);          // VideoPipelineJob, ShortClip, UploadPackage
entity.HasIndex(e => e.AgentKey);        // AgentLocalMemory, CircuitBreakerState
entity.HasIndex(e => e.JobId);           // ScriptOutput, EditPlan, ShortClip, etc.
entity.HasIndex(e => e.CreatedAt);       // BrainTickLog, TrendResult, AnalyticsReport
entity.HasIndex(e => e.TickNumber);      // BrainTickLog
entity.HasIndex(e => e.Platform);        // VideoAnalytics, PlatformPublishJob
```

---

## SEED DATA

### 1. Prompt Templates (8 templates — one per DecisionType)
See File 16 for all 8 prompt templates with system prompt, user template, and JSON schema.

### 2. Default Local Memories (11 agents)
Create default `AgentLocalMemory` for each agent with typed config JSON.

### 3. Default Global Memory
Create default `/memory/global.json` on Drive with folder registry, tier sites, video constraints.

### 4. Default Retry Policies (11 agents)
```csharp
new RetryPolicy { AgentKey = "script-gen-agent", MaxRetries = 3, BackoffSeconds = [30, 120, 300] }
// ... for all 11 agents
```

### 5. Default Circuit Breaker States (11 agents)
```csharp
new CircuitBreakerState { AgentKey = "script-gen-agent", State = "Closed", Threshold = 3 }
// ... for all 11 agents
```

### 6. Initial Brain State
```csharp
new BrainState { Status = BrainStatus.Idle, CurrentTickNumber = 0, GlobalMemoryVersion = "1.0" }
```

---

## MIGRATION COMMAND

```bash
dotnet ef migrations add AddPipelineV2Entities --project AiContentFactory.Infrastructure --startup-project AiContentFactory.Api
dotnet ef database update --project AiContentFactory.Infrastructure --startup-project AiContentFactory.Api
```

---

## STARTUP INITIALIZATION ORDER (Program.cs / StudioDatabaseInitializer.cs)

```
1. Apply EF Core migrations
2. Seed prompt templates (8)
3. Seed default local memories (11)
4. Seed retry policies (11)
5. Seed circuit breaker states (11)
6. Create initial brain state
7. Create/verify global.json on Drive
8. Verify Drive folder structure (/RAW/, /Processed/, etc.)
9. Start Hangfire recurring jobs:
   - Main Brain tick (every 30 seconds)
   - Trend Discovery (hourly)
   - Daily Analytics (daily at 3 AM)
10. Start Background Services:
    - DrivePollingBackgroundService
    - DriveSyncService
    - EmbeddingSyncService
    - MemoryCleanupService
    - MainBrainBackgroundService
```

---

## NEW APPSETTINGS.JSON SECTIONS

```json
{
    "Brain": {
        "TickIntervalSeconds": 30,
        "GlobalMemorySyncIntervalSeconds": 60,
        "MaxConcurrentDispatches": 4,
        "MaxRetryPerJob": 3,
        "CircuitBreakerThreshold": 5,
        "CircuitBreakerPauseMinutes": 10,
        "AutoDispatchOnRawDetected": true,
        "EmitSignalREvents": true,
        "GlobalMemoryDrivePath": "/memory/global.json"
    },
    "Analytics": {
        "CollectionPeriodDays": 7,
        "TopPerformersCount": 10,
        "EnableViralDetection": true,
        "FeedbackLoopEnabled": true
    }
}
```

---

## TOTAL NEW DI REGISTRATIONS (add to DependencyInjection.cs)

```csharp
// Brain (3)
services.Configure<BrainOptions>(configuration.GetSection("Brain"));
services.AddScoped<IBrainOrchestrator, MainBrainService>();
services.AddScoped<MainBrainJob>();

// Global Memory (2)
services.AddScoped<IGlobalMemoryService, GlobalMemoryService>();
services.AddScoped<GlobalMemorySyncJob>();

// Local Memory (1)
services.AddScoped<ILocalMemoryService, LocalMemoryService>();

// Script Gen (1)
services.AddScoped<IScriptGenAgent, ScriptGenExecutionAgent>();

// Edit Agent — already registered, enhance existing

// Shorts — already registered, enhance existing

// Upload (2)
services.AddScoped<IUploadAgent, UploadAgent>();
services.AddScoped<UploadQueueManager>();

// Analytics (1)
services.AddScoped<IAnalyticsAgent, AnalyticsAgent>();

// Error Handling (1)
services.AddScoped<IErrorHandlingService, ErrorHandlingService>();

// Publishing (4)
services.AddScoped<IPlatformPublisher, TikTokPublisher>();
services.AddScoped<IPlatformPublisher, InstagramPublisher>();
services.AddScoped<IPlatformPublisher, FacebookPublisher>();
services.AddScoped<IPlatformPublisher, LinkedInPublisher>();

// Events (1)
services.AddScoped<IRealtimeEventEmitter, SignalREventEmitter>();

// Prompts (1)
services.AddScoped<PromptVersionManager>();
```

---

## VERIFICATION CHECKLIST
- [ ] All 18 new DbSets registered
- [ ] All JSONB columns configured
- [ ] All indexes created
- [ ] Migration applies cleanly
- [ ] Seed data populates correctly
- [ ] Hangfire jobs start on schedule
- [ ] Background services start without errors
- [ ] SignalR hub connects from Angular
- [ ] Drive folder structure verified

## ESTIMATED TIME: 3-4 hours
