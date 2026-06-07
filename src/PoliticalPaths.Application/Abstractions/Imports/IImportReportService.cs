using PoliticalPaths.Application.Results;

namespace PoliticalPaths.Application.Abstractions.Imports;

public interface IImportReportService
{
    Task GenerateReportAsync(ImportSyncResult result, CancellationToken ct = default);
}
