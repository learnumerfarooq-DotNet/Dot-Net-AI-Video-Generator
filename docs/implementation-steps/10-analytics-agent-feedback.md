# 10 — Analytics Agent & Feedback Loop

## Purpose
The Analytics Agent runs as a **daily Hangfire cron job (3 AM)** that collects performance statistics from all publishing platforms, detects viral patterns, and feeds insights back into the system. The feedback loop writes analytics data to `/Logs/analytics/` on Drive and updates Global Memory, which the Trend Agent and Brain then use to adjust future content strategy.

---

## FILE MAP
### Existing: `AnalyticsCollector.cs`, `ViralPatternDetector.cs`, `FeedbackLoopEngine.cs`, `DailyAnalyticsJob.cs` (Infrastructure/Analytics/)
### Existing Domain: `AnalyticsModels.cs` (Domain/Analytics/) — `VideoAnalytics` (15 fields), `ViralPattern` (5 fields), `PlatformPerformanceReport` (5 fields)
### New Files:
| File | Purpose |
|------|---------|
| `AnalyticsReport.cs` (`Domain/Analytics/`) | Daily report entity — **16 fields** |
| `IAnalyticsAgent.cs` (`Application/Agents/`) | Interface — **8 methods** |
| `AnalyticsPrompts.cs` (`Infrastructure/Analytics/`) | AI prompt templates |
| `PlatformStatsCollector.cs` (`Infrastructure/Analytics/`) | Per-platform stats |
| `ContentScoreCalculator.cs` (`Infrastructure/Analytics/`) | Content scoring engine |

---

## ENTITY: AnalyticsReport — 16 Fields
```
AnalyticsReport
├── Id                    : Guid
├── ReportDate            : DateTimeOffset
├── Period                : string         ("daily" | "weekly" | "monthly")
├── TotalVideosAnalyzed   : int
├── TotalViews            : long
├── TotalLikes            : long
├── TotalComments         : long
├── TotalShares           : long
├── AverageCTR            : double
├── AverageWatchTime      : double
├── AverageEngagement     : double
├── TopPerformingVideos   : List<Guid>     (top 10 by views)
├── WorstPerformingVideos : List<Guid>     (bottom 10)
├── DetectedPatterns      : List<ViralPattern>
├── Recommendations       : List<string>   (AI-generated recommendations)
├── DriveFileId           : string?        (saved to /Logs/analytics/)
```

---

## INTERFACE: IAnalyticsAgent — 8 Methods
```csharp
Task<AnalyticsReport> CollectDailyAnalyticsAsync(CancellationToken ct);
Task<List<VideoAnalytics>> CollectPlatformStatsAsync(string platform, CancellationToken ct);
Task<List<ViralPattern>> DetectPatternsAsync(List<VideoAnalytics> stats, CancellationToken ct);
Task ApplyFeedbackAsync(AnalyticsReport report, CancellationToken ct);
Task<AnalyticsReport?> GetLatestReportAsync(CancellationToken ct);
Task<List<AnalyticsReport>> GetReportHistoryAsync(int days, CancellationToken ct);
Task<double> CalculateContentScoreAsync(Guid videoId, CancellationToken ct);
Task UpdateGlobalMemoryWithInsightsAsync(AnalyticsReport report, CancellationToken ct);
```

---

## CLASS: AnalyticsCollector (Enhanced) — 6 Methods

### Method 1: `CollectDailyStatsAsync()` — Collect from all platforms
- For each enabled platform (YouTube, TikTok, Instagram, Facebook, LinkedIn):
  - Call platform API to get video stats (views, likes, comments, shares, CTR, watch time)
  - Map to `VideoAnalytics` entities
  - Save to database
- Aggregate totals for daily report

### Method 2: `CollectYouTubeStatsAsync()` — YouTube Analytics API
- Get video performance data for published videos
- Map: views, likes, comments, shares, averageViewDuration, clickThroughRate

### Method 3: `CollectTikTokStatsAsync()` — TikTok Analytics
### Method 4: `CollectInstagramStatsAsync()` — Instagram Insights API
### Method 5: `CollectFacebookStatsAsync()` — Facebook Insights
### Method 6: `CollectLinkedInStatsAsync()` — LinkedIn Analytics

---

## CLASS: ViralPatternDetector (Enhanced) — 5 Methods

### Method 1: `DetectPatternsAsync(List<VideoAnalytics> stats)`
- Call Decision Engine to analyze stats
- AI identifies patterns like:
  - "Videos uploaded at 6 PM get 3x more views"
  - "Shorts with hooks in first 3 seconds get 2x engagement"
  - "Tech topics outperform entertainment by 40%"
  
### Method 2: `DetectUploadTimePatterns()` — best upload hours
### Method 3: `DetectContentTypePatterns()` — short vs long performance
### Method 4: `DetectTopicPatterns()` — which topics perform best
### Method 5: `CalculateConfidence(ViralPattern pattern)` — statistical confidence

---

## CLASS: FeedbackLoopEngine (Enhanced) — 5 Methods

### Method 1: `ApplyFeedbackAsync(List<ViralPattern> patterns)`
- Write analytics report JSON to `/Logs/analytics/{date}.json` on Drive
- Update Global Memory:
  - `AnalyticsSummary` → latest aggregated stats
  - `ContentStrategy` → AI-adjusted strategy based on patterns
  - `ScheduleSlots` → adjust peak hours based on actual performance
- Signal Trend Agent to re-read analytics data

### Method 2: `UpdateContentStrategyAsync(AnalyticsReport report)` — AI recalculates strategy
### Method 3: `AdjustUploadScheduleAsync(List<ViralPattern> patterns)` — shift peak hours
### Method 4: `GenerateRecommendationsAsync(AnalyticsReport report)` — AI recommendations
### Method 5: `WriteToDriveAsync(AnalyticsReport report)` — save report to Drive

---

## FEEDBACK LOOP FLOW
```
Analytics Agent (daily 3 AM)
    ↓
Collect stats from YouTube/TikTok/Instagram/Facebook/LinkedIn
    ↓
Detect viral patterns (AI analysis)
    ↓
Generate recommendations
    ↓
Write to /Logs/analytics/ on Drive
    ↓
Update Global Memory (AnalyticsSummary, ContentStrategy)
    ↓
Trend Agent reads updated analytics on next hourly tick
    ↓
Brain adjusts scheduling based on new insights
```

---

## PROMPT TEMPLATE

### System Prompt
```
You are a video analytics expert. Analyze performance data
across multiple platforms and identify patterns that drive
viral success. Provide actionable recommendations for
improving content strategy and upload scheduling.
```

---

## HANGFIRE SCHEDULE
- Recurring job: daily at 3 AM UTC (`Cron.Daily(3)`)
- Job ID: `"daily-analytics-loop"`

## OPENROUTER MODEL
| Model | `meta-llama/llama-3.1-8b-instruct:free` |

---

## SIGNALR EVENTS
| Event | Payload |
|-------|---------|
| `OnAnalyticsCollectionStarted` | `{ timestamp }` |
| `OnAnalyticsCollectionComplete` | `{ videosAnalyzed, patternsFound }` |
| `OnViralPatternDetected` | `{ patternType, confidence, description }` |
| `OnFeedbackLoopApplied` | `{ recommendationCount }` |

---

## REST API ENDPOINTS
```
POST   /api/agents/analytics/collect         → Force analytics collection
GET    /api/agents/analytics/latest          → Get latest report
GET    /api/agents/analytics/history?days=30 → Get report history
GET    /api/agents/analytics/patterns        → Get detected patterns
GET    /api/agents/analytics/video/{videoId} → Get per-video analytics
GET    /api/agents/analytics/recommendations → Get AI recommendations
```

## EF CORE: `DbSet<AnalyticsReport>` with JSONB, `DbSet<VideoAnalytics>`, `DbSet<ViralPattern>`

## ESTIMATED TIME: 5-7 hours
