namespace PostalRegistry.Infrastructure.Modules
{
    using Autofac;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Infrastructure;
    using Microsoft.Extensions.Configuration;

    public class CommandHandlingModule : Module
    {
        private readonly IConfiguration _configuration;

        public CommandHandlingModule(IConfiguration configuration)
            => _configuration = configuration;

        protected override void Load(ContainerBuilder containerBuilder)
        {
            containerBuilder
                .RegisterModule<RepositoriesModule>();

            containerBuilder
                .RegisterType<ConcurrentUnitOfWork>()
                .InstancePerLifetimeScope();

            containerBuilder
                .RegisterEventstreamModule(_configuration);

            CommandHandlerModules.Register(containerBuilder);

            containerBuilder
                .RegisterType<CommandHandlerResolver>()
                .As<ICommandHandlerResolver>();
        }
    }
}
