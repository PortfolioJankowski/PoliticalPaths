using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace PoliticalPaths.Shared.Hashing;

public static class RowHashCalculator
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public static string Compute(IReadOnlyDictionary<string, string?> columns)
    {
        var normalized = columns
            .OrderBy(kv => kv.Key, StringComparer.Ordinal)
            .ToDictionary(kv => kv.Key, kv => kv.Value ?? string.Empty, StringComparer.Ordinal);

        var json = JsonSerializer.Serialize(normalized, JsonOptions);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(json));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
