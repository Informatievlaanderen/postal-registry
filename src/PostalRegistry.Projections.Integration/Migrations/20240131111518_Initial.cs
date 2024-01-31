using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostalRegistry.Projections.Integration.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "integration_postal");

            migrationBuilder.CreateTable(
                name: "postal_latest_items",
                schema: "integration_postal",
                columns: table => new
                {
                    postal_code = table.Column<string>(type: "text", nullable: false),
                    nis_code = table.Column<string>(type: "text", nullable: true),
                    is_retired = table.Column<bool>(type: "boolean", nullable: false),
                    puri_id = table.Column<string>(type: "text", nullable: true),
                    @namespace = table.Column<string>(name: "namespace", type: "text", nullable: true),
                    version_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_postal_latest_items", x => x.postal_code);
                });

            migrationBuilder.CreateTable(
                name: "ProjectionStates",
                schema: "integration_postal",
                columns: table => new
                {
                    Name = table.Column<string>(type: "text", nullable: false),
                    Position = table.Column<long>(type: "bigint", nullable: false),
                    DesiredState = table.Column<string>(type: "text", nullable: true),
                    DesiredStateChangedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectionStates", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "postal_information_name",
                schema: "integration_postal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    search_name = table.Column<string>(type: "text", nullable: false),
                    language = table.Column<int>(type: "integer", nullable: false),
                    postal_code = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_postal_information_name", x => x.id);
                    table.ForeignKey(
                        name: "FK_postal_information_name_postal_latest_items_postal_code",
                        column: x => x.postal_code,
                        principalSchema: "integration_postal",
                        principalTable: "postal_latest_items",
                        principalColumn: "postal_code");
                });

            migrationBuilder.CreateIndex(
                name: "IX_postal_information_name_postal_code",
                schema: "integration_postal",
                table: "postal_information_name",
                column: "postal_code");

            migrationBuilder.CreateIndex(
                name: "IX_postal_information_name_search_name",
                schema: "integration_postal",
                table: "postal_information_name",
                column: "search_name");

            migrationBuilder.CreateIndex(
                name: "IX_postal_latest_items_nis_code",
                schema: "integration_postal",
                table: "postal_latest_items",
                column: "nis_code");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "postal_information_name",
                schema: "integration_postal");

            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "integration_postal");

            migrationBuilder.DropTable(
                name: "postal_latest_items",
                schema: "integration_postal");
        }
    }
}
