using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace KriPoint;

public sealed class KriPointEncryptionService : IKriPointEncryptionService
{
    private readonly byte[] _key;

    public KriPointEncryptionService(string base64Key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(base64Key, nameof(base64Key));
        _key = Convert.FromBase64String(base64Key);

        if (_key.Length != 32)
            throw new ArgumentException(
                $"AES-256 requires exactly 32 bytes. Got {_key.Length} bytes after Base64-decoding.",
                nameof(base64Key));
    }

    public KriPointPayload Encrypt<T>(T value)
    {
        using var aes = BuildAes();
        aes.GenerateIV();

        var plaintext = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value));
        using var enc = aes.CreateEncryptor();
        var cipher = enc.TransformFinalBlock(plaintext, 0, plaintext.Length);

        return new KriPointPayload
        {
            Payload = Convert.ToBase64String(cipher),
            Iv      = Convert.ToBase64String(aes.IV)
        };
    }

    public T? Decrypt<T>(KriPointPayload payload)
    {
        ArgumentNullException.ThrowIfNull(payload);
        return JsonSerializer.Deserialize<T>(DecryptToJson(payload));
    }

    public T? DecryptRaw<T>(string base64Payload, string base64Iv)
        => Decrypt<T>(new KriPointPayload { Payload = base64Payload, Iv = base64Iv });

    public string DecryptToJson(KriPointPayload payload)
    {
        ArgumentNullException.ThrowIfNull(payload);

        using var aes = BuildAes();
        aes.IV = Convert.FromBase64String(payload.Iv);

        var cipher    = Convert.FromBase64String(payload.Payload);
        using var dec = aes.CreateDecryptor();
        var plain     = dec.TransformFinalBlock(cipher, 0, cipher.Length);

        return Encoding.UTF8.GetString(plain);
    }

    private Aes BuildAes()
    {
        var aes     = Aes.Create();
        aes.Key     = _key;
        aes.Mode    = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        return aes;
    }
}
