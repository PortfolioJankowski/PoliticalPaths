using System.Text.Json.Serialization;

namespace PoliticalPaths.Application.Abstractions.SejmApiClient;

public record PrintsResponse(
    [property: JsonPropertyName("count")] int Count,
    [property: JsonPropertyName("lastChanged")] DateTime LastChanged,
    [property: JsonPropertyName("link")] string Link
);