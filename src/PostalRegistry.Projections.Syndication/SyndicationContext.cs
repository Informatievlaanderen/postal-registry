namespace PostalRegistry.Projections.Syndication
{
    using System;
    using System.IO;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.MigrationExtensions;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.SqlServer.MigrationExtensions;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.Extensions.Configuration;
    using Municipality;
    using Infrastructure;

    public class SyndicationContext : RunnerDbContext<SyndicationContext>
    {
        public override string ProjectionStateSchema => Schema.Syndication;

        public DbSet<MunicipalityLatestItem> MunicipalityLatestItems { get; set; }

        // This needs to be here to please EF
        public SyndicationContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public SyndicationContext(DbContextOptions<SyndicationContext> options)
            : base(options) { }

        public class ConfigBasedContextFactory : IDesignTimeDbContextFactory<SyndicationContext>
        {
            public SyndicationContext CreateDbContext(string[] args)
            {
                const string migrationConnectionStringName = "SyndicationProjectionsAdmin";

                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                    .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false)
                    .AddEnvironmentVariables()
                    .Build();

                var connectionString = configuration.GetConnectionString(migrationConnectionStringName);
                if (string.IsNullOrEmpty(connectionString))
                    throw new InvalidOperationException($"Could not find a connection string with name '{migrationConnectionStringName}'");

                var builder = new DbContextOptionsBuilder<SyndicationContext>()
                    .UseSqlServer(connectionString, sqlServerOptions =>
                    {
                        sqlServerOptions.EnableRetryOnFailure();
                        sqlServerOptions.MigrationsHistoryTable(MigrationTables.Syndication, Schema.Syndication);
                    })
                    .UseExtendedSqlServerMigrations();

                return new SyndicationContext(builder.Options);
            }
        }
    }
}
