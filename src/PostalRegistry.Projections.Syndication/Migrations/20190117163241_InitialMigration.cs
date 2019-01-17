using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PostalRegistry.Projections.Syndication.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "PostalRegistrySyndication");

            migrationBuilder.CreateTable(
                name: "MunicipalityLatestSyndication",
                schema: "PostalRegistrySyndication",
                columns: table => new
                {
                    MunicipalityId = table.Column<Guid>(nullable: false),
                    NisCode = table.Column<string>(nullable: true),
                    NameDutch = table.Column<string>(nullable: true),
                    NameDutchSearch = table.Column<string>(nullable: true),
                    NameFrench = table.Column<string>(nullable: true),
                    NameFrenchSearch = table.Column<string>(nullable: true),
                    NameGerman = table.Column<string>(nullable: true),
                    NameGermanSearch = table.Column<string>(nullable: true),
                    NameEnglish = table.Column<string>(nullable: true),
                    NameEnglishSearch = table.Column<string>(nullable: true),
                    Version = table.Column<DateTimeOffset>(nullable: true),
                    Position = table.Column<long>(nullable: false),
                    LastUpdatedOn = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MunicipalityLatestSyndication", x => x.MunicipalityId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "ProjectionStates",
                schema: "PostalRegistrySyndication",
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

            migrationBuilder.CreateIndex(
                name: "IX_MunicipalityLatestSyndication_LastUpdatedOn",
                schema: "PostalRegistrySyndication",
                table: "MunicipalityLatestSyndication",
                column: "LastUpdatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_MunicipalityLatestSyndication_NameDutchSearch",
                schema: "PostalRegistrySyndication",
                table: "MunicipalityLatestSyndication",
                column: "NameDutchSearch");

            migrationBuilder.CreateIndex(
                name: "IX_MunicipalityLatestSyndication_NameEnglishSearch",
                schema: "PostalRegistrySyndication",
                table: "MunicipalityLatestSyndication",
                column: "NameEnglishSearch");

            migrationBuilder.CreateIndex(
                name: "IX_MunicipalityLatestSyndication_NameFrenchSearch",
                schema: "PostalRegistrySyndication",
                table: "MunicipalityLatestSyndication",
                column: "NameFrenchSearch");

            migrationBuilder.CreateIndex(
                name: "IX_MunicipalityLatestSyndication_NameGermanSearch",
                schema: "PostalRegistrySyndication",
                table: "MunicipalityLatestSyndication",
                column: "NameGermanSearch");

            migrationBuilder.CreateIndex(
                name: "IX_MunicipalityLatestSyndication_NisCode",
                schema: "PostalRegistrySyndication",
                table: "MunicipalityLatestSyndication",
                column: "NisCode")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_MunicipalityLatestSyndication_Position",
                schema: "PostalRegistrySyndication",
                table: "MunicipalityLatestSyndication",
                column: "Position");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MunicipalityLatestSyndication",
                schema: "PostalRegistrySyndication");

            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "PostalRegistrySyndication");
        }
    }
}
