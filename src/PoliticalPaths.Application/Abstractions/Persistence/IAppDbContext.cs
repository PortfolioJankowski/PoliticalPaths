using Microsoft.EntityFrameworkCore;
using PoliticalPaths.Domain.Candidacies;
using PoliticalPaths.Domain.Elections;
using PoliticalPaths.Domain.Geography;
using PoliticalPaths.Domain.Imports;
using PoliticalPaths.Domain.Mandates;
using PoliticalPaths.Domain.Mapping;
using PoliticalPaths.Domain.Parties;
using PoliticalPaths.Domain.Politicians;
using PoliticalPaths.Domain.Results;

namespace PoliticalPaths.Application.Abstractions.Persistence;

public interface IAppDbContext
{
    // Import
    DbSet<ImportBatch> ImportBatches { get; }
    DbSet<ImportFile> ImportFiles { get; }
    DbSet<ImportRow> ImportRows { get; }
    DbSet<TransformationError> TransformationErrors { get; }

    // Domain
    DbSet<TerritorialUnit> TerritorialUnits { get; }
    DbSet<ElectoralDistrictTerritory> ElectoralDistrictTerritories { get; }
    DbSet<Election> Elections { get; }
    DbSet<ElectoralDistrict> ElectoralDistricts { get; }
    DbSet<ElectoralDistrictSnapshot> ElectoralDistrictSnapshots { get; }
    DbSet<ElectoralCommittee> ElectoralCommittees { get; }
    DbSet<ElectoralList> ElectoralLists { get; }
    DbSet<Politician> Politicians { get; }
    DbSet<PoliticianAlias> PoliticianAliases { get; }
    DbSet<PoliticianMergeOverride> PoliticianMergeOverrides { get; }
    DbSet<IdentityMatchCandidate> IdentityMatchCandidates { get; }
    DbSet<Party> Parties { get; }
    DbSet<PartyAffiliation> PartyAffiliations { get; }
    DbSet<ParliamentaryClub> ParliamentaryClubs { get; }
    DbSet<ClubMembership> ClubMemberships { get; }
    DbSet<Candidacy> Candidacies { get; }
    DbSet<CandidacyVoteResult> CandidacyVoteResults { get; }
    DbSet<ElectoralListVoteResult> ElectoralListVoteResults { get; }
    DbSet<DistrictTurnoutResult> DistrictTurnoutResults { get; }
    DbSet<LegislativeTerm> LegislativeTerms { get; }
    DbSet<Mandate> Mandates { get; }
    DbSet<MandateEvent> MandateEvents { get; }
    DbSet<ElectionMandateAllocation> ElectionMandateAllocations { get; }
    DbSet<ManualMapping> ManualMappings { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
