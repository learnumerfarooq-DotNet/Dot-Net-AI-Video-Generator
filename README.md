# AI Content Factory

MVP scaffold for an autonomous AI content production system.

## What is built

- ASP.NET Core Brain API for task intake and orchestration.
- Memory system with global approval and optional local auto-save.
- Provider interfaces so AI/video/upload services can be swapped by config.
- Script Agent and Upload Agent MVP implementations.
- JSON-backed backlog so generated artifacts survive restarts.

## Run

```powershell
dotnet run --project .\src\AiContentFactory.Api\AiContentFactory.Api.csproj
```

Open the API root shown in the terminal, usually:

```text
http://localhost:5039
```

## Run Dashboard

In another terminal:

```powershell
cd .\src\ai-content-dashboard
npm start
```

Open the Angular URL shown in the terminal, usually:

```text
http://localhost:4200
```

## Try a Brain Task

```powershell
Invoke-RestMethod `
  -Method Post `
  -Uri http://localhost:5039/api/brain/tasks `
  -ContentType "application/json" `
  -Body '{
    "topic": "AI tools for small business",
    "platform": "youtube",
    "format": "short",
    "audience": "small business owners",
    "goal": "explain a practical automation idea",
    "autoSaveLocalMemory": true
  }'
```

## Key Endpoints

- `POST /api/brain/tasks` creates a plan, runs agents, stores backlog, and suggests global memory.
- `GET /api/memory` lists approved memory entries.
- `GET /api/memory/suggestions` lists global memory suggestions waiting for approval.
- `POST /api/memory/suggestions/{id}/approve` approves suggested memory.
- `POST /api/memory/suggestions/{id}/reject` rejects suggested memory.
- `GET /api/backlog` lists generated artifacts.
- `POST /api/backlog/{id}/promote` moves a backlog item to ready.
- `GET /api/providers` shows current provider selections.

## Architecture Notes

Global memory is never written directly by agents. Agents can suggest global learning, and the Brain/API approval path turns accepted suggestions into shared memory. Local memory can be auto-saved per task.

Provider implementations are deliberately thin in this MVP. Replace `TemplateTextProvider`, add concrete video providers, and add real platform upload providers behind the existing interfaces as the system moves into Phase 2.
