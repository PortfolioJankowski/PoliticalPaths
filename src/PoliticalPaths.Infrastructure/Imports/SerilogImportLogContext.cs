using Microsoft.Extensions.Configuration;
using PoliticalPaths.Application.Abstractions.Imports;
using Serilog.Context;

namespace PoliticalPaths.Infrastructure.Imports;

public sealed class SerilogImportLogContext(IConfiguration configuration) : IImportLogContext
{
    public IDisposable BeginFileScope(Guid importBatchId, Guid importFileId, string logicalName)
    {
        var logsRoot = configuration["Import:LogsPath"] ?? "logs/imports";
        var directory = Path.Combine(logsRoot, DateTime.UtcNow.ToString("yyyy"), DateTime.UtcNow.ToString("MM"), importBatchId.ToString());
        Directory.CreateDirectory(directory);
        var logPath = Path.Combine(directory, $"{importFileId}.log");

        return new CompositeScope(
            LogContext.PushProperty("ImportBatchId", importBatchId),
            LogContext.PushProperty("ImportFileId", importFileId),
            LogContext.PushProperty("LogicalName", logicalName),
            LogContext.PushProperty("CorrelationId", importBatchId),
            LogContext.PushProperty("ImportLogFile", logPath));
    }

    private sealed class CompositeScope(params IDisposable[] scopes) : IDisposable
    {
        public void Dispose()
        {
            foreach (var scope in scopes.AsEnumerable().Reverse())
                scope.Dispose();
        }
    }
}
