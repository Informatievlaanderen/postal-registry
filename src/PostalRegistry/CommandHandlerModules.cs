namespace PostalRegistry
{
    using Autofac;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
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
                .RegisterType<PostalInformationCommandHandlerModule>()
                .Named<CommandHandlerModule>(typeof(PostalInformationCommandHandlerModule).FullName)
                .As<CommandHandlerModule>();
        }
    }
}
