using AiContentFactory.Application.Agents;
using AiContentFactory.Domain.Analytics;
using AiContentFactory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AiContentFactory.Infrastructure.Analytics;

public class AnalyticsAgent : IAnalyticsAgent
{
    private readonly AnalyticsCollector _collector;
    private readonly ViralPatternDetector _patternDetector;
    private readonly FeedbackLoopEngine _feedbackEngine;
    private readonly ContentScoreCalculator _scoreCalculator;
    private readonly StudioDbContext _dbContext;
    private readonly ILogger<AnalyticsAgent> _logger;

    public AnalyticsAgent(
        AnalyticsCollector collector,
        ViralPatternDetector patternDetector,
        FeedbackLoopEngine feedbackEngine,
        ContentScoreCalculator scoreCalculator,
        StudioDbContext dbContext,
        ILogger<AnalyticsAgent> logger)
    {
        _collector = collector;
        _patternDetector = patternDetector;
        _feedbackEngine = feedbackEngine;
        _scoreCalculator = scoreCalculator;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<AnalyticsReport> CollectDailyAnalyticsAsync(CancellationToken ct)
    {
        _logger.LogInformation("Starting daily analytics collection...");
        
        // 1. Collect
        var stats = await _collector.CollectDailyStatsAsync(ct);
        
        // 2. Detect Patterns
        var patterns = await _patternDetector.DetectPatternsAsync(stats, ct);
        
        // 3. Create Report
        var report = new AnalyticsReport
        {
            Id = Guid.NewGuid(),
            ReportDate = DateTimeOffset.UtcNow,
            TotalVideosAnalyzed = stats.Count,
            TotalViews = stats.Sum(s => s.Views),
            TotalLikes = stats.Sum(s => s.Likes),
            TotalComments = stats.Sum(s => s.Comments),
            TotalShares = stats.Sum(s => s.Shares),
            DetectedPatterns = patterns,
            Recommendations = patterns.Select(p => $"Action based on {p.PatternType}: {p.Description}").ToList()
        };

        // 4. Save stats to DB
        _dbContext.VideoAnalytics.AddRange(stats);
        _dbContext.AnalyticsReports.Add(report);
        await _dbContext.SaveChangesAsync(ct);
        
        // 5. Apply Feedback Loop
        await ApplyFeedbackAsync(report, ct);
        
        return report;
    }

    public async Task<List<VideoAnalytics>> CollectPlatformStatsAsync(string platform, CancellationToken ct)
    {
        return await _collector.CollectDailyStatsAsync(ct);
    }

    public async Task<List<ViralPattern>> DetectPatternsAsync(List<VideoAnalytics> stats, CancellationToken ct)
    {
        return await _patternDetector.DetectPatternsAsync(stats, ct);
    }

    public async Task ApplyFeedbackAsync(AnalyticsReport report, CancellationToken ct)
    {
        await _feedbackEngine.ApplyFeedbackAsync(report, ct);
    }

    public async Task<AnalyticsReport?> GetLatestReportAsync(CancellationToken ct)
    {
        return await _dbContext.AnalyticsReports
            .OrderByDescending(r => r.ReportDate)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<AnalyticsReport>> GetReportHistoryAsync(int days, CancellationToken ct)
    {
        var cutoff = DateTimeOffset.UtcNow.AddDays(-days);
        return await _dbContext.AnalyticsReports
            .Where(r => r.ReportDate >= cutoff)
            .OrderByDescending(r => r.ReportDate)
            .ToListAsync(ct);
    }

    public async Task<double> CalculateContentScoreAsync(Guid videoId, CancellationToken ct)
    {
        var stats = await _dbContext.VideoAnalytics
            .Where(s => s.VideoId == videoId)
            .FirstOrDefaultAsync(ct);
            
        return stats != null ? _scoreCalculator.CalculateScore(stats) : 0;
    }

    public async Task UpdateGlobalMemoryWithInsightsAsync(AnalyticsReport report, CancellationToken ct)
    {
        await ApplyFeedbackAsync(report, ct);
    }
}
