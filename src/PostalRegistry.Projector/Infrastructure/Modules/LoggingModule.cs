namespace PostalRegistry.Projector.Infrastructure.Modules
{
    using System;
    using Destructurama;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Serilog;
    using Serilog.Debugging;

    public static class LoggingModule
    {
        public static IServiceCollection RegisterLoggingModule(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            SelfLog.Enable(Console.WriteLine);

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .Enrich.WithEnvironmentUserName()
                .Destructure.JsonNetTypes()
                .CreateLogger();

            services.AddLogging(l =>
            {
                l.ClearProviders();
                l.AddSerilog(Log.Logger);
            });

            return services;
        }
    }
}
