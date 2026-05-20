using Microsoft.Extensions.Logging;
using PoliticalPaths.Application.Abstractions.Imports;
using PoliticalPaths.Application.Abstractions.Persistence;
using PoliticalPaths.Application.Imports.Transform;
using PoliticalPaths.Domain.Imports;

namespace PoliticalPaths.Importers.Transform;

/// <summary>
/// Wzorzec transformera: pętla po wierszach, statusy, zapis błędów, logowanie podsumowania.
/// </summary>
public abstract class PipelineTransformerBase(
    IAppDbContext db,
    ITransformationErrorRecorder errorRecorder,
    ILogger logger) : IImportTransformer
{
    public abstract string PipelineKey { get; }

    public virtual async Task<TransformFileResult> TransformFileAsync(
        ImportFile file,
        IReadOnlyList<ImportRow> rows,
        CancellationToken cancellationToken = default)
    {
        var transformed = 0;
        var failed = 0;
        var warnings = 0;

        foreach (var row in rows)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var outcome = await TransformRowSafeAsync(row, cancellationToken);

            switch (outcome.Kind)
            {
                case RowOutcomeKind.Success:
                    row.Status = ImportRowStatus.Transformed;
                    row.TransformedAt = DateTime.UtcNow;
                    transformed++;
                    break;
                case RowOutcomeKind.SuccessWithWarnings:
                    row.Status = ImportRowStatus.Transformed;
                    row.TransformedAt = DateTime.UtcNow;
                    transformed++;
                    warnings += outcome.WarningCount;
                    break;
                case RowOutcomeKind.Failed:
                    row.Status = ImportRowStatus.Failed;
                    failed++;
                    break;
                case RowOutcomeKind.Skipped:
                    row.Status = ImportRowStatus.Skipped;
                    break;
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Pipeline {PipelineKey} file {ImportFileId}: transformed={Transformed}, failed={Failed}, warnings={Warnings}",
            PipelineKey,
            file.Id,
            transformed,
            failed,
            warnings);

        return new TransformFileResult(transformed, failed, warnings);
    }

    private async Task<RowTransformOutcome> TransformRowSafeAsync(
        ImportRow row,
        CancellationToken cancellationToken)
    {
        try
        {
            return await TransformRowAsync(row, cancellationToken);
        }
        catch (Exception ex)
        {
            errorRecorder.Record(
                row,
                "transform.unhandled",
                TransformationSeverity.Error,
                "TRANSFORM_UNHANDLED",
                ex.Message,
                detailsJson: ex.ToString());

            return RowTransformOutcome.Failed();
        }
    }

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
