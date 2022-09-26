using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Wpf.Application.Helpers;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

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
            if (ValidationResult != null)
            {
                CanRegister = ValidationResult.IsValid;
            }
            return ValidationResult;

        }

        public bool CanCancel => true /* can always cancel */;

        private bool _canRegister;
        public bool CanRegister
        {
            get => _canRegister;
            set => Set(ref _canRegister, value);
        }

        public void Cancel()
        {
            System.Windows.Application.Current.Shutdown();
        }

        public async void Register()
        {
            try
            {
                var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                File.Delete(Path.Combine(documentsPath, "ClearDashboard_Projects", "license.txt"));

                var decryptedLicenseKey = LicenseManager.DecryptFromString(LicenseKey);
                var decryptedLicenseUser = LicenseManager.DecryptedJsonToLicenseUser(decryptedLicenseKey);

                LicenseUser givenLicenseUser = new LicenseUser();
                givenLicenseUser.FirstName = FirstName;//_registrationViewModel.FirstName;
                givenLicenseUser.LastName = LastName;//_registrationViewModel.LastName;
                ////givenLicenseUser.LicenseKey = _registrationViewModel.LicenseKey; <-- not the same thing right now.  One is the code that gets decrypted, the other is a Guid

                bool match = LicenseManager.CompareGivenUserAndDecryptedUser(givenLicenseUser, decryptedLicenseUser);
                if (match)
                {
                    File.WriteAllText(Path.Combine(documentsPath, "ClearDashboard_Projects", "license.txt"), LicenseKey);
                    await MoveForwards();
                    //await TryCloseAsync(true);
                }
                else
                {
                    MessageBox.Show(LocalizationStrings.Get("RegistrationDialogViewModel_MismatchCatch", Logger));
                }
            }

            catch (Exception)
            {
                MessageBox.Show(LocalizationStrings.Get("RegistrationDialogViewModel_FaultyKey", Logger));
            }
        }
        #endregion  Methods



    }

}
