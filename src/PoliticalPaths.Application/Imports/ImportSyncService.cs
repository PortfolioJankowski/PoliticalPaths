using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PoliticalPaths.Application.Abstractions;
using PoliticalPaths.Application.Abstractions.Imports;
using PoliticalPaths.Application.Abstractions.Imports.Deserialization;
using PoliticalPaths.Application.Abstractions.Persistence;
using PoliticalPaths.Application.Imports.Transform;
using PoliticalPaths.Application.Pipelines;
using PoliticalPaths.Application.Results;
using PoliticalPaths.Domain.Imports;
using PoliticalPaths.Shared.Paths;

namespace PoliticalPaths.Application.Imports;

public sealed class ImportSyncService(
    IAppDbContext db,
    IPipelineRegistry pipelineRegistry,
    IFileChecksumService checksumService,
    ITransformationExecutor transformationExecutor,
    IMandateGeneratorService mandateGenerator,
    ILogger<ImportSyncService> logger) : IImportSyncService
{
    public async Task<ImportSyncResult> SyncAllAsync(
        ImportSyncOptions options, 
        IProgress<ImportProgressInfo>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var config = pipelineRegistry.GetImportConfiguration();

        var contexts = PipelineContextBuilder.Build(config);

        var summaries = new List<PipelineSyncSummary>();

        var totalImported = 0;
        var totalSkipped = 0;
        var totalRaw = 0;
        var totalTransformed = 0;

        foreach (var context in contexts)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var summary = await SyncPipelineAsync(context, options, progress, cancellationToken);

            summaries.Add(summary);

            totalImported += summary.FilesImported;
            totalSkipped += summary.FilesSkipped;
            totalRaw += summary.RowsRaw;
            totalTransformed += summary.RowsTransformed;
        }

        return new ImportSyncResult(
            contexts.Count,
            totalImported,
            totalSkipped,
            totalRaw,
            totalTransformed,
            summaries);
    }
   
    private async Task<PipelineSyncSummary> SyncPipelineAsync(
        PipelineExecutionContext pipeline,
        ImportSyncOptions options,
        IProgress<ImportProgressInfo>? progress,
        CancellationToken cancellationToken)
    {
        var batch = await GetOrCreateBatchAsync(pipeline, cancellationToken);
        batch.StartBatch();

        var fileStats = new List<FileSyncResult>();

        foreach (var source in pipeline.Sources)
        {
            foreach (var fileName in source.FileNames)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var result = await SyncFileAsync(
                    batch,
                    pipeline,
                    source,
                    fileName,
                    options.ForceReimport,
                    progress,
                    cancellationToken);

                fileStats.Add(result);
            }
        }

        var summary = PipelineSyncSummaryFactory.Create(pipeline.PipelineKey, batch.Id, fileStats, options.ForceReimport);

        var transformSkipped = pipeline.Sources.Count > 0;

        batch.Finish();
     
        await db.SaveChangesAsync(cancellationToken);

        // Generowanie mandatów po eksporcie danych do bazy/
        var elections = await db.Wybory.ToListAsync(cancellationToken);
        foreach (var election in elections)
        {
            await mandateGenerator.GenerateMandatesForElectionAsync(election.Id, cancellationToken); 
        }

        return new PipelineSyncSummary(
            pipeline.PipelineKey,
            batch.Id,
            summary.FilesImported,
            summary.FilesSkipped,
            summary.RowsRaw,
            summary.RowsTransformed,
            summary.RowsFailed,
            transformSkipped);
    }

    private async Task<FileSyncResult> SyncFileAsync(
      ImportBatch batch,
      PipelineExecutionContext context,
      ImportSourceDefinition descriptor,
      string fileName,
      bool forceReimport,
      IProgress<ImportProgressInfo>? progress,
      CancellationToken cancellationToken)
    {
        var filePath = Path.Combine(
        RepoPaths.InboxDirectory(),
        fileName);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException(
                "Import file not found.",
                filePath);
        }

        var checksum = await checksumService.ComputeAsync(
            filePath,
            cancellationToken);

        var existingFile = await db.ImportFiles
            .FirstOrDefaultAsync(
                f => f.ImportBatchId == batch.Id &&
                     f.Sha256 == checksum.Sha256,
                cancellationToken);

        ImportFile importFile;

        if (existingFile is not null)
        {
            if (!forceReimport &&
                existingFile.Status is ImportFileStatus.RawCompleted
                    or ImportFileStatus.Completed
                    or ImportFileStatus.PartiallyCompleted)
            {
                return new FileSyncResult(
                    Skipped: true,
                    RowsRaw: existingFile.TotalRows,
                    RowsTransformed: existingFile.TransformedRows,
                    RowsFailed: existingFile.FailedRows,
                    FileName: fileName,
                    StartedAt: DateTime.UtcNow,
                    FinishedAt: DateTime.UtcNow);
            }

            if (forceReimport)
            {
                await ClearFileImportAsync(
                    existingFile.Id,
                    cancellationToken);

                existingFile.EreaseData();
                
                await db.SaveChangesAsync(cancellationToken);
            }

            importFile = existingFile;
        }
        else
        {
            importFile = new ImportFile
            {
                Id = Guid.NewGuid(),
                ImportBatchId = batch.Id,
                LogicalNames = descriptor.LogicalNames,
                StoragePath = Path.GetFullPath(filePath),
                Sha256 = checksum.Sha256,
                FileSizeBytes = checksum.FileSizeBytes,
                DataSourceType = descriptor.FileType,
                Status = ImportFileStatus.Discovered
            };

            db.ImportFiles.Add(importFile);

            await db.SaveChangesAsync(cancellationToken);
        }

        var startedAt = DateTime.UtcNow;

        var innerProgress = progress != null 
            ? new Progress<TransformationProgress>(p => progress.Report(new ImportProgressInfo(context.PipelineKey, fileName, p.Current, p.Total)))
            : null;

        var transformResult = await RunTransformIfAvailableAsync(
            batch,
            context.PipelineKey,
            descriptor,
            importFile,
            innerProgress,
            cancellationToken);

        progress?.Report(new ImportProgressInfo(context.PipelineKey, fileName, importFile.TotalRows, importFile.TotalRows, true));

        var finishedAt = DateTime.UtcNow;

        return new FileSyncResult(
            Skipped: false,
            RowsRaw: importFile.TotalRows,
            RowsTransformed: transformResult.RowsTransformed,
            RowsFailed: importFile.FailedRows,
            FileName: fileName,
            StartedAt: startedAt,
            FinishedAt: finishedAt);
    }

    private async Task<TransformFileResult> RunTransformIfAvailableAsync(
        ImportBatch batch,
        string pipelineKey,
        ImportSourceDefinition source,
        ImportFile file,
        IProgress<TransformationProgress>? progress,
        CancellationToken cancellationToken)
    {
        return await transformationExecutor.ExecuteAsync(
            pipelineKey,
            source,
            batch,
            file,
            progress,
            cancellationToken);
    }

    private sealed record TransformPassResult(int RowsTransformed, string? Message);

    private async Task ClearFileImportAsync(Guid importFileId, CancellationToken cancellationToken)
    {
        var rows = await db.ImportRows.Where(r => r.ImportFileId == importFileId).ToListAsync(cancellationToken);
        if (rows.Count > 0)
            db.ImportRows.RemoveRange(rows);

        var rowIds = rows.Select(r => r.Id).ToList();
        if (rowIds.Count > 0)
        {
            var errors = await db.TransformationErrors
                .Where(e => rowIds.Contains(e.ImportRowId))
                .ToListAsync(cancellationToken);
            if (errors.Count > 0)
                db.TransformationErrors.RemoveRange(errors);
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<ImportBatch> GetOrCreateBatchAsync(
        PipelineExecutionContext pipelineContext,
        CancellationToken cancellationToken)
    {
        var batch = await db.ImportBatches
            .FirstOrDefaultAsync(b => b.PipelineKey == pipelineContext.PipelineKey, cancellationToken);

        if (batch is not null)
            return batch;

        batch = new ImportBatch
        {
            Id = Guid.NewGuid(),
            PipelineKey = pipelineContext.PipelineKey,
            Status = ImportBatchStatus.Created,
            PrimarySourceType = pipelineContext.Sources.First().FileType,
            StartedAt = DateTime.UtcNow,
            TriggeredBy = "sync",
            Notes = $"Pipeline: {pipelineContext.PipelineKey}"
        };

        db.ImportBatches.Add(batch);
        await db.SaveChangesAsync(cancellationToken);
        return batch;
    }
}
