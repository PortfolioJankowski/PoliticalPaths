namespace PoliticalPaths.Application.Abstractions;

public interface IClubMembershipService
{
    Task UpdateMembershipAsync(
        Guid politykId,
        Guid partiaId,
        Guid wyboryId,
        CancellationToken ct = default);
}
