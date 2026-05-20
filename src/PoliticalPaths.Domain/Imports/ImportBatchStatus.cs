namespace PoliticalPaths.Domain.Imports;

public enum ImportBatchStatus
{
    Created = 0,
    Running = 1,
    RawCompleted = 2,
    Transforming = 3,
    Completed = 4,
    PartiallyCompleted = 5,
    Failed = 6
}
