namespace PostalRegistry
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.CommandHandling.SqlStreamStore.Autofac;
    using Autofac;
    using PostalInformation;

    public static class CommandHandlerModules
    {
        public static void Register(ContainerBuilder containerBuilder)
        {
            containerBuilder
                .RegisterSqlStreamStoreCommandHandler<PostalInformationCommandHandlerModule>(
                    c => handler =>
                        new PostalInformationCommandHandlerModule(
                            c.Resolve<Func<IPostalInformationSet>>(),
                            c.Resolve<Func<ConcurrentUnitOfWork>>(),
                            handler));
        }
    }
}
