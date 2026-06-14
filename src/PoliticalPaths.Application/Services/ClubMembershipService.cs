using Microsoft.EntityFrameworkCore;
using PoliticalPaths.Application.Abstractions;
using PoliticalPaths.Application.Abstractions.Persistence;
using PoliticalPaths.Domain.Formacje;

namespace PoliticalPaths.Application.Services;

public class ClubMembershipService(IAppDbContext db) : IClubMembershipService
{
    public async Task UpdateMembershipAsync(
    Guid politykId,
    Guid partiaId,
    Guid wyborId,
    CancellationToken ct = default)
    {
        var memberships = await db.PartieCzlonkostwa
            .Where(x => x.PolitykId == politykId)
            .ToListAsync(ct);

        var active = memberships.FirstOrDefault(x => x.IsActive);

        if (active == null)
        {
            db.PartieCzlonkostwa.Add(new PartiaCzlonkostwo
            {
                Id = Guid.NewGuid(),
                PolitykId = politykId,
                PartiaId = partiaId,
                WyboryId = wyborId,
                IsActive = true
            });

            return;
        }

        if (active.PartiaId == partiaId)
        {
            return;
        }

        active.IsActive = false;

        db.PartieCzlonkostwa.Add(new PartiaCzlonkostwo
        {
            Id = Guid.NewGuid(),
            PolitykId = politykId,
            PartiaId = partiaId,
            WyboryId = wyborId,
            IsActive = true
        });
    }
}
