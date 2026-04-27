namespace AiContentFactory.Domain.Processing;

public sealed class FFmpegResult
{
    public bool Success { get; }
    public string Output { get; }
    public string? Error { get; }
    public TimeSpan Duration { get; }

    private FFmpegResult(bool success, string output, string? error, TimeSpan duration)
    {
        Success = success;
        Output = output;
        Error = error;
        Duration = duration;
    }

    public static FFmpegResult CreateSuccess(string output, TimeSpan duration)
        => new(true, output, null, duration);
    public static FFmpegResult CreateFailed(string error, TimeSpan duration)
        => new(false, string.Empty, error, duration);
}
