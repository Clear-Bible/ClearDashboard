using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.ViewModels.Workflows;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using LicenseUser = ClearDashboard.Wpf.Models.LicenseUser;

namespace ClearDashboard.Wpf.ViewModels.Popups
{
    public class RegistrationViewModel : ValidatingWorkflowStepViewModel<LicenseUser>
    {
        private RegistrationDialogViewModel _parent;
        public RegistrationViewModel()
        {
            // used by Caliburn Micro for design time    
        }

        public RegistrationViewModel(INavigationService navigationService,
            ILogger<WorkSpaceViewModel> logger,
            DashboardProjectManager projectManager, 
            IEventAggregator eventAggregator,
            IValidator<LicenseUser> licenseValidator)                                               
            : base(eventAggregator, navigationService, logger, projectManager, licenseValidator)
        {
            LicenseUser = new LicenseUser();
        }

        protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            _parent = this.Parent as RegistrationDialogViewModel;
            return base.OnInitializeAsync(cancellationToken);
        }

        private LicenseUser _licenseUser;
        public LicenseUser LicenseUser
        {
            get { return _licenseUser; }
            set { _licenseUser = value; }
        }

        private string _licenseKey;
        public string LicenseKey
        {
            get { return _licenseKey; }
            set
            {
                Set(ref _licenseKey, value);
                LicenseUser.LicenseKey = value;
                ValidationResult = Validate();
                NotifyOfPropertyChange(nameof(LicenseUser));
                
            }
        }

        private string _firstName;
        public string FirstName
        {
            get { return _firstName; }
            set
            {
                Set(ref _firstName, value);
                LicenseUser.FirstName = value;
                ValidationResult = Validate();
                NotifyOfPropertyChange(nameof(LicenseUser));
            }
        }

        private string _lastName;
        public string LastName
        {
            get { return _lastName; }
            set
            {
                Set(ref _lastName, value);
                LicenseUser.LastName = value;
                ValidationResult = Validate();
                NotifyOfPropertyChange(nameof(LicenseUser));
            }
        }

        protected override ValidationResult Validate()
        {
            
            var ValidationResult = Validator.Validate(LicenseUser);
            if (ValidationResult != null && _parent != null)
            {
                _parent.CanRegister = ValidationResult.IsValid;
            }
            return ValidationResult;
           
        }
    }
}
