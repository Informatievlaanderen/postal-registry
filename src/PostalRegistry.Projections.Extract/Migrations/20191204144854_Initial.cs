using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PostalRegistry.Projections.Extract.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "PostalRegistryExtract");

            migrationBuilder.CreateTable(
                name: "Postal",
                schema: "PostalRegistryExtract",
                columns: table => new
                {
                    PostalCode = table.Column<string>(nullable: false),
                    DbaseRecord = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Postal", x => x.PostalCode)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "ProjectionStates",
                schema: "PostalRegistryExtract",
                columns: table => new
                {
                    Name = table.Column<string>(nullable: false),
                    Position = table.Column<long>(nullable: false),
                    DesiredState = table.Column<string>(nullable: true),
                    DesiredStateChangedAt = table.Column<DateTimeOffset>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectionStates", x => x.Name)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Postal_PostalCode",
                schema: "PostalRegistryExtract",
                table: "Postal",
                column: "PostalCode")
                .Annotation("SqlServer:Clustered", true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Postal",
                schema: "PostalRegistryExtract");

            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "PostalRegistryExtract");
        }
    }
}
