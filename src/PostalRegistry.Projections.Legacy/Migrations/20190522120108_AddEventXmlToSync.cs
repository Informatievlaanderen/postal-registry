using Microsoft.EntityFrameworkCore.Migrations;

namespace PostalRegistry.Projections.Legacy.Migrations
{
    public partial class AddEventXmlToSync : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EventDataAsXml",
                schema: "PostalRegistryLegacy",
                table: "PostalInformationSyndication",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EventDataAsXml",
                schema: "PostalRegistryLegacy",
                table: "PostalInformationSyndication");
        }
    }
}
