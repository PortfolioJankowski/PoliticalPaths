using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PoliticalPaths.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DomainModelSkeleton : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ManualMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Category = table.Column<int>(type: "int", nullable: false),
                    SourceKey = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TargetEntityType = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TargetEntityId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Notes = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedBy = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManualMappings", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Parties",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ShortName = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NaturalKey = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parties", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Politicians",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    NormalizedName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DisplayName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: true),
                    PkwCandidateId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Politicians", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TerritorialUnits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TerytCode = table.Column<string>(type: "varchar(12)", maxLength: 12, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Level = table.Column<int>(type: "int", nullable: false),
                    ParentTerytCode = table.Column<string>(type: "varchar(12)", maxLength: 12, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ValidFrom = table.Column<DateOnly>(type: "date", nullable: true),
                    ValidTo = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TerritorialUnits", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "IdentityMatchCandidates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PoliticianId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    MatchedPoliticianId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    SourceImportRowId = table.Column<long>(type: "bigint", nullable: true),
                    Score = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentityMatchCandidates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IdentityMatchCandidates_Politicians_MatchedPoliticianId",
                        column: x => x.MatchedPoliticianId,
                        principalTable: "Politicians",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IdentityMatchCandidates_Politicians_PoliticianId",
                        column: x => x.PoliticianId,
                        principalTable: "Politicians",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PartyAffiliations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PoliticianId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PartyId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ValidFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    ValidTo = table.Column<DateOnly>(type: "date", nullable: true),
                    Source = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartyAffiliations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartyAffiliations_Parties_PartyId",
                        column: x => x.PartyId,
                        principalTable: "Parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartyAffiliations_Politicians_PoliticianId",
                        column: x => x.PoliticianId,
                        principalTable: "Politicians",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PoliticianAliases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PoliticianId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AliasName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NormalizedAlias = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ValidFrom = table.Column<DateOnly>(type: "date", nullable: true),
                    ValidTo = table.Column<DateOnly>(type: "date", nullable: true),
                    Source = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoliticianAliases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PoliticianAliases_Politicians_PoliticianId",
                        column: x => x.PoliticianId,
                        principalTable: "Politicians",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PoliticianMergeOverrides",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    SourcePoliticianId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TargetPoliticianId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Reason = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedBy = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoliticianMergeOverrides", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PoliticianMergeOverrides_Politicians_SourcePoliticianId",
                        column: x => x.SourcePoliticianId,
                        principalTable: "Politicians",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PoliticianMergeOverrides_Politicians_TargetPoliticianId",
                        column: x => x.TargetPoliticianId,
                        principalTable: "Politicians",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Candidacies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PoliticianId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ElectionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Profile = table.Column<int>(type: "int", nullable: false),
                    ElectoralDistrictId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    ElectoralListId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    ElectoralCommitteeId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    ListPosition = table.Column<int>(type: "int", nullable: true),
                    SourceFingerprint = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceImportRowId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Candidacies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Candidacies_Politicians_PoliticianId",
                        column: x => x.PoliticianId,
                        principalTable: "Politicians",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CandidacyVoteResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CandidacyId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ElectionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ElectoralDistrictId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    VotesReceived = table.Column<int>(type: "int", nullable: true),
                    PreferentialVotes = table.Column<int>(type: "int", nullable: true),
                    VotePercent = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: true),
                    Elected = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    SourceImportRowId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidacyVoteResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CandidacyVoteResults_Candidacies_CandidacyId",
                        column: x => x.CandidacyId,
                        principalTable: "Candidacies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ClubMemberships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ParliamentaryClubId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PoliticianId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ValidFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    ValidTo = table.Column<DateOnly>(type: "date", nullable: true),
                    Source = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubMemberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClubMemberships_Politicians_PoliticianId",
                        column: x => x.PoliticianId,
                        principalTable: "Politicians",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DistrictTurnoutResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ElectionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ElectoralDistrictId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BallotsIssued = table.Column<int>(type: "int", nullable: true),
                    VotesValid = table.Column<int>(type: "int", nullable: true),
                    VotesInvalid = table.Column<int>(type: "int", nullable: true),
                    TurnoutPercent = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: true),
                    SourceImportRowId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DistrictTurnoutResults", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ElectionMandateAllocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ElectionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CandidacyId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PoliticianId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ElectoralDistrictId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ElectoralListId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    RankOnListByVotes = table.Column<int>(type: "int", nullable: false),
                    AllocatedSeat = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MandateId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    AllocationAnnouncedOn = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElectionMandateAllocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ElectionMandateAllocations_Candidacies_CandidacyId",
                        column: x => x.CandidacyId,
                        principalTable: "Candidacies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ElectionMandateAllocations_Politicians_PoliticianId",
                        column: x => x.PoliticianId,
                        principalTable: "Politicians",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Elections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Chamber = table.Column<int>(type: "int", nullable: false),
                    Scope = table.Column<int>(type: "int", nullable: false),
                    Profile = table.Column<int>(type: "int", nullable: false),
                    Kind = table.Column<int>(type: "int", nullable: false),
                    VoivodeshipTerritorialUnitId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    ElectionDate = table.Column<DateOnly>(type: "date", nullable: true),
                    NaturalKey = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LegislativeTermId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    ReplacesMandateId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    ParentLegislativeTermId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Elections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Elections_TerritorialUnits_VoivodeshipTerritorialUnitId",
                        column: x => x.VoivodeshipTerritorialUnitId,
                        principalTable: "TerritorialUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ElectoralCommittees",
                
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ElectionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ShortName = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    PartyId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    NaturalKey = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElectoralCommittees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ElectoralCommittees_Elections_ElectionId",
                        column: x => x.ElectionId,
                        principalTable: "Elections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ElectoralCommittees_Parties_PartyId",
                        column: x => x.PartyId,
                        principalTable: "Parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ElectoralDistricts",
                
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ElectionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Chamber = table.Column<int>(type: "int", nullable: false),
                    DistrictNumber = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NaturalKey = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElectoralDistricts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ElectoralDistricts_Elections_ElectionId",
                        column: x => x.ElectionId,
                        principalTable: "Elections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LegislativeTerms",
                
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Body = table.Column<int>(type: "int", nullable: false),
                    TermNumber = table.Column<int>(type: "int", nullable: false),
                    ConstituentSessionDate = table.Column<DateOnly>(type: "date", nullable: true),
                    DissolvedOn = table.Column<DateOnly>(type: "date", nullable: true),
                    FoundingElectionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    VoivodeshipTerritorialUnitId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    NaturalKey = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LegislativeTerms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LegislativeTerms_Elections_FoundingElectionId",
                        column: x => x.FoundingElectionId,
                        principalTable: "Elections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LegislativeTerms_TerritorialUnits_VoivodeshipTerritorialUnit~",
                        column: x => x.VoivodeshipTerritorialUnitId,
                        principalTable: "TerritorialUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ElectoralDistrictSnapshots",
                
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ElectoralDistrictId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ElectionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Population = table.Column<int>(type: "int", nullable: true),
                    EligibleVoters = table.Column<int>(type: "int", nullable: true),
                    RegisteredVoters = table.Column<int>(type: "int", nullable: true),
                    SeatsAllocated = table.Column<int>(type: "int", nullable: true),
                    StatisticsDate = table.Column<DateOnly>(type: "date", nullable: true),
                    SourceImportRowId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElectoralDistrictSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ElectoralDistrictSnapshots_Elections_ElectionId",
                        column: x => x.ElectionId,
                        principalTable: "Elections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ElectoralDistrictSnapshots_ElectoralDistricts_ElectoralDistr~",
                        column: x => x.ElectoralDistrictId,
                        principalTable: "ElectoralDistricts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ElectoralDistrictTerritories",
                
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ElectoralDistrictId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TerritorialUnitId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CoverageType = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElectoralDistrictTerritories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ElectoralDistrictTerritories_ElectoralDistricts_ElectoralDis~",
                        column: x => x.ElectoralDistrictId,
                        principalTable: "ElectoralDistricts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ElectoralDistrictTerritories_TerritorialUnits_TerritorialUni~",
                        column: x => x.TerritorialUnitId,
                        principalTable: "TerritorialUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ElectoralLists",
                
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ElectionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ElectoralDistrictId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ElectoralCommitteeId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ListNumber = table.Column<int>(type: "int", nullable: false),
                    PartyId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    NaturalKey = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElectoralLists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ElectoralLists_Elections_ElectionId",
                        column: x => x.ElectionId,
                        principalTable: "Elections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ElectoralLists_ElectoralCommittees_ElectoralCommitteeId",
                        column: x => x.ElectoralCommitteeId,
                        principalTable: "ElectoralCommittees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ElectoralLists_ElectoralDistricts_ElectoralDistrictId",
                        column: x => x.ElectoralDistrictId,
                        principalTable: "ElectoralDistricts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ElectoralLists_Parties_PartyId",
                        column: x => x.PartyId,
                        principalTable: "Parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ParliamentaryClubs",
                
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    LegislativeTermId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Body = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NaturalKey = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParliamentaryClubs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParliamentaryClubs_LegislativeTerms_LegislativeTermId",
                        column: x => x.LegislativeTermId,
                        principalTable: "LegislativeTerms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ElectoralListVoteResults",
                
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ElectoralListId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ElectionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ElectoralDistrictId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    VotesReceived = table.Column<int>(type: "int", nullable: true),
                    VotePercent = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: true),
                    SeatsWon = table.Column<int>(type: "int", nullable: true),
                    SourceImportRowId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElectoralListVoteResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ElectoralListVoteResults_Elections_ElectionId",
                        column: x => x.ElectionId,
                        principalTable: "Elections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ElectoralListVoteResults_ElectoralDistricts_ElectoralDistric~",
                        column: x => x.ElectoralDistrictId,
                        principalTable: "ElectoralDistricts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ElectoralListVoteResults_ElectoralLists_ElectoralListId",
                        column: x => x.ElectoralListId,
                        principalTable: "ElectoralLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Mandates",
                
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    LegislativeTermId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PoliticianId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Body = table.Column<int>(type: "int", nullable: false),
                    ElectoralDistrictId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    ElectoralListId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    ElectoralCommitteeId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    OriginatingCandidacyId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    OriginatingElectionId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    AcquisitionType = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ValidFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    ValidTo = table.Column<DateOnly>(type: "date", nullable: true),
                    TerminationReason = table.Column<int>(type: "int", nullable: true),
                    TerminationNote = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PredecessorMandateId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    SuccessorPriorityOnList = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mandates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Mandates_Candidacies_OriginatingCandidacyId",
                        column: x => x.OriginatingCandidacyId,
                        principalTable: "Candidacies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Mandates_Elections_OriginatingElectionId",
                        column: x => x.OriginatingElectionId,
                        principalTable: "Elections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Mandates_ElectoralCommittees_ElectoralCommitteeId",
                        column: x => x.ElectoralCommitteeId,
                        principalTable: "ElectoralCommittees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Mandates_ElectoralDistricts_ElectoralDistrictId",
                        column: x => x.ElectoralDistrictId,
                        principalTable: "ElectoralDistricts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Mandates_ElectoralLists_ElectoralListId",
                        column: x => x.ElectoralListId,
                        principalTable: "ElectoralLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Mandates_LegislativeTerms_LegislativeTermId",
                        column: x => x.LegislativeTermId,
                        principalTable: "LegislativeTerms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Mandates_Mandates_PredecessorMandateId",
                        column: x => x.PredecessorMandateId,
                        principalTable: "Mandates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Mandates_Politicians_PoliticianId",
                        column: x => x.PoliticianId,
                        principalTable: "Politicians",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MandateEvents",
                
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MandateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EffectiveDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Reason = table.Column<int>(type: "int", nullable: true),
                    RelatedMandateId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    RelatedElectionId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    SourceUrl = table.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceDocumentRef = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceImportRowId = table.Column<long>(type: "bigint", nullable: true),
                    DetailsJson = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MandateEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MandateEvents_Elections_RelatedElectionId",
                        column: x => x.RelatedElectionId,
                        principalTable: "Elections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MandateEvents_Mandates_MandateId",
                        column: x => x.MandateId,
                        principalTable: "Mandates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MandateEvents_Mandates_RelatedMandateId",
                        column: x => x.RelatedMandateId,
                        principalTable: "Mandates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Candidacies_ElectionId",
                table: "Candidacies",
                column: "ElectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Candidacies_ElectoralCommitteeId",
                table: "Candidacies",
                column: "ElectoralCommitteeId");

            migrationBuilder.CreateIndex(
                name: "IX_Candidacies_ElectoralDistrictId",
                table: "Candidacies",
                column: "ElectoralDistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_Candidacies_ElectoralListId",
                table: "Candidacies",
                column: "ElectoralListId");

            migrationBuilder.CreateIndex(
                name: "IX_Candidacies_PoliticianId_ElectionId",
                table: "Candidacies",
                columns: new[] { "PoliticianId", "ElectionId" });

            migrationBuilder.CreateIndex(
                name: "IX_Candidacies_SourceFingerprint",
                table: "Candidacies",
                column: "SourceFingerprint",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CandidacyVoteResults_CandidacyId",
                table: "CandidacyVoteResults",
                column: "CandidacyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CandidacyVoteResults_ElectionId_ElectoralDistrictId",
                table: "CandidacyVoteResults",
                columns: new[] { "ElectionId", "ElectoralDistrictId" });

            migrationBuilder.CreateIndex(
                name: "IX_CandidacyVoteResults_ElectoralDistrictId",
                table: "CandidacyVoteResults",
                column: "ElectoralDistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_ClubMemberships_ParliamentaryClubId_PoliticianId_ValidFrom",
                table: "ClubMemberships",
                columns: new[] { "ParliamentaryClubId", "PoliticianId", "ValidFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_ClubMemberships_PoliticianId",
                table: "ClubMemberships",
                column: "PoliticianId");

            migrationBuilder.CreateIndex(
                name: "IX_DistrictTurnoutResults_ElectionId_ElectoralDistrictId",
                table: "DistrictTurnoutResults",
                columns: new[] { "ElectionId", "ElectoralDistrictId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DistrictTurnoutResults_ElectoralDistrictId",
                table: "DistrictTurnoutResults",
                column: "ElectoralDistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_ElectionMandateAllocations_CandidacyId",
                table: "ElectionMandateAllocations",
                column: "CandidacyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ElectionMandateAllocations_ElectionId_CandidacyId",
                table: "ElectionMandateAllocations",
                columns: new[] { "ElectionId", "CandidacyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ElectionMandateAllocations_ElectoralDistrictId",
                table: "ElectionMandateAllocations",
                column: "ElectoralDistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_ElectionMandateAllocations_ElectoralListId",
                table: "ElectionMandateAllocations",
                column: "ElectoralListId");

            migrationBuilder.CreateIndex(
                name: "IX_ElectionMandateAllocations_MandateId",
                table: "ElectionMandateAllocations",
                column: "MandateId");

            migrationBuilder.CreateIndex(
                name: "IX_ElectionMandateAllocations_PoliticianId",
                table: "ElectionMandateAllocations",
                column: "PoliticianId");

            migrationBuilder.CreateIndex(
                name: "IX_Elections_LegislativeTermId",
                table: "Elections",
                column: "LegislativeTermId");

            migrationBuilder.CreateIndex(
                name: "IX_Elections_NaturalKey",
                table: "Elections",
                column: "NaturalKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Elections_ParentLegislativeTermId",
                table: "Elections",
                column: "ParentLegislativeTermId");

            migrationBuilder.CreateIndex(
                name: "IX_Elections_VoivodeshipTerritorialUnitId",
                table: "Elections",
                column: "VoivodeshipTerritorialUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Elections_Year_Chamber_Scope",
                table: "Elections",
                columns: new[] { "Year", "Chamber", "Scope" });

            migrationBuilder.CreateIndex(
                name: "IX_ElectoralCommittees_ElectionId",
                table: "ElectoralCommittees",
                column: "ElectionId");

            migrationBuilder.CreateIndex(
                name: "IX_ElectoralCommittees_NaturalKey",
                table: "ElectoralCommittees",
                column: "NaturalKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ElectoralCommittees_PartyId",
                table: "ElectoralCommittees",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "IX_ElectoralDistricts_ElectionId_Chamber_DistrictNumber",
                table: "ElectoralDistricts",
                columns: new[] { "ElectionId", "Chamber", "DistrictNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_ElectoralDistricts_NaturalKey",
                table: "ElectoralDistricts",
                column: "NaturalKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ElectoralDistrictSnapshots_ElectionId",
                table: "ElectoralDistrictSnapshots",
                column: "ElectionId");

            migrationBuilder.CreateIndex(
                name: "IX_ElectoralDistrictSnapshots_ElectoralDistrictId_ElectionId_St~",
                table: "ElectoralDistrictSnapshots",
                columns: new[] { "ElectoralDistrictId", "ElectionId", "StatisticsDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ElectoralDistrictTerritories_ElectoralDistrictId_Territorial~",
                table: "ElectoralDistrictTerritories",
                columns: new[] { "ElectoralDistrictId", "TerritorialUnitId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ElectoralDistrictTerritories_TerritorialUnitId",
                table: "ElectoralDistrictTerritories",
                column: "TerritorialUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_ElectoralLists_ElectionId",
                table: "ElectoralLists",
                column: "ElectionId");

            migrationBuilder.CreateIndex(
                name: "IX_ElectoralLists_ElectoralCommitteeId",
                table: "ElectoralLists",
                column: "ElectoralCommitteeId");

            migrationBuilder.CreateIndex(
                name: "IX_ElectoralLists_ElectoralDistrictId_ListNumber",
                table: "ElectoralLists",
                columns: new[] { "ElectoralDistrictId", "ListNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_ElectoralLists_NaturalKey",
                table: "ElectoralLists",
                column: "NaturalKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ElectoralLists_PartyId",
                table: "ElectoralLists",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "IX_ElectoralListVoteResults_ElectionId",
                table: "ElectoralListVoteResults",
                column: "ElectionId");

            migrationBuilder.CreateIndex(
                name: "IX_ElectoralListVoteResults_ElectoralDistrictId",
                table: "ElectoralListVoteResults",
                column: "ElectoralDistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_ElectoralListVoteResults_ElectoralListId_ElectionId_Electora~",
                table: "ElectoralListVoteResults",
                columns: new[] { "ElectoralListId", "ElectionId", "ElectoralDistrictId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IdentityMatchCandidates_MatchedPoliticianId",
                table: "IdentityMatchCandidates",
                column: "MatchedPoliticianId");

            migrationBuilder.CreateIndex(
                name: "IX_IdentityMatchCandidates_PoliticianId",
                table: "IdentityMatchCandidates",
                column: "PoliticianId");

            migrationBuilder.CreateIndex(
                name: "IX_LegislativeTerms_Body_TermNumber",
                table: "LegislativeTerms",
                columns: new[] { "Body", "TermNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_LegislativeTerms_FoundingElectionId",
                table: "LegislativeTerms",
                column: "FoundingElectionId");

            migrationBuilder.CreateIndex(
                name: "IX_LegislativeTerms_NaturalKey",
                table: "LegislativeTerms",
                column: "NaturalKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LegislativeTerms_VoivodeshipTerritorialUnitId",
                table: "LegislativeTerms",
                column: "VoivodeshipTerritorialUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_MandateEvents_MandateId",
                
                table: "MandateEvents",
                column: "MandateId");

            migrationBuilder.CreateIndex(
                name: "IX_MandateEvents_RelatedElectionId",
                
                table: "MandateEvents",
                column: "RelatedElectionId");

            migrationBuilder.CreateIndex(
                name: "IX_MandateEvents_RelatedMandateId",
                
                table: "MandateEvents",
                column: "RelatedMandateId");

            migrationBuilder.CreateIndex(
                name: "IX_Mandates_ElectoralCommitteeId",
                
                table: "Mandates",
                column: "ElectoralCommitteeId");

            migrationBuilder.CreateIndex(
                name: "IX_Mandates_ElectoralDistrictId_LegislativeTermId",
                
                table: "Mandates",
                columns: new[] { "ElectoralDistrictId", "LegislativeTermId" });

            migrationBuilder.CreateIndex(
                name: "IX_Mandates_ElectoralListId",
                
                table: "Mandates",
                column: "ElectoralListId");

            migrationBuilder.CreateIndex(
                name: "IX_Mandates_LegislativeTermId_Body_ValidFrom_ValidTo",
                
                table: "Mandates",
                columns: new[] { "LegislativeTermId", "Body", "ValidFrom", "ValidTo" });

            migrationBuilder.CreateIndex(
                name: "IX_Mandates_OriginatingCandidacyId",
                
                table: "Mandates",
                column: "OriginatingCandidacyId");

            migrationBuilder.CreateIndex(
                name: "IX_Mandates_OriginatingElectionId",
                
                table: "Mandates",
                column: "OriginatingElectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Mandates_PoliticianId_ValidFrom",
                
                table: "Mandates",
                columns: new[] { "PoliticianId", "ValidFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_Mandates_PredecessorMandateId",
                
                table: "Mandates",
                column: "PredecessorMandateId");

            migrationBuilder.CreateIndex(
                name: "IX_ManualMappings_Category_SourceKey",
                
                table: "ManualMappings",
                columns: new[] { "Category", "SourceKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParliamentaryClubs_LegislativeTermId",
                
                table: "ParliamentaryClubs",
                column: "LegislativeTermId");

            migrationBuilder.CreateIndex(
                name: "IX_ParliamentaryClubs_NaturalKey",
                
                table: "ParliamentaryClubs",
                column: "NaturalKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Parties_NaturalKey",
                
                table: "Parties",
                column: "NaturalKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartyAffiliations_PartyId",
                
                table: "PartyAffiliations",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "IX_PartyAffiliations_PoliticianId_PartyId_ValidFrom",
                
                table: "PartyAffiliations",
                columns: new[] { "PoliticianId", "PartyId", "ValidFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_PoliticianAliases_NormalizedAlias",
                
                table: "PoliticianAliases",
                column: "NormalizedAlias");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticianAliases_PoliticianId",
                
                table: "PoliticianAliases",
                column: "PoliticianId");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticianMergeOverrides_SourcePoliticianId",
                
                table: "PoliticianMergeOverrides",
                column: "SourcePoliticianId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PoliticianMergeOverrides_TargetPoliticianId",
                
                table: "PoliticianMergeOverrides",
                column: "TargetPoliticianId");

            migrationBuilder.CreateIndex(
                name: "IX_Politicians_NormalizedName",
                
                table: "Politicians",
                column: "NormalizedName");

            migrationBuilder.CreateIndex(
                name: "IX_Politicians_PkwCandidateId",
                
                table: "Politicians",
                column: "PkwCandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_TerritorialUnits_TerytCode_ValidFrom",
                
                table: "TerritorialUnits",
                columns: new[] { "TerytCode", "ValidFrom" });

            migrationBuilder.AddForeignKey(
                name: "FK_Candidacies_Elections_ElectionId",
                
                table: "Candidacies",
                column: "ElectionId",
                principalTable: "Elections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Candidacies_ElectoralCommittees_ElectoralCommitteeId",
                
                table: "Candidacies",
                column: "ElectoralCommitteeId",
                principalTable: "ElectoralCommittees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Candidacies_ElectoralDistricts_ElectoralDistrictId",
                
                table: "Candidacies",
                column: "ElectoralDistrictId",
                principalTable: "ElectoralDistricts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Candidacies_ElectoralLists_ElectoralListId",
                
                table: "Candidacies",
                column: "ElectoralListId",
                principalTable: "ElectoralLists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CandidacyVoteResults_Elections_ElectionId",
                
                table: "CandidacyVoteResults",
                column: "ElectionId",
                principalTable: "Elections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CandidacyVoteResults_ElectoralDistricts_ElectoralDistrictId",
                
                table: "CandidacyVoteResults",
                column: "ElectoralDistrictId",
                principalTable: "ElectoralDistricts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ClubMemberships_ParliamentaryClubs_ParliamentaryClubId",
                
                table: "ClubMemberships",
                column: "ParliamentaryClubId",
                principalTable: "ParliamentaryClubs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DistrictTurnoutResults_Elections_ElectionId",
                
                table: "DistrictTurnoutResults",
                column: "ElectionId",
                principalTable: "Elections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DistrictTurnoutResults_ElectoralDistricts_ElectoralDistrictId",
                
                table: "DistrictTurnoutResults",
                column: "ElectoralDistrictId",
                principalTable: "ElectoralDistricts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ElectionMandateAllocations_Elections_ElectionId",
                
                table: "ElectionMandateAllocations",
                column: "ElectionId",
                principalTable: "Elections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ElectionMandateAllocations_ElectoralDistricts_ElectoralDistr~",
                
                table: "ElectionMandateAllocations",
                column: "ElectoralDistrictId",
                principalTable: "ElectoralDistricts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ElectionMandateAllocations_ElectoralLists_ElectoralListId",
                
                table: "ElectionMandateAllocations",
                column: "ElectoralListId",
                principalTable: "ElectoralLists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ElectionMandateAllocations_Mandates_MandateId",
                
                table: "ElectionMandateAllocations",
                column: "MandateId",
                principalTable: "Mandates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Elections_LegislativeTerms_LegislativeTermId",
                
                table: "Elections",
                column: "LegislativeTermId",
                principalTable: "LegislativeTerms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Elections_LegislativeTerms_ParentLegislativeTermId",
                
                table: "Elections",
                column: "ParentLegislativeTermId",
                principalTable: "LegislativeTerms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LegislativeTerms_Elections_FoundingElectionId",
                
                table: "LegislativeTerms");

            migrationBuilder.DropTable(
                name: "CandidacyVoteResults",
                schema: "app");

            migrationBuilder.DropTable(
                name: "ClubMemberships",
                schema: "app");

            migrationBuilder.DropTable(
                name: "DistrictTurnoutResults",
                schema: "app");

            migrationBuilder.DropTable(
                name: "ElectionMandateAllocations",
                schema: "app");

            migrationBuilder.DropTable(
                name: "ElectoralDistrictSnapshots",
                schema: "app");

            migrationBuilder.DropTable(
                name: "ElectoralDistrictTerritories",
                schema: "app");

            migrationBuilder.DropTable(
                name: "ElectoralListVoteResults",
                schema: "app");

            migrationBuilder.DropTable(
                name: "IdentityMatchCandidates",
                schema: "app");

            migrationBuilder.DropTable(
                name: "MandateEvents",
                schema: "app");

            migrationBuilder.DropTable(
                name: "ManualMappings",
                schema: "app");

            migrationBuilder.DropTable(
                name: "PartyAffiliations",
                schema: "app");

            migrationBuilder.DropTable(
                name: "PoliticianAliases",
                schema: "app");

            migrationBuilder.DropTable(
                name: "PoliticianMergeOverrides",
                schema: "app");

            migrationBuilder.DropTable(
                name: "ParliamentaryClubs",
                schema: "app");

            migrationBuilder.DropTable(
                name: "Mandates",
                schema: "app");

            migrationBuilder.DropTable(
                name: "Candidacies",
                schema: "app");

            migrationBuilder.DropTable(
                name: "ElectoralLists",
                schema: "app");

            migrationBuilder.DropTable(
                name: "Politicians",
                schema: "app");

            migrationBuilder.DropTable(
                name: "ElectoralCommittees",
                schema: "app");

            migrationBuilder.DropTable(
                name: "ElectoralDistricts",
                schema: "app");

            migrationBuilder.DropTable(
                name: "Parties",
                schema: "app");

            migrationBuilder.DropTable(
                name: "Elections",
                schema: "app");

            migrationBuilder.DropTable(
                name: "LegislativeTerms",
                schema: "app");

            migrationBuilder.DropTable(
                name: "TerritorialUnits",
                schema: "app");
        }
    }
}
