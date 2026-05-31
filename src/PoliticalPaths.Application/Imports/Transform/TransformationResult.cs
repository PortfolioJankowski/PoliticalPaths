namespace PoliticalPaths.Application.Imports.Transform;

public sealed record TransformationResult(
  bool Skipped,
  int RowsTransformed,
  int RowsFailed,
  string Message)
{
    public static TransformationResult Skip(string message)
        => new(
            Skipped: true,
            RowsTransformed: 0,
            RowsFailed: 0,
            Message: message);
}
