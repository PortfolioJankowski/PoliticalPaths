namespace PoliticalPaths.Application.Abstractions;

/// <summary>Assigns electoral mandates to starts in proportional Sejm elections.</summary>
public interface IDHondtMandateAllocationService
{
    Task AllocateForElectionAsync(Guid electionId, CancellationToken cancellationToken = default);
}
