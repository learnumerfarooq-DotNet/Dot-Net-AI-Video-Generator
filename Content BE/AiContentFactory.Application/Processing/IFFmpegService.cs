using AiContentFactory.Domain.Agents;
using AiContentFactory.Domain.Processing;

namespace AiContentFactory.Application.Processing;

public interface IFFmpegService
{
    Task<FFmpegResult> ExecuteAsync(FFmpegCommand command, CancellationToken ct = default);
    Task<string> CreateTempWorkingDirectoryAsync(Guid jobId);
    Task CleanupTempDirectoryAsync(Guid jobId);

    // NEW: Segment operations
    Task<FFmpegResult> TrimAsync(string input, string output, double startSec, double endSec, CancellationToken ct = default);
    Task<FFmpegResult> ConcatAsync(List<string> inputs, string output, CancellationToken ct = default);

    // NEW: Caption operations
    Task<FFmpegResult> AddCaptionsAsync(string input, string output, List<EditCaption> captions, CancellationToken ct = default);
    Task<FFmpegResult> BurnSubtitlesAsync(string input, string output, string srtPath, CancellationToken ct = default);

    // NEW: Audio operations
    Task<FFmpegResult> NormalizeAudioAsync(string input, string output, CancellationToken ct = default);
    Task<FFmpegResult> AdjustVolumeAsync(string input, string output, double multiplier, CancellationToken ct = default);

    // NEW: Visual operations
    Task<FFmpegResult> ApplyColorGradingAsync(string input, string output, ColorGradingConfig config, CancellationToken ct = default);
    Task<FFmpegResult> AddTransitionAsync(string clip1, string clip2, string output, string transitionType, int durationMs, CancellationToken ct = default);
}

public interface IVideoMetadataExtractor
{
    Task<Domain.Pipeline.VideoMetadata> ExtractAsync(string filePath, CancellationToken ct = default);
}

public interface ITempStorageManager
{
    string CreateJobDirectory(Guid jobId);
    void DeleteJobDirectory(Guid jobId);
    long GetCurrentUsageBytes();
    bool HasSpaceFor(long requiredBytes);
    void CleanupOldJobs(TimeSpan maxAge);
    Task CleanupAfterUploadAsync(Guid jobId);
}