using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PostalRegistry.Projections.Legacy.Migrations
{
    public partial class AddLinkedDataEventStream : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "PostalRegistryLdes");

            migrationBuilder.CreateTable(
                name: "PostalInformation",
                schema: "PostalRegistryLdes",
                columns: table => new
                {
                    Position = table.Column<long>(type: "bigint", nullable: false),
                    PostalCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ChangeType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: true),
                    PostalNames = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EventGeneratedAtTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ObjectIdentifier = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostalInformation", x => x.Position)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateIndex(
                name: "CI_PostalInformation_Position",
                schema: "PostalRegistryLdes",
                table: "PostalInformation",
                column: "Position");

            migrationBuilder.CreateIndex(
                name: "IX_PostalInformation_PostalCode",
                schema: "PostalRegistryLdes",
                table: "PostalInformation",
                column: "PostalCode");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PostalInformation",
                schema: "PostalRegistryLdes");
        }
    }
}
