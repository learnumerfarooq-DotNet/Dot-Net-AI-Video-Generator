namespace AiContentFactory.Application.Configuration;

public sealed class SecurityOptions
{
    public const string SectionName = "Security";

    /// <summary>
    /// Base64 encoded 256-bit key for AES encryption.
    /// </summary>
    public string EncryptionKey { get; init; } = string.Empty;

    /// <summary>
    /// Base64 encoded 128-bit initialization vector.
    /// </summary>
    public string EncryptionIv { get; init; } = string.Empty;
}
