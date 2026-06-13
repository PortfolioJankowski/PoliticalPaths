using Microsoft.EntityFrameworkCore;
using PoliticalPaths.Application.Abstractions;
using PoliticalPaths.Application.Abstractions.Persistence;
using PoliticalPaths.Domain.Formacje;

namespace PoliticalPaths.Application.Services;

public class ClubMembershipService(IAppDbContext db) : IClubMembershipService
{
    public async Task UpdateMembershipAsync(
    Guid politykId,
    Guid klubId,
    Guid wyborId,
    CancellationToken ct = default)
    {
        var memberships = await db.KlubCzlonkostwa
            .Where(x => x.PolitykId == politykId)
            .ToListAsync(ct);

        var active = memberships.FirstOrDefault(x => x.IsActive);

        if (active == null)
        {
            db.KlubCzlonkostwa.Add(new KlubCzlonkostwo
            {
                Id = Guid.NewGuid(),
                PolitykId = politykId,
                KlubId = klubId,
                WyboryId = wyborId,
                IsActive = true
            });

            return;
        }

        if (active.KlubId == klubId)
        {
            return;
        }

        active.IsActive = false;

        db.KlubCzlonkostwa.Add(new KlubCzlonkostwo
        {
            Id = Guid.NewGuid(),
            PolitykId = politykId,
            KlubId = klubId,
            WyboryId = wyborId,
            IsActive = true
        });
    }
}
