using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Wpf;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.ViewModels.Workflows
{
    public abstract  class WorkflowShellViewModel : Conductor<IWorkflowStepViewModel>.Collection.OneActive
    {
        protected ILogger<WorkflowShellViewModel> Logger { get; set; }
        protected IServiceProvider ServiceProvider { get; set; }
        public List<IWorkflowStepViewModel> Steps { get; set; }
        protected IEventAggregator EventAggregator { get; set; }
        protected INavigationService NavigationService { get; set; }
        protected DashboardProjectManager ProjectManager { get; set; }


        private FlowDirection _windowFlowDirection = FlowDirection.LeftToRight;
        public FlowDirection WindowFlowDirection
        {
            get => _windowFlowDirection;
            set => Set(ref _windowFlowDirection, value, nameof(WindowFlowDirection));
        }

        private bool isLastWorkflowStep_;
        public bool IsLastWorkflowStep
        {
            get => isLastWorkflowStep_;
            set
            {
                Set(ref isLastWorkflowStep_, value, nameof(IsLastWorkflowStep));
            }
        }

        private bool enableControls_;
        public bool EnableControls
        {
            get => enableControls_;
            set
            {
                Logger.LogInformation($"WorkflowShellViewModel - Setting EnableControls to {value} at {DateTime.Now:HH:mm:ss.fff}");
                Set(ref enableControls_, value);
            }
        }


        protected WorkflowShellViewModel(DashboardProjectManager projectManager, IServiceProvider serviceProvider, ILogger<WorkflowShellViewModel> logger, INavigationService navigationService, IEventAggregator eventAggregator)
        {
            ProjectManager = projectManager;
            ServiceProvider = serviceProvider;
            Logger = logger;
            NavigationService = navigationService;
            EventAggregator = eventAggregator;
            Steps = new List<IWorkflowStepViewModel>();

            WindowFlowDirection = ProjectManager.CurrentLanguageFlowDirection;

        }

        protected override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            EventAggregator.SubscribeOnUIThread(this);
            return base.OnActivateAsync(cancellationToken);
        }

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            EventAggregator.Unsubscribe(this);
            return base.OnDeactivateAsync(close, cancellationToken);
        }

        public override Task ActivateItemAsync(IWorkflowStepViewModel item, CancellationToken cancellationToken = new CancellationToken())
        {
            // TODO:  do whatever needs to be done when a new workflow step has been activated.
            return base.ActivateItemAsync(item, cancellationToken);
        }

        protected IWorkflowStepViewModel CurrentStep { get; set; }
        protected override IWorkflowStepViewModel DetermineNextItemToActivate(IList<IWorkflowStepViewModel> list, int lastIndex)
        {
            var current = list[lastIndex];

            var currentIndex = Steps.IndexOf(current);
            IWorkflowStepViewModel next;
            switch (current.Direction)
            {
                case Direction.Forwards:
                    next = currentIndex < Steps.Count - 1 ? Steps[++currentIndex] : current;
                    break;
                case Direction.Backwards:
                    next = currentIndex > 0 ? Steps[--currentIndex] : current;
                    break;
                default:
                    return current;
            }

            IsLastWorkflowStep = currentIndex == Steps.Count - 1;

            //if (Steps.Count > 1 && currentIndex == Steps.Count - 1)
            //{
            //    EnablePurchaseButton = IsLastWorkflowStep && ReadyForPurchase;
            //}

            //next.Workflow = current.Workflow;

          

            CurrentStep = next;
            return next;
        }
    }
}
