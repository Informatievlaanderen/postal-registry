namespace PostalRegistry.Projections.Extract
{
    using System;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.SqlServer.MigrationExtensions;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public static class ExtractModule
    {
        public static IServiceCollection RegisterExtractModule(
            this IServiceCollection services,
            IConfiguration configuration,
            ILoggerFactory loggerFactory,
            bool enableRetry = true)
        {
            var logger = loggerFactory.CreateLogger<ExtractContext>();
            var connectionString = configuration.GetConnectionString("ExtractProjections");

            var hasConnectionString = !string.IsNullOrWhiteSpace(connectionString);
            if (hasConnectionString)
                RunOnSqlServer(services, loggerFactory, connectionString, enableRetry);
            else
                RunInMemoryDb(services, loggerFactory, logger);

            logger.LogInformation(
                "Added {Context} to services:" +
                Environment.NewLine +
                "\tSchema: {Schema}" +
                Environment.NewLine +
                "\tTableName: {TableName}",
                nameof(ExtractContext), Schema.Extract, MigrationTables.Extract);

            return services;
        }

        private static void RunOnSqlServer(
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            string backofficeProjectionsConnectionString,
            bool enableRetry)
        {
            services
                .AddDbContext<ExtractContext>((provider, options) => options
                    .UseLoggerFactory(loggerFactory)
                    .UseSqlServer(backofficeProjectionsConnectionString, sqlServerOptions =>
                    {
                        if (enableRetry)
                            sqlServerOptions.EnableRetryOnFailure();

                        sqlServerOptions.MigrationsHistoryTable(MigrationTables.Extract, Schema.Extract);
                    })
                    .UseExtendedSqlServerMigrations());
        }

        private static void RunInMemoryDb(
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            ILogger logger)
        {
            services
                .AddDbContext<ExtractContext>(options => options
                    .UseLoggerFactory(loggerFactory)
                    .UseInMemoryDatabase(Guid.NewGuid().ToString(), sqlServerOptions => { }));

            logger.LogWarning("Running InMemory for {Context}!", nameof(ExtractContext));
        }
    }
}
