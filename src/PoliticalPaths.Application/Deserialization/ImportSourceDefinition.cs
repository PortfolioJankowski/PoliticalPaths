using System.Text.Json.Serialization;

namespace PoliticalPaths.Application.Deserialization;

/// <summary>
/// Struktura do deserializacji pliku file-mappings.json, który zawiera mapowanie pomiędzy nazwami logicznymi źródeł danych, 
/// a ich rzeczywistą zawartością, nazwą pliku, typem oraz przypisanym pipeline'em.
/// </summary>
/// <param name="LogicalName"></param>
/// <param name="RawData"></param>
/// <param name="FileName"></param>
/// <param name="FileType"></param>
/// <param name="Pipeline"></param>
public sealed record ImportSourceDefinition(
    [property: JsonPropertyName("logicalName")] string LogicalName,
    [property: JsonPropertyName("rawData")] string RawData,
    [property: JsonPropertyName("fileName")] string FileName,
    [property: JsonPropertyName("fileType")] string FileType,
    [property: JsonPropertyName("pipeline")] string Pipeline);
