using Autofac;
using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Infrastructure;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ClearApplicationFoundation.Services;
using ClearDashboard.Wpf.Application.Messages;
using ClearDashboard.Wpf.Application.Services;
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
            IMediator mediator, ILifetimeScope? lifetimeScope, TranslationSource translationSource, IValidator<DataAccessLayer.Models.Project> validator, ILocalizationService localizationService)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, validator, localizationService)
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

            var currentlyOpenProjectsList = OpenProjectManager.DeserializeOpenProjectList();
            if (currentlyOpenProjectsList.Contains(ProjectName))
            {
                return;
            }

            EventAggregator.PublishOnUIThreadAsync(new DashboardProjectNameMessage(ProjectName));

            OpenProjectManager.AddProjectToOpenProjectList(ProjectManager);

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
                Set(ref _projectName, value.Replace(' ','_'));
                ProjectManager!.CurrentDashboardProject.ProjectName = _projectName;
                Project.ProjectName = _projectName;
                ValidationResult = Validator.Validate(Project);
                CanCreate = !string.IsNullOrEmpty(_projectName) && ValidationResult.IsValid;
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
