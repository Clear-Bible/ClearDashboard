using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.ViewModels.Workflows;
using ClearDashboard.Wpf.ViewModels.Workflows.CreateNewProject;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ViewModels.Workflows.NewProject
{
    public class NewProjectWorkflowShellViewModel : WorkflowShellViewModel
    {
        public NewProjectWorkflowShellViewModel(DashboardProjectManager projectManager, IServiceProvider serviceProvider, ILogger<WorkflowShellViewModel> logger, INavigationService navigationService, IEventAggregator eventAggregator) : base(projectManager, serviceProvider, logger, navigationService, eventAggregator)
        {

        }


        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            await base.OnInitializeAsync(cancellationToken);

           // var step1 = ServiceProvider.GetService<CreateNewProjectWorkflowStepViewModel>();
           // Steps.Add(step1);

           // var step2 = ServiceProvider.GetService<ProcessUSFMWorkflowStepViewModel>();
           // Steps.Add(step2);

            CurrentStep = Steps[0];
            IsLastWorkflowStep = (Steps.Count == 1);
            await ActivateItemAsync(Steps[0], cancellationToken);
        }
    }
}
