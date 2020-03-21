using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PostalRegistry.Projections.Syndication.Migrations
{
    public partial class ChangeVersieType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Version",
                schema: "PostalRegistrySyndication",
                table: "MunicipalityLatestSyndication",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Version",
                schema: "PostalRegistrySyndication",
                table: "MunicipalityLatestSyndication",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
