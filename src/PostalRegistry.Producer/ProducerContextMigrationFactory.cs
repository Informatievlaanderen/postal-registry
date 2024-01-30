namespace PostalRegistry.Producer
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.SqlServer;
    using Microsoft.EntityFrameworkCore;
    using PostalRegistry.Infrastructure;

    public class ProducerContextMigrationFactory : SqlServerRunnerDbContextMigrationFactory<ProducerContext>
    {
        public ProducerContextMigrationFactory()
            : base("ProducerProjectionsAdmin", HistoryConfiguration) { }

        private static MigrationHistoryConfiguration HistoryConfiguration =>
            new MigrationHistoryConfiguration
            {
                Schema = Schema.Producer,
                Table = MigrationTables.Producer
            };

        protected override ProducerContext CreateContext(DbContextOptions<ProducerContext> migrationContextOptions)
            => new ProducerContext(migrationContextOptions);
    }
}
