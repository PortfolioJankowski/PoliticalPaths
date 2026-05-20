using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PoliticalPaths.Application.Abstractions.Imports;
using PoliticalPaths.Application.Abstractions.Persistence;
using PoliticalPaths.Application.Imports.Inbox;
using PoliticalPaths.Domain.Imports;

namespace PoliticalPaths.Application.Imports;

public sealed class ImportSyncService(
    IAppDbContext db,
    IPipelineRegistry pipelineRegistry,
    IRawImporterRegistry rawImporterRegistry,
    IImportTransformerRegistry transformerRegistry,
    IInboxScanner inboxScanner,
    ISampleDataSeeder sampleSeeder,
    IFileChecksumService checksumService,
    IImportLogContext logContext,
    ILogger<ImportSyncService> logger) : IImportSyncService
{
    public async Task<ImportSyncResult> SyncAllAsync(ImportSyncOptions options, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(options.InboxRoot);

        if (options.SeedIfEmpty)
            EnsureSamplesForEmptyPipelines(options.InboxRoot);

        var pipelines = pipelineRegistry.GetAll();
        var summaries = new List<PipelineSyncSummary>();
        var totalImported = 0;
        var totalSkipped = 0;
        var totalRaw = 0;
        var totalTransformed = 0;

        foreach (var pipeline in pipelines)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var summary = await SyncPipelineAsync(pipeline, options, cancellationToken);
            summaries.Add(summary);
            totalImported += summary.FilesImported;
            totalSkipped += summary.FilesSkipped;
            totalRaw += summary.RowsRaw;
            totalTransformed += summary.RowsTransformed;
        }

        return new ImportSyncResult(
            pipelines.Count,
            totalImported,
            totalSkipped,
            totalRaw,
            totalTransformed,
            summaries);
    }

    private void EnsureSamplesForEmptyPipelines(string inboxRoot)
    {
        foreach (var pipeline in pipelineRegistry.GetAll())
        {
            var dir = Path.Combine(inboxRoot, pipeline.PipelineKey);
            if (!Directory.Exists(dir) || !Directory.EnumerateFiles(dir, "*.xlsx").Any())
                sampleSeeder.EnsureSampleInPipelineFolder(dir, pipeline.PipelineKey);
        }
    }

    private async Task<PipelineSyncSummary> SyncPipelineAsync(
        PipelineDefinition pipeline,
        ImportSyncOptions options,
        CancellationToken cancellationToken)
    {
        var pipelineDir = Path.Combine(options.InboxRoot, pipeline.PipelineKey);
        Directory.CreateDirectory(pipelineDir);

        var batch = await GetOrCreateBatchAsync(pipeline, cancellationToken);
        batch.Status = ImportBatchStatus.Running;
        batch.LastSyncedAt = DateTime.UtcNow;
        batch.TriggeredBy = "sync";
        await db.SaveChangesAsync(cancellationToken);

        var descriptors = inboxScanner.ScanPipeline(pipelineDir, pipeline);
        var filesImported = 0;
        var filesSkipped = 0;
        var rowsRaw = 0;
        var rowsTransformed = 0;
        var rowsFailed = 0;
        var transformSkipped = false;

        foreach (var descriptor in descriptors)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var fileResult = await SyncFileAsync(
                batch,
                pipeline,
                descriptor,
                options.ForceReimport,
                cancellationToken);

            if (fileResult.Skipped)
                filesSkipped++;
            else
                filesImported++;

            rowsRaw += fileResult.RowsRaw;
            rowsTransformed += fileResult.RowsTransformed;
            rowsFailed += fileResult.RowsFailed;
        }

        var transformer = transformerRegistry.Resolve(pipeline.PipelineKey);
        if (transformer is null && descriptors.Count > 0)
            transformSkipped = true;

        if (!transformSkipped && (rowsTransformed > 0 || rowsFailed > 0))
            batch.Status = rowsFailed > 0
                ? ImportBatchStatus.PartiallyCompleted
                : ImportBatchStatus.Completed;
        else if (filesImported > 0)
            batch.Status = ImportBatchStatus.RawCompleted;
        batch.LastSyncedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Pipeline {PipelineKey}: imported={Imported}, skipped={Skipped}, rawRows={Raw}",
            pipeline.PipelineKey,
            filesImported,
            filesSkipped,
            rowsRaw);

        return new PipelineSyncSummary(
            pipeline.PipelineKey,
            batch.Id,
            filesImported,
            filesSkipped,
            rowsRaw,
            rowsTransformed,
            rowsFailed,
            transformSkipped);
    }

    private async Task<FileSyncSummary> SyncFileAsync(
        ImportBatch batch,
        PipelineDefinition pipeline,
        InboxFileDescriptor descriptor,
        bool forceReimport,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(descriptor.FilePath))
            throw new FileNotFoundException("Import file not found.", descriptor.FilePath);

        var checksum = await checksumService.ComputeAsync(descriptor.FilePath, cancellationToken);

        var existingFile = await db.ImportFiles
            .FirstOrDefaultAsync(
                f => f.ImportBatchId == batch.Id && f.Sha256 == checksum.Sha256,
                cancellationToken);

        if (existingFile is not null && !forceReimport)
        {
            if (existingFile.Status is ImportFileStatus.RawCompleted or ImportFileStatus.Completed or ImportFileStatus.PartiallyCompleted)
                return new FileSyncSummary(
                    Path.GetFileName(descriptor.FilePath),
                    descriptor.LogicalName,
                    Skipped: true,
                    RowsRaw: existingFile.TotalRows,
                    RowsTransformed: existingFile.TransformedRows,
                    RowsFailed: existingFile.FailedRows,
                    "Already in batch (same SHA).");

            existingFile = await RunRawForFileAsync(batch, pipeline, existingFile, descriptor, cancellationToken);
        }
        else if (existingFile is not null && forceReimport)
        {
            await ClearFileImportAsync(existingFile.Id, cancellationToken);
            existingFile.Status = ImportFileStatus.Discovered;
            existingFile.TotalRows = 0;
            existingFile.TransformedRows = 0;
            existingFile.FailedRows = 0;
            await db.SaveChangesAsync(cancellationToken);
            existingFile = await RunRawForFileAsync(batch, pipeline, existingFile, descriptor, cancellationToken);
        }
        else
        {
            var newFile = new ImportFile
            {
                Id = Guid.NewGuid(),
                ImportBatchId = batch.Id,
                LogicalName = descriptor.LogicalName,
                StoragePath = Path.GetFullPath(descriptor.FilePath),
                Sha256 = checksum.Sha256,
                FileSizeBytes = checksum.FileSizeBytes,
                DataSourceType = pipeline.DataSourceType,
                FormatVersion = descriptor.FormatVersion,
                Status = ImportFileStatus.Discovered
            };
            db.ImportFiles.Add(newFile);
            await db.SaveChangesAsync(cancellationToken);
            existingFile = await RunRawForFileAsync(batch, pipeline, newFile, descriptor, cancellationToken);
        }

        var transformResult = await RunTransformIfAvailableAsync(
            batch,
            pipeline,
            existingFile,
            cancellationToken);

        return new FileSyncSummary(
            Path.GetFileName(descriptor.FilePath),
            descriptor.LogicalName,
            Skipped: false,
            RowsRaw: existingFile.TotalRows,
            RowsTransformed: transformResult.RowsTransformed,
            RowsFailed: existingFile.FailedRows,
            transformResult.Message);
    }

    private async Task<ImportFile> RunRawForFileAsync(
        ImportBatch batch,
        PipelineDefinition pipeline,
        ImportFile file,
        InboxFileDescriptor descriptor,
        CancellationToken cancellationToken)
    {
        using var _ = logContext.BeginFileScope(batch.Id, file.Id, file.LogicalName);

        file.Status = ImportFileStatus.RawImporting;
        file.RawImportStartedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        var rawImporter = rawImporterRegistry.Resolve(descriptor.LogicalName);
        await using var stream = File.OpenRead(descriptor.FilePath);
        var result = await rawImporter.ImportAsync(file, stream, cancellationToken);

        file.TotalRows = result.RowsImported;
        file.Status = ImportFileStatus.RawCompleted;
        file.RawImportCompletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        return file;
    }

    private async Task<TransformPassResult> RunTransformIfAvailableAsync(
        ImportBatch batch,
        PipelineDefinition pipeline,
        ImportFile file,
        CancellationToken cancellationToken)
    {
        var transformer = transformerRegistry.Resolve(pipeline.PipelineKey);
        if (transformer is null)
            return new TransformPassResult(0, "Transform skipped — no transformer registered.");

        var rows = await db.ImportRows
            .Where(r => r.ImportFileId == file.Id && r.Status == ImportRowStatus.Pending)
            .ToListAsync(cancellationToken);

        if (rows.Count == 0)
            return new TransformPassResult(file.TransformedRows, "Transform skipped — no pending rows.");

        using var _ = logContext.BeginFileScope(batch.Id, file.Id, file.LogicalName);

        file.Status = ImportFileStatus.Transforming;
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Transforming {RowCount} rows for pipeline {PipelineKey}, file {LogicalName}",
            rows.Count,
            pipeline.PipelineKey,
            file.LogicalName);

        var result = await transformer.TransformFileAsync(file, rows, cancellationToken);

        if (result.RowsFailed > 0)
            logger.LogWarning(
                "Transform finished with failures: transformed={Transformed}, failed={Failed}, warnings={Warnings}",
                result.RowsTransformed,
                result.RowsFailed,
                result.Warnings);

        file.TransformedRows = result.RowsTransformed;
        file.FailedRows = result.RowsFailed;
        file.WarningCount = result.Warnings;
        file.Status = result.RowsFailed > 0 ? ImportFileStatus.PartiallyCompleted : ImportFileStatus.Completed;
        await db.SaveChangesAsync(cancellationToken);

        return new TransformPassResult(result.RowsTransformed, null);
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
        PipelineDefinition pipeline,
        CancellationToken cancellationToken)
    {
        var batch = await db.ImportBatches
            .FirstOrDefaultAsync(b => b.PipelineKey == pipeline.PipelineKey, cancellationToken);

        if (batch is not null)
            return batch;

        batch = new ImportBatch
        {
            Id = Guid.NewGuid(),
            PipelineKey = pipeline.PipelineKey,
            Status = ImportBatchStatus.Created,
            PrimarySourceType = pipeline.DataSourceType,
            StartedAt = DateTime.UtcNow,
            TriggeredBy = "sync",
            Notes = $"Pipeline: {pipeline.PipelineKey}"
        };

        db.ImportBatches.Add(batch);
        await db.SaveChangesAsync(cancellationToken);
        return batch;
    }
}
