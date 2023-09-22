using Autofac;
using Caliburn.Micro;
using ClearDashboard.Collaboration.Services;
using ClearDashboard.DAL.Alignment.Features.Denormalization;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Messages;
using ClearDashboard.Wpf.Application.Models.HttpClientFactory;
using ClearDashboard.Wpf.Application.Properties;
using ClearDashboard.Wpf.Application.Services;
using MailKit.Net.Smtp;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using MimeKit;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ClearDashboard.Wpf.Application.ViewModels.Main;

namespace ClearDashboard.Wpf.Application.ViewModels.DashboardSettings
{
    public class DashboardSettingsViewModel : DashboardApplicationScreen
    {

        #region Member Variables

        private readonly IEventAggregator _eventAggregator;
        private readonly CollaborationServerHttpClientServices _collaborationHttpClientServices;
        private readonly CollaborationManager _collaborationManager;
        private readonly ILogger<DashboardSettingsViewModel> _logger;
        private bool _isAquaEnabledOnStartup;
        private string _emailValidationString = string.Empty;

        #endregion //Member Variables


        #region Public Properties


        #endregion //Public Properties


        #region Observable Properties

        private bool _isPowerModesEnabled = true;
        public bool IsPowerModesEnabled
        {
            get => _isPowerModesEnabled;
            set
            {
                _isPowerModesEnabled = value; 
                NotifyOfPropertyChange(() => IsPowerModesEnabled);
            }
        }


        // controls the group box IsEnabled
        private bool _isPowerModesBoxEnabled = true;
        public bool IsPowerModesBoxEnabled
        {
            get => _isPowerModesBoxEnabled;
            set
            {
                _isPowerModesBoxEnabled = value; 
                NotifyOfPropertyChange(() => IsPowerModesBoxEnabled);
            }
        }

        private bool _isAquaEnabled;
        public bool IsAquaEnabled
        {
            get => _isAquaEnabled;
            set
            {
                _isAquaEnabled = value;
                NotifyOfPropertyChange(() => IsAquaEnabled);
            }
        }


        private bool _isAlignmentEditingEnabled;
        public bool IsAlignmentEditingEnabled
        {
            get => _isAlignmentEditingEnabled;
            set
            {
                _isAlignmentEditingEnabled = value;
                NotifyOfPropertyChange(() => IsAlignmentEditingEnabled);
            }
        }


        private bool _runAquaInstall;
        public bool RunAquaInstall
        {
            get => _runAquaInstall;
            set
            {
                _runAquaInstall = value;
                NotifyOfPropertyChange(() => RunAquaInstall);
            }
        }

        private bool _isVerseByVerseTextCollectionsEnabled;
        public bool IsVerseByVerseTextCollectionsEnabled
        {
            get => _isVerseByVerseTextCollectionsEnabled;
            set
            {
                _isVerseByVerseTextCollectionsEnabled = value;
                NotifyOfPropertyChange(() => IsVerseByVerseTextCollectionsEnabled);
            }
        }

        private string _gitRootUrl;
        public string GitRootUrl
        {
            get => _gitRootUrl;
            set
            {
                _gitRootUrl = value;
                SaveGitlabUrlButtonEnabled = true;
                NotifyOfPropertyChange(() => GitRootUrl);
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

        private Visibility _gitlabUserSaveVisibility = Visibility.Visible;
        public Visibility GitlabUserSaveVisibility
        {
            get => _gitlabUserSaveVisibility;
            set
            {
                _gitlabUserSaveVisibility = value; 
                NotifyOfPropertyChange(() => GitlabUserSaveVisibility);
            }
        }

        private string _saveGitLabUserMessage;
        public string SaveGitLabUserMessage
        {
            get => _saveGitLabUserMessage;
            set
            {
                _saveGitLabUserMessage = value; 
                NotifyOfPropertyChange(() =>SaveGitLabUserMessage);
            }
        }

        private bool _saveGitlabUrlButtonEnabled;
        public bool SaveGitlabUrlButtonEnabled
        {
            get => _saveGitlabUrlButtonEnabled;
            set
            {
                _saveGitlabUrlButtonEnabled = value; 
                NotifyOfPropertyChange(() =>SaveGitlabUrlButtonEnabled);
            }
        }

        private bool _gitLabUserFound;
        public bool GitLabUserFound
        {
            get => _gitLabUserFound;
            set
            {
                _gitLabUserFound = value;
                NotifyOfPropertyChange(() => GitLabUserFound);
            }
        }

        private bool _restoreButtonEnabled;
        public bool RestoreButtonEnabled
        {
            get => _restoreButtonEnabled;
            set
            {
                _restoreButtonEnabled = value;
                NotifyOfPropertyChange(() => RestoreButtonEnabled);
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

        private string _email = string.Empty;
        public string Email
        {
            get => _email;
            set
            {
                _email = value.Trim();
                NotifyOfPropertyChange(() => Email);
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


        private Visibility _showExistingCollabUser;
        public Visibility ShowExistingCollabUser
        {
            get => _showExistingCollabUser;
            set
            {
                _showExistingCollabUser = value;
                NotifyOfPropertyChange(() => ShowExistingCollabUser);
            }
        }

        private Visibility _hideExistingCollabUser;
        public Visibility HideExistingCollabUser
        {
            get => _hideExistingCollabUser;
            set
            {
                _hideExistingCollabUser = value;
                NotifyOfPropertyChange(() => HideExistingCollabUser);
            }
        }

        private string _emailMessage;
        public string EmailMessage
        {
            get => _emailMessage;
            set
            {
                _emailMessage = value; 
                NotifyOfPropertyChange(() => EmailMessage);
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


        private bool _differentMonitor;
        public bool DifferentMonitor 
        { 
            get => _differentMonitor; 
            set => Set(ref _differentMonitor, value);
        }


        private bool _thirdMonitor;
        public bool ThirdMonitor 
        { 
            get => _thirdMonitor; 
            set => Set(ref _thirdMonitor, value); 
        }

        #endregion //Observable Properties


        #region Constructor

        // ReSharper disable once EmptyConstructor
        public DashboardSettingsViewModel(
            CollaborationManager collaborationManager,
            DashboardProjectManager projectManager,
            INavigationService navigationService,
            ILogger<DashboardSettingsViewModel> logger,
            IEventAggregator eventAggregator,
            IMediator mediator,
            ILifetimeScope? lifetimeScope,
            CollaborationServerHttpClientServices collaborationHttpClientServices,
            ILocalizationService localizationService)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
        {
            // for Caliburn Micro
            //IoC.Get<ILogger<DashboardSettingsViewModel>>();
            _eventAggregator = eventAggregator;
            _collaborationHttpClientServices = collaborationHttpClientServices;
            _collaborationManager = collaborationManager;
            _logger = logger;
        }

        protected override void OnViewReady(object view)
        {
            // determine if this computer is a laptop or not
            SystemPowerModes systemPowerModes = new SystemPowerModes();
            var isLaptop = systemPowerModes.IsLaptop;
            IsPowerModesBoxEnabled = isLaptop;
            if (isLaptop == false)
            {
                Settings.Default.EnablePowerModes = false;
                Settings.Default.Save();
            }

            IsPowerModesEnabled = Settings.Default.EnablePowerModes;
            IsVerseByVerseTextCollectionsEnabled = Settings.Default.VerseByVerseTextCollectionsEnabled;

            var isEnabled = false;
            try
            {
                using RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"Software\ClearDashboard\AQUA");
                if (key != null)
                {
                    Object o = key.GetValue("IsEnabled")!;
                    if (o is not null)
                    {
                        if (o as string == "true")
                        {
                            isEnabled = true;
                        }
                    }
                }
            }
            catch (Exception)
            {
                _isAquaEnabledOnStartup = Settings.Default.IsAquaEnabled;
            }

            _isAquaEnabledOnStartup = isEnabled;
            IsAquaEnabled = _isAquaEnabledOnStartup;


            // load in Git URL
            GitRootUrl = AbstractionsSettingsHelper.GetGitUrl();
            SaveGitlabUrlButtonEnabled = false;

            // load in the collab user info
            CollaborationConfig = _collaborationManager.GetConfig();

            // load in the monitor settings
            DifferentMonitor = Settings.Default.DifferentMonitor;
            ThirdMonitor = Settings.Default.ThirdMonitor;

            IsAlignmentEditingEnabled = AbstractionsSettingsHelper.GetEnabledAlignmentEditing();


            base.OnViewReady(view);
        }

        protected override async void OnViewLoaded(object view)
        {
            var user = await _collaborationHttpClientServices.GetCollabUserExistsById(CollaborationConfig.UserId);

            if (user.UserId > 0)
            {
                // found user
                GitLabUserFound = true;
                GitlabUserSaveVisibility = Visibility.Collapsed;
                RestoreButtonEnabled = false;

                ShowExistingCollabUser = Visibility.Visible;
                HideExistingCollabUser = Visibility.Collapsed;
            }
            else
            {
                if (CollaborationConfig.UserId > 0)
                {
                    ShowExistingCollabUser = Visibility.Visible;
                    HideExistingCollabUser = Visibility.Collapsed;
                    GitLabUserFound = true;
                }
                else
                {
                    // no user on the server
                    GitLabUserFound = false;
                    GitlabUserSaveVisibility = Visibility.Visible;
                    RestoreButtonEnabled = true;

                    ShowExistingCollabUser = Visibility.Collapsed;
                    HideExistingCollabUser = Visibility.Visible;
                }

            }

            base.OnViewLoaded(view);
        }

        #endregion //Constructor


        #region Methods

        // ReSharper disable once UnusedMember.Global
        
        public void SaveMultiMonitorSettings()
        {
            Settings.Default.DifferentMonitor = DifferentMonitor;
            Settings.Default.ThirdMonitor = ThirdMonitor;
            Settings.Default.Save();
        }



        public void Close()
        {
            TryCloseAsync();
        }

        public void SaveGitUrl()
        {
            AbstractionsSettingsHelper.SaveGitUrl(GitRootUrl.Trim());
        }

        public async void SaveGitLabToServer()
        {
#pragma warning disable CA1416
            var user = new GitLabUser
            {
                Id = _collaborationConfig.UserId,
                UserName = _collaborationConfig.RemoteUserName!,
                Email = _collaborationConfig.RemoteEmail!,
                Password = _collaborationConfig.RemotePersonalPassword!,
                Organization = _collaborationConfig.Group!,
                NamespaceId = _collaborationConfig.NamespaceId
            };
#pragma warning restore CA1416

            var results = await _collaborationHttpClientServices.CreateNewCollabUser(user, _collaborationConfig.RemotePersonalAccessToken);

            if (results)
            {
                SaveGitLabUserMessage = LocalizationStrings.Get("Settings_SavedToRemoteServer", _logger); //"Saved to remote server";
            }
            else
            {
                SaveGitLabUserMessage = LocalizationStrings.Get("Settings_UserAlreadyExists", _logger); //"User already exists on server";
            }

            GitlabUserSaveVisibility = Visibility.Collapsed;
        }

        // ReSharper disable once UnusedMember.Global
        // ReSharper disable once UnusedParameter.Global
        public void PowerModeCheckBox(bool value)
        {
            Settings.Default.EnablePowerModes = IsPowerModesEnabled;
            Settings.Default.Save();
        }

        // ReSharper disable once UnusedMember.Global
        // ReSharper disable once UnusedParameter.Global
        public void AquaEnabledCheckBox(bool value)
        {
            Settings.Default.IsAquaEnabled = IsAquaEnabled;

            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\ClearDashboard\AQUA");

            key.SetValue("IsEnabled", IsAquaEnabled ? "true" : "false");

            if (_isAquaEnabledOnStartup == IsAquaEnabled)
            {
                RunAquaInstall = false;
                Settings.Default.RunAquaInstall = RunAquaInstall;
            }
            else
            {
                RunAquaInstall = true;
                Settings.Default.RunAquaInstall = RunAquaInstall;
            }

            Settings.Default.Save();
        }


        public void EnableAlignmentEditing(bool value)
        {
            AbstractionsSettingsHelper.SaveEnabledAlignmentEditing(IsAlignmentEditingEnabled);
            _eventAggregator.PublishOnUIThreadAsync(new RedrawProjectDesignSurface());
        }

        // ReSharper disable once UnusedParameter.Global
        public void VerseByVerseTextCollectionsEnabledCheckBox(bool value)
        {
            Settings.Default.VerseByVerseTextCollectionsEnabled = IsVerseByVerseTextCollectionsEnabled;
            Settings.Default.Save();

            _eventAggregator.PublishOnUIThreadAsync(new RefreshTextCollectionsMessage());
        }

        public async void SendValidationEmail()
        {
            var user = await _collaborationHttpClientServices.GetCollabUserExistsByEmail(Email);

            if (user.UserId <= 0)
            {
                EmailMessage = LocalizationStrings.Get("Settings_NotFoundOnSystem", _logger); //"Not Found on System!";
                ShowValidateEmailButtonEnabled = false;

                return;
            }

            EmailMessage = "Email Sent";


            _emailValidationString = GenerateRandomPassword.RandomNumber(1000, 9999).ToString();


            var mailMessage = new MimeMessage();
            mailMessage.From.Add(new MailboxAddress("cleardas@cleardashboard.org", "cleardas@cleardashboard.org"));
            mailMessage.To.Add(new MailboxAddress(Email, Email));
            mailMessage.Subject = LocalizationStrings.Get("Settings_DashboardEmailValidationCode", _logger); //"ClearDashboard Email Validation Code";
            mailMessage.Body = new TextPart("plain")
            {
                Text = LocalizationStrings.Get("Settings_EmailVerificationCode", _logger) + ": " + _emailValidationString
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
                _logger.LogError(LocalizationStrings.Get("Settings_EmailSendingError", _logger), e);
                EmailSendError = true;
            }

            EmailSent = true;
            ShowValidateEmailButtonEnabled = true;

        }


        public async void ValidateEmailCode()
        {
            if (EmailCode == _emailValidationString)
            {
                var user = await _collaborationHttpClientServices.GetCollabUserExistsByEmail(Email);

                if (user.UserId <= 0)
                {
                    EmailMessage = LocalizationStrings.Get("Settings_NotFoundOnSystem", _logger); //"Not Found on System!";
                    ShowValidateEmailButtonEnabled = false;

                    return;
                }

                // recreate the json in the user secrets
                CollaborationConfig = new CollaborationConfiguration
                {
                    Group = user.GroupName,
                    RemoteEmail = Email,
                    RemotePersonalAccessToken = Encryption.Decrypt(user.RemotePersonalAccessToken),
                    RemotePersonalPassword = Encryption.Decrypt(user.RemotePersonalPassword),
                    RemoteUrl = "",
                    RemoteUserName = user.RemoteUserName,
                    UserId = user.UserId,
                    NamespaceId = user.NamespaceId,
                };

                _collaborationManager.SaveCollaborationLicense(CollaborationConfig);


                HideExistingCollabUser = Visibility.Collapsed;
                ShowExistingCollabUser = Visibility.Visible;

               await EventAggregator.PublishOnBackgroundThreadAsync(new RebuildMainMenuMessage());
            }
        }

        #endregion // Methods

    }
}
