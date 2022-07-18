using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.ViewModels.Workflows;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.ViewModels.Popups
{
    public class RegistrationViewModel : ValidatingWorkflowStepViewModel<Project>
    {
       
        public RegistrationViewModel()
        {
            // used by Caliburn Micro for design time    
        }

        public RegistrationViewModel(INavigationService navigationService,
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

        
       
        private Project _project;


        public Project Project
        {
            get => _project;
            private init => Set(ref _project, value);
        }

        private string _licenseKey;
        public string LicenseKey
        {
            get => _licenseKey;
            set
            {
                Set(ref _licenseKey, value);
                //ProjectManager.CurrentDashboardProject.LicenseKey = value;
                //Project.LicenseKey = value;
                ValidationResult = Validator.Validate(Project);
                CanMoveForwards = ValidationResult.IsValid;
                CanMoveBackwards = ValidationResult.IsValid;
                NotifyOfPropertyChange(nameof(Project));

            }
        }

     

        protected override ValidationResult Validate()
        {
            return (!string.IsNullOrEmpty(LicenseKey)) ? Validator.Validate(Project) : null;
        }

        //protected override Task OnActivateAsync(CancellationToken cancellationToken)
        //{
        //    //ShowWorkflowButtons();
        //    return base.OnActivateAsync(cancellationToken);
        //}
    }
}
