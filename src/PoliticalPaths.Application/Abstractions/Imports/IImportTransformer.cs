using PoliticalPaths.Application.Abstractions.Imports.Deserialization;
using PoliticalPaths.Application.Imports.ExcelDto;
using PoliticalPaths.Application.Results;
using PoliticalPaths.Domain.Imports;

namespace PoliticalPaths.Application.Abstractions.Imports;

public interface IImportTransformer
{
    string PipelineKey { get; }

    Task<TransformFileResult> TransformFileAsync(
        ImportFile file,
        ExcelWorkbookModel workbook,
        string pipelineKey,
        ImportSourceDefinition source,
        IProgress<TransformationProgress>? progress = null,
        CancellationToken cancellationToken = default);
}
