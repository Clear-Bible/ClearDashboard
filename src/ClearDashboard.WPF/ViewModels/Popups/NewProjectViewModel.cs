using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.ViewModels.Workflows;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.ViewModels.Popups
{
    public class NewProjectViewModel : ValidatingWorkflowStepViewModel<Project>
    {
       
        public NewProjectViewModel()
        {
            // used by Caliburn Micro for design time    
        }

        public NewProjectViewModel(INavigationService navigationService,
            ILogger<WorkSpaceViewModel> logger,
            DashboardProjectManager projectManager, 
            IEventAggregator eventAggregator,
            IValidator<Project> projectValidator)
            : base(eventAggregator, navigationService, logger, projectManager, projectValidator)
        {
          
            if (!ProjectManager.HasDashboardProject)
            {
                ProjectManager.CreateDashboardProject();
            }

            Title = "Create New Project";

            Project = new Project();
        }

        private string _projectName;
        private bool _canCreate;
        private Project _project;


        public Project Project
        {
            get => _project;
            private init => Set(ref _project, value);
        }

        public string ProjectName
        {
            get => _projectName;
            set
            {
                Set(ref _projectName, value);
                ProjectManager.CurrentDashboardProject.ProjectName = value;
                Project.ProjectName = value;
                ValidationResult = Validator.Validate(Project);
                CanCreate = ValidationResult.IsValid;
                NotifyOfPropertyChange(nameof(Project));

            }
        }

        public bool CanCancel => true /* can always cancel */;

        public async void Cancel()
        {
            await TryCloseAsync(false);
        }

        public bool CanCreate
        {
            get => _canCreate;
            set => Set(ref _canCreate , value);
        }
    
        public async void Create()
        {
           await TryCloseAsync(true);
        }

        protected override ValidationResult Validate()
        {
            return (!string.IsNullOrEmpty(ProjectName)) ? Validator.Validate(Project) : null;
        }
    }
}
