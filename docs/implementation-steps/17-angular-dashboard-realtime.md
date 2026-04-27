# 17 — Angular Dashboard & Real-Time Pipeline Visualization

## Purpose
Build a comprehensive real-time dashboard in Angular 21 that visualizes the entire pipeline state, agent statuses, upload progress, analytics, and trends. Uses NgRx Signals for state management and SignalR for real-time updates.

---

## FILE MAP

### Existing Components to ENHANCE
| Component | Path | Changes |
|-----------|------|---------|
| `dashboard-overview` | `features/dashboard/dashboard-overview/` | Add pipeline status cards, brain status |
| `dashboard-usage` | `features/dashboard/dashboard-usage/` | Add real-time API usage charts |
| `dashboard-memory` | `features/dashboard/dashboard-memory/` | Add memory health indicators |
| `dashboard-drive` | `features/dashboard/dashboard-drive/` | Add pipeline folder view |
| `dashboard-published` | `features/dashboard/dashboard-published/` | Add per-platform stats |
| `dashboard-growth` | `features/dashboard/dashboard-growth/` | Add growth projections |

### New Components to CREATE
| Component | Purpose |
|-----------|---------|
| `pipeline-status-card` | Real-time pipeline flow visualization |
| `brain-status-widget` | Main Brain health + tick status |
| `agent-health-grid` | Grid of all 11 agents with status badges |
| `upload-queue-widget` | Upcoming scheduled uploads |
| `trend-radar-widget` | Current trending topics display |
| `analytics-summary-card` | Key metrics (views, likes, CTR) |
| `error-monitor-widget` | Error queue + circuit breaker status |

---

## COMPONENT: pipeline-status-card

### Template Structure — **~80 lines HTML**
```
├── Pipeline Flow Diagram (SVG-based horizontal flow)
│   ├── [RAW] → [Script] → [Edit] → [Shorts] → [Short Edit] → [Upload] → [Published]
│   ├── Each node shows: active job count, last completion time
│   └── Active nodes pulse with animation
├── Active Jobs Table
│   ├── Columns: Video Name, Current Stage, Progress %, Started At, Duration
│   └── Real-time progress bar per job
└── Quick Stats Row: Total Active, Completed Today, Failed Today
```

### TypeScript — **~60 lines**
- Inject `PipelineStore` for real-time job data
- Inject `SignalrService` for live updates
- Computed signals: `activeJobs`, `completedToday`, `failedToday`
- Method: `getStageColor(stage)` → returns color per stage
- Method: `getStageProgress(job)` → returns progress percentage

### CSS — **~100 lines**
- Pipeline flow nodes: circles with connecting lines
- Active node: pulsing green glow animation
- Failed node: red border with error icon
- Progress bars: gradient fill with shimmer animation
- Dark mode support

---

## COMPONENT: brain-status-widget

### Template — **~50 lines**
```
├── Brain Status Badge (Idle/Watching/Processing/Error/Paused)
├── Current Tick: #12,456
├── Last Tick: 30 seconds ago
├── Active Jobs: 3
├── Circuit Breaker: Closed ✓
├── Global Memory: v1.2 (synced 1 min ago)
└── Pause/Resume Button
```

### TypeScript — **~40 lines**
- Signal: `brainStatus` from `PipelineStore`
- Method: `pauseBrain()`, `resumeBrain()` → API calls
- Computed: `lastTickAgo` → "30 seconds ago" formatter

---

## COMPONENT: agent-health-grid

### Template — **~70 lines**
```
├── Grid of 11 Agent Cards (3 columns × 4 rows)
│   Each card shows:
│   ├── Agent icon + name
│   ├── Status badge (Healthy 🟢 / Degraded 🟡 / Failed 🔴 / Disabled ⚫)
│   ├── Last run: "2 min ago"
│   ├── Success rate: "98.5%"
│   └── Run count: 1,245
```

### TypeScript — **~50 lines**
- Inject `AgentsStore` for agent data
- Computed: `agentHealthList` maps agents to health cards
- Method: `getStatusColor(status)` → green/yellow/red/gray
- Method: `formatLastRun(timestamp)` → relative time

---

## COMPONENT: analytics-summary-card

### Template — **~60 lines**
```
├── Total Views (with weekly trend arrow ↑↓)
├── Total Likes
├── Average CTR (with sparkline chart)
├── Average Watch Time
├── Engagement Rate
├── Best Upload Hour: 6 PM UTC+5
├── Top Platform: YouTube
└── Growth: +15% this week
```

---

## DASHBOARD STORE ENHANCEMENTS

**File**: `features/dashboard/store/dashboard.store.ts`
**New computed signals**: 12

```typescript
// New computed signals:
brainStatus          // from SignalR real-time data
activeJobCount       // computed from pipeline store
completedToday       // count of today's completions
failedToday          // count of today's failures
agentHealthSummary   // aggregated agent health
uploadQueueDepth     // pending uploads count
trendingTopics       // latest trend data
analyticsSnapshot    // latest analytics summary
errorCount24h        // errors in last 24 hours
circuitBreakerStatus // open/closed
nextScheduledUpload  // when next upload is due
globalMemoryVersion  // current global.json version
```

---

## SIGNALR HANDLERS TO ADD

| SignalR Event | Dashboard Update |
|---------------|-----------------|
| `BrainTickCompleted` | Update brain status widget |
| `JobStarted` | Add to active jobs list |
| `StageCompleted` | Update pipeline flow diagram |
| `ProgressUpdated` | Update job progress bar |
| `JobFailed` | Add to error monitor |
| `OnTrendDiscoveryComplete` | Update trend radar |
| `OnUploadComplete` | Update published stats |
| `OnAnalyticsCollectionComplete` | Update analytics card |
| `OnCircuitBreakerOpen` | Flash error on brain widget |

---

## CHART LIBRARY

Use Canvas-based charting (no external library) or lightweight:
- Sparkline charts: inline SVG `<path>` elements
- Progress bars: CSS gradients with animation
- Donut charts: SVG `<circle>` with `stroke-dasharray`

---

## RESPONSIVE LAYOUT

```
Desktop (>1200px): 3-column grid
├── Col 1: Pipeline Status + Brain Status
├── Col 2: Agent Health Grid + Error Monitor
└── Col 3: Analytics Summary + Upload Queue + Trends

Tablet (768-1200px): 2-column grid
Mobile (<768px): Single column stack
```

---

## CSS DESIGN SYSTEM

### Colors (Dark Mode)
```css
--bg-primary: #0A0E1A;
--bg-card: #111827;
--bg-card-hover: #1F2937;
--text-primary: #F9FAFB;
--text-secondary: #9CA3AF;
--accent-green: #22C55E;
--accent-blue: #3B82F6;
--accent-purple: #8B5CF6;
--accent-amber: #F59E0B;
--accent-red: #EF4444;
--border: #1F2937;
--glow-green: 0 0 10px rgba(34,197,94,0.3);
```

### Light Mode
```css
--bg-primary: #F8FAFC;
--bg-card: #FFFFFF;
--text-primary: #1E293B;
--accent-green: #16A34A;
```

### Animations
```css
@keyframes pulse-glow { ... }    /* Active node pulsing */
@keyframes shimmer { ... }       /* Progress bar shimmer */
@keyframes fade-in { ... }       /* Card appearance */
@keyframes slide-up { ... }      /* New item animation */
```

---

## ANGULAR ROUTE UPDATES
No new routes needed — dashboard components already routed. Enhance existing components.

## ESTIMATED TIME: 8-10 hours
