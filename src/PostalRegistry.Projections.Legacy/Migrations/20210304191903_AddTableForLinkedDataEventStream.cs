using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PostalRegistry.Projections.Legacy.Migrations
{
    public partial class AddTableForLinkedDataEventStream : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PostalInformationLinkedDataEventStream",
                schema: "PostalRegistryLegacy",
                columns: table => new
                {
                    Position = table.Column<long>(type: "bigint", nullable: false),
                    PostalCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ChangeType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: true),
                    PostalNames = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EventGeneratedAtTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostalInformationLinkedDataEventStream", x => x.Position)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateIndex(
                name: "CI_PostalInformationLinkedDataEventStream_Position",
                schema: "PostalRegistryLegacy",
                table: "PostalInformationLinkedDataEventStream",
                column: "Position");

            migrationBuilder.CreateIndex(
                name: "IX_PostalInformationLinkedDataEventStream_PostalCode",
                schema: "PostalRegistryLegacy",
                table: "PostalInformationLinkedDataEventStream",
                column: "PostalCode");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PostalInformationLinkedDataEventStream",
                schema: "PostalRegistryLegacy");
        }
    }
}
