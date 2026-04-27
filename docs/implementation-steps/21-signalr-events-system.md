# 21 — SignalR Events System

## Purpose
Complete real-time event system connecting .NET 10 backend to Angular 21 frontend via SignalR WebSocket hub.

---

## ALL 30 SIGNALR EVENTS

### Brain Events (5)
| Event | Payload | Emitter |
|-------|---------|---------|
| `BrainTickCompleted` | tickNumber, activeJobs, pendingJobs, failedJobs, durationMs | MainBrain |
| `BrainStatusChanged` | status, previousStatus | MainBrain |
| `GlobalMemorySynced` | version, folderCount | MainBrain |
| `CircuitBreakerStateChanged` | agentKey, state, failureCount | ErrorHandling |
| `BrainPausedResumed` | isPaused, reason | MainBrain |

### Pipeline Events (6)
| Event | Payload | Emitter |
|-------|---------|---------|
| `JobStarted` | jobId, fileName, driveFileId | Orchestrator |
| `StageCompleted` | jobId, stageName, progress | Orchestrator |
| `ProgressUpdated` | jobId, stage, percent, message | Any agent |
| `JobCompleted` | jobId, fileName, durationMs | Orchestrator |
| `JobFailed` | jobId, error, retryCount, isDeadLettered | Orchestrator |
| `JobRetrying` | jobId, attempt, nextRetryAt | RetryManager |

### Agent Events (6)
| Event | Payload | Emitter |
|-------|---------|---------|
| `AgentDispatched` | agentKey, jobId, queue | MainBrain |
| `AgentRunStarted` | agentKey, runId, jobId | Any agent |
| `AgentRunCompleted` | agentKey, runId, status, durationMs | Any agent |
| `AgentHealthChanged` | agentKey, status, reason | MainBrain |
| `AgentChatResponse` | agentKey, message, role | ChatProvider |
| `AgentChatStreamChunk` | agentKey, chunk, type | ChatProvider |

### Content Events (5)
| Event | Payload | Emitter |
|-------|---------|---------|
| `ScriptGenerated` | jobId, title, confidence | ScriptGen |
| `VideoEdited` | jobId, outputFileId, duration | EditAgent |
| `ShortsCreated` | jobId, clipCount, totalDuration | ShortsAgent |
| `ShortEditCompleted` | jobId, clipId, outputFileId | ShortEdit |
| `UploadPackageReady` | packageId, jobId, platformCount | Upload |

### Publishing Events (4)
| Event | Payload | Emitter |
|-------|---------|---------|
| `UploadStarted` | packageId, platform | Publisher |
| `UploadProgress` | packageId, platform, percent | Publisher |
| `UploadCompleted` | packageId, platform, videoId, url | Publisher |
| `UploadFailed` | packageId, platform, error | Publisher |

### System Events (4)
| Event | Payload | Emitter |
|-------|---------|---------|
| `TrendDiscoveryComplete` | topicCount, topKeywords | TrendAgent |
| `AnalyticsReportReady` | reportId, videosAnalyzed | Analytics |
| `MemorySuggestionCreated` | id, scope, agentKey | Memory |
| `DriveFileDetected` | fileId, fileName, folder | DrivePoll |

---

## INTERFACE: IRealtimeEventEmitter — 25 Methods
All methods follow pattern: `Task EmitXxxAsync(params, CancellationToken ct)`

## EVENT PAYLOAD RECORDS — 25 C# records in `Domain/Events/SignalREvents.cs`

## ANGULAR: Register all 30 handlers in `signalr.service.ts` with signals

## HUB GROUPS: brain, pipeline, agent:{key}, publishing, all

## ESTIMATED TIME: 4-6 hours
