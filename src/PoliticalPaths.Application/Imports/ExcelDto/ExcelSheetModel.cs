namespace PoliticalPaths.Application.Imports.ExcelDto;

public sealed record ExcelSheetModel(
    string Name,
    int Index,
    int RowCount,
    int ColumnCount,
    IReadOnlyList<string> Headers,
    IReadOnlyList<RawRowDto> Rows);
