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
    public abstract  class WorkflowShellViewModel : Conductor<WorkflowStepViewModel>.Collection.OneActive
    {
        protected ILogger<WorkflowShellViewModel> Logger { get; set; }
        protected IServiceProvider ServiceProvider { get; set; }
        public List<WorkflowStepViewModel> Steps { get; set; }
        protected IEventAggregator EventAggregator { get; set; }
        protected INavigationService NavigationService { get; set; }
        protected ProjectManager ProjectManager { get; set; }


        private FlowDirection _flowDirection = FlowDirection.LeftToRight;
        public FlowDirection FlowDirection
        {
            get => _flowDirection;
            set => Set(ref _flowDirection, value, nameof(FlowDirection));
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

        protected WorkflowShellViewModel(ProjectManager projectManager, IServiceProvider serviceProvider, ILogger<WorkflowShellViewModel> logger, INavigationService navigationService, IEventAggregator eventAggregator)
        {
            ProjectManager = projectManager;
            ServiceProvider = serviceProvider;
            Logger = logger;
            NavigationService = navigationService;
            EventAggregator = eventAggregator;
            Steps = new List<WorkflowStepViewModel>();

            FlowDirection = ProjectManager.CurrentLanguageFlowDirection;

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

        public override Task ActivateItemAsync(WorkflowStepViewModel item, CancellationToken cancellationToken = new CancellationToken())
        {
            // TODO:  do whatever needs to be done when a new workflow step has been activated.
            return base.ActivateItemAsync(item, cancellationToken);
        }

        protected WorkflowStepViewModel CurrentStep { get; set; }
        protected override WorkflowStepViewModel DetermineNextItemToActivate(IList<WorkflowStepViewModel> list, int lastIndex)
        {
            var current = list[lastIndex];

            var currentIndex = Steps.IndexOf(current);
            WorkflowStepViewModel next;
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
