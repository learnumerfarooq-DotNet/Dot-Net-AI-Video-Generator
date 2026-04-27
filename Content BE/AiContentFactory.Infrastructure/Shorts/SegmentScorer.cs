using AiContentFactory.Domain.Processing;

namespace AiContentFactory.Infrastructure.Shorts;

public class SegmentScorer
{
    public Task<double> ScoreSegmentAsync(double startTime, double endTime, VideoAnalysisResult analysis, CancellationToken ct = default)
    {
        var duration = endTime - startTime;
        if (duration < 15 || duration > 60) return Task.FromResult(0.0);

        double score = 0.5;

        // Boost score if segment starts within the first 10 seconds of the video (good hook potential)
        if (startTime < 10) score += 0.2;

        // Boost score if segment contains scene changes
        var sceneChangesInSegment = analysis.SceneChanges?.Count(s => s >= startTime && s <= endTime) ?? 0;
        if (sceneChangesInSegment > 0) score += 0.1 * Math.Min(3, sceneChangesInSegment);

        // Cap at 1.0
        return Task.FromResult(Math.Min(1.0, score));
    }
}
