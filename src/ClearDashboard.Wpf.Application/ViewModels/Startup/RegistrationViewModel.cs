using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using LicenseUser = ClearDashboard.Wpf.Application.Models.LicenseUser;

namespace ClearDashboard.Wpf.Application.ViewModels.Startup
{
    public class RegistrationViewModel : ValidatingWorkflowStepViewModel<LicenseUser>
    {
        #region Member Variables
        private RegistrationDialogViewModel _parent;
        #endregion

        #region Observable Objects
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
        #endregion

        #region Constructor
        public RegistrationViewModel(
            INavigationService navigationService,
            ILogger<RegistrationViewModel> logger,
            IEventAggregator? eventAggregator,
            IMediator? mediator,
            ILifetimeScope? lifetimeScope,
            IValidator<LicenseUser> licenseValidator)
        : base(navigationService, logger, eventAggregator, mediator, lifetimeScope, licenseValidator)
        {
            LicenseUser = new LicenseUser();
        }

        protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            _parent = this.Parent as RegistrationDialogViewModel;
            return base.OnInitializeAsync(cancellationToken);
        }
        #endregion Constructor

        #region Methods
        protected override ValidationResult Validate()
        {

            var ValidationResult = Validator.Validate(LicenseUser);
            if (ValidationResult != null && _parent != null)
            {
                _parent.CanRegister = ValidationResult.IsValid;
            }
            return ValidationResult;

        }
        #endregion  Methods



    }

}
