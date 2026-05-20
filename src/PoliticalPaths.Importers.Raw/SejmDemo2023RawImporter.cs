using PoliticalPaths.Application.Abstractions.Imports;
using PoliticalPaths.Application.Imports;
using PoliticalPaths.Domain.Imports;

namespace PoliticalPaths.Importers.Raw;

[RawImporter("sejm-demo-2023", "sejm-demo-2023")]
public sealed class SejmDemo2023RawImporter(GenericExcelRawImporter inner) : IRawExcelImporter
{
    public IReadOnlyList<string> LogicalNames { get; } = ["sejm-demo-2023"];
    public DataSourceType DataSourceType => inner.DataSourceType;

    public Task<RawImportResult> ImportAsync(
        ImportFile file,
        Stream excelStream,
        CancellationToken cancellationToken = default) =>
        inner.ImportAsync(file, excelStream, cancellationToken);
}
