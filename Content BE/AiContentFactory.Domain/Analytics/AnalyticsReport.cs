namespace AiContentFactory.Domain.Analytics;

public sealed class AnalyticsReport
{
    public Guid Id { get; set; }
    public DateTimeOffset ReportDate { get; set; }
    public string Period { get; set; } = "daily";
    public int TotalVideosAnalyzed { get; set; }
    public long TotalViews { get; set; }
    public long TotalLikes { get; set; }
    public long TotalComments { get; set; }
    public long TotalShares { get; set; }
    public double AverageCTR { get; set; }
    public double AverageWatchTime { get; set; }
    public double AverageEngagement { get; set; }
    public List<Guid> TopPerformingVideos { get; set; } = new();
    public List<Guid> WorstPerformingVideos { get; set; } = new();
    public List<ViralPattern> DetectedPatterns { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public string? DriveFileId { get; set; }
}
