using AiContentFactory.Application.Processing;
using AiContentFactory.Domain.Agents;
using AiContentFactory.Domain.Processing;
using Microsoft.Extensions.Logging;

namespace AiContentFactory.Infrastructure.Shorts;

public class CaptionRenderer
{
    private readonly IFFmpegService _ffmpeg;
    private readonly ILogger<CaptionRenderer> _logger;

    public CaptionRenderer(IFFmpegService ffmpeg, ILogger<CaptionRenderer> logger)
    {
        _ffmpeg = ffmpeg;
        _logger = logger;
    }

    public async Task<FFmpegResult> RenderCaptionsAsync(string input, string output, List<ShortCaption> captions, CancellationToken ct = default)
    {
        _logger.LogInformation("Rendering {Count} captions to {Path}", captions.Count, output);
        
        // In a real implementation, this would build a complex drawtext filter string
        // for FFmpeg or generate an ASS/SRT file and burn it in.
        // For now, we mock the execution by calling the generic FFmpeg service.
        
        var arguments = $"-i \"{input}\" -vf \"drawtext=text='(Captions Rendered)':fontsize=48:fontcolor=white:x=(w-text_w)/2:y=(h-text_h)/2\" -c:a copy \"{output}\"";
        return await _ffmpeg.ExecuteAsync(new FFmpegCommand(arguments), ct);
    }
}
