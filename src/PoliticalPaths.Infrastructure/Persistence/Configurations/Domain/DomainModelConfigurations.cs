using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PoliticalPaths.Domain.Candidacies;
using PoliticalPaths.Domain.Elections;
using PoliticalPaths.Domain.Geography;
using PoliticalPaths.Domain.Mandates;
using PoliticalPaths.Domain.Mapping;
using PoliticalPaths.Domain.Parties;
using PoliticalPaths.Domain.Politicians;
using PoliticalPaths.Domain.Results;

namespace PoliticalPaths.Infrastructure.Persistence.Configurations.Domain;

internal static class DomainModelConfigurations
{
    public static void Apply(ModelBuilder modelBuilder)
    {
        ConfigureTerritorialUnit(modelBuilder.Entity<TerritorialUnit>());
        ConfigureElectoralDistrictTerritory(modelBuilder.Entity<ElectoralDistrictTerritory>());
        ConfigureElection(modelBuilder.Entity<Election>());
        ConfigureElectoralDistrict(modelBuilder.Entity<ElectoralDistrict>());
        ConfigureElectoralDistrictSnapshot(modelBuilder.Entity<ElectoralDistrictSnapshot>());
        ConfigureElectoralCommittee(modelBuilder.Entity<ElectoralCommittee>());
        ConfigureElectoralList(modelBuilder.Entity<ElectoralList>());
        ConfigurePolitician(modelBuilder.Entity<Politician>());
        ConfigurePoliticianAlias(modelBuilder.Entity<PoliticianAlias>());
        ConfigurePoliticianMergeOverride(modelBuilder.Entity<PoliticianMergeOverride>());
        ConfigureIdentityMatchCandidate(modelBuilder.Entity<IdentityMatchCandidate>());
        ConfigureParty(modelBuilder.Entity<Party>());
        ConfigurePartyAffiliation(modelBuilder.Entity<PartyAffiliation>());
        ConfigureParliamentaryClub(modelBuilder.Entity<ParliamentaryClub>());
        ConfigureClubMembership(modelBuilder.Entity<ClubMembership>());
        ConfigureCandidacy(modelBuilder.Entity<Candidacy>());
        ConfigureCandidacyVoteResult(modelBuilder.Entity<CandidacyVoteResult>());
        ConfigureElectoralListVoteResult(modelBuilder.Entity<ElectoralListVoteResult>());
        ConfigureDistrictTurnoutResult(modelBuilder.Entity<DistrictTurnoutResult>());
        ConfigureLegislativeTerm(modelBuilder.Entity<LegislativeTerm>());
        ConfigureMandate(modelBuilder.Entity<Mandate>());
        ConfigureMandateEvent(modelBuilder.Entity<MandateEvent>());
        ConfigureElectionMandateAllocation(modelBuilder.Entity<ElectionMandateAllocation>());
        ConfigureManualMapping(modelBuilder.Entity<ManualMapping>());
    }

    private static void ConfigureTerritorialUnit(EntityTypeBuilder<TerritorialUnit> b)
    {
        b.ToTable("TerritorialUnits");
        b.HasKey(x => x.Id);
        b.Property(x => x.TerytCode).HasMaxLength(12).IsRequired();
        b.Property(x => x.Name).HasMaxLength(256).IsRequired();
        b.Property(x => x.ParentTerytCode).HasMaxLength(12);
        b.HasIndex(x => new { x.TerytCode, x.ValidFrom });
    }

    private static void ConfigureElectoralDistrictTerritory(EntityTypeBuilder<ElectoralDistrictTerritory> b)
    {
        b.ToTable("ElectoralDistrictTerritories");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.ElectoralDistrictId, x.TerritorialUnitId }).IsUnique();
        b.HasIndex(x => x.TerritorialUnitId);
        b.HasOne(x => x.ElectoralDistrict).WithMany(x => x.Territories).HasForeignKey(x => x.ElectoralDistrictId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.TerritorialUnit).WithMany(x => x.DistrictTerritories).HasForeignKey(x => x.TerritorialUnitId).OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureElection(EntityTypeBuilder<Election> b)
    {
        b.ToTable("Elections");
        b.HasKey(x => x.Id);
        b.Property(x => x.NaturalKey).HasMaxLength(128).IsRequired();
        b.HasIndex(x => x.NaturalKey).IsUnique();
        b.HasIndex(x => new { x.Year, x.Chamber, x.Scope });
        b.HasOne(x => x.VoivodeshipTerritorialUnit).WithMany().HasForeignKey(x => x.VoivodeshipTerritorialUnitId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.LegislativeTerm).WithMany().HasForeignKey(x => x.LegislativeTermId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ParentLegislativeTerm).WithMany().HasForeignKey(x => x.ParentLegislativeTermId).OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureElectoralDistrict(EntityTypeBuilder<ElectoralDistrict> b)
    {
        b.ToTable("ElectoralDistricts");
        b.HasKey(x => x.Id);
        b.Property(x => x.NaturalKey).HasMaxLength(128).IsRequired();
        b.Property(x => x.Name).HasMaxLength(256);
        b.HasIndex(x => x.NaturalKey).IsUnique();
        b.HasIndex(x => new { x.ElectionId, x.Chamber, x.DistrictNumber });
        b.HasOne(x => x.Election).WithMany(x => x.Districts).HasForeignKey(x => x.ElectionId).OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureElectoralDistrictSnapshot(EntityTypeBuilder<ElectoralDistrictSnapshot> b)
    {
        b.ToTable("ElectoralDistrictSnapshots");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.ElectoralDistrictId, x.ElectionId, x.StatisticsDate }).IsUnique();
        b.HasOne(x => x.ElectoralDistrict).WithMany(x => x.Snapshots).HasForeignKey(x => x.ElectoralDistrictId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Election).WithMany().HasForeignKey(x => x.ElectionId).OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureElectoralCommittee(EntityTypeBuilder<ElectoralCommittee> b)
    {
        b.ToTable("ElectoralCommittees");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(512).IsRequired();
        b.Property(x => x.ShortName).HasMaxLength(128);
        b.Property(x => x.NaturalKey).HasMaxLength(128).IsRequired();
        b.HasIndex(x => x.NaturalKey).IsUnique();
        b.HasOne(x => x.Election).WithMany(x => x.Committees).HasForeignKey(x => x.ElectionId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Party).WithMany().HasForeignKey(x => x.PartyId).OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureElectoralList(EntityTypeBuilder<ElectoralList> b)
    {
        b.ToTable("ElectoralLists");
        b.HasKey(x => x.Id);
        b.Property(x => x.NaturalKey).HasMaxLength(128).IsRequired();
        b.HasIndex(x => x.NaturalKey).IsUnique();
        b.HasIndex(x => new { x.ElectoralDistrictId, x.ListNumber });
        b.HasOne(x => x.Election).WithMany(x => x.Lists).HasForeignKey(x => x.ElectionId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.ElectoralDistrict).WithMany(x => x.Lists).HasForeignKey(x => x.ElectoralDistrictId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ElectoralCommittee).WithMany(x => x.Lists).HasForeignKey(x => x.ElectoralCommitteeId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Party).WithMany().HasForeignKey(x => x.PartyId).OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigurePolitician(EntityTypeBuilder<Politician> b)
    {
        b.ToTable("Politicians");
        b.HasKey(x => x.Id);
        b.Property(x => x.NormalizedName).HasMaxLength(256).IsRequired();
        b.Property(x => x.DisplayName).HasMaxLength(256);
        b.Property(x => x.PkwCandidateId).HasMaxLength(64);
        b.HasIndex(x => x.NormalizedName);
        b.HasIndex(x => x.PkwCandidateId);
    }

    private static void ConfigurePoliticianAlias(EntityTypeBuilder<PoliticianAlias> b)
    {
        b.ToTable("PoliticianAliases");
        b.HasKey(x => x.Id);
        b.Property(x => x.AliasName).HasMaxLength(256).IsRequired();
        b.Property(x => x.NormalizedAlias).HasMaxLength(256).IsRequired();
        b.Property(x => x.Source).HasMaxLength(128);
        b.HasIndex(x => x.NormalizedAlias);
        b.HasOne(x => x.Politician).WithMany(x => x.Aliases).HasForeignKey(x => x.PoliticianId).OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigurePoliticianMergeOverride(EntityTypeBuilder<PoliticianMergeOverride> b)
    {
        b.ToTable("PoliticianMergeOverrides");
        b.HasKey(x => x.Id);
        b.Property(x => x.Reason).HasMaxLength(512);
        b.Property(x => x.CreatedBy).HasMaxLength(128);
        b.HasIndex(x => x.SourcePoliticianId).IsUnique();
        b.HasOne(x => x.SourcePolitician).WithMany().HasForeignKey(x => x.SourcePoliticianId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.TargetPolitician).WithMany().HasForeignKey(x => x.TargetPoliticianId).OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureIdentityMatchCandidate(EntityTypeBuilder<IdentityMatchCandidate> b)
    {
        b.ToTable("IdentityMatchCandidates");
        b.HasKey(x => x.Id);
        b.Property(x => x.Score).HasPrecision(5, 4);
        b.HasOne(x => x.Politician).WithMany().HasForeignKey(x => x.PoliticianId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.MatchedPolitician).WithMany().HasForeignKey(x => x.MatchedPoliticianId).OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureParty(EntityTypeBuilder<Party> b)
    {
        b.ToTable("Parties");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(256).IsRequired();
        b.Property(x => x.ShortName).HasMaxLength(64);
        b.Property(x => x.NaturalKey).HasMaxLength(128).IsRequired();
        b.HasIndex(x => x.NaturalKey).IsUnique();
    }

    private static void ConfigurePartyAffiliation(EntityTypeBuilder<PartyAffiliation> b)
    {
        b.ToTable("PartyAffiliations");
        b.HasKey(x => x.Id);
        b.Property(x => x.Source).HasMaxLength(128);
        b.HasIndex(x => new { x.PoliticianId, x.PartyId, x.ValidFrom });
        b.HasOne(x => x.Politician).WithMany().HasForeignKey(x => x.PoliticianId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Party).WithMany().HasForeignKey(x => x.PartyId).OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureParliamentaryClub(EntityTypeBuilder<ParliamentaryClub> b)
    {
        b.ToTable("ParliamentaryClubs");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(256).IsRequired();
        b.Property(x => x.NaturalKey).HasMaxLength(128).IsRequired();
        b.HasIndex(x => x.NaturalKey).IsUnique();
        b.HasOne(x => x.LegislativeTerm).WithMany().HasForeignKey(x => x.LegislativeTermId).OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureClubMembership(EntityTypeBuilder<ClubMembership> b)
    {
        b.ToTable("ClubMemberships");
        b.HasKey(x => x.Id);
        b.Property(x => x.Source).HasMaxLength(128);
        b.HasIndex(x => new { x.ParliamentaryClubId, x.PoliticianId, x.ValidFrom });
        b.HasOne(x => x.ParliamentaryClub).WithMany(x => x.Memberships).HasForeignKey(x => x.ParliamentaryClubId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Politician).WithMany().HasForeignKey(x => x.PoliticianId).OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureCandidacy(EntityTypeBuilder<Candidacy> b)
    {
        b.ToTable("Candidacies");
        b.HasKey(x => x.Id);
        b.Property(x => x.SourceFingerprint).HasMaxLength(256).IsRequired();
        b.HasIndex(x => x.SourceFingerprint).IsUnique();
        b.HasIndex(x => new { x.PoliticianId, x.ElectionId });
        b.HasOne(x => x.Politician).WithMany().HasForeignKey(x => x.PoliticianId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Election).WithMany().HasForeignKey(x => x.ElectionId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ElectoralDistrict).WithMany().HasForeignKey(x => x.ElectoralDistrictId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ElectoralList).WithMany().HasForeignKey(x => x.ElectoralListId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ElectoralCommittee).WithMany().HasForeignKey(x => x.ElectoralCommitteeId).OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureCandidacyVoteResult(EntityTypeBuilder<CandidacyVoteResult> b)
    {
        b.ToTable("CandidacyVoteResults");
        b.HasKey(x => x.Id);
        b.Property(x => x.VotePercent).HasPrecision(9, 4);
        b.HasIndex(x => new { x.ElectionId, x.ElectoralDistrictId });
        b.HasIndex(x => x.CandidacyId).IsUnique();
        b.HasOne(x => x.Candidacy).WithOne(x => x.VoteResult).HasForeignKey<CandidacyVoteResult>(x => x.CandidacyId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Election).WithMany().HasForeignKey(x => x.ElectionId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ElectoralDistrict).WithMany().HasForeignKey(x => x.ElectoralDistrictId).OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureElectoralListVoteResult(EntityTypeBuilder<ElectoralListVoteResult> b)
    {
        b.ToTable("ElectoralListVoteResults");
        b.HasKey(x => x.Id);
        b.Property(x => x.VotePercent).HasPrecision(9, 4);
        b.HasIndex(x => new { x.ElectoralListId, x.ElectionId, x.ElectoralDistrictId }).IsUnique();
        b.HasOne(x => x.ElectoralList).WithMany().HasForeignKey(x => x.ElectoralListId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Election).WithMany().HasForeignKey(x => x.ElectionId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ElectoralDistrict).WithMany().HasForeignKey(x => x.ElectoralDistrictId).OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureDistrictTurnoutResult(EntityTypeBuilder<DistrictTurnoutResult> b)
    {
        b.ToTable("DistrictTurnoutResults");
        b.HasKey(x => x.Id);
        b.Property(x => x.TurnoutPercent).HasPrecision(9, 4);
        b.HasIndex(x => new { x.ElectionId, x.ElectoralDistrictId }).IsUnique();
        b.HasOne(x => x.Election).WithMany().HasForeignKey(x => x.ElectionId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ElectoralDistrict).WithMany().HasForeignKey(x => x.ElectoralDistrictId).OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureLegislativeTerm(EntityTypeBuilder<LegislativeTerm> b)
    {
        b.ToTable("LegislativeTerms");
        b.HasKey(x => x.Id);
        b.Property(x => x.NaturalKey).HasMaxLength(128).IsRequired();
        b.HasIndex(x => x.NaturalKey).IsUnique();
        b.HasIndex(x => new { x.Body, x.TermNumber });
        b.HasOne(x => x.FoundingElection).WithMany().HasForeignKey(x => x.FoundingElectionId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.VoivodeshipTerritorialUnit).WithMany().HasForeignKey(x => x.VoivodeshipTerritorialUnitId).OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureMandate(EntityTypeBuilder<Mandate> b)
    {
        b.ToTable("Mandates");
        b.HasKey(x => x.Id);
        b.Property(x => x.TerminationNote).HasMaxLength(512);
        b.HasIndex(x => new { x.LegislativeTermId, x.Body, x.ValidFrom, x.ValidTo });
        b.HasIndex(x => new { x.PoliticianId, x.ValidFrom });
        b.HasIndex(x => new { x.ElectoralDistrictId, x.LegislativeTermId });
        b.HasIndex(x => x.PredecessorMandateId);
        b.HasOne(x => x.LegislativeTerm).WithMany(x => x.Mandates).HasForeignKey(x => x.LegislativeTermId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Politician).WithMany().HasForeignKey(x => x.PoliticianId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ElectoralDistrict).WithMany().HasForeignKey(x => x.ElectoralDistrictId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ElectoralList).WithMany().HasForeignKey(x => x.ElectoralListId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ElectoralCommittee).WithMany().HasForeignKey(x => x.ElectoralCommitteeId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.OriginatingCandidacy).WithMany().HasForeignKey(x => x.OriginatingCandidacyId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.OriginatingElection).WithMany().HasForeignKey(x => x.OriginatingElectionId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.PredecessorMandate).WithMany(x => x.SuccessorMandates).HasForeignKey(x => x.PredecessorMandateId).OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureMandateEvent(EntityTypeBuilder<MandateEvent> b)
    {
        b.ToTable("MandateEvents");
        b.HasKey(x => x.Id);
        b.Property(x => x.SourceUrl).HasMaxLength(1024);
        b.Property(x => x.SourceDocumentRef).HasMaxLength(256);
        b.Property(x => x.DetailsJson).HasColumnType("longtext");
        b.HasIndex(x => x.MandateId);
        b.HasOne(x => x.Mandate).WithMany(x => x.Events).HasForeignKey(x => x.MandateId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.RelatedMandate).WithMany().HasForeignKey(x => x.RelatedMandateId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.RelatedElection).WithMany().HasForeignKey(x => x.RelatedElectionId).OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureElectionMandateAllocation(EntityTypeBuilder<ElectionMandateAllocation> b)
    {
        b.ToTable("ElectionMandateAllocations");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.ElectionId, x.CandidacyId }).IsUnique();
        b.HasOne(x => x.Election).WithMany().HasForeignKey(x => x.ElectionId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Candidacy).WithOne(x => x.MandateAllocation).HasForeignKey<ElectionMandateAllocation>(x => x.CandidacyId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Politician).WithMany().HasForeignKey(x => x.PoliticianId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ElectoralDistrict).WithMany().HasForeignKey(x => x.ElectoralDistrictId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ElectoralList).WithMany().HasForeignKey(x => x.ElectoralListId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Mandate).WithMany().HasForeignKey(x => x.MandateId).OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureManualMapping(EntityTypeBuilder<ManualMapping> b)
    {
        b.ToTable("ManualMappings");
        b.HasKey(x => x.Id);
        b.Property(x => x.SourceKey).HasMaxLength(256).IsRequired();
        b.Property(x => x.TargetEntityType).HasMaxLength(128).IsRequired();
        b.Property(x => x.Notes).HasMaxLength(512);
        b.Property(x => x.CreatedBy).HasMaxLength(128);
        b.HasIndex(x => new { x.Category, x.SourceKey }).IsUnique();
    }
}
