namespace PoliticalPaths.Application.Imports.ExcelDto;

public sealed record ExcelWorkbookModel(
    IReadOnlyList<ExcelSheetModel> Sheets);
