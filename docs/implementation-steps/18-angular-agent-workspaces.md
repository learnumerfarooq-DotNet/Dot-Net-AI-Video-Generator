# 18 — Angular Agent Workspaces

## Purpose
Each of the 11 agents gets a dedicated workspace UI with: chat interface (AI conversation), agent controls (start/stop/configure), local memory viewer, run history timeline, and real-time status monitoring.

---

## EXISTING COMPONENTS (11 agents, same pattern)
All in `features/agents/`:
- `main-brain/`, `trend-agent/`, `script-agent/`, `video-generation-agent/`
- `shorts-agent-1/`, `shorts-agent-2/`
- `youtube-agent/`, `tiktok-agent/`, `instagram-agent/`, `facebook-agent/`, `linkedin-agent/`

Each has: `.ts` (~80 lines), `.html` (~60 lines), `.css` (~shared via `agent-workspace-shared.css`)

---

## ENHANCEMENT PLAN PER AGENT WORKSPACE

### Shared Layout (all agents) — **~120 lines HTML**
```
├── Header Bar
│   ├── Agent Icon + Name
│   ├── Status Badge (Connected/Disconnected/Running/Error)
│   ├── Model Name (e.g., "llama-3.1-8b-instruct:free")
│   └── Actions: [Start Run] [Stop] [Configure] [View Memory]
├── Two-Column Layout
│   ├── Left Panel (60%): Chat Interface
│   │   ├── Message Thread (AI responses with markdown rendering)
│   │   ├── Input Field with Send Button
│   │   └── "Clear History" button
│   └── Right Panel (40%): Agent Info
│       ├── Local Memory Card (key-value display)
│       ├── Run History Timeline
│       ├── Active Job Card (if running)
│       └── Error Log (last 5 errors)
└── Footer: Last Run Time, Total Runs, Success Rate
```

---

## SHARED TYPES TO ADD (`content-factory.models.ts`)

### AgentWorkspaceState — 10 Fields
```typescript
export type AgentWorkspaceState = {
    agentKey: string;
    status: 'idle' | 'running' | 'error' | 'disabled';
    isConnected: boolean;
    modelName: string;
    localMemory: Record<string, any>;
    recentRuns: AgentRun[];
    activeJobId: string | null;
    chatMessages: ChatMessage[];
    errorLog: ErrorLog[];
    lastRunAt: string | null;
};
```

---

## AGENTS STORE ENHANCEMENTS

**File**: `features/agents/store/agents.store.ts`
**New methods**: 8

```typescript
// New methods:
startAgentRun(agentKey: string);           // POST /api/agents/{key}/run
stopAgentRun(agentKey: string);            // POST /api/agents/{key}/stop
sendChatMessage(agentKey: string, message: string);  // POST /api/agents/{key}/chat
clearChatHistory(agentKey: string);        // DELETE /api/agents/{key}/chat/cleanup
getLocalMemory(agentKey: string);          // GET /api/memory/local/{key}
updateLocalMemory(agentKey: string, config: any);    // PUT /api/memory/local/{key}
getRunHistory(agentKey: string);           // GET /api/agents/{key}/runs
getErrorLog(agentKey: string);             // GET /api/errors/{key}
```

---

## AGENT-SPECIFIC ENHANCEMENTS

### Main Brain Workspace
- **Extra Panel**: Brain State overview (tick #, active jobs, circuit breaker)
- **Extra Control**: Pause/Resume Brain button
- **Extra Widget**: Pipeline visualization mini-map
- **Chat context**: Can ask Brain about pipeline state, force actions

### Script Agent Workspace
- **Extra Panel**: Script preview with syntax highlighting
- **Extra Control**: Style/Tone dropdowns for script regeneration
- **Extra Widget**: Generated script viewer with copy button

### Shorts Agent Workspace
- **Extra Panel**: Clip timeline with thumbnail previews
- **Extra Control**: Max shorts slider (1-5), Min duration slider (15-60)
- **Extra Widget**: Short clip cards with engagement scores

### Trend Agent Workspace
- **Extra Panel**: Trending topics list with source badges
- **Extra Control**: "Force Discovery" button
- **Extra Widget**: Trend history chart (7-day sparkline)

### Upload Platform Agents (YouTube, TikTok, Instagram, Facebook, LinkedIn)
- **Extra Panel**: Platform connection status + OAuth button
- **Extra Control**: Upload queue for this platform
- **Extra Widget**: Platform-specific analytics (views, likes for this platform)
- **Extra Panel**: Published videos list with links

---

## AGENT WORKSPACE SERVICES

**File**: `features/agents/services/agent-workspace.service.ts`
**Methods**: 10

```typescript
export class AgentWorkspaceService {
    startRun(agentKey: string): Observable<{ runId: string }>;
    stopRun(agentKey: string): Observable<void>;
    sendChat(agentKey: string, message: string): Observable<AgentChatResponse>;
    clearChat(agentKey: string): Observable<void>;
    streamChat(agentKey: string, message: string): Observable<AgentStreamChunk>;
    getRunHistory(agentKey: string, limit?: number): Observable<AgentRun[]>;
    getActiveJob(agentKey: string): Observable<VideoPipelineJob | null>;
    getLocalMemory(agentKey: string): Observable<any>;
    updateLocalMemory(agentKey: string, config: any): Observable<void>;
    getErrorLog(agentKey: string, limit?: number): Observable<ErrorLog[]>;
}
```

---

## CSS: `agent-workspace-shared.css` ENHANCEMENTS

### New Styles (~200 lines)
```css
/* Chat Interface */
.chat-thread { ... }
.chat-message { ... }
.chat-message--user { ... }
.chat-message--ai { ... }
.chat-input-row { ... }

/* Agent Info Panel */
.agent-info-panel { ... }
.local-memory-card { ... }
.memory-key-value { ... }
.run-history-timeline { ... }
.timeline-item { ... }
.timeline-dot { ... }

/* Status Badges */
.status-badge { ... }
.status-badge--running { animation: pulse 2s infinite; }
.status-badge--error { color: var(--accent-red); }
.status-badge--connected { color: var(--accent-green); }

/* Active Job Card */
.active-job-card { ... }
.progress-bar-container { ... }
.progress-bar-fill { animation: shimmer 2s infinite; }
```

---

## SIGNALR REAL-TIME UPDATES PER AGENT

| Event | Agent Update |
|-------|-------------|
| `OnAgentRunStarted` | Set status=running, show active job card |
| `OnAgentRunCompleted` | Set status=idle, add to run history |
| `OnChatResponse` | Append AI message to chat thread |
| `OnLocalMemoryUpdated` | Refresh memory card |

---

## BACKEND API ENDPOINTS NEEDED

```
POST   /api/agents/{key}/run               → Start manual run
POST   /api/agents/{key}/stop              → Stop running job
POST   /api/agents/{key}/chat              → Send chat message
DELETE /api/agents/{key}/chat/cleanup       → Clear chat history (existing)
GET    /api/agents/{key}/runs?limit=20      → Run history
GET    /api/agents/{key}/active-job         → Current active job
GET    /api/agents/{key}/errors?limit=5     → Recent errors
```

## ESTIMATED TIME: 8-12 hours (all 11 agents)
