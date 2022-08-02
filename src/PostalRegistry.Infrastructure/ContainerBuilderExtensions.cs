namespace PostalRegistry.Infrastructure
{
    using System;
    using Autofac;
    using Autofac.Core.Registration;
    using Be.Vlaanderen.Basisregisters.AggregateSource.SqlStreamStore.Autofac;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.SqlStreamStore;
    using Microsoft.Extensions.Configuration;

    public static class ContainerBuilderExtensions
    {
        public static ContainerBuilder RegisterEventstreamModule(
            this ContainerBuilder builder,
            IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Events");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Missing 'Events' connectionstring.");
            }

            builder
                .RegisterModule(new SqlStreamStoreModule(connectionString, Schema.Default))
                .RegisterModule(new TraceSqlStreamStoreModule(configuration["DataDog:ServiceName"]));

            return builder;
        }

        public static IModuleRegistrar RegisterEventstreamModule(
            this IModuleRegistrar builder,
            IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Events");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Missing 'Events' connectionstring.");
            }

            return builder
                .RegisterModule(new SqlStreamStoreModule(connectionString, Schema.Default))
                .RegisterModule(new TraceSqlStreamStoreModule(configuration["DataDog:ServiceName"]));
        }
    }
}
