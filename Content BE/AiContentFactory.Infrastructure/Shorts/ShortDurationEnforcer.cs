using AiContentFactory.Application.Processing;
using AiContentFactory.Domain.Processing;
using Microsoft.Extensions.Logging;

namespace AiContentFactory.Infrastructure.Shorts;

public sealed class ShortDurationEnforcer
{
    private readonly IFFmpegService _ffmpeg;
    private readonly ILogger<ShortDurationEnforcer> _logger;

    public ShortDurationEnforcer(IFFmpegService ffmpeg, ILogger<ShortDurationEnforcer> logger)
    {
        _ffmpeg = ffmpeg;
        _logger = logger;
    }

    public async Task<FFmpegResult> EnforceMax60s(string inputPath, string outputPath, CancellationToken ct = default)
    {
        _logger.LogInformation("Enforcing 60s limit on {Path}", inputPath);

        var arguments = $"-i \"{inputPath}\" -t 60 -c copy \"{outputPath}\"";
        var command = new FFmpegCommand(arguments);
        return await _ffmpeg.ExecuteAsync(command, ct);
    }

    public async Task<FFmpegResult> TrimToDuration(string inputPath, string outputPath, double start, double end, CancellationToken ct = default)
    {
        var duration = end - start;
        if (duration > 60)
        {
            _logger.LogWarning("AI requested {Duration}s segment. Trimming to 60s limit.", duration);
            duration = 60;
        }

        var arguments = $"-ss {start} -t {duration} -i \"{inputPath}\" -c copy \"{outputPath}\"";
        var command = new FFmpegCommand(arguments);
        return await _ffmpeg.ExecuteAsync(command, ct);
    }
    public async Task<double> ValidateDuration(string filePath, CancellationToken ct = default)
    {
        // Mock returning 60s since FFprobe isn't hooked up simply here
        return await Task.FromResult(60.0);
    }

    public async Task<FFmpegResult> EnforceDurationLimit(string input, string output, int maxSeconds, CancellationToken ct = default)
    {
        var arguments = $"-i \"{input}\" -t {maxSeconds} -c copy \"{output}\"";
        return await _ffmpeg.ExecuteAsync(new FFmpegCommand(arguments), ct);
    }

    public async Task<List<string>> SplitIfTooLong(string input, int maxSeconds, CancellationToken ct = default)
    {
        // Mock splitting
        return await Task.FromResult(new List<string> { input });
    }
}
