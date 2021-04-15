namespace PostalRegistry.Infrastructure
{
    public class Schema
    {
        public const string Default = "PostalRegistry";

        public const string Import = "PostalRegistryImport";
        public const string Legacy = "PostalRegistryLegacy";
        public const string LinkedDataEventStream = "PostalRegistryLdes";
        public const string Extract = "PostalRegistryExtract";
        public const string Syndication = "PostalRegistrySyndication";
    }

    public class MigrationTables
    {
        public const string Legacy = "__EFMigrationsHistoryLegacy";
        public const string Extract = "__EFMigrationsHistoryExtract";
        public const string Syndication = "__EFMigrationsHistorySyndication";
    }
}
