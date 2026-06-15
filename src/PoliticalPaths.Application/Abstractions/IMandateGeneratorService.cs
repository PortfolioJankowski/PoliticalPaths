namespace PoliticalPaths.Application.Abstractions;

public interface IMandateGeneratorService
{
    Task GenerateMandatesForElectionAsync(Guid wyboryId, CancellationToken ct = default);
}
