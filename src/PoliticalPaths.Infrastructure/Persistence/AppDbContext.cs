using Microsoft.EntityFrameworkCore;
using PoliticalPaths.Application.Abstractions.Persistence;
using PoliticalPaths.Domain.Candidacies;
using PoliticalPaths.Domain.Elections;
using PoliticalPaths.Domain.Geography;
using PoliticalPaths.Domain.Imports;
using PoliticalPaths.Domain.Mandates;
using PoliticalPaths.Domain.Mapping;
using PoliticalPaths.Domain.Parties;
using PoliticalPaths.Domain.Politicians;
using PoliticalPaths.Domain.Results;
using PoliticalPaths.Infrastructure.Persistence.Configurations.Domain;

namespace PoliticalPaths.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IAppDbContext
{
    // Import (ETL)
    public DbSet<ImportBatch> ImportBatches => Set<ImportBatch>();
    public DbSet<ImportFile> ImportFiles => Set<ImportFile>();
    public DbSet<ImportRow> ImportRows => Set<ImportRow>();
    public DbSet<TransformationError> TransformationErrors => Set<TransformationError>();


    // Geography
    public DbSet<TerritorialUnit> TerritorialUnits => Set<TerritorialUnit>();
    public DbSet<ElectoralDistrictTerritory> ElectoralDistrictTerritories => Set<ElectoralDistrictTerritory>();

    // Elections
    public DbSet<Election> Elections => Set<Election>();
    public DbSet<ElectoralDistrict> ElectoralDistricts => Set<ElectoralDistrict>();
    public DbSet<ElectoralDistrictSnapshot> ElectoralDistrictSnapshots => Set<ElectoralDistrictSnapshot>();
    public DbSet<ElectoralCommittee> ElectoralCommittees => Set<ElectoralCommittee>();
    public DbSet<ElectoralList> ElectoralLists => Set<ElectoralList>();

    // Politicians & identity
    public DbSet<Politician> Politicians => Set<Politician>();
    public DbSet<PoliticianAlias> PoliticianAliases => Set<PoliticianAlias>();
    public DbSet<PoliticianMergeOverride> PoliticianMergeOverrides => Set<PoliticianMergeOverride>();
    public DbSet<IdentityMatchCandidate> IdentityMatchCandidates => Set<IdentityMatchCandidate>();

    // Parties & clubs
    public DbSet<Party> Parties => Set<Party>();
    public DbSet<PartyAffiliation> PartyAffiliations => Set<PartyAffiliation>();
    public DbSet<ParliamentaryClub> ParliamentaryClubs => Set<ParliamentaryClub>();
    public DbSet<ClubMembership> ClubMemberships => Set<ClubMembership>();

    // Candidacies & results
    public DbSet<Candidacy> Candidacies => Set<Candidacy>();
    public DbSet<CandidacyVoteResult> CandidacyVoteResults => Set<CandidacyVoteResult>();
    public DbSet<ElectoralListVoteResult> ElectoralListVoteResults => Set<ElectoralListVoteResult>();
    public DbSet<DistrictTurnoutResult> DistrictTurnoutResults => Set<DistrictTurnoutResult>();

    // Mandates & tenure
    public DbSet<LegislativeTerm> LegislativeTerms => Set<LegislativeTerm>();
    public DbSet<Mandate> Mandates => Set<Mandate>();
    public DbSet<MandateEvent> MandateEvents => Set<MandateEvent>();
    public DbSet<ElectionMandateAllocation> ElectionMandateAllocations => Set<ElectionMandateAllocation>();

    // Manual mappings
    public DbSet<ManualMapping> ManualMappings => Set<ManualMapping>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("app");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        DomainModelConfigurations.Apply(modelBuilder);
        base.OnModelCreating(modelBuilder);
    }
}
