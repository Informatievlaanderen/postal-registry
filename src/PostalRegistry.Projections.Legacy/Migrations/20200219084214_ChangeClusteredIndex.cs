using Microsoft.EntityFrameworkCore.Migrations;

namespace PostalRegistry.Projections.Legacy.Migrations
{
    public partial class ChangeClusteredIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PostalInformation",
                schema: "PostalRegistryLegacy",
                table: "PostalInformation");

            migrationBuilder.DropIndex(
                name: "IX_PostalInformation_NisCode",
                schema: "PostalRegistryLegacy",
                table: "PostalInformation");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PostalInformation",
                schema: "PostalRegistryLegacy",
                table: "PostalInformation",
                column: "PostalCode")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_PostalInformation_NisCode",
                schema: "PostalRegistryLegacy",
                table: "PostalInformation",
                column: "NisCode");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PostalInformation",
                schema: "PostalRegistryLegacy",
                table: "PostalInformation");

            migrationBuilder.DropIndex(
                name: "IX_PostalInformation_NisCode",
                schema: "PostalRegistryLegacy",
                table: "PostalInformation");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PostalInformation",
                schema: "PostalRegistryLegacy",
                table: "PostalInformation",
                column: "PostalCode")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_PostalInformation_NisCode",
                schema: "PostalRegistryLegacy",
                table: "PostalInformation",
                column: "NisCode")
                .Annotation("SqlServer:Clustered", true);
        }
    }
}
