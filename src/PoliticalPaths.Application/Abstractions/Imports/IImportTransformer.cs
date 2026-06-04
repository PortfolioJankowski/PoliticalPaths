using PoliticalPaths.Application.Imports.ExcelDto;
using PoliticalPaths.Application.Pipelines;
using PoliticalPaths.Application.Results;
using PoliticalPaths.Domain.Imports;

namespace PoliticalPaths.Application.Abstractions.Imports;

public interface IImportTransformer
{
    string PipelineKey { get; }

    Task<TransformFileResult> TransformFileAsync(
        ImportFile file,
        ExcelWorkbookModel workbook,
        PipelineExecutionContext context,
        CancellationToken cancellationToken = default);
}
