using Autofac;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Features.Denormalization;
using ClearDashboard.DAL.Alignment.Features.Events;
using ClearDashboard.DAL.Alignment.Features.Translation;
using ClearDashboard.DAL.CQRS;
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
using ClearDashboard.DataAccessLayer.Threading;

namespace ClearDashboard.DAL.Alignment.BackgroundServices
{
    public class AlignmentTargetTextDenormalizer : 
        BackgroundService, 
        IHandle<AlignmentSetCreatedEvent>, 
        IHandle<AlignmentAddedRemovedEvent>, 
        IHandle<AlignmentSetSourceTokenIdsUpdatedEvent>, 
        IHandle<AlignmentSetSourceTrainingTextsUpdatedEvent>, 
        IHandle<BackgroundTaskChangedMessage>
    {
        private readonly IMediator _mediator;
        private readonly IProjectProvider _projectProvider;
        private readonly ILogger<AlignmentTargetTextDenormalizer> _logger;
        private readonly BackgroundServiceDelegateProgress<AlignmentTargetTextDenormalizer>.Factory _progressReporterFactory;

        private TaskCompletionSource<bool>? _tcs = null;
        private readonly object _delayLock = new();
        private bool _shouldDelay = true;
        private bool _rootScopeLifetimeEnding = false;

        public AlignmentTargetTextDenormalizer(
            IMediator mediator,
            IProjectProvider projectProvider,
            ILogger<AlignmentTargetTextDenormalizer> logger,
            IEventAggregator eventAggregator,
            IHostApplicationLifetime hostApplicationLifetime,
            BackgroundServiceDelegateProgress<AlignmentTargetTextDenormalizer>.Factory progressReporterFactory,
            ILifetimeScope rootLifetimeScope)
        {
            _mediator = mediator;
            _projectProvider = projectProvider;
            _logger = logger;
            _progressReporterFactory = progressReporterFactory;

            hostApplicationLifetime.ApplicationStarted.Register(() => _logger.LogInformation("Background service started"));
            hostApplicationLifetime.ApplicationStopping.Register(() => _logger.LogInformation("Background service stopping"));
            hostApplicationLifetime.ApplicationStopped.Register(() => _logger.LogInformation("Background service stopped"));

            rootLifetimeScope.CurrentScopeEnding += (o, eventArgs) => _rootScopeLifetimeEnding = true;

            eventAggregator.SubscribeOnPublishedThread(this);
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
                if (_projectProvider.CanRunDenormalization)
                {
                    _logger.LogDebug($"AlignmentTargetTextDenormalizer running denormalization.");
                    await RunDenormalization(stoppingToken);
                }
                else
                {
                    waitTime = 500;
                }

                _logger.LogDebug($"AlignmentTargetTextDenormalizer waiting for event or completion of delay time.");

                _tcs = InitializeTaskCompletionSource();
                await Task.WhenAny(tasks: new Task[] { _tcs.Task, Task.Delay(waitTime, stoppingToken) });
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            try 
            {
                lock (_delayLock)
                {
                    _tcs?.TrySetCanceled(cancellationToken);
                    _shouldDelay = false;
                }
            } 
            catch (ObjectDisposedException) { }

            return base.StopAsync(cancellationToken);
        }

        private TaskCompletionSource<bool> InitializeTaskCompletionSource()
        {
            lock (_delayLock)
            {
                var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                if (!_shouldDelay)
                {
                    try
                    {
                        tcs.TrySetResult(true);
                        _logger.LogInformation($"AlignmentTargetTextDenormalizer no delay on next delay!.");

                    }
                    catch (ObjectDisposedException) { }
                }
                _shouldDelay = true;  // Set it back to default value for next iteration
                return tcs;
            }
        }

        private async Task TriggerTaskCompletionSource()
        {
            try
            {
                lock (_delayLock)
                {
                    _tcs?.TrySetResult(true);
                    _shouldDelay = false;
                }
            }
            catch (ObjectDisposedException)
            {
                _logger.LogDebug($"AlignmentTargetTextDenormalizer TaskCompletionSource already disposed.");
            }

            await Task.CompletedTask;
        }

        private async Task RunDenormalization(CancellationToken cancellationToken)
        {
            try
            {
                // FIXME:  we need to look at the application shutdown sequence to see why the DI container
                // resources are getting disposed before this BackgroundService StopAsync is complete. (Resulting
                // in a ObjectDisposedException when RunDenormalization uses Mediator to invoke its command).
                // As a stopgap, we are using the root container's CurrentScopeEnding event to set
                // _rootScopeLifetimeEnding, and using it to .  

                var result = (!_rootScopeLifetimeEnding) ? 
                    await _mediator.Send(
                        new DenormalizeAlignmentTopTargetsCommand(Guid.Empty, _progressReporterFactory(cancellationToken)), 
                        cancellationToken) :
                    new RequestResult<int>() { Success = false, Message = "Unable to call denormalization handler because root ILifetimeScope is disposing/ending"};

                if (result.Success)
                {
                    return;
                }
                else
                {
                    _logger.LogDebug($"AlignmentTargetTextDenormalizer failed due to: {result.Message}");
                }
            }
            catch (ObjectDisposedException)
            {
                if (!_rootScopeLifetimeEnding)
                {
                    _logger.LogError($"AlignmentTargetTextDenormalizer failed due to ObjectDisposedException");
                }
            }
        }

        public async Task HandleAsync(AlignmentSetCreatedEvent message, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"AlignmentTargetTextDenormalizer received AlignmentSetCreatedEvent.");
            await TriggerTaskCompletionSource();
        }

        public async Task HandleAsync(AlignmentAddedRemovedEvent message, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"AlignmentTargetTextDenormalizer received AlignmentAddedRemovedEvent.");
            await TriggerTaskCompletionSource();
        }

        public async Task HandleAsync(AlignmentSetSourceTokenIdsUpdatedEvent message, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"AlignmentTargetTextDenormalizer received AlignmentSetSourceTokenIdsUpdatedEvent.");
            await TriggerTaskCompletionSource();
        }

        public async Task HandleAsync(AlignmentSetSourceTrainingTextsUpdatedEvent message, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"AlignmentTargetTextDenormalizer received AlignmentSetSourceTrainingTextsUpdatedEvent.");
            await TriggerTaskCompletionSource();
        }

        public async Task HandleAsync(BackgroundTaskChangedMessage message, CancellationToken cancellationToken)
        {
            if (message.Status.Name == LocalizationStrings.Get("Denormalization_AlignmentTopTargets_BackgroundTaskName", _logger) && 
                message.Status.TaskLongRunningProcessStatus == LongRunningTaskStatus.CancellationRequested)
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
