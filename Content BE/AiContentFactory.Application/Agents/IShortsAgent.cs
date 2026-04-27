using AiContentFactory.Domain.Agents;
using AiContentFactory.Domain.Processing;

namespace AiContentFactory.Application.Agents;

public interface IShortsAgent
{
    // Identify best segments and create short clips
    Task<List<ShortClip>> GenerateShortsAsync(Guid jobId, CancellationToken ct = default);

    // Process a single short clip (trim + resize)
    Task ProcessShortClipAsync(ShortClip clip, string sourcePath, CancellationToken ct = default);

    // Get all shorts for a job
    Task<List<ShortClip>> GetShortsAsync(Guid jobId, CancellationToken ct = default);

    // Regenerate shorts with different parameters
    Task<List<ShortClip>> RegenerateShortsAsync(Guid jobId, int maxShorts, int minDuration, CancellationToken ct = default);

    // Validate short meets platform constraints
    Task<bool> ValidateShortAsync(ShortClip clip, CancellationToken ct = default);

    // Score a segment for engagement potential
    Task<double> ScoreSegmentAsync(double startTime, double endTime, VideoAnalysisResult analysis, CancellationToken ct = default);
}
