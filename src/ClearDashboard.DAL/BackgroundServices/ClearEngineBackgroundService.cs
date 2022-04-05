using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DataAccessLayer.BackgroundServices
{
    public sealed class ClearEngineBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ClearEngineBackgroundService> _logger;

        public ClearEngineBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<ClearEngineBackgroundService> logger) =>
            (_serviceProvider, _logger) = (serviceProvider, logger);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                $"{nameof(ClearEngineBackgroundService)} is running.");

            await DoWorkAsync(stoppingToken);
        }

        private async Task DoWorkAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                $"{nameof(ClearEngineBackgroundService)} is working.");

            using var scope = _serviceProvider.CreateScope();
            var scopedProcessingService =
                scope.ServiceProvider.GetRequiredService<IClearEngineProcessingService>();

            await scopedProcessingService.DoWorkAsync(stoppingToken);
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                $"{nameof(ClearEngineBackgroundService)} is stopping.");

            await base.StopAsync(stoppingToken);
        }
    }
}
