using PoliticalPaths.Domain.Imports;

namespace PoliticalPaths.Application.Imports.Transform;

public interface ITransformationErrorRecorder
{
    TransformationError Record(
        ImportRow row,
        string stepName,
        TransformationSeverity severity,
        string errorCode,
        string message,
        string? fieldName = null,
        string? rawValue = null,
        string? detailsJson = null);
}
