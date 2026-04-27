using System.Diagnostics;
using System.Text.Json;
using AiContentFactory.Application.Processing;
using AiContentFactory.Domain.Pipeline;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AiContentFactory.Infrastructure.Processing;

public sealed class VideoMetadataExtractor : IVideoMetadataExtractor
{
    private readonly FFmpegOptions _options;
    private readonly ILogger<VideoMetadataExtractor> _logger;

    public VideoMetadataExtractor(IOptions<FFmpegOptions> options, ILogger<VideoMetadataExtractor> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<VideoMetadata> ExtractAsync(string filePath, CancellationToken ct = default)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _options.FFprobePath,
                Arguments = $"-v quiet -print_format json -show_format -show_streams \"{filePath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            }
        };

        process.Start();
        var output = await process.StandardOutput.ReadToEndAsync(ct);
        await process.WaitForExitAsync(ct);

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"FFprobe failed for {filePath}");
        }

        var json = JsonDocument.Parse(output);
        var format = json.RootElement.GetProperty("format");
        var streams = json.RootElement.GetProperty("streams");

        var videoStream = streams.EnumerateArray().FirstOrDefault(s =>
            s.GetProperty("codec_type").GetString() == "video");

        var duration = format.TryGetProperty("duration", out var durProp)
            ? double.Parse(durProp.GetString() ?? "0")
            : 0;

        var width = videoStream.TryGetProperty("width", out var wProp) ? wProp.GetInt32() : 0;
        var height = videoStream.TryGetProperty("height", out var hProp) ? hProp.GetInt32() : 0;
        var fpsString = videoStream.TryGetProperty("r_frame_rate", out var fpsProp)
            ? fpsProp.GetString() ?? "30/1" : "30/1";
        var fps = ParseFps(fpsString);

        var aspectRatio = width > 0 && height > 0
            ? SimplifyRatio(width, height)
            : "unknown";

        var fileSize = format.TryGetProperty("size", out var sizeProp)
            ? long.Parse(sizeProp.GetString() ?? "0")
            : new FileInfo(filePath).Length;

        return new VideoMetadata
        {
            Id = Guid.NewGuid(),
            DurationSeconds = duration,
            Resolution = $"{width}x{height}",
            AspectRatio = aspectRatio,
            Width = width,
            Height = height,
            Codec = videoStream.TryGetProperty("codec_name", out var codecProp)
                ? codecProp.GetString() ?? "unknown" : "unknown",
            Format = format.TryGetProperty("format_name", out var fmtProp)
                ? fmtProp.GetString()?.Split(',')[0] ?? "mp4" : "mp4",
            FileSizeBytes = fileSize,
            Fps = fps
        };
    }

    private static int ParseFps(string fpsString)
    {
        var parts = fpsString.Split('/');
        if (parts.Length == 2 && double.TryParse(parts[0], out var num) && double.TryParse(parts[1], out var den))
        {
            return den == 0 ? 30 : (int)Math.Round(num / den);
        }
        return 30;
    }

    private static string SimplifyRatio(int width, int height)
    {
        var gcd = GCD(width, height);
        return $"{width / gcd}:{height / gcd}";
    }

    private static int GCD(int a, int b)
    {
        while (b != 0) { var t = b; b = a % b; a = t; }
        return a;
    }
}