using ClosedXML.Excel;
using PoliticalPaths.Application.Abstractions.Imports;
using PoliticalPaths.Application.Imports.ExcelDto;

namespace PoliticalPaths.Importers.Raw.Excel;

public sealed class ExcelProcessor : IExcelProcessor
{
    public ExcelWorkbookModel GetWorkbook(string filePath)
    {
        using var workbook = new XLWorkbook(filePath);
        var sheets = new List<ExcelSheetModel>();

        for (int i = 1; i <= workbook.Worksheets.Count; i++)
        {
            var worksheet = workbook.Worksheet(i);
            var sheetModel = ProcessSheet(worksheet, i - 1);
            sheets.Add(sheetModel);
        }

        return new ExcelWorkbookModel(sheets);
    }

    private static ExcelSheetModel ProcessSheet(IXLWorksheet worksheet, int sheetIndex)
    {
        var range = worksheet.RangeUsed();
        if (range == null || range.RowCount() == 0)
        {
            return new ExcelSheetModel(
                worksheet.Name,
                sheetIndex,
                0,
                0,
                [],
                []);
        }

        var firstRow = range.FirstRow();
        var headers = firstRow.Cells()
            .Select(c => c.Value.ToString().Trim())
            .ToList();
        
        var rows = new List<RawRowDto>();
        var rowCount = range.RowCount();
        var columnCount = range.ColumnCount();

        // Skip header row
        foreach (var xlRow in range.Rows().Skip(1))
        {
            var columns = new Dictionary<string, string?>();
            var values = new List<string?>();

            for (int col = 1; col <= columnCount; col++)
            {
                var header = headers.ElementAtOrDefault(col - 1) ?? $"Column{col}";
                var cell = xlRow.Cell(col);
                var cellValue = cell.Value.ToString();
                var val = string.IsNullOrWhiteSpace(cellValue) ? null : cellValue.Trim();
                
                columns[header] = val;
                values.Add(val);
            }

            rows.Add(new RawRowDto(
                worksheet.Name,
                sheetIndex,
                xlRow.RowNumber(),
                columns,
                values));
        }

        return new ExcelSheetModel(
            worksheet.Name,
            sheetIndex,
            rowCount,
            columnCount,
            headers,
            rows);
    }
}
