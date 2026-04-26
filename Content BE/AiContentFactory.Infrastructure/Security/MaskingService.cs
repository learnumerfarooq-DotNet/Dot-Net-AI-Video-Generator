using System.Text.RegularExpressions;
using AiContentFactory.Application.Studio;

namespace AiContentFactory.Infrastructure.Security;

public sealed class MaskingService : IMaskingService
{
    private static readonly Regex SecretRegex = new(
        @"(sk-[a-zA-Z0-9]{20,}|AIza[a-zA-Z0-9_-]{35}|[0-9a-f]{32,}|ghp_[a-zA-Z0-9]{36,})",
        RegexOptions.Compiled);

    public string Mask(string? value, int prefixLength = 4, int suffixLength = 4)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        if (value.Length <= prefixLength + suffixLength) return "****";

        var prefix = value[..prefixLength];
        var suffix = value[^suffixLength..];
        
        return $"{prefix}...{suffix}";
    }

    public string Scrub(string? content)
    {
        if (string.IsNullOrWhiteSpace(content)) return string.Empty;

        return SecretRegex.Replace(content, match => 
        {
            var val = match.Value;
            return Mask(val);
        });
    }
}
