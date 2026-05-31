namespace PoliticalPaths.Application.Results;

public sealed record TransformFileResult(
    int RowsTransformed,
    int RowsFailed,
    int Warnings)
{
    public static TransformFileResult Skip(string message)
        => new(
            RowsTransformed: 0,
            RowsFailed: 0,
            Warnings: 0);
};
