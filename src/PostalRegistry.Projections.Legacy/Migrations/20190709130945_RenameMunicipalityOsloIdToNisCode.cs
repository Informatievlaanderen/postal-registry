using Microsoft.EntityFrameworkCore.Migrations;

namespace PostalRegistry.Projections.Legacy.Migrations
{
    public partial class RenameMunicipalityOsloIdToNisCode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MunicipalityOsloId",
                schema: "PostalRegistryLegacy",
                table: "PostalInformationSyndication",
                newName: "MunicipalityNisCode");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MunicipalityNisCode",
                schema: "PostalRegistryLegacy",
                table: "PostalInformationSyndication",
                newName: "MunicipalityOsloId");
        }
    }
}
