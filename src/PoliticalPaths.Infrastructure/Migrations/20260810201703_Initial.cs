using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PoliticalPaths.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ImportBatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PipelineKey = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PrimarySourceType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ElectionYear = table.Column<int>(type: "int", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastSyncedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    TriggeredBy = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Notes = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SupersedesBatchId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportBatches", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "KomitetyWyborcze",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Nazwa = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Skrot = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KomitetyWyborcze", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ListaWyborcza",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    OkregId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    NumerListy = table.Column<int>(type: "int", nullable: false),
                    WyboryId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    KomitetWyborczyId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListaWyborcza", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OkregWyborczy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    NumerOkregu = table.Column<int>(type: "int", nullable: false),
                    RodzajWyborowId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OkregWyborczy", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Partia",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Nazwa = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Skrot = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataZalozenia = table.Column<DateOnly>(type: "date", nullable: true),
                    DataZakonczeniaDzialalnosci = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Partia", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Politycy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Imie = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DrugieImie = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Nazwisko = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataUrodzenia = table.Column<DateOnly>(type: "date", nullable: true),
                    MiejsceUrodzenia = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    InformacjeDodatkowe = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Politycy", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RodzajeWyborow",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Nazwa = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Poziom = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RodzajeWyborow", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WynikiWyborow",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    LiczbaGlosow = table.Column<int>(type: "int", nullable: false),
                    CzyMandat = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WynikiWyborow", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ImportFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ImportBatchId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    LogicalNames = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StoragePath = table.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Sha256 = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    ContentType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataSourceType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FormatVersion = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TotalRows = table.Column<int>(type: "int", nullable: false),
                    TransformedRows = table.Column<int>(type: "int", nullable: false),
                    FailedRows = table.Column<int>(type: "int", nullable: false),
                    WarningCount = table.Column<int>(type: "int", nullable: false),
                    LastProcessedRowId = table.Column<long>(type: "bigint", nullable: true),
                    LogFilePath = table.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RawImportStartedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    RawImportCompletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImportFiles_ImportBatches_ImportBatchId",
                        column: x => x.ImportBatchId,
                        principalTable: "ImportBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Wybory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    RodzajWyborowId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DataOgloszenia = table.Column<DateOnly>(type: "date", nullable: true),
                    DataWyborow = table.Column<DateOnly>(type: "date", nullable: false),
                    Ordynacja = table.Column<int>(type: "int", nullable: false),
                    Kadencja = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Tura = table.Column<int>(type: "int", nullable: true),
                    CzyPrzedterminowe = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wybory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Wybory_RodzajeWyborow_RodzajWyborowId",
                        column: x => x.RodzajWyborowId,
                        principalTable: "RodzajeWyborow",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ImportRows",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ImportFileId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    SheetName = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SheetIndex = table.Column<int>(type: "int", nullable: false),
                    RowNumber = table.Column<int>(type: "int", nullable: false),
                    RowHash = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RawPayloadJson = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ImportedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TransformedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DomainEntityType = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DomainEntityId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportRows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImportRows_ImportFiles_ImportFileId",
                        column: x => x.ImportFileId,
                        principalTable: "ImportFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PartieCzlonkostwa",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PolitykId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PartiaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    WyboryId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartieCzlonkostwa", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartieCzlonkostwa_Partia_PartiaId",
                        column: x => x.PartiaId,
                        principalTable: "Partia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PartieCzlonkostwa_Politycy_PolitykId",
                        column: x => x.PolitykId,
                        principalTable: "Politycy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PartieCzlonkostwa_Wybory_WyboryId",
                        column: x => x.WyboryId,
                        principalTable: "Wybory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "StartyWyborcze",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PolitykId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    NumerNaLiscie = table.Column<int>(type: "int", nullable: true),
                    ListaId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Zawod = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Wyksztalcenie = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MiejsceZamieszkania = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PartiaId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    KomitetId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    WynikiId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PopierajacaPartiaId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    WyboryId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StartyWyborcze", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StartyWyborcze_ListaWyborcza_ListaId",
                        column: x => x.ListaId,
                        principalTable: "ListaWyborcza",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StartyWyborcze_Politycy_PolitykId",
                        column: x => x.PolitykId,
                        principalTable: "Politycy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StartyWyborcze_Wybory_WyboryId",
                        column: x => x.WyboryId,
                        principalTable: "Wybory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StartyWyborcze_WynikiWyborow_WynikiId",
                        column: x => x.WynikiId,
                        principalTable: "WynikiWyborow",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SzczegolyOkregow",
                columns: table => new
                {
                    OkregId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    WyboryId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    RokWyborow = table.Column<int>(type: "int", nullable: false),
                    Mieszkancy = table.Column<int>(type: "int", nullable: false),
                    Uprawnieni = table.Column<int>(type: "int", nullable: false),
                    LiczbaMandatow = table.Column<int>(type: "int", nullable: false),
                    LiczbaList = table.Column<int>(type: "int", nullable: false),
                    LiczbaKandydatow = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SzczegolyOkregow", x => new { x.OkregId, x.WyboryId });
                    table.ForeignKey(
                        name: "FK_SzczegolyOkregow_OkregWyborczy_OkregId",
                        column: x => x.OkregId,
                        principalTable: "OkregWyborczy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SzczegolyOkregow_Wybory_WyboryId",
                        column: x => x.WyboryId,
                        principalTable: "Wybory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TransformationErrors",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ImportRowId = table.Column<long>(type: "bigint", nullable: false),
                    StepName = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Severity = table.Column<int>(type: "int", nullable: false),
                    ErrorCode = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Message = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FieldName = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RawValue = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DetailsJson = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransformationErrors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransformationErrors_ImportRows_ImportRowId",
                        column: x => x.ImportRowId,
                        principalTable: "ImportRows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Mandaty",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PolitykId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    StartWyborczyId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DataOd = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TypObjecia = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mandaty", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Mandaty_Politycy_PolitykId",
                        column: x => x.PolitykId,
                        principalTable: "Politycy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Mandaty_StartyWyborcze_StartWyborczyId",
                        column: x => x.StartWyborczyId,
                        principalTable: "StartyWyborcze",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ZdarzeniaMandatowe",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MandatId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PolitykId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PolitykId1 = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Typ = table.Column<int>(type: "int", nullable: false),
                    DataZdarzenia = table.Column<DateOnly>(type: "date", nullable: false),
                    Opis = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DokumentReferencyjny = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZdarzeniaMandatowe", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ZdarzeniaMandatowe_Mandaty_MandatId",
                        column: x => x.MandatId,
                        principalTable: "Mandaty",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ZdarzeniaMandatowe_Politycy_PolitykId",
                        column: x => x.PolitykId,
                        principalTable: "Politycy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ZdarzeniaMandatowe_Politycy_PolitykId1",
                        column: x => x.PolitykId1,
                        principalTable: "Politycy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ImportBatches_PipelineKey",
                table: "ImportBatches",
                column: "PipelineKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImportBatches_StartedAt",
                table: "ImportBatches",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ImportBatches_Status",
                table: "ImportBatches",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ImportFiles_ImportBatchId_Sha256",
                table: "ImportFiles",
                columns: new[] { "ImportBatchId", "Sha256" });

            migrationBuilder.CreateIndex(
                name: "IX_ImportRows_ImportFileId_SheetName_RowNumber",
                table: "ImportRows",
                columns: new[] { "ImportFileId", "SheetName", "RowNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImportRows_ImportFileId_Status",
                table: "ImportRows",
                columns: new[] { "ImportFileId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Mandaty_PolitykId",
                table: "Mandaty",
                column: "PolitykId");

            migrationBuilder.CreateIndex(
                name: "IX_Mandaty_StartWyborczyId",
                table: "Mandaty",
                column: "StartWyborczyId");

            migrationBuilder.CreateIndex(
                name: "IX_PartieCzlonkostwa_PartiaId",
                table: "PartieCzlonkostwa",
                column: "PartiaId");

            migrationBuilder.CreateIndex(
                name: "IX_PartieCzlonkostwa_PolitykId",
                table: "PartieCzlonkostwa",
                column: "PolitykId");

            migrationBuilder.CreateIndex(
                name: "IX_PartieCzlonkostwa_WyboryId",
                table: "PartieCzlonkostwa",
                column: "WyboryId");

            migrationBuilder.CreateIndex(
                name: "IX_StartyWyborcze_ListaId",
                table: "StartyWyborcze",
                column: "ListaId");

            migrationBuilder.CreateIndex(
                name: "IX_StartyWyborcze_PolitykId",
                table: "StartyWyborcze",
                column: "PolitykId");

            migrationBuilder.CreateIndex(
                name: "IX_StartyWyborcze_WyboryId",
                table: "StartyWyborcze",
                column: "WyboryId");

            migrationBuilder.CreateIndex(
                name: "IX_StartyWyborcze_WynikiId",
                table: "StartyWyborcze",
                column: "WynikiId");

            migrationBuilder.CreateIndex(
                name: "IX_SzczegolyOkregow_WyboryId",
                table: "SzczegolyOkregow",
                column: "WyboryId");

            migrationBuilder.CreateIndex(
                name: "IX_TransformationErrors_ImportRowId",
                table: "TransformationErrors",
                column: "ImportRowId");

            migrationBuilder.CreateIndex(
                name: "IX_Wybory_RodzajWyborowId",
                table: "Wybory",
                column: "RodzajWyborowId");

            migrationBuilder.CreateIndex(
                name: "IX_ZdarzeniaMandatowe_MandatId",
                table: "ZdarzeniaMandatowe",
                column: "MandatId");

            migrationBuilder.CreateIndex(
                name: "IX_ZdarzeniaMandatowe_PolitykId",
                table: "ZdarzeniaMandatowe",
                column: "PolitykId");

            migrationBuilder.CreateIndex(
                name: "IX_ZdarzeniaMandatowe_PolitykId1",
                table: "ZdarzeniaMandatowe",
                column: "PolitykId1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KomitetyWyborcze");

            migrationBuilder.DropTable(
                name: "PartieCzlonkostwa");

            migrationBuilder.DropTable(
                name: "SzczegolyOkregow");

            migrationBuilder.DropTable(
                name: "TransformationErrors");

            migrationBuilder.DropTable(
                name: "ZdarzeniaMandatowe");

            migrationBuilder.DropTable(
                name: "Partia");

            migrationBuilder.DropTable(
                name: "OkregWyborczy");

            migrationBuilder.DropTable(
                name: "ImportRows");

            migrationBuilder.DropTable(
                name: "Mandaty");

            migrationBuilder.DropTable(
                name: "ImportFiles");

            migrationBuilder.DropTable(
                name: "StartyWyborcze");

            migrationBuilder.DropTable(
                name: "ImportBatches");

            migrationBuilder.DropTable(
                name: "ListaWyborcza");

            migrationBuilder.DropTable(
                name: "Politycy");

            migrationBuilder.DropTable(
                name: "Wybory");

            migrationBuilder.DropTable(
                name: "WynikiWyborow");

            migrationBuilder.DropTable(
                name: "RodzajeWyborow");
        }
    }
}
