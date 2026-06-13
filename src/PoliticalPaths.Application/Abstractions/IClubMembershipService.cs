namespace PoliticalPaths.Application.Abstractions;

public interface IClubMembershipService
{
    Task UpdateMembershipAsync(
        Guid politykId,
        Guid klubId,
        Guid wyborId,
        CancellationToken ct = default);
}
