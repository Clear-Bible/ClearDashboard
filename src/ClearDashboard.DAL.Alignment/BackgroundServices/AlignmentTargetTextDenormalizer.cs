using Autofac;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Features.Denormalization;
using ClearDashboard.DAL.Alignment.Features.Events;
using ClearDashboard.DAL.Alignment.Features.Translation;
using ClearDashboard.DAL.Interfaces;
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
        private TaskCompletionSource<bool>? _tcs = null;

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
                var waitTime = 120000;
                if (_serviceScope.Resolve<IProjectProvider>().HasCurrentProject)
                {
                    _logger.LogDebug($"AlignmentTargetTextDenormalizer running denormalization.");
                    await Task.Run(() => RunDenormalization(stoppingToken), stoppingToken);
                }
                else
                {
                    waitTime = 500;
                }

                _logger.LogDebug($"AlignmentTargetTextDenormalizer waiting for event or completion of delay time.");

                _tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                await Task.WhenAny(tasks: new Task[] { _tcs.Task, Task.Delay(waitTime, stoppingToken) });
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            try 
            { 
                _tcs?.TrySetCanceled(cancellationToken); 
            } 
            catch (ObjectDisposedException) { }

            return base.StopAsync(cancellationToken);
        }

        private async Task TriggerTaskCompletionSource()
        {
            try
            {
                _tcs?.TrySetResult(true);
            }
            catch (ObjectDisposedException)
            {
                _logger.LogDebug($"AlignmentTargetTextDenormalizer TaskCompletionSource already disposed.");
            }

            await Task.CompletedTask;
        }

        private async Task RunDenormalization(CancellationToken cancellationToken)
        {
            var name = "Denormalization";
            await PublishStatusMessage(name, LongRunningProcessStatus.Working, cancellationToken, "Running alignment data denormalization");

            var result = await _mediator.Send(
                new DenormalizeAlignmentTopTargetsCommand(Guid.Empty), 
                cancellationToken);

            if (result.Success)
            {
                await PublishStatusMessage(name, LongRunningProcessStatus.Completed, cancellationToken, "Completed alignment data denormalization");
                return;
            }
            else
            {
                await PublishStatusMessage(name, LongRunningProcessStatus.Completed, cancellationToken, "Error running alignment data denormalization (see logs for detailed error message)", DateTime.Now.AddMinutes(-1));
                _logger.LogDebug($"AlignmentTargetTextDenormalizer failed due to: {result.Message}");

                //await Task.Delay(2000, cancellationToken).ContinueWith(cw => PublishStatusMessage(name, LongRunningProcessStatus.Completed, cancellationToken, null, DateTime.Now.AddMinutes(-5)));
            }
        }
        private async Task PublishStatusMessage(string name, LongRunningProcessStatus status, CancellationToken cancellationToken, string? description = null, DateTime? endTime = null, Exception? exception = null)
        {
            var backgroundTaskStatus = new BackgroundTaskStatus
            {
                Name = name,
                EndTime = endTime ?? DateTime.Now,
                Description = !string.IsNullOrEmpty(description) ? description : null,
                ErrorMessage = exception != null ? $"{exception}" : null,
                TaskLongRunningProcessStatus = status
            };
            await _eventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(backgroundTaskStatus), cancellationToken);
        }

        public async Task HandleAsync(AlignmentSetCreatedEvent message, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"AlignmentTargetTextDenormalizer received AlignmentSetCreatedEvent.");
            await TriggerTaskCompletionSource();
        }

        public async Task HandleAsync(AlignmentAddedRemovedEvent message, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"AlignmentTargetTextDenormalizer received AlignmentAddedRemovedEvent.");
            await TriggerTaskCompletionSource();
        }

    }
}
