using Autofac;
using Caliburn.Micro;
using ClearDashboard.Collaboration.Services;
using ClearDashboard.DAL.Alignment.Features.Denormalization;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Models.HttpClientFactory;
using ClearDashboard.Wpf.Application.Properties;
using ClearDashboard.Wpf.Application.Services;
using MailKit.Net.Smtp;
using MediatR;
using Microsoft.Extensions.Logging;
using MimeKit;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace ClearDashboard.Wpf.Application.ViewModels.PopUps
{
    public class NewCollabUserViewModel : DashboardApplicationScreen
    {
        #region Member Variables

        private readonly ILogger<AboutViewModel> _logger;
        private readonly DashboardProjectManager? _projectManager;
        private readonly HttpClientServices _httpClientServices;
        private readonly CollaborationHttpClientServices _collaborationHttpClientServices;
        private readonly CollaborationManager _collaborationManager;
        private CollaborationConfiguration _collaborationConfiguration;

        private string _emailValidationString = "";


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
            HttpClientServices httpClientServices,
            CollaborationHttpClientServices collaborationHttpClientServices,
            CollaborationManager collaborationManager,
            CollaborationConfiguration collaborationConfiguration)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
        {
            _logger = logger;
            _projectManager = projectManager;
            _httpClientServices = httpClientServices;
            _collaborationHttpClientServices = collaborationHttpClientServices;
            _collaborationManager = collaborationManager;
            _collaborationConfiguration = collaborationConfiguration;
        }

        protected override async void OnViewLoaded(object view)
        {
            if (_projectManager.CurrentUser is not null)
            {
                FirstName = _projectManager.CurrentUser.FirstName ?? string.Empty;
                LastName = _projectManager.CurrentUser.LastName ?? string.Empty;
            }

            if (InternetAvailability.IsInternetAvailable())
            {
                Groups = await _httpClientServices.GetAllGroups();
            }

            base.OnViewLoaded(view);
        }

        #endregion //Constructor


        #region Methods

        /// <summary>
        /// Function to generate a user name from first & lastnames
        /// </summary>
        /// <returns></returns>
        private string GetUserName() => (FirstName + "." + LastName).ToLower();



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
            var isAlreadyOnGitLab = await _httpClientServices.CheckForExistingUser(username, Email);

            // not a user
            if (isAlreadyOnGitLab == false)
            {
                _emailValidationString = GenerateRandomPassword.RandomNumber(1000, 9999).ToString();


                var mailMessage = new MimeMessage();
                mailMessage.From.Add(new MailboxAddress("cleardas@cleardashboard.org", "cleardas@cleardashboard.org"));
                mailMessage.To.Add(new MailboxAddress(FirstName + " " + LastName, Email));
                mailMessage.Subject = LocalizationStrings.Get("NewCollabUserView_DashboardEmailValidationCode", _logger); //"ClearDashboard Email Validation Code";
                mailMessage.Body = new TextPart("plain")
                {
                    Text = LocalizationStrings.Get("NewCollabUserView_EmailValidationCode", _logger) + " " + _emailValidationString //Email Verification Code: 
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
                ErrorMessage = LocalizationStrings.Get("NewCollabUserView_UserOnSystem", _logger); //"User is already on the system!";
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

            GitLabUser user = await _httpClientServices.CreateNewUser(FirstName, LastName, GetUserName(), password,
                Email, SelectedGroup.Name).ConfigureAwait(false);

            if (user.Id == 0)
            {
                ErrorMessage = LocalizationStrings.Get("NewCollabUserView_ErrorCreatingUser", _logger); //"Error Creating user on Server";

                CollaborationConfig = new();
            }
            else
            {
                var accessToken = await _httpClientServices.GeneratePersonalAccessToken(user).ConfigureAwait(false);

                CollaborationConfig = new CollaborationConfiguration
                {
                    Group = SelectedGroup.Name,
                    RemoteEmail = Email,
                    RemotePersonalAccessToken = accessToken,
                    RemotePersonalPassword = password,
                    RemoteUrl = "",
                    RemoteUserName = user.UserName,
                    UserId = user.Id,
                    NamespaceId = user.NamespaceId,
                };

                _collaborationConfiguration = CollaborationConfig;
                _collaborationManager.SaveCollaborationLicense(_collaborationConfiguration);

                user.Password = password;

                var results = await _collaborationHttpClientServices.CreateNewUser(user, accessToken).ConfigureAwait(false);

                if (results)
                {
                    SaveGitLabUserMessage = LocalizationStrings.Get("NewCollabUserView_SavedToRemoteServer", _logger); //Saved to remote server
                    SaveMessageForegroundColor = Brushes.Green;
                }
                else
                {
                    SaveGitLabUserMessage = LocalizationStrings.Get("NewCollabUserView_UserAlreadyExists", _logger); //User already exists on server
                    SaveMessageForegroundColor = Brushes.Red;
                }
            }

            ShowGenerateUserButtonEnabled = false;
        }


        /// <summary>
        /// checks to see if email validation code sent through the email
        /// is the same as the one the user inputted
        /// </summary>
        public void ValidateEmailCode()
        {
            if (EmailCode == _emailValidationString)
            {
                BadEmailValidationCode = true;
                ShowGenerateUserButtonEnabled = true;
            }
            else
            {
                BadEmailValidationCode = false;
            }
        }


        #endregion // Methods

    }
}
