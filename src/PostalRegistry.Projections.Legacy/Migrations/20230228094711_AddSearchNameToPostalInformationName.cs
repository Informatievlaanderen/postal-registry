using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostalRegistry.Projections.Legacy.Migrations
{
    using System;
    using System.IO;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Dapper;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Configuration;
    
    public partial class AddSearchNameToPostalInformationName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SearchName",
                schema: "PostalRegistryLegacy",
                table: "PostalInformationName",
                type: "nvarchar(450)",
                nullable: true);

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString("LegacyProjectionsAdmin");
            using var connection = new SqlConnection(connectionString);
            connection.Open();

            var results = connection.Query<(Guid id, string name)>("SELECT Id, Name from PostalRegistryLegacy.PostalInformationName");

            foreach (var (id, name) in results)
            {
                migrationBuilder.UpdateData(
                    schema: "PostalRegistryLegacy",
                    table: "PostalInformationName",
                    keyColumn: "Id",
                    keyValue: id,
                    column: "SearchName",
                    value: name.RemoveDiacritics());
            }

            migrationBuilder.AlterColumn<string>(
                name: "SearchName",
                schema: "PostalRegistryLegacy",
                table: "PostalInformationName",
                type: "nvarchar(450)",
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_PostalInformationName_SearchName",
                schema: "PostalRegistryLegacy",
                table: "PostalInformationName",
                column: "SearchName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PostalInformationName_SearchName",
                schema: "PostalRegistryLegacy",
                table: "PostalInformationName");

            migrationBuilder.DropColumn(
                name: "SearchName",
                schema: "PostalRegistryLegacy",
                table: "PostalInformationName");
        }
    }
}
