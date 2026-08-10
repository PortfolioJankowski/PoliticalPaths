using System.Text.Json.Serialization;

namespace PoliticalPaths.Application.Abstractions.Imports.Deserialization;

/// <summary>
/// Struktura do deserializacji pliku file-mappings.json, który zawiera mapowanie pomiędzy nazwami logicznymi źródeł danych, 
/// a ich rzeczywistą zawartością, nazwą pliku, typem oraz przypisanym pipeline'em.
/// </summary>

public sealed record ImportSourceDefinition(
    [property: JsonPropertyName("NazwyLogiczne")]
    string[] LogicalNames,
    [property: JsonPropertyName("Link")] string RawData,
    [property: JsonPropertyName("NazwyPlikow")]
    string[] FileNames,
    [property: JsonPropertyName("TypPlikow")]
    string FileType,
    [property: JsonPropertyName("Organ")] string Assembly,
    [property: JsonPropertyName("DataWyborow")]
    DateOnly ElectionDate,
    [property: JsonPropertyName("DataOgloszenia")]
    DateOnly AnnouncementDate,
    [property: JsonPropertyName("Tura")] string Round,
    [property: JsonPropertyName("Kadencja")]
    string? Term,
    [property: JsonPropertyName("CzyUzupelniajace")]
    bool IsSupplementary = false);


