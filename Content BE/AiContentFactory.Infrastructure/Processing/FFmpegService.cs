using System.Diagnostics;
using System.Text.RegularExpressions;
using AiContentFactory.Application.Processing;
using AiContentFactory.Domain.Agents;
using AiContentFactory.Domain.Processing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AiContentFactory.Infrastructure.Processing;

public sealed class FFmpegOptions
{
    public string FFmpegPath { get; set; } = "/usr/bin/ffmpeg";
    public string FFprobePath { get; set; } = "/usr/bin/ffprobe";
    public int MaxConcurrentJobs { get; set; } = 4;
    public string TempStoragePath { get; set; } = "/tmp/videofactory";
    public long MaxTempStorageBytes { get; set; } = 180L * 1024 * 1024 * 1024;
    public bool UseHardwareAcceleration { get; set; } = true;
}

public sealed class FFmpegService : IFFmpegService
{
    private static readonly SemaphoreSlim _concurrencySemaphore = new(4, 4);
    private readonly FFmpegOptions _options;
    private readonly ILogger<FFmpegService> _logger;
    private readonly ITempStorageManager _tempStorage;
    private string? _detectedCodec;
    private readonly SemaphoreSlim _codecSemaphore = new(1, 1);

    public FFmpegService(
        IOptions<FFmpegOptions> options,
        ILogger<FFmpegService> logger,
        ITempStorageManager tempStorage)
    {
        _options = options.Value;
        _logger = logger;
        _tempStorage = tempStorage;
    }

    public async Task<FFmpegResult> ExecuteAsync(FFmpegCommand command, CancellationToken ct = default)
    {
        await _concurrencySemaphore.WaitAsync(ct);
        _logger.LogInformation("Acquired FFmpeg slot. Queue remaining: {Remaining}", _concurrencySemaphore.CurrentCount);

        try
        {
            var startTime = DateTimeOffset.UtcNow;
            var arguments = await ResolveArgumentsAsync(command.Arguments, ct);
            
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _options.FFmpegPath,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            var outputBuilder = new System.Text.StringBuilder();
            var errorBuilder = new System.Text.StringBuilder();
            var progressRegex = new Regex(@"time=(\d{2}):(\d{2}):(\d{2}\.\d{2})");

            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null) outputBuilder.AppendLine(e.Data);
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    errorBuilder.AppendLine(e.Data);
                    var match = progressRegex.Match(e.Data);
                    if (match.Success && command.OnProgress != null)
                    {
                        var hours = int.Parse(match.Groups[1].Value);
                        var minutes = int.Parse(match.Groups[2].Value);
                        var seconds = double.Parse(match.Groups[3].Value);
                        var totalSeconds = hours * 3600 + minutes * 60 + seconds;
                        command.OnProgress(totalSeconds);
                    }
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync(ct);

            var duration = DateTimeOffset.UtcNow - startTime;

            if (process.ExitCode != 0)
            {
                _logger.LogError("FFmpeg failed with exit code {ExitCode}. Error: {Error}",
                    process.ExitCode, errorBuilder.ToString());
                return FFmpegResult.CreateFailed(errorBuilder.ToString(), duration);
            }

            _logger.LogInformation("FFmpeg completed in {DurationMs}ms", duration.TotalMilliseconds);
            return FFmpegResult.CreateSuccess(outputBuilder.ToString(), duration);
        }
        finally
        {
            _concurrencySemaphore.Release();
            _logger.LogInformation("Released FFmpeg slot. Queue remaining: {Remaining}", _concurrencySemaphore.CurrentCount);
        }
    }

    public async Task<string> CreateTempWorkingDirectoryAsync(Guid jobId)
    {
        var path = Path.Combine(_options.TempStoragePath, jobId.ToString());
        Directory.CreateDirectory(path);
        return path;
    }

    public Task CleanupTempDirectoryAsync(Guid jobId)
    {
        var path = Path.Combine(_options.TempStoragePath, jobId.ToString());
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
            _logger.LogInformation("Cleaned up temp directory: {Path}", path);
        }
        return Task.CompletedTask;
    }

    public async Task<FFmpegResult> TrimAsync(string input, string output, double startSec, double endSec, CancellationToken ct = default)
    {
        var command = new FFmpegCommand($"-i \"{input}\" -ss {startSec} -to {endSec} -c copy \"{output}\"");
        return await ExecuteAsync(command, ct);
    }

    public async Task<FFmpegResult> ConcatAsync(List<string> inputs, string output, CancellationToken ct = default)
    {
        var listFile = Path.Combine(Path.GetDirectoryName(output)!, "inputs.txt");
        await File.WriteAllLinesAsync(listFile, inputs.Select(i => $"file '{i}'"), ct);
        
        var command = new FFmpegCommand($"-f concat -safe 0 -i \"{listFile}\" -c copy \"{output}\"");
        return await ExecuteAsync(command, ct);
    }

    public async Task<FFmpegResult> AddCaptionsAsync(string input, string output, List<EditCaption> captions, CancellationToken ct = default)
    {
        var command = new FFmpegCommand($"-i \"{input}\" -c copy \"{output}\"");
        return await ExecuteAsync(command, ct);
    }

    public async Task<FFmpegResult> BurnSubtitlesAsync(string input, string output, string srtPath, CancellationToken ct = default)
    {
        var command = new FFmpegCommand($"-i \"{input}\" -vf subtitles=\"{srtPath}\" \"{output}\"");
        return await ExecuteAsync(command, ct);
    }

    public async Task<FFmpegResult> NormalizeAudioAsync(string input, string output, CancellationToken ct = default)
    {
        var command = new FFmpegCommand($"-i \"{input}\" -af loudnorm=I=-16:TP=-1.5:LRA=11 -c:v copy \"{output}\"");
        return await ExecuteAsync(command, ct);
    }

    public async Task<FFmpegResult> AdjustVolumeAsync(string input, string output, double multiplier, CancellationToken ct = default)
    {
        var command = new FFmpegCommand($"-i \"{input}\" -filter:a \"volume={multiplier}\" -c:v copy \"{output}\"");
        return await ExecuteAsync(command, ct);
    }

    public async Task<FFmpegResult> ApplyColorGradingAsync(string input, string output, ColorGradingConfig config, CancellationToken ct = default)
    {
        var command = new FFmpegCommand($"-i \"{input}\" -vf eq=brightness={config.Brightness - 1.0}:contrast={config.Contrast}:saturation={config.Saturation}:gamma={config.Gamma} -c:a copy \"{output}\"");
        return await ExecuteAsync(command, ct);
    }

    public async Task<FFmpegResult> AddTransitionAsync(string clip1, string clip2, string output, string transitionType, int durationMs, CancellationToken ct = default)
    {
        var listFile = Path.Combine(Path.GetDirectoryName(output)!, "trans_inputs.txt");
        await File.WriteAllLinesAsync(listFile, new[] { $"file '{clip1}'", $"file '{clip2}'" }, ct);
        var command = new FFmpegCommand($"-f concat -safe 0 -i \"{listFile}\" -c copy \"{output}\"");
        return await ExecuteAsync(command, ct);
    }

    private async Task<string> ResolveArgumentsAsync(string arguments, CancellationToken ct)
    {
        if (!_options.UseHardwareAcceleration || !arguments.Contains("libx264"))
        {
            return arguments;
        }

        var bestCodec = await GetBestCodecAsync(ct);
        return arguments.Replace("libx264", bestCodec);
    }

    private async Task<string> GetBestCodecAsync(CancellationToken ct)
    {
        if (_detectedCodec != null) return _detectedCodec;

        await _codecSemaphore.WaitAsync(ct);
        try
        {
            if (_detectedCodec != null) return _detectedCodec;

            _logger.LogInformation("Detecting best available FFmpeg encoder...");
            
            var encoders = await GetAvailableEncodersAsync(ct);
            
            if (encoders.Contains("h264_nvenc")) _detectedCodec = "h264_nvenc";
            else if (encoders.Contains("h264_vaapi")) _detectedCodec = "h264_vaapi";
            else if (encoders.Contains("h264_videotoolbox")) _detectedCodec = "h264_videotoolbox";
            else if (encoders.Contains("h264_amf")) _detectedCodec = "h264_amf";
            else _detectedCodec = "libx264";

            _logger.LogInformation("Best encoder detected: {Codec}", _detectedCodec);
            return _detectedCodec;
        }
        finally
        {
            _codecSemaphore.Release();
        }
    }

    private async Task<List<string>> GetAvailableEncodersAsync(CancellationToken ct)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _options.FFmpegPath,
                    Arguments = "-encoders",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync(ct);
            await process.WaitForExitAsync(ct);

            return output.Split('\n')
                .Select(line => line.Trim())
                .Where(line => line.Contains("V..."))
                .Select(line => line.Split(' ').LastOrDefault() ?? "")
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to detect FFmpeg encoders. Falling back to libx264.");
            return new List<string>();
        }
    }
}