using MediatR;
using PoliticalPaths.Application.Abstractions.Persistence;
using PoliticalPaths.Domain.Imports;

namespace PoliticalPaths.Application.Imports.Commands.CreateImportBatch;

public sealed class CreateImportBatchHandler(IAppDbContext db) : IRequestHandler<CreateImportBatchCommand, Guid>
{
    public async Task<Guid> Handle(CreateImportBatchCommand request, CancellationToken cancellationToken)
    {
        var batch = new ImportBatch
        {
            Id = Guid.NewGuid(),
            Status = ImportBatchStatus.Created,
            ElectionYear = request.ElectionYear,
            PrimarySourceType = request.PrimarySourceType,
            TriggeredBy = request.TriggeredBy ?? "cli",
            Notes = request.Notes,
            StartedAt = DateTime.UtcNow
        };

        db.ImportBatches.Add(batch);
        await db.SaveChangesAsync(cancellationToken);
        return batch.Id;
    }
}
