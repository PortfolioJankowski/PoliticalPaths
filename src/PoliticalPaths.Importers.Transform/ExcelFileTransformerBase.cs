using Microsoft.Extensions.Logging;
using PoliticalPaths.Application.Abstractions.Imports;
using PoliticalPaths.Application.Abstractions.Persistence;
using PoliticalPaths.Application.Imports.ExcelDto;
using PoliticalPaths.Application.Imports.Transform;
using PoliticalPaths.Application.Pipelines;
using PoliticalPaths.Application.Results;
using PoliticalPaths.Domain.Imports;

namespace PoliticalPaths.Importers.Transform;

/// <summary>
/// Wzorzec transformera pliku Excel: odczyt workbooka, pętla po arkuszach/wierszach, statusy, zapis błędów.
/// </summary>
public abstract class ExcelFileTransformerBase(
    IAppDbContext db,
    ITransformationErrorRecorder errorRecorder,
    ILogger logger) : IImportTransformer
{
    protected readonly IAppDbContext Db = db;
    protected readonly ITransformationErrorRecorder ErrorRecorder = errorRecorder;
    protected readonly ILogger Logger = logger;

    public abstract string PipelineKey { get; }

    public abstract Task<TransformFileResult> TransformFileAsync(
        ImportFile file,
        ExcelWorkbookModel workbook,
        PipelineExecutionContext context,
        CancellationToken cancellationToken = default);

    protected void RecordError(
        ImportRow row,
        string stepName,
        string errorCode,
        string message,
        string? fieldName = null,
        string? rawValue = null) =>
        ErrorRecorder.Record(
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
        ErrorRecorder.Record(
            row,
            stepName,
            TransformationSeverity.Warning,
            errorCode,
            message,
            fieldName,
            rawValue);
}
