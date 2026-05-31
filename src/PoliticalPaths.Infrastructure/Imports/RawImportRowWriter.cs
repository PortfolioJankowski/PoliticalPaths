using System.Text.Json;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using PoliticalPaths.Application.Abstractions.Imports;
using PoliticalPaths.Application.Abstractions.Persistence;
using PoliticalPaths.Domain.Imports;
using PoliticalPaths.Shared.Hashing;
using PoliticalPaths.Application.Imports.ExcelDto;

namespace PoliticalPaths.Infrastructure.Imports;

public sealed class RawImportRowWriter(IAppDbContext db) : IRawImportRowWriter
{
    private const int BatchSize = 500;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public async Task<int> PersistRowsAsync(
        ImportFile file,
        IAsyncEnumerable<RawRowDto> rows,
        CancellationToken cancellationToken = default)
    {
        var existingHashList = await db.ImportRows
            .AsNoTracking()
            .Where(r => r.ImportFileId == file.Id)
            .Select(r => r.RowHash)
            .ToListAsync(cancellationToken);
        var existingHashes = existingHashList.ToHashSet(StringComparer.Ordinal);

        var buffer = new List<ImportRow>(BatchSize);
        var total = 0;
        var now = DateTime.UtcNow;

        await foreach (var dto in rows.WithCancellation(cancellationToken))
        {
            var rowHash = RowHashCalculator.Compute(dto.Columns);
            if (!existingHashes.Add(rowHash))
                continue;

            buffer.Add(new ImportRow
            {
                ImportFileId = file.Id,
                SheetName = dto.SheetName,
                SheetIndex = dto.SheetIndex,
                RowNumber = dto.RowNumber,
                RowHash = rowHash,
                RawPayloadJson = JsonSerializer.Serialize(dto.Columns, JsonOptions),
                Status = ImportRowStatus.Pending,
                ImportedAt = now
            });

            if (buffer.Count >= BatchSize)
            {
                total += await FlushAsync(buffer, cancellationToken);
            }
        }

        total += await FlushAsync(buffer, cancellationToken);
        return total;
    }

    private async Task<int> FlushAsync(List<ImportRow> buffer, CancellationToken cancellationToken)
    {
        if (buffer.Count == 0)
            return 0;

        db.ImportRows.AddRange(buffer);
        await db.SaveChangesAsync(cancellationToken);
        var count = buffer.Count;
        buffer.Clear();
        return count;
    }
}
