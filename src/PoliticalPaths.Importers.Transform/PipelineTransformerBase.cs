using PoliticalPaths.Application.Abstractions.Imports;
using PoliticalPaths.Application.Abstractions.Persistence;
using PoliticalPaths.Application.Imports.Transform;
using PoliticalPaths.Application.Pipelines;
using PoliticalPaths.Application.Results;
using PoliticalPaths.Domain.Imports;

namespace PoliticalPaths.Importers.Transform;

/// <summary>
/// Wzorzec transformera: pętla po wierszach, statusy, zapis błędów, logowanie podsumowania.
/// </summary>
public abstract class PipelineTransformerBase(
    IAppDbContext db,
    ITransformationErrorRecorder errorRecorder) : IImportTransformer
{
    public abstract string PipelineKey { get; }
    public abstract Task<TransformFileResult> TransformFileAsync(ImportFile file, PipelineExecutionContext context, CancellationToken cancellationToken = default);
    
    protected abstract Task<RowTransformOutcome> TransformRowAsync(
        ImportRow row,
        CancellationToken cancellationToken);

    protected void RecordError(
        ImportRow row,
        string stepName,
        string errorCode,
        string message,
        string? fieldName = null,
        string? rawValue = null) =>
        errorRecorder.Record(
            row,
            stepName,
            TransformationSeverity.Error,
            errorCode,
            message,
            fieldName,
            rawValue);

    protected void RecordWarning(
        ImportRow row,
        string stepName,
        string errorCode,
        string message,
        string? fieldName = null,
        string? rawValue = null) =>
        errorRecorder.Record(
            row,
            stepName,
            TransformationSeverity.Warning,
            errorCode,
            message,
            fieldName,
            rawValue);
}
