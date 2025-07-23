using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostalRegistry.Projections.Legacy.Migrations
{
    /// <inheritdoc />
    public partial class AddIsRemoved : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRemoved",
                schema: "PostalRegistryLegacy",
                table: "PostalInformationSyndication",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRemoved",
                schema: "PostalRegistryLegacy",
                table: "PostalInformation",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_PostalInformationSyndication_IsRemoved",
                schema: "PostalRegistryLegacy",
                table: "PostalInformationSyndication",
                column: "IsRemoved");

            migrationBuilder.CreateIndex(
                name: "IX_PostalInformation_IsRemoved",
                schema: "PostalRegistryLegacy",
                table: "PostalInformation",
                column: "IsRemoved");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PostalInformationSyndication_IsRemoved",
                schema: "PostalRegistryLegacy",
                table: "PostalInformationSyndication");

            migrationBuilder.DropIndex(
                name: "IX_PostalInformation_IsRemoved",
                schema: "PostalRegistryLegacy",
                table: "PostalInformation");

            migrationBuilder.DropColumn(
                name: "IsRemoved",
                schema: "PostalRegistryLegacy",
                table: "PostalInformationSyndication");

            migrationBuilder.DropColumn(
                name: "IsRemoved",
                schema: "PostalRegistryLegacy",
                table: "PostalInformation");
        }
    }
}
