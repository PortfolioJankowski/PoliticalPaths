namespace PoliticalPaths.Application.Imports.ExcelDto;

public sealed record RawRowDto(
    string SheetName,
    int SheetIndex,
    int RowNumber,
    IReadOnlyDictionary<string, string?> Columns);
