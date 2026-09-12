using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using PoliticalPaths.Infrastructure.Persistence;

#nullable disable

namespace PoliticalPaths.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260912110000_MakeDistrictResidentsNullable")]
public partial class MakeDistrictResidentsNullable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<int>(
            name: "Mieszkancy",
            table: "SzczegolyOkregow",
            type: "int",
            nullable: true,
            oldClrType: typeof(int),
            oldType: "int");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("UPDATE SzczegolyOkregow SET Mieszkancy = 0 WHERE Mieszkancy IS NULL");
        migrationBuilder.AlterColumn<int>(
            name: "Mieszkancy",
            table: "SzczegolyOkregow",
            type: "int",
            nullable: false,
            defaultValue: 0,
            oldClrType: typeof(int),
            oldType: "int",
            oldNullable: true);
    }
}
