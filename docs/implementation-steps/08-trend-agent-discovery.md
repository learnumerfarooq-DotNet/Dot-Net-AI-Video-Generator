# 08 — Trend Agent Discovery & Scheduling

## Purpose
The Trend Agent runs as an **hourly Hangfire cron job** that discovers trending topics by scraping the top 50 sites (configured in Global Memory), then uses AI to analyze trends and create upload schedule slots. It searches Tier 1 sites first (YouTube, TikTok, Google Trends, Reddit, Twitter), then Tier 2 (news sites), then Tier 3 (niche sites). Falls back to OpenRouter web_search tool only after all configured sites are checked.

---

## FILE MAP
### Existing: `TrendDiscoveryJob.cs`, `SiteScraper.cs`, `TrendDecisionAgent.cs`, `TrendScheduler.cs` (Infrastructure/Trends/)
### New Files:
| File | Purpose |
|------|---------|
| `TrendResult.cs` (`Domain/Trends/`) | Trend result entity — **14 fields** |
| `ScrapeResult.cs` (`Domain/Trends/`) | Per-site scrape result — **10 fields** |
| `ITrendAgent.cs` (`Application/Agents/`) | Interface — **7 methods** |
| `TrendPrompts.cs` (`Infrastructure/Trends/`) | Prompt templates |
| `OpenRouterWebSearch.cs` (`Infrastructure/Trends/`) | Fallback web search |
| `TrendAnalyzer.cs` (`Infrastructure/Trends/`) | AI trend analysis |

---

## ENTITY: TrendResult — 14 Fields
```
TrendResult
├── Id                 : Guid
├── DiscoveredAt       : DateTimeOffset
├── Topics             : List<DiscoveredTopic>    (trending topics found)
├── PlannedUploads     : List<PlannedUpload>      (scheduled uploads)
├── AnalysisSummary    : string                   (AI analysis of trends)
├── ValidUntil         : DateTimeOffset            (when trends expire)
├── TotalSitesScraped  : int
├── SuccessfulScrapes  : int
├── FailedScrapes      : int
├── UsedOpenRouterFallback : bool
├── TopKeywords        : List<string>
├── TopHashtags        : List<string>
├── ConfidenceScore    : double
├── DriveFileId        : string?       (saved to /Scheduler/slots/)
```

## SUB-ENTITY: DiscoveredTopic — 10 Fields
```
├── Keyword, Source, Category, RelevanceScore (0-1), SearchVolume, Competition ("low"|"medium"|"high"),
├── SuggestedPlatforms (List<string>), ContentType ("short"|"long"|"both"), Rank (1-50), DiscoveredAt
```

## ENTITY: ScrapeResult — 10 Fields
```
├── Id, SiteUrl, Tier (1/2/3), Success, ErrorMessage, TopicsFound (int), RawContent, ScrapedAt, DurationMs, ResponseCode
```

---

## INTERFACE: ITrendAgent — 7 Methods
```csharp
Task<TrendResult> DiscoverTrendsAsync(CancellationToken ct);
Task<List<ScrapeResult>> ScrapeAllSitesAsync(CancellationToken ct);
Task<ScrapeResult> ScrapeSiteAsync(string siteUrl, int tier, CancellationToken ct);
Task<TrendResult> AnalyzeTrendsAsync(List<ScrapeResult> scrapes, CancellationToken ct);
Task<List<PlannedUpload>> CreateScheduleSlotsAsync(TrendResult trends, CancellationToken ct);
Task<TrendResult?> GetLatestTrendResultAsync(CancellationToken ct);
Task<List<TrendResult>> GetTrendHistoryAsync(int days, CancellationToken ct);
```

---

## CLASS: SiteScraper (Enhanced) — 6 Methods

### Method 1: `ScrapeAllAsync()` — scrape Tier 1, then Tier 2, then Tier 3
- Load tier sites from Global Memory `TrendAgentConfig`
- Scrape in order: Tier 1 → Tier 2 → Tier 3
- Stop early if enough topics found (>20)
- Track success/failure per site

### Method 2: `ScrapeSiteAsync(string url, int tier)` — HTTP GET + HTML parse
- Use `IHttpClientFactory` for HTTP requests
- Parse HTML for trending content (h1, h2, trending sections)
- Extract: title, description, keywords
- Timeout: 10 seconds per site

### Method 3: `ScrapeGoogleTrends()` — Special handler for Google Trends API
### Method 4: `ScrapeYouTubeTrending()` — YouTube trending page parser
### Method 5: `ScrapeRedditHot()` — Reddit /r/popular hot posts
### Method 6: `FallbackToOpenRouter()` — OpenRouter web_search tool as last resort

---

## CLASS: TrendAnalyzer — 4 Methods

### Method 1: `AnalyzeTrendsAsync(List<ScrapeResult> scrapes)` — AI analysis
- Build context from all successful scrape results
- Call Decision Engine with `DecisionType.TrendDiscovery`
- Parse `TrendDecisionPayload` from AI response
- Score and rank topics by relevance

### Method 2: `DeduplicateTopics(List<DiscoveredTopic> topics)` — remove duplicates
### Method 3: `ScoreRelevance(DiscoveredTopic topic)` — calculate relevance score
### Method 4: `FilterByNiche(List<DiscoveredTopic> topics, List<string> preferredNiches)` — filter

---

## CLASS: TrendScheduler (Enhanced) — 5 Methods

### Method 1: `ScheduleUploadsAsync(TrendDecisionPayload decision)`
- Load peak slots from Global Memory: `[8am, 12pm, 6pm, 9pm UTC+5]`
- Match trending topics to upload slots
- Create `ScheduleSlot` entries in Global Memory
- Save schedule JSON to `/Scheduler/slots/` on Drive
- For shorts: assign to `/Scheduler/shorts/`
- For long-form: assign to `/Scheduler/main/`

### Method 2: `AssignJobToSlot(Guid jobId, ScheduleSlot slot)` — link job to slot
### Method 3: `GetAvailableSlots(DateTimeOffset from, DateTimeOffset to)` — get open slots
### Method 4: `RescheduleSlot(Guid slotId, DateTimeOffset newTime)` — move a slot
### Method 5: `MarkSlotCompleted(Guid slotId)` — mark slot as uploaded

---

## SCRAPING PRIORITY ORDER

| Priority | Tier | Sites | Approach |
|----------|------|-------|----------|
| 1 | Tier 1 | youtube.com, tiktok.com, google.com/trends, trends.google.com, reddit.com, twitter.com, x.com, instagram.com | Must-check first |
| 2 | Tier 2 | bbc.com, cnn.com, reuters.com, techcrunch.com, theverge.com, wired.com | News sources |
| 3 | Tier 3 | buzzfeed.com, mashable.com, medium.com, dev.to, hackernoon.com, producthunt.com | Niche/social |
| 4 | Fallback | OpenRouter web_search tool | Only if all 50 sites checked |

---

## PROMPT TEMPLATE

### System Prompt
```
You are a trend analysis AI for video content creators.
Analyze scraped web data and identify the top trending topics
that would make engaging video content. Rank by relevance and
virality potential. Create an upload schedule for peak hours.
```

### JSON Schema
```json
{
    "topics": [
        { "keyword": "string", "source": "string", "relevanceScore": 0.9, "category": "string",
          "suggestedPlatforms": ["YouTube"], "contentType": "short" }
    ],
    "plannedUploads": [
        { "topic": "string", "scheduledTime": "ISO8601", "platforms": ["YouTube", "TikTok"] }
    ],
    "analysisSummary": "string",
    "validUntil": "ISO8601"
}
```

---

## HANGFIRE SCHEDULE
- Recurring job: every hour (`Cron.Hourly`)
- Job ID: `"hourly-trend-discovery"`
- Queue: `ai`

## OPENROUTER MODEL
| Model | `mistralai/mistral-7b-instruct:free` |
| Feature | `web_search` tool enabled |

---

## SIGNALR EVENTS
| Event | Payload |
|-------|---------|
| `OnTrendDiscoveryStarted` | `{ timestamp }` |
| `OnTrendDiscoveryComplete` | `{ topicCount, slotCount, sitesScraped }` |
| `OnNewTrendFound` | `{ keyword, relevanceScore, source }` |

---

## REST API ENDPOINTS
```
POST   /api/agents/trend/discover           → Force trend discovery
GET    /api/agents/trend/latest             → Get latest trends
GET    /api/agents/trend/history?days=7     → Get trend history
GET    /api/agents/trend/schedule           → Get current schedule slots
POST   /api/agents/trend/schedule/reschedule → Reschedule a slot
```

## EF CORE: `DbSet<TrendResult>`, `DbSet<ScrapeResult>` with JSONB for lists

## ESTIMATED TIME: 5-7 hours
