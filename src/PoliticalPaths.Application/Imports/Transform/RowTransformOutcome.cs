namespace PoliticalPaths.Application.Imports.Transform;

public enum RowOutcomeKind
{
    Success,
    SuccessWithWarnings,
    Failed,
    Skipped
}

public sealed class RowTransformOutcome
{
    public RowOutcomeKind Kind { get; init; }
    public int WarningCount { get; init; }

    public static RowTransformOutcome Success() =>
        new() { Kind = RowOutcomeKind.Success };

    public static RowTransformOutcome SuccessWithWarnings(int count) =>
        new() { Kind = RowOutcomeKind.SuccessWithWarnings, WarningCount = count };

    public static RowTransformOutcome Failed() =>
        new() { Kind = RowOutcomeKind.Failed };

    public static RowTransformOutcome Skipped(string? reason = null) =>
        new() { Kind = RowOutcomeKind.Skipped };
}
