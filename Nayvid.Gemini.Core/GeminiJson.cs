using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Nayvid.Gemini.Core;

/// <summary>
/// Centralized System.Text.Json helper so all domain clients share a single configuration.
/// </summary>
public static class GeminiJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    public static string Serialize<T>(T value) => JsonSerializer.Serialize(value, Options);
    public static byte[] SerializeToUtf8Bytes<T>(T value) => JsonSerializer.SerializeToUtf8Bytes(value, Options);
    public static T? Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, Options);
    public static ValueTask<T?> DeserializeAsync<T>(Stream utf8Json) => JsonSerializer.DeserializeAsync<T>(utf8Json, Options);
    public static bool TryDeserialize<T>(string json, out T? result)
    {
        try { result = Deserialize<T>(json); return true; } catch { result = default; return false; }
    }
}
