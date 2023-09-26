using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.Infrastructure
{
    public abstract class DashboardApplicationWorkflowStepViewModel<TParentViewModel>: WorkflowStepViewModel
    where TParentViewModel : class
    {
        protected DashboardProjectManager? ProjectManager { get; }
        protected ILocalizationService? LocalizationService { get; }
        public TParentViewModel? ParentViewModel => Parent as TParentViewModel;

        protected DashboardApplicationWorkflowStepViewModel()
        {
           
        }

        protected DashboardApplicationWorkflowStepViewModel (DashboardProjectManager projectManager,
            INavigationService navigationService, ILogger logger, IEventAggregator eventAggregator,
            IMediator mediator, ILifetimeScope? lifetimeScope, ILocalizationService localizationService)
            : base(navigationService, logger, eventAggregator, mediator, lifetimeScope)
        {
            ProjectManager = projectManager;
            LocalizationService = localizationService;
        }

        protected async Task SendBackgroundStatus(string name, LongRunningTaskStatus status,
            CancellationToken cancellationToken, string? description = null, Exception? exception = null,
            BackgroundTaskStatus.BackgroundTaskMode backgroundTaskMode = BackgroundTaskStatus.BackgroundTaskMode.Normal)
        {
            if (exception is not null || description is not null)
            {
                Logger!.LogInformation("Task '{name}' reports status '{status}' with message '{message}'", name, status, exception?.Message ?? description);
            }
            else
            {
                Logger!.LogInformation("Task '{name}' reports status '{status}'", name, status);
            }

            var backgroundTaskStatus = new BackgroundTaskStatus
            {
                Name = name,
                EndTime = DateTime.Now,
                Description = !string.IsNullOrEmpty(description) ? description : null,
                ErrorMessage = exception != null ? $"{exception}" : null,
                TaskLongRunningProcessStatus = status,
                BackgroundTaskType = backgroundTaskMode,
            };
            await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(backgroundTaskStatus), cancellationToken);
        }

    }
}
