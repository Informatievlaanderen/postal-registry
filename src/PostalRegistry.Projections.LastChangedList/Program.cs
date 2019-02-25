namespace PostalRegistry.Projections.LastChangedList
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.LastChangedList;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Autofac.Features.OwnedInstances;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Modules;
    using Serilog;
    using SqlStreamStore;

    public class Program
    {
        private static readonly AutoResetEvent Closing = new AutoResetEvent(false);
        private static readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        public static async Task Main(string[] args)
        {
            var ct = CancellationTokenSource.Token;

            ct.Register(() => Closing.Set());
            Console.CancelKeyPress += (sender, eventArgs) => CancellationTokenSource.Cancel();

            Console.WriteLine("Starting PostalRegistry.Projections.LastChangedList");

            AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) =>
                Log.Debug(
                    eventArgs.Exception,
                    "FirstChanceException event raised in {AppDomain}.",
                    AppDomain.CurrentDomain.FriendlyName);

            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
                Log.Fatal((Exception)eventArgs.ExceptionObject, "Encountered a fatal exception, exiting program.");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .AddCommandLine(args ?? new string[0])
                .Build();

            var container = ConfigureServices(configuration);
            var logger = container.GetService<ILogger<Program>>();

            try
            {
                var runner = container.GetService<PostalInfoLastChangedListRunner>();

                var lastChangedList = new LastChangedListMigrationsHelper(
                    configuration.GetConnectionString("LastChangedList"),
                    container.GetService<ILoggerFactory>());

                await lastChangedList.RunMigrationsAsync(ct);

                await runner.StartAsync(
                    container.GetService<IStreamStore>(),
                    container.GetService<Func<Owned<LastChangedListContext>>>(),
                    ct);

                Console.WriteLine("Running... Press CTRL + C to exit.");
                Closing.WaitOne();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                logger.LogCritical(e, "Encountered a fatal exception, exiting program.");
                Log.CloseAndFlush();

                // Allow some time for flushing before shutdown.
                Thread.Sleep(1000);
                throw;
            }

            Console.WriteLine("Stopping...");
            Closing.Close();
        }

        private static IServiceProvider ConfigureServices(IConfiguration configuration)
        {
            var services = new ServiceCollection();

            var builder = new ContainerBuilder();

            builder.RegisterModule(new LoggingModule(configuration, services));

            var tempProvider = services.BuildServiceProvider();
            builder.RegisterModule(
                new LastChangedListModule(
                    configuration.GetConnectionString("LastChangedList"),
                    configuration["DataDog:ServiceName"],
                    services,
                    tempProvider.GetService<ILoggerFactory>()));

            builder.RegisterModule(new ProjectionsModule(configuration));

            builder
                .RegisterType<PostalInfoLastChangedListRunner>()
                .SingleInstance();

            builder.Populate(services);

            return new AutofacServiceProvider(builder.Build());
        }
    }
}
