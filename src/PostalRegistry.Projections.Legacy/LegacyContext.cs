namespace PostalRegistry.Projections.Legacy
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;

    public class LegacyContext : RunnerDbContext<LegacyContext>
    {
        public override string ProjectionStateSchema => Schema.Legacy;

        public DbSet<PostalInformation.PostalInformation> PostalInformation { get; set; }
        public DbSet<PostalInformationSyndication.PostalInformationSyndicationItem> PostalInformationSyndication { get; set; }

        // This needs to be here to please EF
        public LegacyContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public LegacyContext(DbContextOptions<LegacyContext> options)
            : base(options) { }
    }
}
