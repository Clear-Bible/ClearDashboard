using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using Caliburn.Micro;
using ClearDashboard.Common.Models;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.Helpers;
using ClearDashboard.Wpf.Views;
using ClearDashboard.Wpf.Views.Workflows.NewProject;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Path = System.Windows.Shapes.Path;



namespace ClearDashboard.Wpf.ViewModels.Workflows.NewProject
{
    public class CreateNewProjectWorkflowShellViewModel : WorkflowShellViewModel
    {
        public CreateNewProjectWorkflowShellViewModel(ProjectManager projectManager, IServiceProvider serviceProvider, ILogger<WorkflowShellViewModel> logger, INavigationService navigationService, IEventAggregator eventAggregator) : base(projectManager, serviceProvider, logger, navigationService, eventAggregator)
        {
            FlowDirection = ProjectManager.CurrentLanguageFlowDirection;
        }

        private FlowDirection _flowDirection = FlowDirection.LeftToRight;
        public FlowDirection FlowDirection
        {
            get => _flowDirection;
            set => Set(ref _flowDirection, value, nameof(FlowDirection));
        }

        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            await base.OnInitializeAsync(cancellationToken);

            var step1 = ServiceProvider.GetService<CreateNewProjectWorkflowStepViewModel>();
            Steps.Add(step1);

            CurrentStep = Steps[0];
            IsLastWorkflowStep = (Steps.Count == 1);
            await ActivateItemAsync(Steps[0], cancellationToken);
        }
    }
}
