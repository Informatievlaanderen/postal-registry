using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostalRegistry.Producer.Ldes.Migrations
{
    /// <inheritdoc />
    public partial class IsRemoved : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRemoved",
                schema: "PostalRegistryProducerLdes",
                table: "PostalInformation",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRemoved",
                schema: "PostalRegistryProducerLdes",
                table: "PostalInformation");
        }
    }
}
