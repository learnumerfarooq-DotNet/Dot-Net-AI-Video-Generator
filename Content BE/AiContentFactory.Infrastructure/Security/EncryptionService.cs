using System.Security.Cryptography;
using System.Text;
using AiContentFactory.Application.Configuration;
using Microsoft.Extensions.Options;

namespace AiContentFactory.Infrastructure.Security;

public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}

public sealed class EncryptionService(IOptions<SecurityOptions> options) : IEncryptionService
{
    private readonly byte[] _key = Convert.FromBase64String(options.Value.EncryptionKey);
    private readonly byte[] _iv = Convert.FromBase64String(options.Value.EncryptionIv);

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return string.Empty;

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var sw = new StreamWriter(cs))
        {
            sw.Write(plainText);
        }

        return Convert.ToBase64String(ms.ToArray());
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText)) return string.Empty;

        try
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(Convert.FromBase64String(cipherText));
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);

            return sr.ReadToEnd();
        }
        catch
        {
            // Fallback for unencrypted data during migration or invalid keys
            return cipherText;
        }
    }
}
