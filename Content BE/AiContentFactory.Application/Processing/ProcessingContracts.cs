using AiContentFactory.Domain.Processing;

namespace AiContentFactory.Application.Processing;

public sealed class FFmpegCommand
{
    public string Arguments { get; }
    public Action<double>? OnProgress { get; init; }
    public TimeSpan? ExpectedDuration { get; init; }

    public FFmpegCommand(string arguments)
    {
        Arguments = arguments;
    }

    public static FFmpegCommand Trim(string inputPath, string outputPath, TimeSpan start, TimeSpan duration)
    {
        return new FFmpegCommand($"-i \"{inputPath}\" -ss {start.TotalSeconds} -t {duration.TotalSeconds} -c copy \"{outputPath}\"");
    }

    public static FFmpegCommand ResizeTo916(string inputPath, string outputPath, int maxDurationSec = 60)
    {
        return new FFmpegCommand(
            $"-i \"{inputPath}\" -vf \"scale=1080:1920:force_original_aspect_ratio=decrease,pad=1080:1920:(ow-iw)/2:(oh-ih)/2,format=yuv420p\" " +
            $"-c:v libx264 -preset fast -crf 23 -c:a aac -b:a 192k -movflags +faststart -t {maxDurationSec} \"{outputPath}\"");
    }

    public static FFmpegCommand BurnCaptions(string inputPath, string outputPath, string captionText)
    {
        var escapedCaption = captionText.Replace("'", "'\\''").Replace(":", "\\:");
        return new FFmpegCommand(
            $"-i \"{inputPath}\" -vf \"drawtext=text='{escapedCaption}':fontsize=48:fontcolor=white:box=1:boxcolor=black@0.5:x=(w-text_w)/2:y=h-150" +
            $" -c:v libx264 -preset fast -crf 23 -c:a copy \"{outputPath}\"");
    }

    public static FFmpegCommand ExtractAudio(string inputPath, string outputPath)
    {
        return new FFmpegCommand($"-i \"{inputPath}\" -vn -acodec copy \"{outputPath}\"");
    }

    public static FFmpegCommand MergeAudio(string videoPath, string audioPath, string outputPath)
    {
        return new FFmpegCommand(
            $"-i \"{videoPath}\" -i \"{audioPath}\" -c:v copy -c:a aac -b:a 192k -shortest \"{outputPath}\"");
    }

    public static FFmpegCommand Convert(string inputPath, string outputPath, string codec = "libx264")
    {
        return new FFmpegCommand($"-i \"{inputPath}\" -c:v {codec} -preset fast -crf 23 -c:a aac -b:a 192k -movflags +faststart \"{outputPath}\"");
    }
}