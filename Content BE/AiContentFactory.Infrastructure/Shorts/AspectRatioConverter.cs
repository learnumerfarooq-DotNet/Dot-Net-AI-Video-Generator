using AiContentFactory.Application.Processing;
using AiContentFactory.Domain.Processing;
using Microsoft.Extensions.Logging;

namespace AiContentFactory.Infrastructure.Shorts;

public sealed class AspectRatioConverter
{
    private readonly IFFmpegService _ffmpeg;
    private readonly ILogger<AspectRatioConverter> _logger;

    public AspectRatioConverter(IFFmpegService ffmpeg, ILogger<AspectRatioConverter> logger)
    {
        _ffmpeg = ffmpeg;
        _logger = logger;
    }

    public async Task<FFmpegResult> ConvertTo916(string inputPath, string outputPath, CancellationToken ct = default)
    {
        _logger.LogInformation("Converting {Path} to 9:16 aspect ratio.", inputPath);

        // FFmpeg command: scale to fit 1080:1920 while decreasing to fit, then pad to 1080:1920
        var arguments = $"-i \"{inputPath}\" -vf \"scale=1080:1920:force_original_aspect_ratio=decrease,pad=1080:1920:(ow-iw)/2:(oh-ih)/2,format=yuv420p\" -c:v libx264 -preset fast -crf 23 -c:a aac -b:a 192k -movflags +faststart \"{outputPath}\"";
        
        var command = new FFmpegCommand(arguments);
        return await _ffmpeg.ExecuteAsync(command, ct);
    }

    public bool ValidateDimensions(int width, int height)
    {
        return width == 1080 && height == 1920;
    }

    public async Task<FFmpegResult> ConvertTo916WithBlurBackground(string input, string output, CancellationToken ct = default)
    {
        _logger.LogInformation("Converting {Path} to 9:16 aspect ratio with blur background.", input);
        // Complex filter: scale original to fit, scale+blur for background, overlay
        var arguments = $"-i \"{input}\" -lavfi \"[0:v]scale=1080:1920,boxblur=luma_radius=min(h\\,w)/20:luma_power=1:chroma_radius=min(cw\\,ch)/20:chroma_power=1[bg];[0:v]scale=-1:1920[fg];[bg][fg]overlay=(W-w)/2:(H-h)/2\" -c:v libx264 -preset fast -crf 23 -c:a copy \"{output}\"";
        return await _ffmpeg.ExecuteAsync(new FFmpegCommand(arguments), ct);
    }

    public async Task<FFmpegResult> ConvertTo916WithPadding(string input, string output, string padColor, CancellationToken ct = default)
    {
        _logger.LogInformation("Converting {Path} to 9:16 aspect ratio with {Color} padding.", input, padColor);
        var arguments = $"-i \"{input}\" -vf \"scale=1080:1920:force_original_aspect_ratio=decrease,pad=1080:1920:(ow-iw)/2:(oh-ih)/2:color={padColor}\" -c:v libx264 -preset fast -crf 23 -c:a copy \"{output}\"";
        return await _ffmpeg.ExecuteAsync(new FFmpegCommand(arguments), ct);
    }

    public Task<string> GetSourceAspectRatio(string input, CancellationToken ct = default)
    {
        // Mock method to return ratio
        return Task.FromResult("16:9");
    }
}
