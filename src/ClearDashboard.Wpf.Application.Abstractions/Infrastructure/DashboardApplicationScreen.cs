using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using System;
using static ClearDashboard.DataAccessLayer.Threading.BackgroundTaskStatus;

namespace ClearDashboard.Wpf.Application.Infrastructure
{
    public abstract class DashboardApplicationScreen : ApplicationScreen
    {
        private string? _message;

        protected DashboardApplicationScreen()
        {
          
        }

        protected DashboardApplicationScreen(DashboardProjectManager? projectManager,
            INavigationService? navigationService, ILogger? logger, IEventAggregator? eventAggregator,
            IMediator? mediator, ILifetimeScope? lifetimeScope, ILocalizationService localizationService) : base(navigationService, logger, eventAggregator, mediator, lifetimeScope)
        {
            ProjectManager = projectManager;
            LocalizationService = localizationService;
        }

        public DashboardProjectManager? ProjectManager { get; private set; }

        protected ILocalizationService? LocalizationService { get; }

        public string? Message
        {
            get => _message;
            set => Set(ref _message, value);
        }

        public async Task SendBackgroundStatus(string name, 
            LongRunningTaskStatus status, 
            CancellationToken cancellationToken, 
            string? description = null, 
            Exception? exception = null,
                BackgroundTaskMode backgroundTaskMode = BackgroundTaskMode.Normal)
        {
            Message = !string.IsNullOrEmpty(description) ? description : null;
            var backgroundTaskStatus = new BackgroundTaskStatus
            {
                Name = name,
                EndTime = DateTime.Now,
                Description = !string.IsNullOrEmpty(description) ? description : null,
                ErrorMessage = exception != null ? $"{exception}" : null,
                TaskLongRunningProcessStatus = status,
                BackgroundTaskType = backgroundTaskMode
            };
            await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(backgroundTaskStatus), cancellationToken);
        }

    }
}
