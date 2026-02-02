using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostalRegistry.Projections.Feed.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "PostalRegistryFeed");

            migrationBuilder.CreateSequence(
                name: "PostalFeedSequence",
                schema: "PostalRegistryFeed");

            migrationBuilder.CreateTable(
                name: "PostalDocuments",
                schema: "PostalRegistryFeed",
                columns: table => new
                {
                    PostalCode = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    LastChangedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RecordCreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Document = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostalDocuments", x => x.PostalCode)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "PostalFeed",
                schema: "PostalRegistryFeed",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Page = table.Column<int>(type: "int", nullable: false),
                    Position = table.Column<long>(type: "bigint", nullable: false),
                    PostalCode = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    NisCode = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    Application = table.Column<int>(type: "int", nullable: true),
                    Modification = table.Column<int>(type: "int", nullable: true),
                    Operator = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Organisation = table.Column<int>(type: "int", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CloudEvent = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostalFeed", x => x.Id)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "ProjectionStates",
                schema: "PostalRegistryFeed",
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
                name: "IX_PostalDocuments_PostalCode",
                schema: "PostalRegistryFeed",
                table: "PostalDocuments",
                column: "PostalCode");

            migrationBuilder.CreateIndex(
                name: "IX_PostalFeed_Page",
                schema: "PostalRegistryFeed",
                table: "PostalFeed",
                column: "Page");

            migrationBuilder.CreateIndex(
                name: "IX_PostalFeed_Position",
                schema: "PostalRegistryFeed",
                table: "PostalFeed",
                column: "Position");

            migrationBuilder.CreateIndex(
                name: "IX_PostalFeed_PostalCode",
                schema: "PostalRegistryFeed",
                table: "PostalFeed",
                column: "PostalCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PostalDocuments",
                schema: "PostalRegistryFeed");

            migrationBuilder.DropTable(
                name: "PostalFeed",
                schema: "PostalRegistryFeed");

            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "PostalRegistryFeed");

            migrationBuilder.DropSequence(
                name: "PostalFeedSequence",
                schema: "PostalRegistryFeed");
        }
    }
}
