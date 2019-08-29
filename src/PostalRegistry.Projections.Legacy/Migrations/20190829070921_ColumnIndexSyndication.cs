using Microsoft.EntityFrameworkCore.Migrations;

namespace PostalRegistry.Projections.Legacy.Migrations
{
    public partial class ColumnIndexSyndication : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "CI_PostalInformationSyndication_Position",
                schema: "PostalRegistryLegacy",
                table: "PostalInformationSyndication",
                column: "Position")
                .Annotation("SqlServer:ColumnStoreIndex", "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "CI_PostalInformationSyndication_Position",
                schema: "PostalRegistryLegacy",
                table: "PostalInformationSyndication");
        }
    }
}
