using Autofac;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Features.Events;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboard.DAL.Alignment.BackgroundServices
{
    public class AlignmentTargetTextDenormalizer : BackgroundService, IHandle<AlignmentSetCreatedEvent>, IHandle<AlignmentAddedRemovedEvent>
    {
        private readonly ILifetimeScope _serviceScope;
        private readonly IMediator _mediator;
        private readonly IEventAggregator _eventAggregator;
        private readonly ILogger<AlignmentTargetTextDenormalizer> _logger;

        public AlignmentTargetTextDenormalizer(ILifetimeScope serviceProvider, IMediator mediator, IEventAggregator eventAggregator, ILogger<AlignmentTargetTextDenormalizer> logger)
        {
            _serviceScope = serviceProvider;
            _mediator = mediator;
            _eventAggregator = eventAggregator;
            _logger = logger;

            eventAggregator.SubscribeOnBackgroundThread(this);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.Register(() =>
                _logger.LogDebug($" AlignmentTargetTextDenormalizer background task is stopping."));

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogDebug($"AlignmentTargetTextDenormalizer task doing background work.");

                // This eShopOnContainers method is querying a database table
                // and publishing events into the Event Bus (RabbitMQ / ServiceBus)

                await Task.Delay(10000, stoppingToken);
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug($"AlignmentTargetTextDenormalizer stopAsync.");
            return base.StopAsync(cancellationToken);
        }
        private async Task SendBackgroundStatus(string name, LongRunningProcessStatus status, CancellationToken cancellationToken, string? description = null, Exception? exception = null)
        {
            var backgroundTaskStatus = new BackgroundTaskStatus
            {
                Name = name,
                EndTime = DateTime.Now,
                Description = !string.IsNullOrEmpty(description) ? description : null,
                ErrorMessage = exception != null ? $"{exception}" : null,
                TaskLongRunningProcessStatus = status
            };
            await _eventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(backgroundTaskStatus), cancellationToken);
        }

        public async Task HandleAsync(AlignmentSetCreatedEvent message, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            return;
        }

        public async Task HandleAsync(AlignmentAddedRemovedEvent message, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            return;
        }
    }
}
