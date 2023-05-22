using System;
using System.Diagnostics;
using System.IO;
using Caliburn.Micro;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.Wpf.Application.Messages;
using ClearDashboard.Wpf.Application.Properties;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.ParatextViews;
using ClearDashboard.Wpf.Application.Views.ParatextViews;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace ClearDashboard.Wpf.Application.ViewModels.DashboardSettings
{
    public class DashboardSettingsViewModel : Screen
    {

        #region Member Variables   

        private readonly ILogger<DashboardSettingsViewModel> _logger;
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


        #endregion //Observable Properties


        #region Constructor

        // ReSharper disable once EmptyConstructor
        public DashboardSettingsViewModel()
        {
            // for Caliburn Micro
            _logger = IoC.Get<ILogger<DashboardSettingsViewModel>>();
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
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\ClearDashboard\AQUA"))
                {
                    if (key != null)
                    {
                        Object o = key.GetValue("IsEnabled");
                        if (o != null)
                        {
                            if (o as string == "true")
                            {
                                isEnabled = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _isAquaEnabledOnStartup = Settings.Default.IsAquaEnabled;
            }

            _isAquaEnabledOnStartup = isEnabled;
            IsAquaEnabled = _isAquaEnabledOnStartup;

            base.OnViewReady(view);
        }

        #endregion //Constructor


        #region Methods

        public void Close()
        {
            TryCloseAsync();
        }

        public void PowerModeCheckBox(bool value)
        {
            Settings.Default.EnablePowerModes = IsPowerModesEnabled;
            Settings.Default.Save();
        }

        public void AquaEnabledCheckBox(bool value)
        {
            Settings.Default.IsAquaEnabled = IsAquaEnabled;

            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\ClearDashboard\AQUA");

            if (IsAquaEnabled)
            {
                key.SetValue("IsEnabled", "true");
            }
            else
            {
                
                key.SetValue("IsEnabled", "false");
            }

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

        public void VerseByVerseTextCollectionsEnabledCheckBox(bool value)
        {
            Settings.Default.VerseByVerseTextCollectionsEnabled = IsVerseByVerseTextCollectionsEnabled;
            Settings.Default.Save();

            var eventAggregator = IoC.Get<EventAggregator>();
            eventAggregator.PublishOnUIThreadAsync(new RefreshTextCollectionsMessage());
        }

        #endregion // Methods

    }
}
