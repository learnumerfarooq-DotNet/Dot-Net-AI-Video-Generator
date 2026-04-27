using AiContentFactory.Application.Processing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AiContentFactory.Infrastructure.Processing;

public sealed class TempStorageManager : ITempStorageManager
{
    private readonly FFmpegOptions _options;
    private readonly ILogger<TempStorageManager> _logger;

    public TempStorageManager(IOptions<FFmpegOptions> options, ILogger<TempStorageManager> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public string CreateJobDirectory(Guid jobId)
    {
        var path = Path.Combine(_options.TempStoragePath, jobId.ToString());
        Directory.CreateDirectory(path);
        return path;
    }

    public void DeleteJobDirectory(Guid jobId)
    {
        var path = Path.Combine(_options.TempStoragePath, jobId.ToString());
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
            _logger.LogInformation("Deleted temp directory: {Path}", path);
        }
    }

    public long GetCurrentUsageBytes()
    {
        if (!Directory.Exists(_options.TempStoragePath)) return 0;

        return Directory.GetFiles(_options.TempStoragePath, "*", SearchOption.AllDirectories)
            .Sum(f => new FileInfo(f).Length);
    }

    public bool HasSpaceFor(long requiredBytes)
    {
        var current = GetCurrentUsageBytes();
        return (current + requiredBytes) <= _options.MaxTempStorageBytes;
    }

    public void CleanupOldJobs(TimeSpan maxAge)
    {
        if (!Directory.Exists(_options.TempStoragePath)) return;

        foreach (var dir in Directory.GetDirectories(_options.TempStoragePath))
        {
            var info = new DirectoryInfo(dir);
            if (DateTime.UtcNow - info.LastWriteTimeUtc > maxAge)
            {
                Directory.Delete(dir, recursive: true);
                _logger.LogInformation("Cleaned up old temp directory: {Path}", dir);
            }
        }
    }

    public async Task CleanupAfterUploadAsync(Guid jobId)
    {
        DeleteJobDirectory(jobId);
        await Task.CompletedTask;
    }
}
