using MediatR;
using Microsoft.EntityFrameworkCore;
using PoliticalPaths.Application.Abstractions.Imports;
using PoliticalPaths.Application.Abstractions.Persistence;
using PoliticalPaths.Domain.Imports;

namespace PoliticalPaths.Application.Imports.Commands.RegisterImportFile;

public sealed class RegisterImportFileHandler(
    IAppDbContext db,
    IFileChecksumService checksumService) : IRequestHandler<RegisterImportFileCommand, RegisterImportFileResult>
{
    public async Task<RegisterImportFileResult> Handle(RegisterImportFileCommand request, CancellationToken cancellationToken)
    {
        if (!File.Exists(request.FilePath))
            throw new FileNotFoundException("Import file not found.", request.FilePath);

        var batch = await db.ImportBatches
            .FirstOrDefaultAsync(b => b.Id == request.ImportBatchId, cancellationToken)
            ?? throw new InvalidOperationException($"Import batch {request.ImportBatchId} not found.");

        var checksum = await checksumService.ComputeAsync(request.FilePath, cancellationToken);

        if (request.SkipIfSameShaInBatch)
        {
            var existing = await db.ImportFiles
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    f => f.ImportBatchId == request.ImportBatchId && f.Sha256 == checksum.Sha256,
                    cancellationToken);

            if (existing is not null)
                return new RegisterImportFileResult(existing.Id, checksum.Sha256, SkippedAsDuplicate: true);
        }

        var file = new ImportFile
        {
            Id = Guid.NewGuid(),
            ImportBatchId = batch.Id,
            LogicalName = request.LogicalName,
            StoragePath = Path.GetFullPath(request.FilePath),
            Sha256 = checksum.Sha256,
            FileSizeBytes = checksum.FileSizeBytes,
            DataSourceType = request.DataSourceType,
            FormatVersion = request.FormatVersion,
            Status = ImportFileStatus.Discovered
        };

        db.ImportFiles.Add(file);
        await db.SaveChangesAsync(cancellationToken);

        return new RegisterImportFileResult(file.Id, checksum.Sha256, SkippedAsDuplicate: false);
    }
}
