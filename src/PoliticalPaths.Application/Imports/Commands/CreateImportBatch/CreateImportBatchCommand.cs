using MediatR;
using PoliticalPaths.Domain.Imports;

namespace PoliticalPaths.Application.Imports.Commands.CreateImportBatch;

public sealed record CreateImportBatchCommand(
    string? TriggeredBy,
    string? Notes,
    int? ElectionYear,
    DataSourceType? PrimarySourceType) : IRequest<Guid>;
