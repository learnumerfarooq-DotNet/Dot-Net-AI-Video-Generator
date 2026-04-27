# 01 — Main Brain Orchestrator

## Purpose
The Main Brain is the **central orchestrator** of the entire AI Video Pipeline V2. It is a Hangfire-backed `BackgroundService` that:
- Reads Global Memory (`/memory/global.json` from Google Drive) on every tick
- Monitors all pipeline stages and agent states
- Dispatches agents to the correct Hangfire queues
- Handles retries, routes tasks, and emits SignalR events to the Angular dashboard
- Tracks the overall pipeline state machine

## Architecture Position
```
Angular 21 Dashboard ──→ .NET 10 API ──→ MAIN BRAIN ──→ All Agents
                                             ↓
                                      Global Memory (Drive)
                                             ↓
                                      Local Memory per Agent
```

---

## FILE MAP (What Exists vs What To Create)

### Existing Files to MODIFY
| File | Path | Current State |
|------|------|---------------|
| `PipelineOrchestrator.cs` | `Infrastructure/Pipeline/PipelineOrchestrator.cs` | 131 lines, skeleton with 5 methods |
| `DrivePollingBackgroundService.cs` | `Infrastructure/Pipeline/DrivePollingBackgroundService.cs` | ~50 lines, polls /RAW/ only |
| `AgentDispatcher.cs` | `Infrastructure/Pipeline/AgentDispatcher.cs` | ~20 lines, empty stub |
| `DependencyInjection.cs` | `Infrastructure/DependencyInjection.cs` | Needs new registrations |
| `Program.cs` | `Api/Program.cs` | Needs Main Brain recurring job |

### New Files to CREATE
| File | Path | Purpose |
|------|------|---------|
| `MainBrainService.cs` | `Infrastructure/Brain/MainBrainService.cs` | Core BackgroundService |
| `MainBrainJob.cs` | `Infrastructure/Brain/MainBrainJob.cs` | Hangfire recurring job |
| `BrainState.cs` | `Domain/Brain/BrainState.cs` | Brain state entity |
| `IBrainOrchestrator.cs` | `Application/Brain/IBrainOrchestrator.cs` | Interface contract |
| `BrainOptions.cs` | `Application/Brain/BrainOptions.cs` | Configuration options |

---

## ENTITY: BrainState

**File**: `Domain/Brain/BrainState.cs`
**Fields**: 14

```
BrainState
├── Id                      : Guid           (PK)
├── Status                  : BrainStatus    (enum: Idle, Watching, Processing, Error, Paused)
├── CurrentTickNumber       : long           (auto-increment per tick)
├── LastTickAt              : DateTimeOffset  (when last tick completed)
├── LastGlobalMemorySync    : DateTimeOffset  (when global.json was last read)
├── ActiveJobCount          : int            (currently active pipeline jobs)
├── PendingJobCount         : int            (jobs waiting in queue)
├── FailedJobCount          : int            (jobs in error state)
├── CompletedJobCount       : int            (total completed since startup)
├── LastErrorMessage        : string?        (most recent error if Status=Error)
├── AgentHealthMap          : Dictionary<string, AgentHealthStatus>  (JSON column)
├── GlobalMemoryVersion     : string         (version from global.json)
├── IsCircuitBreakerOpen    : bool           (true if too many failures)
├── CreatedAt               : DateTimeOffset
```

**Enum**: `BrainStatus` (5 values)
```
Idle = 0
Watching = 1
Processing = 2
Error = 3
Paused = 4
```

**Enum**: `AgentHealthStatus` (4 values)
```
Healthy = 0
Degraded = 1
Failed = 2
Disabled = 3
```

---

## ENTITY: BrainTickLog

**File**: `Domain/Brain/BrainTickLog.cs`
**Fields**: 10

```
BrainTickLog
├── Id                : Guid            (PK)
├── TickNumber        : long
├── StartedAt         : DateTimeOffset
├── CompletedAt       : DateTimeOffset?
├── DurationMs        : long
├── JobsDispatched    : int             (how many agents were triggered)
├── JobsCompleted     : int             (how many completed this tick)
├── JobsFailed        : int             (how many failed this tick)
├── GlobalMemoryRead  : bool            (whether global.json was successfully read)
├── Notes             : string          (summary of what happened)
```

---

## INTERFACE: IBrainOrchestrator

**File**: `Application/Brain/IBrainOrchestrator.cs`
**Methods**: 8

```csharp
public interface IBrainOrchestrator
{
    // Core tick method — called every N seconds by Hangfire
    Task ExecuteTickAsync(CancellationToken ct = default);

    // Read global memory from Drive and update in-memory state
    Task<GlobalMemory> SyncGlobalMemoryAsync(CancellationToken ct = default);

    // Dispatch a specific agent to process a job
    Task DispatchAgentAsync(string agentKey, Guid jobId, CancellationToken ct = default);

    // Check health of all registered agents
    Task<Dictionary<string, AgentHealthStatus>> CheckAgentHealthAsync(CancellationToken ct = default);

    // Get current brain state
    Task<BrainState> GetStateAsync(CancellationToken ct = default);

    // Pause/Resume the brain
    Task PauseAsync(CancellationToken ct = default);
    Task ResumeAsync(CancellationToken ct = default);

    // Force re-read of global memory
    Task ForceGlobalMemoryRefreshAsync(CancellationToken ct = default);
}
```

---

## CLASS: BrainOptions

**File**: `Application/Brain/BrainOptions.cs`
**Fields**: 10

```csharp
public sealed class BrainOptions
{
    public const string SectionName = "Brain";

    public int TickIntervalSeconds { get; set; } = 30;          // How often brain ticks
    public int GlobalMemorySyncIntervalSeconds { get; set; } = 60;  // How often to re-read global.json
    public int MaxConcurrentDispatches { get; set; } = 4;       // Max parallel agent dispatches
    public int MaxRetryPerJob { get; set; } = 3;                // Max retries before dead-letter
    public int CircuitBreakerThreshold { get; set; } = 5;       // Failures before circuit opens
    public int CircuitBreakerPauseMinutes { get; set; } = 10;   // How long circuit stays open
    public bool AutoDispatchOnRawDetected { get; set; } = true; // Auto-start pipeline on new RAW
    public bool EmitSignalREvents { get; set; } = true;         // Push events to Angular
    public string GlobalMemoryDrivePath { get; set; } = "/memory/global.json";
    public string[] PeakUploadSlotsUtc { get; set; } = { "08:00", "12:00", "18:00", "21:00" };
}
```

---

## CLASS: MainBrainService (BackgroundService)

**File**: `Infrastructure/Brain/MainBrainService.cs`
**Methods**: 7

### Method 1: `ExecuteAsync(CancellationToken stoppingToken)`
- **Purpose**: Main loop that runs every `TickIntervalSeconds`
- **Logic**:
  1. Create a `PeriodicTimer` with `BrainOptions.TickIntervalSeconds`
  2. On each tick, call `ExecuteTickAsync()`
  3. Log tick number, duration, and any errors
  4. If circuit breaker is open, skip dispatch and log warning
  5. Emit `BrainTickCompleted` SignalR event

### Method 2: `ExecuteTickAsync(CancellationToken ct)`
- **Purpose**: Single tick execution
- **Logic**:
  1. Increment `BrainState.CurrentTickNumber`
  2. Check if `GlobalMemorySyncIntervalSeconds` has elapsed → call `SyncGlobalMemoryAsync()`
  3. Query `VideoPipelineJobs` with status `!= Published && != Failed`
  4. For each active job, determine next agent based on `CurrentStage`
  5. Dispatch agents via `DispatchAgentAsync()`
  6. Check for stuck jobs (no progress in 10 minutes) → retry or fail
  7. Update `BrainState` in database
  8. Create `BrainTickLog` record
  9. Emit SignalR event `OnBrainTickCompleted`

### Method 3: `SyncGlobalMemoryAsync(CancellationToken ct)`
- **Purpose**: Read `/memory/global.json` from Google Drive
- **Logic**:
  1. Use `IGoogleDriveService` to download `global.json`
  2. Deserialize to `GlobalMemory` object
  3. Validate folder registry paths exist
  4. Update `BrainState.GlobalMemoryVersion`
  5. Update `BrainState.LastGlobalMemorySync`
  6. Return the `GlobalMemory` object
  7. If file not found, create default and upload

### Method 4: `DispatchAgentAsync(string agentKey, Guid jobId, CancellationToken ct)`
- **Purpose**: Route a job to the correct agent via Hangfire
- **Logic**:
  1. Determine Hangfire queue from `agentKey`:
     - `script-gen` → queue "ai"
     - `edit-agent` → queue "ffmpeg"
     - `shorts-agent` → queue "ffmpeg"
     - `short-edit-agent` → queue "ffmpeg"
     - `upload-agent` → queue "upload"
     - `trend-agent` → queue "ai"
     - `analytics-agent` → queue "ai"
  2. Enqueue the appropriate agent handler class
  3. Log dispatch event
  4. Emit SignalR `OnAgentDispatched` event

### Method 5: `CheckAgentHealthAsync(CancellationToken ct)`
- **Purpose**: Check health status of all registered agents
- **Logic**:
  1. For each agent in `GlobalMemory.FolderRegistry.AgentFolders`:
     - Check if Drive folder exists
     - Check last run timestamp
     - Check error count in last hour
     - Assign `AgentHealthStatus` (Healthy/Degraded/Failed/Disabled)
  2. Update `BrainState.AgentHealthMap`
  3. Return dictionary

### Method 6: `DetermineNextAgent(VideoPipelineJob job)`
- **Purpose**: State machine to determine next agent for a job
- **Logic** (switch on `CurrentStage`):
  ```
  RawDetection      → ScriptGeneration (script-gen-agent)
  ScriptGeneration  → VideoEditing (edit-agent)
  VideoEditing      → ShortGeneration (shorts-agent) + TrendDiscovery (trend-agent)
  ShortGeneration   → ShortEditing (short-edit-agent)
  ShortEditing      → UploadScheduling (upload-agent)
  TrendDiscovery    → UploadScheduling (upload-agent)
  UploadScheduling  → PlatformPublishing (youtube/tiktok/instagram agents)
  PlatformPublishing → AnalyticsCollection (analytics-agent)
  ```

### Method 7: `HandleStuckJobsAsync(CancellationToken ct)`
- **Purpose**: Detect jobs that haven't progressed in 10+ minutes
- **Logic**:
  1. Query jobs where `UpdatedAt < DateTime.UtcNow.AddMinutes(-10)`
  2. For each stuck job:
     - If `RetryCount < MaxRetryPerJob` → re-dispatch current stage
     - If `RetryCount >= MaxRetryPerJob` → move to Failed status
     - Emit SignalR `OnJobStuck` event
  3. If total stuck jobs > `CircuitBreakerThreshold`:
     - Set `IsCircuitBreakerOpen = true`
     - Log critical error
     - Emit SignalR `OnCircuitBreakerOpen`

---

## CLASS: MainBrainJob (Hangfire Recurring)

**File**: `Infrastructure/Brain/MainBrainJob.cs`
**Methods**: 1

### Method: `ExecuteAsync()`
- **Purpose**: Hangfire-callable wrapper that delegates to `IBrainOrchestrator.ExecuteTickAsync()`
- **Called by**: Hangfire recurring schedule (every 30 seconds)
- **Registration in Program.cs**:
  ```csharp
  recurringJobManager.AddOrUpdate<MainBrainJob>(
      "main-brain-tick",
      job => job.ExecuteAsync(),
      "*/30 * * * * *"); // Every 30 seconds
  ```

---

## SIGNALR EVENTS (emitted by Main Brain)

| Event Name | Payload | When |
|------------|---------|------|
| `OnBrainTickCompleted` | `{ tickNumber, activeJobs, pendingJobs, failedJobs }` | Every tick |
| `OnAgentDispatched` | `{ agentKey, jobId, queue }` | When an agent is dispatched |
| `OnJobStuck` | `{ jobId, stage, stuckMinutes }` | When a job is detected as stuck |
| `OnCircuitBreakerOpen` | `{ failedCount, pauseMinutes }` | When circuit breaker opens |
| `OnGlobalMemorySynced` | `{ version, folderCount }` | When global.json is re-read |

---

## HANGFIRE QUEUE ROUTING

| Agent Key | Hangfire Queue | Worker Count |
|-----------|---------------|--------------|
| `script-gen-agent` | `ai` | 2 |
| `edit-agent` | `ffmpeg` | 2 |
| `shorts-agent` | `ffmpeg` | 2 |
| `short-edit-agent` | `ffmpeg` | 2 |
| `trend-agent` | `ai` | 1 |
| `upload-agent` | `upload` | 2 |
| `analytics-agent` | `ai` | 1 |
| `youtube-agent` | `upload` | 1 |
| `tiktok-agent` | `upload` | 1 |
| `instagram-agent` | `upload` | 1 |

---

## DI REGISTRATION (add to DependencyInjection.cs)

```csharp
// Brain
services.Configure<BrainOptions>(configuration.GetSection(BrainOptions.SectionName));
services.AddScoped<IBrainOrchestrator, MainBrainService>();
services.AddScoped<MainBrainJob>();
services.AddHostedService<MainBrainBackgroundService>();
```

---

## APPSETTINGS.JSON ADDITION

```json
"Brain": {
    "TickIntervalSeconds": 30,
    "GlobalMemorySyncIntervalSeconds": 60,
    "MaxConcurrentDispatches": 4,
    "MaxRetryPerJob": 3,
    "CircuitBreakerThreshold": 5,
    "CircuitBreakerPauseMinutes": 10,
    "AutoDispatchOnRawDetected": true,
    "EmitSignalREvents": true,
    "GlobalMemoryDrivePath": "/memory/global.json",
    "PeakUploadSlotsUtc": ["08:00", "12:00", "18:00", "21:00"]
}
```

---

## STATE MACHINE DIAGRAM

```
[Idle] ──→ [Watching] ──→ [RawDetected] ──→ [ScriptGenerated]
                                                    │
                                                    ▼
                                              [Edited] ──→ [ShortClipped]
                                                    │              │
                                                    ▼              ▼
                                          [TrendScheduled]  [ShortEdited]
                                                    │              │
                                                    └──────┬───────┘
                                                           ▼
                                                   [ReadyToUpload]
                                                           │
                                                           ▼
                                                     [Uploading]
                                                           │
                                                           ▼
                                                    [Published]
                                                           │
                                                           ▼
                                                 [AnalyticsCollected]
```

At any point, a job can transition to:
- `[Failed]` → increments RetryCount
- `[RetryPending]` → re-queued for current stage

---

## EF CORE CONFIGURATION

```csharp
// In StudioDbContext.cs — add:
public DbSet<BrainState> BrainStates { get; set; }
public DbSet<BrainTickLog> BrainTickLogs { get; set; }

// In OnModelCreating:
modelBuilder.Entity<BrainState>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.Property(e => e.AgentHealthMap)
          .HasColumnType("jsonb");
    entity.HasIndex(e => e.Status);
});

modelBuilder.Entity<BrainTickLog>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.HasIndex(e => e.TickNumber);
    entity.HasIndex(e => e.StartedAt);
});
```

---

## ANGULAR INTEGRATION

### Store Update: `pipeline.store.ts`
Add method `handleBrainTick(data)` to update brain status in real-time.

### SignalR Handler: `signalr.service.ts`
Add handler for `OnBrainTickCompleted`:
```typescript
this.hubConnection.on('BrainTickCompleted', (data) => {
    this.pipelineStore.handleBrainTick(data);
});
```

### Dashboard Widget: Main Brain Status Card
Display:
- Brain Status (Idle/Watching/Processing)
- Active Jobs count
- Last Tick timestamp
- Circuit Breaker status (Open/Closed)
- Agent Health grid

---

## TESTING PLAN

1. **Unit Test**: `MainBrainService.ExecuteTickAsync()` — mock all dependencies, verify dispatch order
2. **Unit Test**: `DetermineNextAgent()` — verify state machine transitions for all 14 statuses
3. **Integration Test**: Full tick cycle with in-memory database
4. **Manual Test**: Start API, upload file to /RAW/, watch pipeline execute through all stages via SignalR

---

## DEPENDENCIES

- Hangfire 3 (already registered)
- Google.Apis.Drive.v3 (already registered)
- SignalR (already registered)
- PostgreSQL via EF Core (already registered)

## ESTIMATED IMPLEMENTATION TIME
- Backend: 4-6 hours
- Frontend integration: 2-3 hours
- Testing: 2 hours
