namespace PostalRegistry
{
    using Autofac;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using PostalInformation;

    public static class CommandHandlerModules
    {
        public static void Register(ContainerBuilder containerBuilder)
        {
            containerBuilder
                .RegisterType<BPostPostalInformationProvenanceFactory>()
                .SingleInstance();

            containerBuilder
                .RegisterType<CrabPostalInformationProvenanceFactory>()
                .SingleInstance();

            containerBuilder
                .RegisterType<PostalInformationProvenanceFactory>()
                .As<IProvenanceFactory<PostalInformation.PostalInformation>>()
                .AsSelf()
                .SingleInstance();

            containerBuilder
                .RegisterType<PostalInformationCommandHandlerModule>()
                .Named<CommandHandlerModule>(typeof(PostalInformationCommandHandlerModule).FullName)
                .As<CommandHandlerModule>();
        }
    }
}
