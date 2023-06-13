using Autofac;
using Caliburn.Micro;
using ClearDashboard.Collaboration.Services;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Messages;
using ClearDashboard.Wpf.Application.Properties;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System;
using System.Threading;
using System.Windows;
using ClearDashboard.DataAccessLayer.Models;
using static ClearDashboard.DataAccessLayer.Features.GitLabUser.GitLabUserSlice;
using ClearDashboard.DataAccessLayer.Features.GitLabUser;

namespace ClearDashboard.Wpf.Application.ViewModels.DashboardSettings
{
    public class DashboardSettingsViewModel : DashboardApplicationScreen
    {

        #region Member Variables

        private readonly IEventAggregator _eventAggregator;
        private readonly CollaborationManager _collaborationManager;
        private bool _isAquaEnabledOnStartup;

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
            ILocalizationService localizationService)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
        {
            // for Caliburn Micro
            //IoC.Get<ILogger<DashboardSettingsViewModel>>();
            _eventAggregator = eventAggregator;
            _collaborationManager = collaborationManager;
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

            base.OnViewReady(view);
        }

        protected override async void OnViewLoaded(object view)
        {
            var results =
                await ExecuteRequest(
                    new GitLabUserExistsQuery(MySqlHelper.BuildConnectionString(), CollaborationConfig.UserId,
                        CollaborationConfig.RemoteUserName, CollaborationConfig.RemoteEmail), CancellationToken.None);

            if (results.Data)
            {
                GitLabUserFound = true;
                GitlabUserSaveVisibility = Visibility.Collapsed;
                RestoreButtonEnabled = false;
            }
            else
            {
                GitLabUserFound = false; 
                GitlabUserSaveVisibility = Visibility.Visible;
                RestoreButtonEnabled = true;
            }

            base.OnViewLoaded(view);
        }

        #endregion //Constructor


        #region Methods

        // ReSharper disable once UnusedMember.Global
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
            var userId = _collaborationConfig.UserId;
            var remoteUserName = _collaborationConfig.RemoteUserName;
            var remoteEmail = _collaborationConfig.RemoteEmail;
            var remotePersonalAccessToken = _collaborationConfig.RemotePersonalAccessToken;
            var remotePersonalPassword = _collaborationConfig.RemotePersonalPassword;
            var group = _collaborationConfig.Group;
            var namespaceId = _collaborationConfig.NamespaceId;


            var results =
                await ExecuteRequest(
                    new PostGitLabUserQuery(MySqlHelper.BuildConnectionString(), userId, remoteUserName, remoteEmail,
                        remotePersonalAccessToken, remotePersonalPassword, group, namespaceId), CancellationToken.None);
            if (results.Success)
            {
                SaveGitLabUserMessage = "Saved to remote server";
            }
            else
            {
                SaveGitLabUserMessage = "User already exists on server";
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

        // ReSharper disable once UnusedParameter.Global
        public void VerseByVerseTextCollectionsEnabledCheckBox(bool value)
        {
            Settings.Default.VerseByVerseTextCollectionsEnabled = IsVerseByVerseTextCollectionsEnabled;
            Settings.Default.Save();

            _eventAggregator.PublishOnUIThreadAsync(new RefreshTextCollectionsMessage());
        }

        #endregion // Methods

    }
}
