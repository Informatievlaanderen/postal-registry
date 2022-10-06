using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostalRegistry.Projections.Syndication.Migrations
{
    using Infrastructure;
    using Municipality;

    public partial class AddPrimaryLanguage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"TRUNCATE TABLE {Schema.Syndication}.{MunicipalityItemConfiguration.TableName}");
            migrationBuilder.Sql($"UPDATE {Schema.Syndication}.ProjectionStates SET [Position] = -1 WHERE [Name] = '{typeof(MunicipalityLatestProjections).FullName}'");

            migrationBuilder.AddColumn<int>(
                name: "PrimaryLanguage",
                schema: "PostalRegistrySyndication",
                table: "MunicipalityLatestSyndication",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrimaryLanguage",
                schema: "PostalRegistrySyndication",
                table: "MunicipalityLatestSyndication");
        }
    }
}
