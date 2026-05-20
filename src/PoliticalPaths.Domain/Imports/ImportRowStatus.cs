namespace PoliticalPaths.Domain.Imports;

public enum ImportRowStatus
{
    Pending = 0,
    Transformed = 1,
    Skipped = 2,
    Failed = 3,
    NeedsManualReview = 4
}
