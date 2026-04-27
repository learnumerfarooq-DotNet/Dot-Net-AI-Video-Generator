# 11 — Error Queue & Retry System

## Purpose
Centralized error handling with automatic retry (×3 with exponential backoff), circuit breaker pattern, dead-letter queue, and error reporting. Failed jobs move to `/Errors/retry/` on Google Drive. The Main Brain monitors failure states in real-time.

---

## FILE MAP
### Existing: `RetryManager.cs`, `CircuitBreakerManager.cs`, `FailureMonitor.cs` (Infrastructure/Errors/)
### Existing Domain: `ErrorLog.cs` — 7 fields
### New Files:
| File | Purpose |
|------|---------|
| `RetryPolicy.cs` (`Domain/Errors/`) | Retry policy entity — **10 fields** |
| `CircuitBreakerState.cs` (`Domain/Errors/`) | Circuit state — **9 fields** |
| `DeadLetterEntry.cs` (`Domain/Errors/`) | Dead-letter queue — **12 fields** |
| `IErrorHandlingService.cs` (`Application/Errors/`) | Interface — **10 methods** |
| `ErrorHandlingService.cs` (`Infrastructure/Errors/`) | Implementation |
| `ErrorReportGenerator.cs` (`Infrastructure/Errors/`) | Report generation |

---

## ENTITY: RetryPolicy — 10 Fields
```
├── Id, AgentKey, MaxRetries (3), BackoffSeconds ([30,120,300]), BackoffType ("exponential"|"linear"),
├── RetryOnExceptions (List<string>), SkipOnExceptions (List<string>), TimeoutSeconds (300),
├── IsEnabled (bool), LastUpdated
```

## ENTITY: CircuitBreakerState — 9 Fields
```
├── Id, AgentKey, State ("Closed"|"Open"|"HalfOpen"), FailureCount, Threshold (3),
├── PauseMinutes (10), LastFailureAt, OpenedAt, NextRetryAt
```

## ENTITY: DeadLetterEntry — 12 Fields
```
├── Id, JobId, AgentKey, Stage, OriginalError, AllErrors (List<string>), RetryAttempts,
├── FirstFailedAt, LastFailedAt, IsResolvable, ResolutionNotes, ArchivedAt
```

---

## INTERFACE: IErrorHandlingService — 10 Methods
```csharp
Task HandleErrorAsync(Guid jobId, string agentKey, string error, CancellationToken ct);
Task<bool> ShouldRetryAsync(Guid jobId, string agentKey, CancellationToken ct);
Task RetryJobAsync(Guid jobId, string agentKey, CancellationToken ct);
Task MoveToDeadLetterAsync(Guid jobId, string reason, CancellationToken ct);
Task<CircuitBreakerState> GetCircuitStateAsync(string agentKey, CancellationToken ct);
Task OpenCircuitBreakerAsync(string agentKey, CancellationToken ct);
Task CloseCircuitBreakerAsync(string agentKey, CancellationToken ct);
Task<List<DeadLetterEntry>> GetDeadLetterQueueAsync(CancellationToken ct);
Task<ErrorSummary> GetErrorSummaryAsync(CancellationToken ct);
Task ResolveDeadLetterAsync(Guid entryId, string resolution, CancellationToken ct);
```

---

## RETRY STRATEGY
```
Attempt 1 → Wait 30 seconds → Retry
Attempt 2 → Wait 120 seconds → Retry
Attempt 3 → Wait 300 seconds → Retry
Attempt 4 → Move to Dead Letter Queue
```

## CIRCUIT BREAKER LOGIC
```
Closed → 3 consecutive failures → Open (pause 10 min) → HalfOpen (try 1 job) → Success → Closed
                                                                              → Failure → Open
```

---

## DRIVE ERROR FOLDER
```
/Errors/
├── retry/
│   ├── {jobId}_attempt_1.json
│   ├── {jobId}_attempt_2.json
│   └── {jobId}_attempt_3.json
└── dead-letter/
    └── {jobId}_dead.json
```

---

## SIGNALR EVENTS
| Event | Payload |
|-------|---------|
| `OnJobRetrying` | `{ jobId, attempt, nextRetryIn }` |
| `OnJobDeadLettered` | `{ jobId, totalAttempts, reason }` |
| `OnCircuitBreakerOpen` | `{ agentKey, failureCount }` |
| `OnCircuitBreakerClosed` | `{ agentKey }` |

## REST API ENDPOINTS
```
GET    /api/errors/summary              → Error summary
GET    /api/errors/dead-letter          → Dead letter queue
POST   /api/errors/dead-letter/{id}/resolve → Resolve dead letter
POST   /api/errors/circuit/{agentKey}/reset → Reset circuit breaker
GET    /api/errors/retry-queue          → Current retry queue
```

## EF CORE: `DbSet<ErrorLog>`, `DbSet<DeadLetterEntry>`, `DbSet<CircuitBreakerState>`

## ESTIMATED TIME: 3-4 hours
