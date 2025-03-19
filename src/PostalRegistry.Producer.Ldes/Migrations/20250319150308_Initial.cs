using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostalRegistry.Producer.Ldes.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "PostalRegistryProducerLdes");

            migrationBuilder.CreateTable(
                name: "PostalInformation",
                schema: "PostalRegistryProducerLdes",
                columns: table => new
                {
                    PostalCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NisCode = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsRetired = table.Column<bool>(type: "bit", nullable: false),
                    VersionTimestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    NamesDutch = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NamesEnglish = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NamesFrench = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NamesGerman = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostalInformation", x => x.PostalCode)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "ProjectionStates",
                schema: "PostalRegistryProducerLdes",
                columns: table => new
                {
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Position = table.Column<long>(type: "bigint", nullable: false),
                    DesiredState = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DesiredStateChangedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectionStates", x => x.Name);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PostalInformation_NisCode",
                schema: "PostalRegistryProducerLdes",
                table: "PostalInformation",
                column: "NisCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PostalInformation",
                schema: "PostalRegistryProducerLdes");

            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "PostalRegistryProducerLdes");
        }
    }
}
