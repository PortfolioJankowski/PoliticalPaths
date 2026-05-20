namespace PoliticalPaths.Application.Imports;

public sealed record RawRowDto(
    string SheetName,
    int SheetIndex,
    int RowNumber,
    IReadOnlyDictionary<string, string?> Columns);
