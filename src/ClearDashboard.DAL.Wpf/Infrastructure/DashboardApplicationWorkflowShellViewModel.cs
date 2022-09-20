using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;

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


    }
}
