using System;
using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.ViewModels.Infrastructure;
using ClearDashboard.Wpf.Application.ViewModels.Main;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using FluentValidation;
using FluentValidation.Results;
using ClearDashboard.DataAccessLayer;

namespace ClearDashboard.Wpf.Application.ViewModels.Startup
{
    public class ProjectSetupViewModel : ApplicationValidatingWorkflowStepViewModel<DataAccessLayer.Models.Project>, IStartupDialog
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
            INavigationService navigationService, ILogger<MainViewModel> logger, IEventAggregator eventAggregator,
            IMediator mediator, ILifetimeScope? lifetimeScope, TranslationSource translationSource, IValidator<DataAccessLayer.Models.Project> validator)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, validator)
        {
            if (!ProjectManager!.HasDashboardProject)
            {
                ProjectManager.CreateDashboardProject();
            }

            Project = new DataAccessLayer.Models.Project();
        }

        protected async Task OnInitializeAsync(CancellationToken cancellationToken)
        {

            CanMoveForwards = true;
            CanMoveBackwards = true;
            EnableControls = true;

            ProjectName = string.Empty;
        }

        public void Create()
        {
            if (CheckIfConnectedToParatext() == false)
            {
                return;
            }

            ProjectManager!.CurrentDashboardProject.ProjectName = Project.ProjectName;
            ProjectManager.CurrentDashboardProject.IsNew = true;
            var startupDialogViewModel = Parent as StartupDialogViewModel;
            startupDialogViewModel!.ExtraData = ProjectManager.CurrentDashboardProject;
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

        public object? ExtraData { get; set; }


        private DataAccessLayer.Models.Project _project;
        public DataAccessLayer.Models.Project Project
        {
            get => _project;
            private init => Set(ref _project, value);
        }

        private string _projectName;
        public string ProjectName
        {
            get => _projectName;
            set
            {
                Set(ref _projectName, value);
                ProjectManager!.CurrentDashboardProject.ProjectName = value;
                Project.ProjectName = value;
                ValidationResult = Validator.Validate(Project);
                CanCreate = !string.IsNullOrEmpty(value) && ValidationResult.IsValid;
                NotifyOfPropertyChange(nameof(Project));
            }
        }


        public bool CanCancel => true /* can always cancel */;
        public async void Cancel()
        {
            if (!string.IsNullOrEmpty(ProjectName))
            {
                var deletedProject = await ProjectManager.DeleteProject(ProjectName);
            }

            ProjectManager.CurrentProject = null;
            await TryCloseAsync(false);
        }


        private bool _canCreate;
        public bool CanCreate
        {
            get => _canCreate;
            set => Set(ref _canCreate, value);
        }


        protected override ValidationResult Validate()
        {
            return (!string.IsNullOrEmpty(ProjectName)) ? Validator.Validate(Project) : null;
        }

  
    }
}
