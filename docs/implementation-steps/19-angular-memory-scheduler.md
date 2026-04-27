# 19 — Angular Memory & Scheduler UIs

## Purpose
Build the Memory management UI (Global Memory viewer/editor, Local Memory per-agent, Review Queue with approve/reject workflow) and the Scheduler UI (Manual schedule creation, Daily posting view, Retry queue management, Queue execution monitor).

---

## PART A: MEMORY UI

### Existing Components
- `memory-global/` — Global Memory viewer (enhance)
- `memory-local/` — Local Memory per-agent (enhance)
- `memory-review/` — Review Queue (enhance)
- `features/memory/store/memory.store.ts` — Memory store (enhance)
- `features/memory/services/` — Memory API service

---

### COMPONENT: memory-global (Enhanced) — ~120 lines HTML

```
├── Header: "Global Memory" + Version Badge + "Force Refresh" Button
├── Tabs:
│   ├── Folder Registry Tab
│   │   └── Table: Agent Name → Drive Path (editable)
│   ├── Video Constraints Tab
│   │   ├── Shorts: max duration, aspect ratio, resolution, fps, max file size
│   │   └── Long-form: max duration, aspect ratio, resolution, fps
│   ├── Trend Config Tab
│   │   ├── Tier 1 Sites list (editable, add/remove)
│   │   ├── Tier 2 Sites list
│   │   ├── Tier 3 Sites list
│   │   └── OpenRouter fallback toggle
│   ├── Schedule Slots Tab
│   │   └── Timeline view of peak upload slots
│   └── Agent Statuses Tab
│       └── Agent health grid (from Global Memory)
├── Footer: Last Updated timestamp + "Save Changes" button
```

### TypeScript — ~80 lines
```typescript
// Signals:
globalMemory = signal<GlobalMemoryFull | null>(null);
activeTab = signal<string>('folders');
editMode = signal<boolean>(false);

// Methods:
loadGlobalMemory();          // GET /api/memory/global
saveGlobalMemory();          // PUT /api/memory/global
forceRefresh();              // POST /api/memory/global/refresh
addTierSite(tier: number, site: string);
removeTierSite(tier: number, site: string);
updateConstraint(key: string, value: any);
updateFolderMapping(agentKey: string, path: string);
```

---

### COMPONENT: memory-local (Enhanced) — ~100 lines HTML

```
├── Agent Selector Dropdown (11 agents)
├── Selected Agent Card:
│   ├── Agent Name + Status Badge
│   ├── Config JSON Editor (formatted key-value pairs)
│   ├── Run Statistics:
│   │   ├── Total Runs, Successes, Failures
│   │   ├── Success Rate %
│   │   └── Last Run timestamp
│   ├── Actions: [Save Changes] [Reset to Defaults] [Sync to Drive]
│   └── Config History (last 5 changes)
```

### TypeScript — ~70 lines
```typescript
selectedAgent = signal<string>('script-gen-agent');
localMemory = signal<AgentLocalMemory | null>(null);

loadLocalMemory(agentKey: string);
saveLocalMemory(agentKey: string, config: any);
resetToDefaults(agentKey: string);
syncToDrive(agentKey: string);
```

---

### COMPONENT: memory-review (Enhanced) — ~90 lines HTML

```
├── Pending Review Count Badge
├── Review Queue List:
│   Each item:
│   ├── Scope Badge (Global/Local)
│   ├── Agent Name (if local)
│   ├── Content Preview (truncated)
│   ├── Reason for suggestion
│   ├── Created At timestamp
│   └── Actions: [Approve ✓] [Reject ✗] [View Full]
├── Approved/Rejected History (collapsible)
```

---

### MEMORY STORE ENHANCEMENTS
**New methods**: 8
```typescript
loadGlobalMemory();
saveGlobalMemory(memory: GlobalMemoryFull);
refreshGlobalMemory();
loadLocalMemory(agentKey: string);
saveLocalMemory(agentKey: string, config: any);
resetLocalMemory(agentKey: string);
approveMemorySuggestion(id: string);
rejectMemorySuggestion(id: string);
```

---

## PART B: SCHEDULER UI

### Existing Components
- `scheduler-manual/` — Manual schedule creation (enhance)
- `scheduler-daily/` — Daily posting schedule (enhance)
- `scheduler-retry/` — Retry queue (enhance)
- `scheduler-queue/` — Queue execution monitor (enhance)
- `features/scheduler/store/scheduler.store.ts` — Scheduler store

---

### COMPONENT: scheduler-manual (Enhanced) — ~100 lines HTML

```
├── "Create Schedule" Form:
│   ├── Name input
│   ├── Agent dropdown (11 agents)
│   ├── Trigger selector: "Once" | "Daily" | "Hourly" | "Custom Cron"
│   ├── Date/Time picker (for "Once")
│   ├── Cron expression input (for "Custom")
│   ├── Notes textarea
│   ├── Enabled toggle
│   └── [Create Schedule] button
├── Existing Schedules Table:
│   ├── Columns: Name, Agent, Trigger, Status, Next Run, Last Run, Actions
│   ├── Actions: [Edit] [Enable/Disable] [Delete] [Run Now]
│   └── Sort by: Next Run (ascending)
```

---

### COMPONENT: scheduler-daily (Enhanced) — ~80 lines HTML

```
├── Calendar/Timeline View:
│   ├── Today's Schedule (hourly slots)
│   │   ├── 8:00 AM → [Trend Discovery] Running...
│   │   ├── 12:00 PM → [Upload: "AI Tutorial"] Scheduled
│   │   ├── 6:00 PM → [Upload: "Tech News"] Scheduled
│   │   └── 9:00 PM → [Upload: "Quick Tips"] Scheduled
│   └── Peak Hours highlighted (from Global Memory)
├── Upcoming 7-Day View (collapsible)
├── Stats: Uploads Today, Uploads This Week, Success Rate
```

---

### COMPONENT: scheduler-retry (Enhanced) — ~80 lines HTML

```
├── Retry Queue Table:
│   ├── Columns: Job ID, Agent, Error, Retry #, Next Retry At, Actions
│   ├── Actions: [Retry Now] [Move to Dead Letter] [Dismiss]
│   └── Sort by: Next Retry (ascending)
├── Dead Letter Queue (collapsible):
│   ├── Columns: Job ID, Agent, Total Attempts, All Errors, Actions
│   └── Actions: [Resolve] [Re-queue] [Archive]
├── Stats: Pending Retries, Dead Letters, Resolution Rate
```

---

### COMPONENT: scheduler-queue (Enhanced) — ~80 lines HTML

```
├── Queue Depth Chart (last 24 hours)
├── Active Queue Items:
│   ├── Columns: ID, Type (AI/FFmpeg/Upload), Agent, Queued At, Started At, Status
│   └── Real-time progress for running items
├── Queue Health: Workers Active, Queue Depth, Avg Wait Time
├── Hangfire Queue Stats: [AI Queue] [FFmpeg Queue] [Upload Queue] [Default Queue]
```

---

### SCHEDULER STORE ENHANCEMENTS
**New methods**: 10
```typescript
createManualSchedule(draft: ManualScheduleDraft);
updateSchedule(id: string, updates: Partial<ScheduleJob>);
deleteSchedule(id: string);
toggleScheduleEnabled(id: string);
runScheduleNow(id: string);
getRetryQueue();
retryNow(jobId: string);
moveToDeadLetter(jobId: string);
resolveDeadLetter(id: string, resolution: string);
getQueueStats();
```

---

## BACKEND API ENDPOINTS NEEDED

### Memory APIs
```
GET    /api/memory/global                  → Load global memory
PUT    /api/memory/global                  → Save global memory
POST   /api/memory/global/refresh          → Force refresh
GET    /api/memory/local                   → All local memories
GET    /api/memory/local/{agentKey}        → Specific agent
PUT    /api/memory/local/{agentKey}        → Update agent config
POST   /api/memory/local/{agentKey}/reset  → Reset defaults
POST   /api/memory/local/{agentKey}/sync   → Sync to Drive
POST   /api/memory/{id}/approve            → Approve suggestion (existing)
POST   /api/memory/{id}/reject             → Reject suggestion (existing)
```

### Scheduler APIs
```
GET    /api/scheduler/manual               → Get manual schedules
POST   /api/scheduler/manual               → Create schedule (existing)
PUT    /api/scheduler/{id}                 → Update schedule
DELETE /api/scheduler/{id}                 → Delete schedule
POST   /api/scheduler/{id}/toggle          → Enable/disable
POST   /api/scheduler/{id}/run-now         → Run immediately
GET    /api/scheduler/daily                → Get daily posting schedule
GET    /api/scheduler/retry                → Get retry queue
POST   /api/scheduler/retry/{jobId}/now    → Retry immediately
GET    /api/scheduler/queue                → Get queue stats
GET    /api/scheduler/dead-letter          → Get dead letter queue
```

## ESTIMATED TIME: 8-10 hours (Memory: 4h, Scheduler: 4-6h)
