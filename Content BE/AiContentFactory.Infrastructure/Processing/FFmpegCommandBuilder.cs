using System.Text;
using AiContentFactory.Domain.Agents;

namespace AiContentFactory.Infrastructure.Processing;

public class FFmpegCommandBuilder
{
    private readonly StringBuilder _builder = new StringBuilder();
    private string? _input;
    private string? _output;

    public FFmpegCommandBuilder Input(string path)
    {
        _input = path;
        _builder.Append($"-i \"{path}\" ");
        return this;
    }

    public FFmpegCommandBuilder Output(string path)
    {
        _output = path;
        return this;
    }

    public FFmpegCommandBuilder Trim(double startSec, double endSec)
    {
        _builder.Append($"-ss {startSec} -to {endSec} ");
        return this;
    }

    public FFmpegCommandBuilder SetResolution(int width, int height)
    {
        _builder.Append($"-vf scale={width}:{height} ");
        return this;
    }

    public FFmpegCommandBuilder SetFps(int fps)
    {
        _builder.Append($"-r {fps} ");
        return this;
    }

    public FFmpegCommandBuilder SetCodec(string codec)
    {
        _builder.Append($"-c:v {codec} ");
        return this;
    }

    public FFmpegCommandBuilder SetFormat(string format)
    {
        _builder.Append($"-f {format} ");
        return this;
    }

    public FFmpegCommandBuilder AddCaption(string text, double startSec, double endSec, string style)
    {
        // Mock caption burn-in
        var escaped = text.Replace("'", "'\\''");
        _builder.Append($"-vf \"drawtext=text='{escaped}':enable='between(t,{startSec},{endSec})'\" ");
        return this;
    }

    public FFmpegCommandBuilder NormalizeAudio()
    {
        _builder.Append("-af loudnorm=I=-16:TP=-1.5:LRA=11 ");
        return this;
    }

    public FFmpegCommandBuilder SetVolume(double multiplier)
    {
        _builder.Append($"-filter:a \"volume={multiplier}\" ");
        return this;
    }

    public FFmpegCommandBuilder ApplyColorGrading(ColorGradingConfig config)
    {
        _builder.Append($"-vf eq=brightness={config.Brightness - 1.0}:contrast={config.Contrast}:saturation={config.Saturation}:gamma={config.Gamma} ");
        return this;
    }

    public FFmpegCommandBuilder SetSpeed(double speed)
    {
        if (Math.Abs(speed - 1.0) > 0.01)
        {
            _builder.Append($"-filter:v \"setpts={1.0 / speed}*PTS\" -filter:a \"atempo={speed}\" ");
        }
        return this;
    }

    public FFmpegCommandBuilder OverwriteOutput()
    {
        _builder.Append("-y ");
        return this;
    }

    public string Build()
    {
        if (!string.IsNullOrEmpty(_output))
        {
            _builder.Append($"\"{_output}\"");
        }
        return _builder.ToString().Trim();
    }
}
