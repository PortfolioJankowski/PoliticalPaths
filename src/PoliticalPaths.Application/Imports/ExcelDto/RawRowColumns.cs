using System.Globalization;
using System.Text.Json;
using PoliticalPaths.Domain.Imports;

namespace PoliticalPaths.Application.Imports.ExcelDto;

public static class RawRowColumns
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static bool TryParse(ImportRow row, out Dictionary<string, string?> columns, out string? error)
    {
        try
        {
            columns = JsonSerializer.Deserialize<Dictionary<string, string?>>(row.RawPayloadJson, JsonOptions)
                ?? [];
            error = null;
            return true;
        }
        catch (JsonException ex)
        {
            columns = [];
            error = ex.Message;
            return false;
        }
    }

    public static string? Get(Dictionary<string, string?> columns, params string[] names)
    {
        foreach (var name in names)
        {
            if (columns.TryGetValue(name, out var value) && !string.IsNullOrWhiteSpace(value))
                return value.Trim();
        }

        return null;
    }

    public static int? GetInt(Dictionary<string, string?> columns, params string[] names)
    {
        var raw = Get(columns, names);
        return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? value
            : null;
    }

    public static decimal? GetDecimal(Dictionary<string, string?> columns, params string[] names)
    {
        var raw = Get(columns, names);
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        raw = raw.Replace(',', '.');
        return decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var value)
            ? value
            : null;
    }

    public static bool GetBool(Dictionary<string, string?> columns, params string[] names)
    {
        var raw = Get(columns, names);
        if (string.IsNullOrWhiteSpace(raw))
            return false;

        return raw.Equals("tak", StringComparison.OrdinalIgnoreCase)
            || raw.Equals("yes", StringComparison.OrdinalIgnoreCase)
            || raw.Equals("1", StringComparison.OrdinalIgnoreCase)
            || raw.Equals("true", StringComparison.OrdinalIgnoreCase);
    }
}
