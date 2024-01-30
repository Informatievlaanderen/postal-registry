namespace PostalRegistry.Producer.Snapshot.Oslo
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
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
                Schema = Schema.ProducerSnapshotOslo,
                Table = MigrationTables.ProducerSnapshotOslo
            };

        protected override ProducerContext CreateContext(DbContextOptions<ProducerContext> migrationContextOptions)
            => new ProducerContext(migrationContextOptions);
    }
}
