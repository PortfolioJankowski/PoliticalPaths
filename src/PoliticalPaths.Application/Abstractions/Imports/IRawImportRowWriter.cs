using PoliticalPaths.Application.Imports;
using PoliticalPaths.Domain.Imports;

namespace PoliticalPaths.Application.Abstractions.Imports;

public interface IRawImportRowWriter
{
    Task<int> PersistRowsAsync(
        ImportFile file,
        IAsyncEnumerable<RawRowDto> rows,
        CancellationToken cancellationToken = default);
}
