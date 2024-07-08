using Autofac;
using Caliburn.Micro;
using ClearDashboard.Collaboration.Services;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Models.LicenseGenerator;
using ClearDashboard.DataAccessLayer.Paratext;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Models.HttpClientFactory;
using ClearDashboard.Wpf.Application.Properties;
using ClearDashboard.Wpf.Application.Services;
using MailKit.Net.Smtp;
using MediatR;
using Microsoft.Extensions.Logging;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ClearDashboard.Wpf.Application.ViewModels.PopUps
{
    public class NewCollabUserViewModel : DashboardApplicationScreen
    {
        #region Member Variables

        private readonly ILogger<AboutViewModel> _logger;
        private readonly DashboardProjectManager? _projectManager;
        private readonly ILocalizationService _localizationService;
        private readonly GitLabHttpClientServices _gitLabHttpClientServices;
        private readonly CollaborationServerHttpClientServices _collaborationHttpClientServices;
        private readonly CollaborationManager _collaborationManager;
        private CollaborationConfiguration _collaborationConfiguration;
        private DashboardUser _dashboardUser;
        private User _licenseUser;
        private readonly ParatextProxy _paratextProxy;

        private List<string> _emailValidationStringList = new List<string>();

        private RegistrationData _registration;


        #endregion //Member Variables


        #region Public Properties

        #endregion //Public Properties


        #region Observable Properties

        private List<GitLabGroup> _groups;
        public List<GitLabGroup> Groups
        {
            get => _groups;
            set
            {
                _groups = value;
                NotifyOfPropertyChange(() => Groups);
            }
        }

        private GitLabGroup _selectedGroup;
        public GitLabGroup SelectedGroup
        {
            get => _selectedGroup;
            set
            {
                _selectedGroup = value;
                NotifyOfPropertyChange(() => SelectedGroup);
                CheckEntryFields();
            }
        }

        private string _firstName = string.Empty;
        public string FirstName
        {
            get => _firstName;
            set
            {
                _firstName = value.Trim();
                NotifyOfPropertyChange(() => FirstName);
                CheckEntryFields();
            }
        }

        private string _lastName = string.Empty;
        public string LastName
        {
            get => _lastName;
            set
            {
                _lastName = value.Trim();
                NotifyOfPropertyChange(() => LastName);
                CheckEntryFields();
            }
        }

        private string _email = string.Empty;
        public string Email
        {
            get => _email;
            set
            {
                _email = value.Trim();
                NotifyOfPropertyChange(() => Email);
                CheckEntryFields();
            }
        }

        private bool _selectedGroupEnabled = true;
        public bool SelectedGroupEnabled
        {
            get => _selectedGroupEnabled;
            set
            {
                _selectedGroupEnabled = value;
                NotifyOfPropertyChange(() => SelectedGroupEnabled);
            }
        }

        private bool _firstNameEnabled = true;
        public bool FirstNameEnabled
        {
            get => _firstNameEnabled;
            set
            {
                _firstNameEnabled = value;
                NotifyOfPropertyChange(() => FirstNameEnabled);
            }
        }

        private bool _lastNameEnabled = true;
        public bool LastNameEnabled
        {
            get => _lastNameEnabled;
            set
            {
                _lastNameEnabled = value;
                NotifyOfPropertyChange(() => LastNameEnabled);
            }
        }

        private bool _emailEnabled = true;
        public bool EmailEnabled
        {
            get => _emailEnabled;
            set
            {
                _emailEnabled = value;
                NotifyOfPropertyChange(() => EmailEnabled);
            }
        }

        private CollaborationConfiguration _collaborationConfig = new();
        public CollaborationConfiguration CollaborationConfig
        {
            get => _collaborationConfig;
            set
            {
                _collaborationConfig = value;
                NotifyOfPropertyChange(() => CollaborationConfig);
            }
        }

        private string _emailCode = "";
        public string EmailCode
        {
            get => _emailCode;
            set
            {
                _emailCode = value.Trim();
                NotifyOfPropertyChange(() => EmailCode);
            }
        }

        private bool _badEmailValidationCode = false;
        public bool BadEmailValidationCode
        {
            get => _badEmailValidationCode;
            set
            {
                _badEmailValidationCode = value;
                NotifyOfPropertyChange(() => BadEmailValidationCode);
            }
        }

        private bool _emailSendError = false;
        public bool EmailSendError
        {
            get => _emailSendError;
            set
            {
                _emailSendError = value;
                NotifyOfPropertyChange(() => EmailSendError);
            }
        }

        private bool _emailSent;
        public bool EmailSent
        {
            get => _emailSent;
            set
            {
                _emailSent = value;
                NotifyOfPropertyChange(() => EmailSent);
            }
        }

        private bool _showCheckUserButtonEnabled;
        public bool ShowCheckUserButtonEnabled
        {
            get => _showCheckUserButtonEnabled;
            set
            {
                _showCheckUserButtonEnabled = value;
                NotifyOfPropertyChange(() => ShowCheckUserButtonEnabled);
            }
        }

        private bool _showValidateEmailButtonEnabled;
        public bool ShowValidateEmailButtonEnabled
        {
            get => _showValidateEmailButtonEnabled;
            set
            {
                _showValidateEmailButtonEnabled = value;
                NotifyOfPropertyChange(() => ShowValidateEmailButtonEnabled);
            }
        }

        private bool _showGenerateUserButtonEnabled;
        public bool ShowGenerateUserButtonEnabled
        {
            get => _showGenerateUserButtonEnabled;
            set
            {
                _showGenerateUserButtonEnabled = value;
                NotifyOfPropertyChange(() => ShowGenerateUserButtonEnabled);
            }
        }

        private string _errorMessage = "";
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                NotifyOfPropertyChange(() => ErrorMessage);
            }
        }


        private string _saveGitLabUserMessage;
        public string SaveGitLabUserMessage
        {
            get => _saveGitLabUserMessage;
            set
            {
                _saveGitLabUserMessage = value;
                NotifyOfPropertyChange(() => SaveGitLabUserMessage);
            }
        }



        private Brush _saveMessageForegroundColor = Brushes.Green;
        public Brush SaveMessageForegroundColor
        {
            get => _saveMessageForegroundColor;
            set
            {
                _saveMessageForegroundColor = value;
                NotifyOfPropertyChange(() => SaveMessageForegroundColor);
            }
        }

        private Visibility _closeVisibility = Visibility.Hidden;
        public Visibility CloseVisibility
        {
            get => _closeVisibility;
            set
            {
                _closeVisibility = value;
                NotifyOfPropertyChange(() => CloseVisibility);
            }
        }


        #endregion //Observable Properties


        #region Constructor

        public NewCollabUserViewModel()
        {
            // no-op used by caliburn micro XAML
        }


        public NewCollabUserViewModel(INavigationService navigationService,
            ILogger<AboutViewModel> logger,
            DashboardProjectManager? projectManager,
            IEventAggregator eventAggregator,
            IMediator mediator,
            ILifetimeScope? lifetimeScope,
            ILocalizationService localizationService,
            GitLabHttpClientServices gitLabHttpClientServices,
            CollaborationServerHttpClientServices collaborationHttpClientServices,
            CollaborationManager collaborationManager,
            CollaborationConfiguration collaborationConfiguration,
            ParatextProxy paratextProxy)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
        {
            _logger = logger;
            _projectManager = projectManager;
            _localizationService = localizationService;
            _gitLabHttpClientServices = gitLabHttpClientServices;
            _collaborationHttpClientServices = collaborationHttpClientServices;
            _collaborationManager = collaborationManager;
            _collaborationConfiguration = collaborationConfiguration;
            _paratextProxy=paratextProxy;
        }

        protected override async void OnViewLoaded(object view)
        {
            if (_projectManager.CurrentUser is not null)
            {
                FirstName = _projectManager.CurrentUser.FirstName ?? string.Empty;
                LastName = _projectManager.CurrentUser.LastName ?? string.Empty;

                if (FirstName != string.Empty)
                {
                    FirstNameEnabled = false;
                }
                if (LastName != string.Empty)
                {
                    LastNameEnabled = false;
                }
            }

            _licenseUser = LicenseManager.GetUserFromLicense();
            _dashboardUser = await _collaborationHttpClientServices.GetDashboardUserExistsById(_licenseUser.Id);
            _registration = _paratextProxy.GetParatextRegistrationData();
            if (!string.IsNullOrWhiteSpace(_dashboardUser.Email))
            {
                Email = _dashboardUser.Email ?? string.Empty;
                EmailEnabled = false;
            }
            else
            {
                Email = _registration.Email;
            }

            if (InternetAvailability.IsInternetAvailable())
            {
                Groups = await _gitLabHttpClientServices.GetAllGroups();
            }

            bool orgFound = false;
            if (_dashboardUser.Organization != null)
            {
                foreach (var group in Groups)
                {
                    if (group.Name == _dashboardUser.Organization)
                    {
                        SelectedGroup = group;
                        SelectedGroupEnabled = false;
                        orgFound = true;
                        break;
                    }
                }
            }
            if (!orgFound)
            {
                var orgName = _paratextProxy.GetOrganizationNameFromEmail();
                foreach (var group in Groups)
                {
                    if (group.Name ==orgName)
                    {
                        SelectedGroup = group;
                        orgFound = true;
                        break;
                    }
                }
            }
            if (!orgFound)
            {
                foreach (var group in Groups)
                {
                    if (group.Name == _registration.SupporterName ||
                        group.Description == _registration.SupporterName)
                    {
                        SelectedGroup = group;
                        break;
                    }
                }
            }

            base.OnViewLoaded(view);
        }

        #endregion //Constructor


        #region Methods
        /// <summary>
        /// Function to generate a user name from first & lastnames
        /// </summary>
        /// <returns></returns>
        private string GetUserName() => (Regex.Replace(FirstName, @"\s|\p{P}|[']", "") + "." + Regex.Replace(LastName, @"\s|\p{P}|[']", "")).ToLower();



        public async void Close()
        {
            await TryCloseAsync();
        }


        private void CheckEntryFields()
        {
            ErrorMessage = "";

            // from https://uibakery.io/regex-library/email-regex-csharp
            Regex validateEmailRegex = new Regex("(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|\"(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21\\x23-\\x5b\\x5d-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])*\")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21-\\x5a\\x53-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])+)\\])");
            var isMatch = validateEmailRegex.IsMatch(Email!.Trim());
            if (isMatch == false)
            {
                ShowCheckUserButtonEnabled = false;
                return;
            }

            if (FirstName == string.Empty || LastName == string.Empty || SelectedGroup is null)
            {
                ShowCheckUserButtonEnabled = false;
                return;
            }

            ShowCheckUserButtonEnabled = true;
        }


        /// <summary>
        /// checks to see if the user is already on the system and sends out a validation email
        /// </summary>
        public async void CheckUser()
        {
            var username = GetUserName();
            var isAlreadyOnGitLab = await _gitLabHttpClientServices.CheckForExistingUser(username, Email);

            // not a user
            if (isAlreadyOnGitLab == false)
            {
                _emailValidationStringList.Add(GenerateRandomPassword.RandomNumber(1000, 9999).ToString());


                var mailMessage = new MimeMessage();
                mailMessage.From.Add(new MailboxAddress("cleardas@cleardashboard.org", "cleardas@cleardashboard.org"));
                mailMessage.To.Add(new MailboxAddress(FirstName + " " + LastName, Email));
                mailMessage.Subject = _localizationService["NewCollabUserView_DashboardEmailValidationCode"]; //"ClearDashboard Email Validation Code";
                mailMessage.Body = new TextPart("plain")
                {
                    Text = _localizationService["NewCollabUserView_EmailValidationCode"] + " " + _emailValidationStringList.LastOrDefault() //Email Verification Code: 
                };

                try
                {
                    using var smtpClient = new SmtpClient();
                    await smtpClient.ConnectAsync("mail.cleardashboard.org", 465, true);

                    var userName = Encryption.Decrypt(Settings.Default.EmailUser);
                    var pass = Encryption.Decrypt(Settings.Default.EmailPass);

                    await smtpClient.AuthenticateAsync(userName, pass);
                    await smtpClient.SendAsync(mailMessage);
                    await smtpClient.DisconnectAsync(true);

                    ShowValidateEmailButtonEnabled = true;
                }
                catch (Exception e)
                {
                    _logger.LogError("Email Sending Error", e);
                    EmailSendError = true;
                }

                EmailSent = true;
                ErrorMessage = "";
            }
            else
            {
                ErrorMessage = _localizationService["NewCollabUserView_UserAlreadyExists"]; //"User is already on the system!";
            }


        }

        public void GroupSelected()
        {
            // for caliburn
        }


        /// <summary>
        /// Creates the User on the GitLab Server
        /// </summary>
        public async void CreateGitLabUser()
        {
            var password = GenerateRandomPassword.RandomPassword(16);

            GitLabUser user = await _gitLabHttpClientServices.CreateNewUser(FirstName, LastName, GetUserName(), password,
                Email, SelectedGroup.Name);

            if (user.Id == 0)
            {
                ErrorMessage = _localizationService["NewCollabUserView_ErrorCreatingUser"]; //"Error Creating user on Server";

                CollaborationConfig = new();
            }
            else
            {
                var accessToken = await _gitLabHttpClientServices.GeneratePersonalAccessToken(user);

                CollaborationConfig = new CollaborationConfiguration
                {
                    Group = SelectedGroup.Name,
                    RemoteEmail = Email,
                    RemotePersonalAccessToken = accessToken.Token,
                    RemotePersonalPassword = password,
                    RemoteUrl = "",
                    RemoteUserName = user.UserName,
                    UserId = user.Id,
                    NamespaceId = user.NamespaceId,
                    TokenId = accessToken.Id
                };

                _collaborationConfiguration = CollaborationConfig;
                _collaborationManager.SaveCollaborationLicense(_collaborationConfiguration);

                user.Password = password;

                var results = await _collaborationHttpClientServices.CreateNewCollabUser(user, accessToken.Token);

                if (results)
                {
                    SaveGitLabUserMessage = _localizationService["NewCollabUserView_SavedToRemoteServer"]; //Saved to remote server
                    SaveMessageForegroundColor = Brushes.Green;
                }
                else
                {
                    SaveGitLabUserMessage = _localizationService["NewCollabUserView_UserAlreadyExists"]; //User already exists on server
                    SaveMessageForegroundColor = Brushes.Red;
                }

                if (_dashboardUser.GitLabUserId == 0)
                {
                    await CreateDashboardUser();
                }
            }

            ShowGenerateUserButtonEnabled = false;

            CloseVisibility = Visibility.Visible;
        }

        public async Task CreateDashboardUser()
        {
            if (_dashboardUser.Id == Guid.Empty) //make a DashboardUser
            {
                var newDashboardUser = new DashboardUser(
                    _licenseUser,
                    Email,
                    LicenseManager.GetLicenseFromFile(LicenseManager.LicenseFilePath),
                    SelectedGroup.Name,
                    CollaborationConfig.UserId,
                    Assembly.GetEntryAssembly()?.GetName().Version?.ToString(),
                    DateTime.Today.Date,
                    _registration.Name);

                await _collaborationHttpClientServices.CreateNewDashboardUser(newDashboardUser);
            }
            else //update a DashboardUser
            {
                _dashboardUser.ParatextUserName = _registration.Name;
                _dashboardUser.Organization = SelectedGroup.Name;
                _dashboardUser.GitLabUserId = CollaborationConfig.UserId;
                _dashboardUser.AppVersionNumber = Assembly.GetEntryAssembly()?.GetName().Version?.ToString();
                _dashboardUser.AppLastDate = DateTime.Today.Date;
                
                await _collaborationHttpClientServices.UpdateDashboardUser(_dashboardUser);
            }
        }


        /// <summary>
        /// checks to see if email validation code sent through the email
        /// is the same as the one the user inputted
        /// </summary>
        public void ValidateEmailCode()
        {
            if (_emailValidationStringList.Contains(EmailCode))
            {
                BadEmailValidationCode = true;
                ShowGenerateUserButtonEnabled = true;
            }
            else
            {
                BadEmailValidationCode = false;
            }
        }

        protected override void Dispose(bool disposing)
        {
            System.Windows.Application.Current.Shutdown();
            
            base.Dispose(disposing);
        }

        #endregion // Methods

    }
}
