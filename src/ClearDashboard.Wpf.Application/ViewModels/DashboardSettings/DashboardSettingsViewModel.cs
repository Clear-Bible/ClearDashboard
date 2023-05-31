using Caliburn.Micro;
using ClearDashboard.Collaboration.Services;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Messages;
using ClearDashboard.Wpf.Application.Properties;
using ClearDashboard.Wpf.Application.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System;

namespace ClearDashboard.Wpf.Application.ViewModels.DashboardSettings
{
    public class DashboardSettingsViewModel : Screen
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


        #endregion //Observable Properties


        #region Constructor

        // ReSharper disable once EmptyConstructor
        public DashboardSettingsViewModel(IEventAggregator eventAggregator, CollaborationManager collaborationManager)
        {
            // for Caliburn Micro
            IoC.Get<ILogger<DashboardSettingsViewModel>>();
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

            // load in the collab user info
            CollaborationConfig = _collaborationManager.GetConfig();

            base.OnViewReady(view);
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
            AbstractionsSettingsHelper.SaveGitUrl(GitRootUrl);
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
