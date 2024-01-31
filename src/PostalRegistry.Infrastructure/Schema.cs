namespace PostalRegistry.Infrastructure
{
    public static class Schema
    {
        public const string Default = "PostalRegistry";

        public const string Import = "PostalRegistryImport";
        public const string Legacy = "PostalRegistryLegacy";
        public const string Extract = "PostalRegistryExtract";
        public const string Syndication = "PostalRegistrySyndication";

        public const string Producer = "PostalRegistryProducer";
        public const string ProducerSnapshotOslo = "PostalRegistryProducerSnapshotOslo";

        public const string Integration = "integration_postal";
    }

    public static class MigrationTables
    {
        public const string Legacy = "__EFMigrationsHistoryLegacy";
        public const string Extract = "__EFMigrationsHistoryExtract";
        public const string RedisDataMigration = "__EFMigrationsHistoryDataMigration";
        public const string Syndication = "__EFMigrationsHistorySyndication";

        public const string Producer = "__EFMigrationsHistoryProducer";
        public const string ProducerSnapshotOslo = "__EFMigrationsHistoryProducerSnapshotOslo";

        public const string Integration = "__EFMigrationsHistory";
    }
}
