using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Wpf;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
namespace ClearDashboard.Wpf.ViewModels.Project
{
    public class NewProjectDialogViewModel : ValidatingApplicationScreen<DataAccessLayer.Models.Project>
    {
       
        public NewProjectDialogViewModel()
        {
            // used by Caliburn Micro for design time    
        }

        public NewProjectDialogViewModel(INavigationService navigationService,
            ILogger<NewProjectDialogViewModel> logger,
            DashboardProjectManager projectManager, 
            IEventAggregator eventAggregator,
            IValidator<DataAccessLayer.Models.Project> projectValidator)
            : base(navigationService, logger, projectManager, eventAggregator, projectValidator)
        {
          
            if (!ProjectManager.HasDashboardProject)
            {
                ProjectManager.CreateDashboardProject();
            }

            //Title = "Create New Project";
            DisplayName = "**** Create New Project";

            Project = new DataAccessLayer.Models.Project();
        }

        private string _dialogTitle;
        public string DialogTitle
        {
            get => _dialogTitle;
            set => Set(ref _dialogTitle, string.IsNullOrEmpty(value) ? "Create New Project" : $"Create New Project: {value}");
        }

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
                ProjectManager.CurrentDashboardProject.ProjectName = value;
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

        public async void Create()
        {
            await TryCloseAsync(true);
        }


        protected override ValidationResult Validate()
        {
            return (!string.IsNullOrEmpty(ProjectName)) ? Validator.Validate(Project) : null;
        }

       // override 

        //protected override Task OnActivateAsync(CancellationToken cancellationToken)
        //{
        //    //ShowWorkflowButtons();
        //    return base.OnActivateAsync(cancellationToken);
        //}
    }
}
