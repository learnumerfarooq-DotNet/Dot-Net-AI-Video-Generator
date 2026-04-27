using AiContentFactory.Application.Processing;
using AiContentFactory.Domain.Agents;
using AiContentFactory.Domain.Processing;
using Microsoft.Extensions.Logging;

namespace AiContentFactory.Infrastructure.Shorts;

public class MusicOverlayService
{
    private readonly IFFmpegService _ffmpeg;
    private readonly ILogger<MusicOverlayService> _logger;

    public MusicOverlayService(IFFmpegService ffmpeg, ILogger<MusicOverlayService> logger)
    {
        _ffmpeg = ffmpeg;
        _logger = logger;
    }

    public async Task<FFmpegResult> AddMusicAsync(string input, string output, MusicTrackConfig config, CancellationToken ct = default)
    {
        _logger.LogInformation("Adding music {TrackName} to {Path}", config.TrackName, output);
        
        // Mock FFmpeg command to mix audio
        // Real command would be: ffmpeg -i video.mp4 -i music.mp3 -filter_complex "[1:a]volume=0.3[music];[0:a][music]amix=inputs=2:duration=first" output.mp4
        
        var arguments = $"-i \"{input}\" -filter:a \"volume=1.0\" -c:v copy \"{output}\"";
        return await _ffmpeg.ExecuteAsync(new FFmpegCommand(arguments), ct);
    }
}
