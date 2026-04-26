namespace AiContentFactory.Application.Studio;

public interface IMaskingService
{
    /// <summary>
    /// Masks a sensitive value (e.g., "sk-12345678" -> "sk-12...678").
    /// </summary>
    string Mask(string? value, int prefixLength = 4, int suffixLength = 4);

    /// <summary>
    /// Scrubs potential secrets from a large block of text.
    /// </summary>
    string Scrub(string? content);
}
