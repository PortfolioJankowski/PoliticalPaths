using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PoliticalPaths.Application.Abstractions.Imports;
using PoliticalPaths.Application.Abstractions.Persistence;
using PoliticalPaths.Domain.Imports;

namespace PoliticalPaths.Application.Imports.Commands.RunRawImport;

public sealed class RunRawImportHandler(
    IAppDbContext db,
    IRawImporterRegistry importerRegistry,
    IImportLogContext logContext,
    ILogger<RunRawImportHandler> logger) : IRequestHandler<RunRawImportCommand, RunRawImportResult>
{
    public async Task<RunRawImportResult> Handle(RunRawImportCommand request, CancellationToken cancellationToken)
    {
        var file = await db.ImportFiles
            .Include(f => f.ImportBatch)
            .FirstOrDefaultAsync(f => f.Id == request.ImportFileId, cancellationToken)
            ?? throw new InvalidOperationException($"Import file {request.ImportFileId} not found.");

        if (file.Status == ImportFileStatus.RawCompleted && !request.ForceReimport)
            return new RunRawImportResult(file.Id, file.TotalRows, Skipped: true, "RAW already completed. Use --force to reimport.");

        if (!File.Exists(file.StoragePath))
            throw new FileNotFoundException("Stored import file missing.", file.StoragePath);

        using var _ = logContext.BeginFileScope(file.ImportBatchId, file.Id, file.LogicalName);

        var batch = file.ImportBatch;
        batch.Status = ImportBatchStatus.Running;
        file.Status = ImportFileStatus.RawImporting;
        file.RawImportStartedAt = DateTime.UtcNow;

        if (request.ForceReimport)
        {
            var existingRows = await db.ImportRows
                .Where(r => r.ImportFileId == file.Id)
                .ToListAsync(cancellationToken);
            db.ImportRows.RemoveRange(existingRows);
            file.TotalRows = 0;
        }

        await db.SaveChangesAsync(cancellationToken);

        var importer = importerRegistry.Resolve(file.LogicalName);

        await using var stream = File.OpenRead(file.StoragePath);
        var result = await importer.ImportAsync(file, stream, cancellationToken);

        file.TotalRows = result.RowsImported;
        file.Status = ImportFileStatus.RawCompleted;
        file.RawImportCompletedAt = DateTime.UtcNow;

        var allFilesDone = await db.ImportFiles
            .Where(f => f.ImportBatchId == batch.Id)
            .AllAsync(f => f.Status == ImportFileStatus.RawCompleted, cancellationToken);

        if (allFilesDone)
            batch.Status = ImportBatchStatus.RawCompleted;

        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "RAW import completed for {LogicalName}: {Rows} rows, {Sheets} sheets",
            file.LogicalName,
            result.RowsImported,
            result.SheetsProcessed);

        return new RunRawImportResult(file.Id, result.RowsImported, Skipped: false, null);
    }
}
