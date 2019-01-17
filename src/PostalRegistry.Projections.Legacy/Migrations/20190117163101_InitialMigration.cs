using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PostalRegistry.Projections.Legacy.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "PostalRegistryLegacy");

            migrationBuilder.CreateTable(
                name: "PostalInformation",
                schema: "PostalRegistryLegacy",
                columns: table => new
                {
                    PostalCode = table.Column<string>(nullable: false),
                    NisCode = table.Column<string>(nullable: true),
                    IsRetired = table.Column<bool>(nullable: false),
                    VersionTimestamp = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostalInformation", x => x.PostalCode)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "PostalInformationSyndication",
                schema: "PostalRegistryLegacy",
                columns: table => new
                {
                    Position = table.Column<long>(nullable: false),
                    PostalCode = table.Column<string>(nullable: false),
                    ChangeType = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: true),
                    MunicipalityOsloId = table.Column<string>(nullable: true),
                    PostalNames = table.Column<string>(nullable: true),
                    RecordCreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    LastChangedOn = table.Column<DateTimeOffset>(nullable: false),
                    Application = table.Column<int>(nullable: true),
                    Modification = table.Column<int>(nullable: true),
                    Operator = table.Column<string>(nullable: true),
                    Organisation = table.Column<int>(nullable: true),
                    Plan = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostalInformationSyndication", x => x.Position)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "ProjectionStates",
                schema: "PostalRegistryLegacy",
                columns: table => new
                {
                    Name = table.Column<string>(nullable: false),
                    Position = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectionStates", x => x.Name)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "PostalInformationName",
                schema: "PostalRegistryLegacy",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Language = table.Column<int>(nullable: false),
                    PostalCode = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostalInformationName", x => x.Id)
                        .Annotation("SqlServer:Clustered", true);
                    table.ForeignKey(
                        name: "FK_PostalInformationName_PostalInformation_PostalCode",
                        column: x => x.PostalCode,
                        principalSchema: "PostalRegistryLegacy",
                        principalTable: "PostalInformation",
                        principalColumn: "PostalCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PostalInformation_NisCode",
                schema: "PostalRegistryLegacy",
                table: "PostalInformation",
                column: "NisCode")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_PostalInformationName_PostalCode",
                schema: "PostalRegistryLegacy",
                table: "PostalInformationName",
                column: "PostalCode");

            migrationBuilder.CreateIndex(
                name: "IX_PostalInformationSyndication_PostalCode",
                schema: "PostalRegistryLegacy",
                table: "PostalInformationSyndication",
                column: "PostalCode");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PostalInformationName",
                schema: "PostalRegistryLegacy");

            migrationBuilder.DropTable(
                name: "PostalInformationSyndication",
                schema: "PostalRegistryLegacy");

            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "PostalRegistryLegacy");

            migrationBuilder.DropTable(
                name: "PostalInformation",
                schema: "PostalRegistryLegacy");
        }
    }
}
