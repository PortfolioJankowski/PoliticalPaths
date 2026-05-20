namespace PoliticalPaths.Application.Abstractions.Imports;

public interface IImportLogContext
{
    IDisposable BeginFileScope(Guid importBatchId, Guid importFileId, string logicalName);
}
