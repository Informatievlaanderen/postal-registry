namespace PostalRegistry.Api.Oslo.Infrastructure.Modules
{
    using Autofac;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Nuts;
    using Projections.Syndication;

    public class ApiModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterModule(new SyndicationModule());

            builder
                .RegisterType<ProblemDetailsHelper>()
                .AsSelf();

            builder.RegisterType<Nuts3Service>()
                .SingleInstance();
        }
    }
}
