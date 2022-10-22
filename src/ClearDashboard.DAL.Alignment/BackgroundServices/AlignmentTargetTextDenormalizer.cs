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
using System.Xml.Linq;

namespace ClearDashboard.DAL.Alignment.BackgroundServices
{
    public class AlignmentTargetTextDenormalizer : BackgroundService, IHandle<AlignmentSetCreatedEvent>, IHandle<AlignmentAddedRemovedEvent>, IHandle<BackgroundTaskChangedMessage>
    {
        private readonly IMediator _mediator;
        private readonly IProjectProvider _projectProvider;
        private readonly ILogger<AlignmentTargetTextDenormalizer> _logger;
        private readonly BackgroundServiceDelegateProgress<AlignmentTargetTextDenormalizer>.Factory _progressReporterFactory;
        private TaskCompletionSource<bool>? _tcs = null;

        public AlignmentTargetTextDenormalizer(
            IMediator mediator, 
            IProjectProvider projectProvider, 
            ILogger<AlignmentTargetTextDenormalizer> logger, 
            IEventAggregator eventAggregator, 
            IHostApplicationLifetime hostApplicationLifetime,
            BackgroundServiceDelegateProgress<AlignmentTargetTextDenormalizer>.Factory progressReporterFactory)
        {
            _mediator = mediator;
            _projectProvider = projectProvider;
            _logger = logger;
            _progressReporterFactory = progressReporterFactory;

            hostApplicationLifetime.ApplicationStarted.Register(() => _logger.LogInformation("Background service started"));
            hostApplicationLifetime.ApplicationStopping.Register(() => _logger.LogInformation("Background service stopping"));
            hostApplicationLifetime.ApplicationStopped.Register(() => _logger.LogInformation("Background service stopped"));

            eventAggregator.SubscribeOnBackgroundThread(this);
            eventAggregator.SubscribeOnUIThread(this);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.Register(() =>
                _logger.LogDebug($" AlignmentTargetTextDenormalizer background task is stopping."));

            // Await right away so Host Startup can continue.
            await Task.Delay(10, stoppingToken).ConfigureAwait(false);

            while (!stoppingToken.IsCancellationRequested)
            {
                var waitTime = 30000;
                if (_projectProvider.HasCurrentProject)
                {
                    _logger.LogDebug($"AlignmentTargetTextDenormalizer running denormalization.");
                    await RunDenormalization(stoppingToken);
                }
                else
                {
                    waitTime = 500;
                }

                if (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogDebug("Cancellation REQUESTED!");
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
            var result = await _mediator.Send(
                new DenormalizeAlignmentTopTargetsCommand(Guid.Empty, _progressReporterFactory(cancellationToken)), 
                cancellationToken);

            if (result.Success)
            {
                return;
            }
            else
            {
                _logger.LogDebug($"AlignmentTargetTextDenormalizer failed due to: {result.Message}");
            }
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

        public async Task HandleAsync(BackgroundTaskChangedMessage message, CancellationToken cancellationToken)
        {
            if (message.Status.Name == LocalizationStrings.Get("Denormalization_AlignmentTopTargets_BackgroundTaskName", _logger) && 
                message.Status.TaskLongRunningProcessStatus == LongRunningProcessStatus.CancelTaskRequested)
            {
                if (!ExecuteTask.IsCompleted)
                {
                    await StopAsync(cancellationToken);
                    await Task.Delay(30000, cancellationToken);
                    await StartAsync(cancellationToken);
                }
            }
        }
    }
}
