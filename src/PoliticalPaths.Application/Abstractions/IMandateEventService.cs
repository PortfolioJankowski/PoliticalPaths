using PoliticalPaths.Domain.Enums;

namespace PoliticalPaths.Application.Abstractions;

public interface IMandateEventService
{
    Task AddEventAsync(
        Guid mandatId, 
        TypZdarzeniaMandatowego typ, 
        DateOnly data, 
        string? opis = null, 
        string? dokument = null, 
        CancellationToken ct = default);
}
