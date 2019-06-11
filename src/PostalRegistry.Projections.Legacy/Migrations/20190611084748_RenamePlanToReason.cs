using Microsoft.EntityFrameworkCore.Migrations;

namespace PostalRegistry.Projections.Legacy.Migrations
{
    public partial class RenamePlanToReason : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Plan",
                schema: "PostalRegistryLegacy",
                table: "PostalInformationSyndication");

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                schema: "PostalRegistryLegacy",
                table: "PostalInformationSyndication",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Reason",
                schema: "PostalRegistryLegacy",
                table: "PostalInformationSyndication");

            migrationBuilder.AddColumn<int>(
                name: "Plan",
                schema: "PostalRegistryLegacy",
                table: "PostalInformationSyndication",
                nullable: true);
        }
    }
}
