using Microsoft.EntityFrameworkCore.Migrations;

namespace PostalRegistry.Projections.Legacy.Migrations
{
    public partial class AdoptTableForLDES : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Application",
                schema: "PostalRegistryLegacy",
                table: "PostalInformationSyndication");

            migrationBuilder.DropColumn(
                name: "EventDataAsXml",
                schema: "PostalRegistryLegacy",
                table: "PostalInformationSyndication");

            migrationBuilder.DropColumn(
                name: "Modification",
                schema: "PostalRegistryLegacy",
                table: "PostalInformationSyndication");

            migrationBuilder.DropColumn(
                name: "MunicipalityNisCode",
                schema: "PostalRegistryLegacy",
                table: "PostalInformationSyndication");

            migrationBuilder.DropColumn(
                name: "Operator",
                schema: "PostalRegistryLegacy",
                table: "PostalInformationSyndication");

            migrationBuilder.DropColumn(
                name: "Organisation",
                schema: "PostalRegistryLegacy",
                table: "PostalInformationSyndication");

            migrationBuilder.RenameColumn(
                name: "Reason",
                schema: "PostalRegistryLegacy",
                table: "PostalInformationSyndication",
                newName: "EventDataAsJsonLd");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EventDataAsJsonLd",
                schema: "PostalRegistryLegacy",
                table: "PostalInformationSyndication",
                newName: "Reason");

            migrationBuilder.AddColumn<int>(
                name: "Application",
                schema: "PostalRegistryLegacy",
                table: "PostalInformationSyndication",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EventDataAsXml",
                schema: "PostalRegistryLegacy",
                table: "PostalInformationSyndication",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Modification",
                schema: "PostalRegistryLegacy",
                table: "PostalInformationSyndication",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MunicipalityNisCode",
                schema: "PostalRegistryLegacy",
                table: "PostalInformationSyndication",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Operator",
                schema: "PostalRegistryLegacy",
                table: "PostalInformationSyndication",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Organisation",
                schema: "PostalRegistryLegacy",
                table: "PostalInformationSyndication",
                type: "int",
                nullable: true);
        }
    }
}
