using System.Text.Json.Serialization;

namespace KriPoint;

public sealed class KriPointPayload
{
    [JsonPropertyName("payload")]
    public required string Payload { get; init; }

    [JsonPropertyName("iv")]
    public required string Iv { get; init; }
}

public interface IKriPointEncryptionService
{
    KriPointPayload Encrypt<T>(T value);
    T? Decrypt<T>(KriPointPayload payload);
    T? DecryptRaw<T>(string base64Payload, string base64Iv);
    string DecryptToJson(KriPointPayload payload);
}
