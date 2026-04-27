using AiContentFactory.Application.Memory;
using AiContentFactory.Application.Studio;
using AiContentFactory.Domain.Analytics;
using AiContentFactory.Domain.GlobalMemory;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.IO;

namespace AiContentFactory.Infrastructure.Analytics;

public sealed class FeedbackLoopEngine
{
    private readonly IGoogleDriveService _drive;
    private readonly IGlobalMemoryService _globalMemory;
    private readonly IStudioWorkspaceStore _workspaceStore;
    private readonly ILogger<FeedbackLoopEngine> _logger;

    public FeedbackLoopEngine(
        IGoogleDriveService drive,
        IGlobalMemoryService globalMemory,
        IStudioWorkspaceStore workspaceStore,
        ILogger<FeedbackLoopEngine> logger)
    {
        _drive = drive;
        _globalMemory = globalMemory;
        _workspaceStore = workspaceStore;
        _logger = logger;
    }

    public async Task ApplyFeedbackAsync(AnalyticsReport report, CancellationToken ct = default)
    {
        _logger.LogInformation("Applying feedback loop for report {ReportDate}", report.ReportDate);

        // 1. Write to Drive
        var settings = await _workspaceStore.GetDriveSettingsAsync(ct);
        if (settings != null && !string.IsNullOrEmpty(settings.RootFolderId))
        {
            var logsFolder = await _drive.CreateFolderAsync(settings, settings.RootFolderId, "Logs", ct);
            if (logsFolder != null)
            {
                var analyticsFolder = await _drive.CreateFolderAsync(settings, logsFolder.Id, "Analytics", ct);
                if (analyticsFolder != null)
                {
                    var json = JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
                    using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
                    await _drive.UploadFileAsync(settings, analyticsFolder.Id, $"analytics_{report.ReportDate:yyyy-MM-dd}.json", "application/json", stream, ct);
                }
            }
        }

        // 2. Update Global Memory Summary
        var summary = new AnalyticsSummary
        {
            TotalViews = report.TotalViews,
            TotalLikes = report.TotalLikes,
            TotalComments = report.TotalComments,
            TotalShares = report.TotalShares,
            AverageCTR = report.AverageCTR,
            AverageWatchTime = report.AverageWatchTime,
            AverageEngagement = report.AverageEngagement,
            GeneratedAt = DateTimeOffset.UtcNow
        };
        await _globalMemory.UpdateAnalyticsSummaryAsync(summary, ct);

        // 3. Update Content Strategy in Global Memory
        var memory = await _globalMemory.LoadAsync(ct);
        memory.ContentStrategy ??= new ContentStrategy();
        memory.ContentStrategy.FocusTopics = report.DetectedPatterns.Select(p => p.Description).ToList();
        memory.ContentStrategy.GeneratedAt = DateTimeOffset.UtcNow;
        
        await _globalMemory.SaveAsync(memory, ct);

        _logger.LogInformation("Feedback loop applied successfully.");
    }
}
