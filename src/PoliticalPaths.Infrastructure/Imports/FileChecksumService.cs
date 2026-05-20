using System.Security.Cryptography;
using PoliticalPaths.Application.Abstractions.Imports;

namespace PoliticalPaths.Infrastructure.Imports;

public sealed class FileChecksumService : IFileChecksumService
{
    public async Task<FileChecksumResult> ComputeAsync(string filePath, CancellationToken cancellationToken = default)
    {
        await using var stream = File.OpenRead(filePath);
        var hash = await SHA256.HashDataAsync(stream, cancellationToken);
        var info = new FileInfo(filePath);
        return new FileChecksumResult(Convert.ToHexString(hash).ToLowerInvariant(), info.Length);
    }
}
