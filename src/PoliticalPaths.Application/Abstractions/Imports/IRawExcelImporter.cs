using PoliticalPaths.Domain.Imports;

namespace PoliticalPaths.Application.Abstractions.Imports;

public interface IRawExcelImporter
{
    IReadOnlyList<string> LogicalNames { get; }
    DataSourceType DataSourceType { get; }

    Task<RawImportResult> ImportAsync(
        ImportFile file,
        Stream excelStream,
        CancellationToken cancellationToken = default);
}

public sealed record RawImportResult(int RowsImported, int SheetsProcessed);
