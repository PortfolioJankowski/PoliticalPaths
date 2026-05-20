namespace PoliticalPaths.Domain.Imports;

public enum ImportJobStatus
{
    Pending = 0,
    Running = 1,
    Completed = 2,
    Failed = 3,
    ScheduledRetry = 4
}
