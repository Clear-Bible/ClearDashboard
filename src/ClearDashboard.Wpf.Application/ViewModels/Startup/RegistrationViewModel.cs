using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Models.LicenseGenerator;
using ClearDashboard.Wpf.Application.Messages;
using ClearDashboard.Wpf.Application.Services;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.Collaboration.Services;

namespace ClearDashboard.Wpf.Application.ViewModels.Startup
{
    public class RegistrationViewModel : ValidatingWorkflowStepViewModel<DashboardUser>
    {
        private readonly DashboardProjectManager _dashboardProjectManager;
        private readonly ILocalizationService _localizationService;
        private readonly CollaborationManager _collaborationManager;

        #region Member Variables
        private RegistrationDialogViewModel _parent;
        #endregion

        #region Observable Objects
        private DashboardUser _licenseUser;
        public DashboardUser LicenseUser
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

        private string _matchType;
        public string MatchType
        {
            get { return _matchType; }
            set => Set(ref _matchType, value);
        }

        
        #endregion

        #region Constructor
        public RegistrationViewModel( DashboardProjectManager dashboardProjectManager,
            INavigationService navigationService,
            ILogger<RegistrationViewModel> logger,
            IEventAggregator? eventAggregator,
            IMediator? mediator,
            ILifetimeScope? lifetimeScope,
            IValidator<DashboardUser> licenseValidator,
            ILocalizationService localizationService,
            CollaborationManager collaborationManager)
        : base(navigationService, logger, eventAggregator, mediator, lifetimeScope, licenseValidator)
        {
            _dashboardProjectManager = dashboardProjectManager;
            _localizationService = localizationService;
            LicenseUser = new DashboardUser();
            _collaborationManager = collaborationManager;
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
                if (File.Exists(LicenseManager.LicenseFilePath))
                {
                    File.Delete(LicenseManager.LicenseFilePath);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Deleting the LicenseFilePath failed: "+ex);
            }

            //parsing LicenseKey
            var licenseArray = LicenseKey.Split('^');

            var encryptedLicense = licenseArray.FirstOrDefault();
            string encryptedCollabConfig;
            if (licenseArray.Length > 1)
            {
                encryptedCollabConfig = licenseArray.LastOrDefault();

                var collaborationConfig = LicenseManager.DecryptCollabToConfiguration(encryptedCollabConfig);

                _collaborationManager.SaveCollaborationLicense(collaborationConfig);

                await EventAggregator.PublishOnBackgroundThreadAsync(new RebuildMainMenuMessage());
            }
            

            string decryptedLicenseKey = string.Empty;
            try
            {
                Logger.LogInformation("LicenseKey is: "+encryptedLicense);
                decryptedLicenseKey = LicenseManager.DecryptLicenseFromString(encryptedLicense);
            }
            catch (Exception ex)
            {
                Logger.LogError("DecryptLicenseFromString failed: "+ex);
            }
            Logger.LogInformation("decryptedLicenseKey is: "+decryptedLicenseKey);

            User decryptedLicenseUser = new User();
            try
            {
                decryptedLicenseUser = LicenseManager.DecryptedJsonToUser(decryptedLicenseKey);
            }
            catch (Exception ex)
            {
                Logger.LogError("DecryptJsonToUser failed: "+ex);
            }
            Logger.LogInformation("decryptedLicenseUser is: "+decryptedLicenseUser);

            try
            {
                if (decryptedLicenseUser.Id == Guid.Empty)
                {
                    Logger.LogError("decryptedLicenseUser.Id is equal to Guid.Empty");
                    throw new Exception("License has empty guid.");
                    
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Evaluating decryptedLicenseUser.Id failed: "+ex);
            }

            Logger.LogInformation("FirstName is: "+FirstName);
            Logger.LogInformation("LastName is: "+LastName);
            User givenLicenseUser = new User();
            try
            {
                givenLicenseUser = new User
                {
                    FirstName = FirstName, //_registrationViewModel.FirstName;
                    LastName = LastName //_registrationViewModel.LastName;
                };
                ////givenLicenseUser.LicenseKey = _registrationViewModel.LicenseKey; <-- not the same thing right now.  One is the code that gets decrypted, the other is a Guid

            }
            catch (Exception ex)
            {
                Logger.LogError("givenLicenseUser failed to set: "+ex);
            }
            Logger.LogInformation("givenLicenseUser is: "+givenLicenseUser);

            LicenseUserMatchType match = LicenseUserMatchType.Error;
            try
            {
                match = LicenseManager.CompareGivenUserAndDecryptedUser(givenLicenseUser, decryptedLicenseUser);
            }
            catch (Exception ex)
            {
                Logger.LogError("CompareGivenUserAndDecryptedUser failed: "+ex);
            }
            Logger.LogInformation("match is: "+match);

            try
            {
                switch (match)
                {
                    case LicenseUserMatchType.Match:
                        if (!Directory.Exists(LicenseManager.LicenseFilePath))
                        {
                            Directory.CreateDirectory(LicenseManager.LicenseFolderPath);
                        }
                        File.WriteAllText(LicenseManager.LicenseFilePath, encryptedLicense);
                        await MoveForwards();
                        await _dashboardProjectManager.UpdateCurrentUserWithParatextUserInformation();
                        break;
                    case LicenseUserMatchType.BothNameMismatch:
                        MatchType = "The license key does not match either name provided.";
                        break;
                    case LicenseUserMatchType.FirstNameMismatch:
                        MatchType = "Your first name does not match the license key.";
                        break;
                    case LicenseUserMatchType.LastNameMismatch:
                        MatchType = "Your last name does not match the license key.";
                        break;
                    case LicenseUserMatchType.Error:
                        MatchType = "There is an unknown issue with your license key.";
                        break;
                    default:
                        MatchType = "License key comparison is null.";
                        break;
                }

                
            }
            catch (Exception ex)
            {
                Logger.LogError("LicenseUserMatchType switch statement failed: "+ex);
            }
            Logger.LogInformation("MatchType is: "+MatchType);
        }
        #endregion  Methods



    }

}
