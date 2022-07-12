using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.ViewModels.Workflows;
using ClearDashboard.Wpf.ViewModels.Workflows.CreateNewProject;
using Microsoft.Extensions.Logging;

namespace ViewModels.Workflows.NewProject
{
    public class NewProjectWorkflowStepViewModel : WorkflowStepViewModel
    {


        private DashboardProject _dashboardProject;

        #region Constructors

        /// <summary>
        /// Required for design-time support
        /// </summary>
        public NewProjectWorkflowStepViewModel()
        {

        }

        public NewProjectWorkflowStepViewModel(IEventAggregator eventAggregator, INavigationService navigationService,
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
