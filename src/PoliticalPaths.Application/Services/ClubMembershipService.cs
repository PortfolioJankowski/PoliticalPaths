using Microsoft.EntityFrameworkCore;
using PoliticalPaths.Application.Abstractions;
using PoliticalPaths.Application.Abstractions.Persistence;
using PoliticalPaths.Domain.Formacje;

namespace PoliticalPaths.Application.Services;

public class ClubMembershipService(IAppDbContext db) : IClubMembershipService
{
    private readonly Dictionary<Guid, PartiaCzlonkostwo?> _activeMemberships = new();

    public async Task UpdateMembershipAsync(
    Guid politykId,
    Guid partiaId,
    Guid wyborId,
    CancellationToken ct = default)
    {
        if (!_activeMemberships.TryGetValue(politykId, out var active))
        {
            active = await db.PartieCzlonkostwa
                .FirstOrDefaultAsync(x => x.PolitykId == politykId && x.IsActive, ct);
            _activeMemberships[politykId] = active;
        }

        if (active == null)
        {
            var membership = new PartiaCzlonkostwo
            {
                Id = Guid.NewGuid(),
                PolitykId = politykId,
                PartiaId = partiaId,
                WyboryId = wyborId,
                IsActive = true
            };
            db.PartieCzlonkostwa.Add(membership);
            _activeMemberships[politykId] = membership;

            return;
        }

        if (active.PartiaId == partiaId)
        {
            return;
        }

        active.IsActive = false;

        var replacement = new PartiaCzlonkostwo
        {
            Id = Guid.NewGuid(),
            PolitykId = politykId,
            PartiaId = partiaId,
            WyboryId = wyborId,
            IsActive = true
        };
        db.PartieCzlonkostwa.Add(replacement);
        _activeMemberships[politykId] = replacement;
    }
}
