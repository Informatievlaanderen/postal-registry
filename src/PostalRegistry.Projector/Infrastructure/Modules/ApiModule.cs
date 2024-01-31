namespace PostalRegistry.Projector.Infrastructure.Modules
{
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Microsoft;
    using Be.Vlaanderen.Basisregisters.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.LastChangedList;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Autofac;
    using Be.Vlaanderen.Basisregisters.Projector;
    using Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections;
    using Be.Vlaanderen.Basisregisters.Projector.Modules;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using PostalRegistry.Infrastructure;
    using PostalRegistry.Projections.Extract;
    using PostalRegistry.Projections.Extract.PostalInformationExtract;
    using PostalRegistry.Projections.Integration;
    using PostalRegistry.Projections.Integration.Infrastructure;
    using PostalRegistry.Projections.LastChangedList;
    using PostalRegistry.Projections.Legacy;
    using PostalRegistry.Projections.Legacy.PostalInformation;
    using PostalRegistry.Projections.Legacy.PostalInformationSyndication;

    public class ApiModule : Module
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceCollection _services;
        private readonly ILoggerFactory _loggerFactory;

        private readonly ConnectedProjectionSettings _connectedProjectionSettings;

        public ApiModule(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _services = services;
            _loggerFactory = loggerFactory;

            var catchUpSize = configuration.GetValue<int>("CatchUpSize", 100);

            _connectedProjectionSettings = ConnectedProjectionSettings
                .Configure(x => x
                    .ConfigureCatchUpPageSize(catchUpSize)
                    .ConfigureCatchUpUpdatePositionMessageInterval(catchUpSize));
        }

        protected override void Load(ContainerBuilder builder)
        {
            _services.RegisterModule(new DataDogModule(_configuration));

            RegisterProjectionSetup(builder);

            builder
                .RegisterType<ProblemDetailsHelper>()
                .AsSelf();

            builder.Populate(_services);
        }

        private void RegisterProjectionSetup(ContainerBuilder builder)
        {
            builder.RegisterModule(
                    new EventHandlingModule(
                        typeof(DomainAssemblyMarker).Assembly,
                        EventsJsonSerializerSettingsProvider.CreateSerializerSettings()))
                .RegisterModule<EnvelopeModule>()
                .RegisterEventstreamModule(_configuration)
                .RegisterModule(new ProjectorModule(_configuration));

            RegisterExtractProjections(builder);
            RegisterLastChangedProjections(builder);
            RegisterLegacyProjections(builder);

            if (_configuration.GetSection("Integration").GetValue("Enabled", false))
                RegisterIntegrationProjections(builder);
        }

        private void RegisterIntegrationProjections(ContainerBuilder builder)
        {
            builder
                .RegisterModule(
                    new IntegrationModule(
                        _configuration,
                        _services,
                        _loggerFactory));

            builder
                .RegisterProjectionMigrator<IntegrationContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)
                .RegisterProjections<PostalLatestItemProjections, IntegrationContext>(
                    context => new PostalLatestItemProjections(context.Resolve<IOptions<IntegrationOptions>>()),
                    _connectedProjectionSettings);
        }

        private void RegisterExtractProjections(ContainerBuilder builder)
        {
            builder.RegisterModule(
                new ExtractModule(
                    _configuration,
                    _services,
                    _loggerFactory));

            builder
                .RegisterProjectionMigrator<ExtractContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)
                .RegisterProjections<PostalInformationExtractProjections, ExtractContext>(
                    context => new PostalInformationExtractProjections(context.Resolve<IOptions<ExtractConfig>>(),
                        DbaseCodePage.Western_European_ANSI.ToEncoding()),
                    _connectedProjectionSettings);
        }

        private void RegisterLastChangedProjections(ContainerBuilder builder)
        {
            builder.RegisterModule(
                new PostalLastChangedListModule(
                    _configuration.GetConnectionString("LastChangedList"),
                    _configuration["DataDog:ServiceName"],
                    _services,
                    _loggerFactory));

            builder
                .RegisterProjectionMigrator<
                    PostalRegistry.Projections.LastChangedList.LastChangedListContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)
                .RegisterProjectionMigrator<DataMigrationContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)
                .RegisterProjections<LastChangedListProjections, LastChangedListContext>(_connectedProjectionSettings);
        }

        private void RegisterLegacyProjections(ContainerBuilder builder)
        {
            builder
                .RegisterModule(
                    new LegacyModule(
                        _configuration,
                        _services,
                        _loggerFactory));
            builder
                .RegisterProjectionMigrator<LegacyContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)
                .RegisterProjections<PostalInformationProjections, LegacyContext>(_connectedProjectionSettings)
                .RegisterProjections<PostalInformationSyndicationProjections, LegacyContext>(_connectedProjectionSettings);
        }
    }
}
