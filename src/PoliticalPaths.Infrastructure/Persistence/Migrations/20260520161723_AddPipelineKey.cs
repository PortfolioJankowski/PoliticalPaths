using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PoliticalPaths.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPipelineKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastSyncedAt",
                table: "ImportBatches",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PipelineKey",
                table: "ImportBatches",
                type: "varchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ImportBatches_PipelineKey",
                table: "ImportBatches",
                column: "PipelineKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ImportBatches_PipelineKey",
                table: "ImportBatches");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "ImportBatches");

            migrationBuilder.DropColumn(
                name: "PipelineKey",
                table: "ImportBatches");
        }
    }
}
