using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using ClearApplicationFoundation.Services;

namespace ClearDashboard.Wpf.Application.Infrastructure
{
    public class DashboardApplicationWorkflowShellViewModel : WorkflowShellViewModel, IDialog
    {
        private string? _currentStepTitle;
        private string? _currentProject;
        private DialogMode _dialogMode;

        public DashboardApplicationWorkflowShellViewModel()
        {
            DialogMode = DialogMode.Add;
        }

        public DashboardApplicationWorkflowShellViewModel(DashboardProjectManager? projectManager, 
            INavigationService navigationService,
            ILogger logger,
            IEventAggregator eventAggregator, IMediator mediator, ILifetimeScope lifetimeScope, ILocalizationService localizationService) : base(
            navigationService, logger, eventAggregator, mediator, lifetimeScope)
        {
            ProjectManager = projectManager;
            LocalizationService = localizationService;
            DialogMode = DialogMode.Add;
        }

        public DashboardProjectManager? ProjectManager { get; private set; }
        protected ILocalizationService? LocalizationService { get; }

        public DialogMode DialogMode
        {
            get => _dialogMode;
            set => Set(ref _dialogMode, value);
        }

        public string? CurrentStepTitle
        {
            get => _currentStepTitle;
            set
            {
                Set(ref _currentStepTitle, value);
                Title = $"{DisplayName} - {_currentStepTitle}";
            }
        }

        public string? CurrentProject
        {
            get => _currentProject;
            set
            {
                Set(ref _currentProject, value);
                Title = $"{DisplayName} - {_currentProject}";
            }
        }

        private string? _message;
        public string? Message
        {
            get => _message;
            set => Set(ref _message, value);
        }

        public async Task SendBackgroundStatus(string name, LongRunningTaskStatus status, CancellationToken cancellationToken, string? description = null, Exception? exception = null)
        {
            Message = !string.IsNullOrEmpty(description) ? description : null;
            var backgroundTaskStatus = new BackgroundTaskStatus
            {
                Name = name,
                EndTime = DateTime.Now,
                Description = !string.IsNullOrEmpty(description) ? description : null,
                ErrorMessage = exception != null ? $"{exception}" : null,
                TaskLongRunningProcessStatus = status
            };
            await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(backgroundTaskStatus), cancellationToken);
        }


    }
}
