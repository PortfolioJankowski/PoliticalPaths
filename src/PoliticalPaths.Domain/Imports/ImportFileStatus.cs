namespace PoliticalPaths.Domain.Imports;

public enum ImportFileStatus
{
    Discovered = 0,
    RawImporting = 1,
    RawCompleted = 2,
    Transforming = 3,
    Completed = 4,
    PartiallyCompleted = 5,
    Failed = 6
}
