using Autofac;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.ViewModels.Main;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Windows;
using ClearDashboard.DataAccessLayer.Wpf.Infrastructure;
using ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Wpf.Application.ViewModels.Startup
{
    public class ProjectSetupViewModel : DashboardApplicationValidatingWorkflowStepViewModel<StartupDialogViewModel,DataAccessLayer.Models.Project>, IHandle<CreateProjectMessage>
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

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                NotifyOfPropertyChange(() => SearchText);
            }
        }

        public ProjectSetupViewModel(DashboardProjectManager projectManager,
            INavigationService navigationService, ILogger<ProjectSetupViewModel> logger, IEventAggregator eventAggregator,
            IMediator mediator, ILifetimeScope? lifetimeScope, TranslationSource translationSource, IValidator<DataAccessLayer.Models.Project> validator)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, validator)
        {
            if (!ProjectManager!.HasDashboardProject)
            {
                ProjectManager.CreateDashboardProject();
            }

            Project = new DataAccessLayer.Models.Project();

            CanMoveForwards = true;
            CanMoveBackwards = true;
            EnableControls = true;
           
            ProjectName = string.Empty;

            //return base.OnInitializeAsync(cancellationToken);
        }
        
        public void Create()
        {
            if (CheckIfConnectedToParatext() == false)
            {
                return;
            }

            ProjectManager!.CurrentDashboardProject.ProjectName = Project.ProjectName;
            ProjectManager.CurrentDashboardProject.IsNew = true;
           
            ParentViewModel!.ExtraData = ProjectManager.CurrentDashboardProject;
            ParentViewModel?.Ok();
        }

        public void Create(string name)
        {
            ProjectName = name;
            Create();
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


        public Task HandleAsync(CreateProjectMessage message, CancellationToken cancellationToken)
        {
            Create(message.Message);
            return Task.CompletedTask;
        }
    }
}
