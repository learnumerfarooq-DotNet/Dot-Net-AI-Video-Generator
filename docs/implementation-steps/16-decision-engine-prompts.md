# 16 — Decision Engine & Prompt Templates

## Purpose
The Decision Engine is the AI brain behind every agent. It manages prompt templates, calls OpenRouter with structured JSON output expectations, validates responses, caches results, and tracks decision history. This file details the complete prompt template system for all 8 decision types.

---

## FILE MAP
### Existing: `DecisionEngine.cs`, `DecisionValidator.cs`, `AgentDecisionFacade.cs`, `DecisionCache.cs`, `DefaultPrompts.cs` (Infrastructure/Decisions/)
### Existing Domain: `AgentDecision.cs`, `DecisionPayloads.cs`
### Files to Enhance/Create:
| File | Purpose |
|------|---------|
| `DefaultPrompts.cs` (enhance) | All 8 prompt templates — one per DecisionType |
| `PromptTemplateSeeder.cs` (`Infrastructure/Persistence/`) | Seed all templates to DB |
| `PromptVersionManager.cs` (`Infrastructure/Decisions/`) | Version management — **5 methods** |
| `DecisionAuditLog.cs` (`Domain/Decisions/`) | Audit trail — **10 fields** |

---

## ENTITY: DecisionAuditLog — 10 Fields
```
├── Id, DecisionId, AgentKey, DecisionType, InputContextHash, RawResponse, ValidatedResponse,
├── ConfidenceScore, LatencyMs, CreatedAt
```

---

## ALL 8 PROMPT TEMPLATES

### Template 1: ScriptGeneration
- **Agent**: script-gen-agent
- **System**: "Expert video script writer for social media..."
- **Input**: fileName, duration, resolution, style, tone
- **Output Schema**: { title, hook, body, callToAction, keywords[], hashtags[] }
- **Model**: llama-3.1-8b-instruct:free

### Template 2: VideoEditing
- **Agent**: edit-agent
- **System**: "Professional video editor AI..."
- **Input**: videoAnalysis, scriptContent, style preferences
- **Output Schema**: { segments[], captions[], audioAdjustments[], colorGrading }
- **Model**: gemini-flash-1.5-8b:free

### Template 3: ShortGeneration
- **Agent**: shorts-agent
- **System**: "Expert at identifying viral moments in videos..."
- **Input**: duration, sceneChanges[], audioHotspots[], scriptSummary
- **Output Schema**: { shorts[{ startTime, endTime, title, hook, rationale }] }
- **Model**: llama-3.2-90b:free

### Template 4: ShortEditing
- **Agent**: short-edit-agent
- **System**: "Short-form video editor specializing in hooks and captions..."
- **Input**: clipDuration, hookStyle, captionPreference
- **Output Schema**: { hookOverlay, captions[], musicTrack, emojiOverlays[] }
- **Model**: gemini-flash-1.5-8b:free

### Template 5: TrendDiscovery
- **Agent**: trend-agent
- **System**: "Trend analysis AI for video content creators..."
- **Input**: scrapedData, previousTrends, currentPerformance
- **Output Schema**: { topics[], plannedUploads[], analysisSummary }
- **Model**: mistral-7b-instruct:free

### Template 6: UploadMetadata
- **Agent**: upload-agent
- **System**: "Social media content optimizer..."
- **Input**: scriptContent, trendKeywords, targetPlatform
- **Output Schema**: { title, description, keywords[], hashtags[], category }
- **Model**: llama-3.1-8b-instruct:free

### Template 7: AnalyticsInsight
- **Agent**: analytics-agent
- **System**: "Video analytics expert..."
- **Input**: videoStats, platformData, historicalPerformance
- **Output Schema**: { patterns[], recommendations[], contentScore }
- **Model**: llama-3.1-8b-instruct:free

### Template 8: ContentVariation
- **Agent**: main-brain
- **System**: "Content strategy AI..."
- **Input**: currentStrategy, performance, trends
- **Output Schema**: { focusTopics[], avoidTopics[], contentMix, toneAdjustment }
- **Model**: llama-3.1-8b-instruct:free

---

## CLASS: PromptVersionManager — 5 Methods
```csharp
Task<PromptTemplate> GetActiveTemplateAsync(string agentKey, DecisionType type, CancellationToken ct);
Task<PromptTemplate> CreateNewVersionAsync(string agentKey, DecisionType type, string system, string user, string schema, CancellationToken ct);
Task ActivateVersionAsync(Guid templateId, CancellationToken ct);
Task<List<PromptTemplate>> GetVersionHistoryAsync(string agentKey, DecisionType type, CancellationToken ct);
Task RollbackAsync(string agentKey, DecisionType type, string version, CancellationToken ct);
```

---

## CLASS: PromptTemplateSeeder — Seed all 8 templates on startup

Each template includes:
- `SystemPrompt` (~200-500 chars)
- `UserPromptTemplate` (~300-800 chars with `{placeholders}`)
- `JsonOutputSchema` (~200-600 chars)

---

## DECISION ENGINE ENHANCEMENTS

### Enhanced `MakeDecisionAsync()`:
- Add audit logging: save every AI call to `DecisionAuditLog`
- Add latency tracking
- Add retry on parse failure (up to 2 attempts)
- Add model fallback: if primary model fails, try fallback model

### Fallback Chain:
```
Primary Model (per-agent) → Secondary Model → Default Model (llama-3.1-8b)
```

---

## OPENROUTER MODELS (ALL FREE)
| Agent | Primary Model | Fallback |
|-------|--------------|----------|
| script-gen | llama-3.1-8b-instruct:free | mistral-7b:free |
| edit-agent | gemini-flash-1.5-8b:free | llama-3.1-8b:free |
| shorts-agent | llama-3.2-90b:free | llama-3.1-8b:free |
| short-edit | gemini-flash-1.5-8b:free | llama-3.1-8b:free |
| trend-agent | mistral-7b:free | llama-3.1-8b:free |
| upload-agent | llama-3.1-8b:free | mistral-7b:free |
| analytics | llama-3.1-8b:free | mistral-7b:free |
| main-brain | llama-4-maverick:free | llama-3.1-8b:free |

---

## REST API ENDPOINTS
```
GET    /api/decisions/templates                    → All templates
GET    /api/decisions/templates/{agentKey}/{type}   → Get active template
POST   /api/decisions/templates                    → Create new version
PUT    /api/decisions/templates/{id}/activate       → Activate version
GET    /api/decisions/audit?agentKey=&type=         → Decision audit log
GET    /api/decisions/cache/stats                  → Cache hit/miss stats
POST   /api/decisions/cache/clear                  → Clear all caches
```

## EF CORE: `DbSet<DecisionAuditLog>`, enhance `DbSet<PromptTemplate>`

## ESTIMATED TIME: 4-5 hours
