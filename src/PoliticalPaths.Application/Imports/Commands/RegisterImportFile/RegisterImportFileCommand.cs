using MediatR;
using PoliticalPaths.Domain.Imports;

namespace PoliticalPaths.Application.Imports.Commands.RegisterImportFile;

public sealed record RegisterImportFileCommand(
    Guid ImportBatchId,
    string FilePath,
    string LogicalName,
    string FormatVersion,
    DataSourceType DataSourceType,
    bool SkipIfSameShaInBatch = true) : IRequest<RegisterImportFileResult>;

public sealed record RegisterImportFileResult(
    Guid ImportFileId,
    string Sha256,
    bool SkippedAsDuplicate);
