namespace PostalRegistry.Projections.LastChangedList
{
    using MigrationFactory = Be.Vlaanderen.Basisregisters.ProjectionHandling.LastChangedList.LastChangedListContextMigrationFactory;

    public class LastChangedListContextMigrationFactory : MigrationFactory
    {
        public LastChangedListContextMigrationFactory()
            : base("LastChangedListAdmin")
        { }
    }
}
