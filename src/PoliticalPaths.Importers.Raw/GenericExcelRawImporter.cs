using ClosedXML.Excel;
using PoliticalPaths.Application.Abstractions.Imports;
using PoliticalPaths.Application.Imports;
using PoliticalPaths.Domain.Imports;

namespace PoliticalPaths.Importers.Raw;

[RawImporter("test-sample", "test-sample", "generic-excel")]
public sealed class GenericExcelRawImporter(IRawImportRowWriter rowWriter) : IRawExcelImporter
{
    public IReadOnlyList<string> LogicalNames { get; } = ["test-sample", "generic-excel"];
    public DataSourceType DataSourceType => DataSourceType.GenericExcel;

    public async Task<RawImportResult> ImportAsync(
        ImportFile file,
        Stream excelStream,
        CancellationToken cancellationToken = default)
    {
        using var workbook = new XLWorkbook(excelStream);
        var sheetsProcessed = 0;

        async IAsyncEnumerable<RawRowDto> ReadRows()
        {
            var sheetIndex = 0;
            foreach (var worksheet in workbook.Worksheets)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var sheetName = worksheet.Name;
                var rowsUsed = worksheet.RowsUsed().ToList();
                if (rowsUsed.Count == 0)
                {
                    sheetIndex++;
                    continue;
                }

                var headerRow = rowsUsed[0];
                var headers = headerRow.CellsUsed()
                    .Select((cell, i) => new { Index = cell.Address.ColumnNumber, Name = cell.GetString().Trim() })
                    .Where(h => !string.IsNullOrEmpty(h.Name))
                    .OrderBy(h => h.Index)
                    .Select(h => h.Name)
                    .ToList();

                if (headers.Count == 0)
                    headers = headerRow.CellsUsed().Select((c, i) => $"Column{i + 1}").ToList();

                for (var i = 1; i < rowsUsed.Count; i++)
                {
                    var row = rowsUsed[i];
                    if (row.CellsUsed().All(c => string.IsNullOrWhiteSpace(c.GetString())))
                        continue;

                    var columns = new Dictionary<string, string?>(StringComparer.Ordinal);
                    for (var col = 0; col < headers.Count; col++)
                    {
                        var header = headers[col];
                        var cell = row.Cell(col + 1);
                        columns[header] = cell.GetFormattedString().Trim();
                    }

                    yield return new RawRowDto(sheetName, sheetIndex, row.RowNumber(), columns);
                }

                sheetsProcessed++;
                sheetIndex++;
            }
        }

        var rowsImported = await rowWriter.PersistRowsAsync(file, ReadRows(), cancellationToken);
        return new RawImportResult(rowsImported, sheetsProcessed);
    }
}
