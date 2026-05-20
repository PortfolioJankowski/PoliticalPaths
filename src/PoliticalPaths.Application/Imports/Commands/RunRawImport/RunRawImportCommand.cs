using MediatR;

namespace PoliticalPaths.Application.Imports.Commands.RunRawImport;

public sealed record RunRawImportCommand(Guid ImportFileId, bool ForceReimport = false) : IRequest<RunRawImportResult>;

public sealed record RunRawImportResult(
    Guid ImportFileId,
    int RowsImported,
    bool Skipped,
    string? Message);
