namespace PostalRegistry.Producer
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public class PostalProducer : BackgroundService
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly ILogger<PostalProducer> _logger;
        private readonly IConnectedProjectionsManager _projectionManager;

        public PostalProducer(
            IHostApplicationLifetime hostApplicationLifetime,
            IConnectedProjectionsManager projectionManager,
            ILogger<PostalProducer> logger)
        {
            _hostApplicationLifetime = hostApplicationLifetime;
            _projectionManager = projectionManager;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await _projectionManager.Start(stoppingToken);
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, $"Critical error occured in {nameof(PostalProducer)}.");
                _hostApplicationLifetime.StopApplication();
                throw;
            }
        }
    }
}
