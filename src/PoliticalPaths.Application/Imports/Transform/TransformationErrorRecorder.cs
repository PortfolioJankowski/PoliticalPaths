using Microsoft.Extensions.Logging;
using PoliticalPaths.Application.Abstractions.Persistence;
using PoliticalPaths.Domain.Imports;

namespace PoliticalPaths.Application.Imports.Transform;

public sealed class TransformationErrorRecorder(
    IAppDbContext db,
    ILogger<TransformationErrorRecorder> logger) : ITransformationErrorRecorder
{
    public TransformationError Record(
        ImportRow row,
        string stepName,
        TransformationSeverity severity,
        string errorCode,
        string message,
        string? fieldName = null,
        string? rawValue = null,
        string? detailsJson = null)
    {
        var error = new TransformationError
        {
            ImportRowId = row.Id,
            StepName = stepName,
            Severity = severity,
            ErrorCode = errorCode,
            Message = message,
            FieldName = fieldName,
            RawValue = rawValue,
            DetailsJson = detailsJson,
            CreatedAt = DateTime.UtcNow
        };

        db.TransformationErrors.Add(error);
        row.Errors.Add(error);

        var logLevel = severity == TransformationSeverity.Error
            ? LogLevel.Error
            : LogLevel.Warning;

        logger.Log(
            logLevel,
            "Transform row {Sheet}:{RowNumber} [{ErrorCode}] {Step}: {Message} (field={Field}, raw={RawValue})",
            row.SheetName,
            row.RowNumber,
            errorCode,
            stepName,
            message,
            fieldName,
            rawValue);

        return error;
    }
}
