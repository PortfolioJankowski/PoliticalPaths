namespace PoliticalPaths.Application.Abstractions.Imports;

public interface IFileChecksumService
{
    Task<FileChecksumResult> ComputeAsync(string filePath, CancellationToken cancellationToken = default);
}

public sealed record FileChecksumResult(string Sha256, long FileSizeBytes);
