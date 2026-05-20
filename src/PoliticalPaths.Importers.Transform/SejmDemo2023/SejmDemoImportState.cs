using Microsoft.EntityFrameworkCore;
using PoliticalPaths.Application.Abstractions.Persistence;
using PoliticalPaths.Domain.Candidacies;
using PoliticalPaths.Domain.Elections;
using PoliticalPaths.Domain.Enums;
using PoliticalPaths.Domain.Geography;
using PoliticalPaths.Domain.Mandates;
using PoliticalPaths.Domain.Parties;
using PoliticalPaths.Domain.Politicians;
using PoliticalPaths.Domain.Results;

namespace PoliticalPaths.Importers.Transform.SejmDemo2023;

internal sealed class SejmDemoImportState(IAppDbContext db)
{
    public const string ElectionKey = "sejm-demo-2023";
    public const string TermKey = "sejm-term-demo-10";
    private static readonly DateOnly ElectionDate = new(2023, 10, 15);
    private static readonly DateOnly MandateStart = new(2023, 11, 13);

    public Guid ElectionId { get; private set; }
    public Guid TermId { get; private set; }

    public Dictionary<int, Guid> Districts { get; } = [];
    public Dictionary<string, Guid> Lists { get; } = [];
    public Dictionary<string, Guid> Committees { get; } = [];
    public Dictionary<string, Guid> Parties { get; } = [];
    public Dictionary<string, Guid> Politicians { get; } = [];
    public Dictionary<Guid, int> ListVoteTotals { get; } = [];

    public async Task EnsureBootstrapAsync(CancellationToken ct)
    {
        var election = await db.Elections.FirstOrDefaultAsync(e => e.NaturalKey == ElectionKey, ct);
        if (election is null)
        {
            election = new Election
            {
                Id = Guid.NewGuid(),
                Year = 2023,
                Chamber = ElectoralChamber.Sejm,
                Scope = ElectionScope.National,
                Profile = ElectionProfile.SejmProportional,
                Kind = ElectionKind.General,
                ElectionDate = ElectionDate,
                NaturalKey = ElectionKey
            };
            db.Elections.Add(election);
            await db.SaveChangesAsync(ct);
        }

        ElectionId = election.Id;

        var term = await db.LegislativeTerms.FirstOrDefaultAsync(t => t.NaturalKey == TermKey, ct);
        if (term is null)
        {
            term = new LegislativeTerm
            {
                Id = Guid.NewGuid(),
                Body = CollegialBodyType.Sejm,
                TermNumber = 10,
                FoundingElectionId = ElectionId,
                ConstituentSessionDate = MandateStart,
                NaturalKey = TermKey
            };
            db.LegislativeTerms.Add(term);
            election.LegislativeTermId = term.Id;
            await db.SaveChangesAsync(ct);
        }

        TermId = term.Id;
    }

    public async Task<Guid> GetOrCreateTerritoryAsync(string teryt, string name, CancellationToken ct)
    {
        var unit = await db.TerritorialUnits
            .FirstOrDefaultAsync(t => t.TerytCode == teryt, ct);

        if (unit is not null)
            return unit.Id;

        unit = new TerritorialUnit
        {
            Id = Guid.NewGuid(),
            TerytCode = teryt,
            Name = name,
            Level = TerritorialUnitLevel.Voivodeship,
            ValidFrom = new DateOnly(2010, 1, 1)
        };
        db.TerritorialUnits.Add(unit);
        await db.SaveChangesAsync(ct);
        return unit.Id;
    }

    public async Task<Guid> GetOrCreateDistrictAsync(
        int number,
        string name,
        int? population,
        int? eligible,
        int? seats,
        string teryt,
        long? sourceRowId,
        CancellationToken ct)
    {
        if (Districts.TryGetValue(number, out var cached))
            return cached;

        var naturalKey = $"{ElectionKey}:sejm:{number}";
        var district = await db.ElectoralDistricts
            .FirstOrDefaultAsync(d => d.NaturalKey == naturalKey, ct);

        if (district is null)
        {
            district = new ElectoralDistrict
            {
                Id = Guid.NewGuid(),
                ElectionId = ElectionId,
                Chamber = ElectoralChamber.Sejm,
                DistrictNumber = number,
                Name = name,
                NaturalKey = naturalKey
            };
            db.ElectoralDistricts.Add(district);
            await db.SaveChangesAsync(ct);

            var territoryId = await GetOrCreateTerritoryAsync(teryt, name, ct);
            db.ElectoralDistrictTerritories.Add(new ElectoralDistrictTerritory
            {
                Id = Guid.NewGuid(),
                ElectoralDistrictId = district.Id,
                TerritorialUnitId = territoryId,
                CoverageType = TerritoryCoverageType.Primary
            });

            db.ElectoralDistrictSnapshots.Add(new ElectoralDistrictSnapshot
            {
                Id = Guid.NewGuid(),
                ElectoralDistrictId = district.Id,
                ElectionId = ElectionId,
                Population = population,
                EligibleVoters = eligible,
                SeatsAllocated = seats,
                StatisticsDate = ElectionDate,
                SourceImportRowId = sourceRowId,
                CreatedAt = DateTime.UtcNow
            });

            await db.SaveChangesAsync(ct);
        }

        Districts[number] = district.Id;
        return district.Id;
    }

    public async Task<Guid> GetOrCreatePartyAsync(string partyName, CancellationToken ct)
    {
        var key = Slug($"party:{partyName}");
        if (Parties.TryGetValue(key, out var cached))
            return cached;

        var party = await db.Parties.FirstOrDefaultAsync(p => p.NaturalKey == key, ct);
        if (party is null)
        {
            party = new Party
            {
                Id = Guid.NewGuid(),
                Name = partyName,
                ShortName = partyName.Length > 32 ? partyName[..32] : partyName,
                NaturalKey = key
            };
            db.Parties.Add(party);
            await db.SaveChangesAsync(ct);
        }

        Parties[key] = party.Id;
        return party.Id;
    }

    public async Task<(Guid CommitteeId, Guid ListId)> GetOrCreateListAsync(
        int districtNumber,
        int listNumber,
        string listName,
        string committeeName,
        string committeeShort,
        string partyName,
        CancellationToken ct)
    {
        var listKey = $"{ElectionKey}:{districtNumber}:{listNumber}";
        if (Lists.TryGetValue(listKey, out var cachedList))
            return (Committees[$"{ElectionKey}:{committeeShort}"], cachedList);

        if (!Districts.TryGetValue(districtNumber, out var districtId))
            throw new InvalidOperationException($"Okręg {districtNumber} nie istnieje — najpierw arkusz Okregi.");

        var committeeKey = $"{ElectionKey}:committee:{committeeShort}";
        var committee = await db.ElectoralCommittees
            .FirstOrDefaultAsync(c => c.NaturalKey == committeeKey, ct);

        if (committee is null)
        {
            var partyId = await GetOrCreatePartyAsync(partyName, ct);
            committee = new ElectoralCommittee
            {
                Id = Guid.NewGuid(),
                ElectionId = ElectionId,
                Name = committeeName,
                ShortName = committeeShort,
                Type = ElectoralCommitteeType.Party,
                PartyId = partyId,
                NaturalKey = committeeKey
            };
            db.ElectoralCommittees.Add(committee);
            await db.SaveChangesAsync(ct);
        }

        Committees[committeeKey] = committee.Id;

        var list = await db.ElectoralLists.FirstOrDefaultAsync(l => l.NaturalKey == listKey, ct);
        if (list is null)
        {
            list = new ElectoralList
            {
                Id = Guid.NewGuid(),
                ElectionId = ElectionId,
                ElectoralDistrictId = districtId,
                ElectoralCommitteeId = committee.Id,
                ListNumber = listNumber,
                PartyId = committee.PartyId,
                NaturalKey = listKey
            };
            db.ElectoralLists.Add(list);
            await db.SaveChangesAsync(ct);
        }

        Lists[listKey] = list.Id;
        return (committee.Id, list.Id);
    }

    public async Task<Guid> GetOrCreatePoliticianAsync(string lastName, string firstName, CancellationToken ct)
    {
        var normalized = NormalizeName(lastName, firstName);
        if (Politicians.TryGetValue(normalized, out var cached))
            return cached;

        var politician = await db.Politicians
            .FirstOrDefaultAsync(p => p.NormalizedName == normalized, ct);

        if (politician is null)
        {
            politician = new Politician
            {
                Id = Guid.NewGuid(),
                NormalizedName = normalized,
                DisplayName = $"{lastName} {firstName}",
                CreatedAt = DateTime.UtcNow
            };
            db.Politicians.Add(politician);
            db.PoliticianAliases.Add(new PoliticianAlias
            {
                Id = Guid.NewGuid(),
                PoliticianId = politician.Id,
                AliasName = politician.DisplayName!,
                NormalizedAlias = normalized,
                Source = ElectionKey
            });
            await db.SaveChangesAsync(ct);
        }

        Politicians[normalized] = politician.Id;
        return politician.Id;
    }

    public async Task<Candidacy> UpsertCandidacyAsync(
        Guid politicianId,
        Guid districtId,
        Guid listId,
        Guid committeeId,
        int listPosition,
        long? sourceRowId,
        CancellationToken ct)
    {
        var fingerprint = $"{ElectionId}:{politicianId}:{districtId}:{listId}:{listPosition}";
        var candidacy = await db.Candidacies
            .FirstOrDefaultAsync(c => c.SourceFingerprint == fingerprint, ct);

        if (candidacy is null)
        {
            candidacy = new Candidacy
            {
                Id = Guid.NewGuid(),
                PoliticianId = politicianId,
                ElectionId = ElectionId,
                Profile = ElectionProfile.SejmProportional,
                ElectoralDistrictId = districtId,
                ElectoralListId = listId,
                ElectoralCommitteeId = committeeId,
                ListPosition = listPosition,
                SourceFingerprint = fingerprint,
                SourceImportRowId = sourceRowId
            };
            db.Candidacies.Add(candidacy);
            await db.SaveChangesAsync(ct);
        }

        return candidacy;
    }

    public async Task UpsertVoteResultAsync(
        Candidacy candidacy,
        Guid districtId,
        int? votes,
        decimal? percent,
        bool elected,
        long? sourceRowId,
        CancellationToken ct)
    {
        var result = await db.CandidacyVoteResults
            .FirstOrDefaultAsync(r => r.CandidacyId == candidacy.Id, ct);

        if (result is null)
        {
            result = new CandidacyVoteResult
            {
                Id = Guid.NewGuid(),
                CandidacyId = candidacy.Id,
                ElectionId = ElectionId,
                ElectoralDistrictId = districtId
            };
            db.CandidacyVoteResults.Add(result);
        }

        result.VotesReceived = votes;
        result.VotePercent = percent;
        result.Elected = elected;
        result.SourceImportRowId = sourceRowId;

        if (candidacy.ElectoralListId is { } listId && votes is > 0)
            ListVoteTotals[listId] = ListVoteTotals.GetValueOrDefault(listId) + votes.Value;

        if (elected)
            await UpsertMandateChainAsync(candidacy, districtId, sourceRowId, ct);

        await db.SaveChangesAsync(ct);
    }

    private async Task UpsertMandateChainAsync(
        Candidacy candidacy,
        Guid districtId,
        long? sourceRowId,
        CancellationToken ct)
    {
        var allocation = await db.ElectionMandateAllocations
            .FirstOrDefaultAsync(a => a.CandidacyId == candidacy.Id, ct);

        if (allocation is null)
        {
            allocation = new ElectionMandateAllocation
            {
                Id = Guid.NewGuid(),
                ElectionId = ElectionId,
                CandidacyId = candidacy.Id,
                PoliticianId = candidacy.PoliticianId,
                ElectoralDistrictId = districtId,
                ElectoralListId = candidacy.ElectoralListId,
                RankOnListByVotes = candidacy.ListPosition ?? 0,
                AllocatedSeat = true,
                AllocationAnnouncedOn = ElectionDate
            };
            db.ElectionMandateAllocations.Add(allocation);
            await db.SaveChangesAsync(ct);
        }

        if (allocation.MandateId is not null)
            return;

        var mandate = new Mandate
        {
            Id = Guid.NewGuid(),
            LegislativeTermId = TermId,
            PoliticianId = candidacy.PoliticianId,
            Body = CollegialBodyType.Sejm,
            ElectoralDistrictId = districtId,
            ElectoralListId = candidacy.ElectoralListId,
            ElectoralCommitteeId = candidacy.ElectoralCommitteeId,
            OriginatingCandidacyId = candidacy.Id,
            OriginatingElectionId = ElectionId,
            AcquisitionType = MandateAcquisitionType.InitialElection,
            Status = MandateStatus.Active,
            ValidFrom = MandateStart,
            SuccessorPriorityOnList = candidacy.ListPosition
        };
        db.Mandates.Add(mandate);
        allocation.MandateId = mandate.Id;

        db.MandateEvents.Add(new MandateEvent
        {
            MandateId = mandate.Id,
            Type = MandateEventType.OathTaken,
            OccurredAt = DateTime.UtcNow,
            EffectiveDate = MandateStart,
            SourceImportRowId = sourceRowId
        });

        await db.SaveChangesAsync(ct);
    }

    public async Task UpsertTurnoutAsync(
        int districtNumber,
        int? ballots,
        int? valid,
        int? invalid,
        decimal? turnout,
        long? sourceRowId,
        CancellationToken ct)
    {
        if (!Districts.TryGetValue(districtNumber, out var districtId))
            return;

        var existing = await db.DistrictTurnoutResults
            .FirstOrDefaultAsync(t => t.ElectionId == ElectionId && t.ElectoralDistrictId == districtId, ct);

        if (existing is null)
        {
            existing = new DistrictTurnoutResult
            {
                Id = Guid.NewGuid(),
                ElectionId = ElectionId,
                ElectoralDistrictId = districtId
            };
            db.DistrictTurnoutResults.Add(existing);
        }

        existing.BallotsIssued = ballots;
        existing.VotesValid = valid;
        existing.VotesInvalid = invalid;
        existing.TurnoutPercent = turnout;
        existing.SourceImportRowId = sourceRowId;
        await db.SaveChangesAsync(ct);
    }

    public async Task UpsertListVoteResultsAsync(CancellationToken ct)
    {
        foreach (var (listId, totalVotes) in ListVoteTotals)
        {
            var list = await db.ElectoralLists.FirstAsync(l => l.Id == listId, ct);
            var existing = await db.ElectoralListVoteResults
                .FirstOrDefaultAsync(r =>
                    r.ElectoralListId == listId &&
                    r.ElectionId == ElectionId &&
                    r.ElectoralDistrictId == list.ElectoralDistrictId, ct);

            if (existing is null)
            {
                existing = new ElectoralListVoteResult
                {
                    Id = Guid.NewGuid(),
                    ElectoralListId = listId,
                    ElectionId = ElectionId,
                    ElectoralDistrictId = list.ElectoralDistrictId
                };
                db.ElectoralListVoteResults.Add(existing);
            }

            existing.VotesReceived = totalVotes;
        }

        await db.SaveChangesAsync(ct);
    }

    public async Task UpsertClubMembershipAsync(
        string clubName,
        string lastName,
        string firstName,
        DateOnly validFrom,
        CancellationToken ct)
    {
        var politicianId = await GetOrCreatePoliticianAsync(lastName, firstName, ct);
        var clubKey = $"{TermKey}:{Slug(clubName)}";

        var club = await db.ParliamentaryClubs.FirstOrDefaultAsync(c => c.NaturalKey == clubKey, ct);
        if (club is null)
        {
            club = new ParliamentaryClub
            {
                Id = Guid.NewGuid(),
                LegislativeTermId = TermId,
                Body = CollegialBodyType.Sejm,
                Name = clubName,
                NaturalKey = clubKey
            };
            db.ParliamentaryClubs.Add(club);
            await db.SaveChangesAsync(ct);
        }

        var exists = await db.ClubMemberships.AnyAsync(m =>
            m.ParliamentaryClubId == club.Id &&
            m.PoliticianId == politicianId &&
            m.ValidFrom == validFrom, ct);

        if (!exists)
        {
            db.ClubMemberships.Add(new ClubMembership
            {
                Id = Guid.NewGuid(),
                ParliamentaryClubId = club.Id,
                PoliticianId = politicianId,
                ValidFrom = validFrom,
                Source = ElectionKey
            });
            await db.SaveChangesAsync(ct);
        }
    }

    private static string NormalizeName(string lastName, string firstName) =>
        $"{lastName} {firstName}".Trim().ToLowerInvariant();

    private static string Slug(string value)
    {
        var chars = value.ToLowerInvariant()
            .Where(char.IsLetterOrDigit)
            .ToArray();
        return chars.Length > 0 ? new string(chars) : "unknown";
    }
}
