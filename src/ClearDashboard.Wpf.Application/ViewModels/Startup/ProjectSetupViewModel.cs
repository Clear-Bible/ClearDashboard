using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.ViewModels.Main;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Wpf.Application.ViewModels.Startup
{
    public class ProjectSetupViewModel : ApplicationWorkflowStepViewModel, IStartupDialog
    {
        private Visibility _alertVisibility = Visibility.Visible;
        public Visibility AlertVisibility
        {
            get => _alertVisibility;
            set
            {
                _alertVisibility = value;
                NotifyOfPropertyChange(() => AlertVisibility);
            }
        }

        public ProjectSetupViewModel(DashboardProjectManager projectManager,
            INavigationService? navigationService, ILogger<MainViewModel> logger, IEventAggregator? eventAggregator,
            IMediator mediator, ILifetimeScope? lifetimeScope, TranslationSource translationSource)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope)
        {

        }

        protected async Task OnInitializeAsync(CancellationToken cancellationToken)
        {

            CanMoveForwards = true;
            CanMoveBackwards = true;
            EnableControls = true;
        }

        public async Task Create(DashboardProject project)
        {
            if (CheckIfConnectedToParatext() == false)
            {
                return;
            }

            ProjectManager!.CurrentDashboardProject = project;
            var startupDialogViewModel = Parent as StartupDialogViewModel;
            startupDialogViewModel!.ExtraData = project;
            startupDialogViewModel?.Ok();
        }

        private bool CheckIfConnectedToParatext()
        {
            if (ProjectManager?.HasCurrentParatextProject == false)
            {
                AlertVisibility = Visibility.Visible;
                return false;
            }
            return true;
        }

        public object ExtraData { get; set; }
    }
}
