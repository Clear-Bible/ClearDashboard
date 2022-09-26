using System;
using System.Threading;
using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ClearDashboard.DataAccessLayer.Wpf.Infrastructure
{
    public class DashboardApplicationWorkflowShellViewModel : WorkflowShellViewModel, IDialog
    {
        private string? _currentStepTitle;
        private DialogMode _dialogMode;

        public DashboardApplicationWorkflowShellViewModel()
        {
            DialogMode = DialogMode.Add;
        }

        public DashboardApplicationWorkflowShellViewModel(DashboardProjectManager? projectManager, 
            INavigationService navigationService,
            ILogger logger,
            IEventAggregator eventAggregator, IMediator mediator, ILifetimeScope lifetimeScope) : base(
            navigationService, logger, eventAggregator, mediator, lifetimeScope)
        {
            ProjectManager = projectManager;
            DialogMode = DialogMode.Add;
        }

        public DashboardProjectManager? ProjectManager { get; private set; }

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

        public async Task SendBackgroundStatus(string name, LongRunningProcessStatus status, CancellationToken cancellationToken, string? description = null, Exception? ex = null)
        {

            var backgroundTaskStatus = new BackgroundTaskStatus
            {
                Name = name,
                EndTime = DateTime.Now,
                Description = !string.IsNullOrEmpty(description) ? description : null,
                ErrorMessage = ex != null ? $"{ex}" : null,
                TaskLongRunningProcessStatus = status
            };
            await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(backgroundTaskStatus), cancellationToken);
        }


    }
}
