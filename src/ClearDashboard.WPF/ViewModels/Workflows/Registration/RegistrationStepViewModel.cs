using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.ViewModels.Workflows.CreateNewProject;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.ViewModels.Workflows.NewProject
{
    public class RegistrationWorkflowStepViewModel : WorkflowStepViewModel
    {


        private DashboardProject _dashboardProject;

        #region Constructors

        /// <summary>
        /// Required for design-time support
        /// </summary>
        public RegistrationWorkflowStepViewModel()
        {

        }

        public RegistrationWorkflowStepViewModel(IEventAggregator eventAggregator, INavigationService navigationService,
            ILogger<CreateNewProjectWorkflowStepViewModel> logger, DashboardProjectManager projectManager) : base(eventAggregator, navigationService, logger, projectManager)
        {

        }



        #endregion

        public DashboardProject DashboardProject
        {
            get => _dashboardProject;
            set
            {
               Set(ref _dashboardProject, value);
            }
        }

        protected override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            if (ProjectManager.HasDashboardProject)
            {
                ProjectManager.CreateDashboardProject();
            }
            return base.OnActivateAsync(cancellationToken);
        }
    }
}
