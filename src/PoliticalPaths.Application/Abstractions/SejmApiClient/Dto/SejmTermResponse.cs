using System.Text.Json.Serialization;

namespace PoliticalPaths.Application.Abstractions.SejmApiClient;

public record SejmTermResponse(
    [property: JsonPropertyName("current")] bool Current,
    [property: JsonPropertyName("from")] DateOnly From,
    [property: JsonPropertyName("num")] int Num,
    [property: JsonPropertyName("prints")] PrintsResponse Prints
);