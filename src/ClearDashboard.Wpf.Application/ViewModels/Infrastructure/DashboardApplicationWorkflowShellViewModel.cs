using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.Wpf.Application.ViewModels.Project.ParallelCorpusDialog;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.ViewModels.Infrastructure
{
    public enum WorkflowMode
    {
        Add,
        Edit
    }

    public class DashboardApplicationWorkflowShellViewModel : WorkflowShellViewModel
    {

        public DashboardApplicationWorkflowShellViewModel()
        {

        }

        public DashboardApplicationWorkflowShellViewModel(DashboardProjectManager? projectManager, 
            INavigationService navigationService,
            ILogger logger,
            IEventAggregator eventAggregator, IMediator mediator, ILifetimeScope lifetimeScope) : base(
            navigationService, logger, eventAggregator, mediator, lifetimeScope)
        {
            ProjectManager = projectManager;
            WorkflowMode = WorkflowMode.Add;
        }

        public DashboardProjectManager? ProjectManager { get; private set; }

        public WorkflowMode WorkflowMode { get; set; }
    }
}
