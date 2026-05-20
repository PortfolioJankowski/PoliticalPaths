using System.Text.Json;
using PoliticalPaths.Domain.Imports;

namespace PoliticalPaths.Importers.Transform.TestSample;

public sealed record ParseFieldError(string Field, string ErrorCode, string Message, string? RawValue);

public sealed record TestSampleParseResult(
    bool Success,
    TestSampleRowModel? Model,
    IReadOnlyList<ParseFieldError> Errors)
{
    public static TestSampleParseResult Ok(TestSampleRowModel model) =>
        new(true, model, []);

    public static TestSampleParseResult Fail(IReadOnlyList<ParseFieldError> errors) =>
        new(false, null, errors);
}

public static class TestSampleRowParser
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static TestSampleParseResult Parse(ImportRow row)
    {
        Dictionary<string, string?> columns;
        try
        {
            columns = JsonSerializer.Deserialize<Dictionary<string, string?>>(row.RawPayloadJson, JsonOptions)
                ?? [];
        }
        catch (JsonException ex)
        {
            return TestSampleParseResult.Fail([
                new ParseFieldError("RawPayloadJson", "TEST_SAMPLE_JSON", $"Nie można odczytać JSON wiersza: {ex.Message}", null)
            ]);
        }

        var errors = new List<ParseFieldError>();

        var lastName = Require(columns, "Nazwisko", errors);
        var firstName = Require(columns, "Imie", errors);
        var district = ParseInt(columns, "Okręg", errors);
        var list = ParseInt(columns, "Lista", errors);
        var votes = ParseInt(columns, "Głosy", errors);

        if (errors.Count > 0)
            return TestSampleParseResult.Fail(errors);

        return TestSampleParseResult.Ok(new TestSampleRowModel(
            lastName!,
            firstName!,
            district!.Value,
            list!.Value,
            votes!.Value));
    }

    private static string? Require(
        Dictionary<string, string?> columns,
        string field,
        List<ParseFieldError> errors)
    {
        if (!columns.TryGetValue(field, out var value) || string.IsNullOrWhiteSpace(value))
        {
            errors.Add(new ParseFieldError(
                field,
                "TEST_SAMPLE_REQUIRED",
                $"Pole '{field}' jest wymagane.",
                value));
            return null;
        }

        return value.Trim();
    }

    private static int? ParseInt(
        Dictionary<string, string?> columns,
        string field,
        List<ParseFieldError> errors)
    {
        if (!columns.TryGetValue(field, out var value) || string.IsNullOrWhiteSpace(value))
        {
            errors.Add(new ParseFieldError(
                field,
                "TEST_SAMPLE_REQUIRED",
                $"Pole '{field}' jest wymagane.",
                value));
            return null;
        }

        if (!int.TryParse(value.Trim(), out var parsed))
        {
            errors.Add(new ParseFieldError(
                field,
                "TEST_SAMPLE_INVALID_INT",
                $"Pole '{field}' musi być liczbą całkowitą.",
                value));
            return null;
        }

        return parsed;
    }
}
