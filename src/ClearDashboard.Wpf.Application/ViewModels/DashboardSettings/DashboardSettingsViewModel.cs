using System;
using System.IO;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.Wpf.Application.Properties;
using ClearDashboard.Wpf.Application.Services;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.ViewModels.DashboardSettings
{
    public class DashboardSettingsViewModel : Screen
    {

        #region Member Variables   

        private readonly ILogger<DashboardSettingsViewModel> _logger;

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
            IsAquaEnabled = Settings.Default.IsAquaEnabled;

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
            Settings.Default.Save();

            // copy files from install directory to the proper spots
            if (IsAquaEnabled)
            {
                var appStartupPath = AppContext.BaseDirectory;
                _logger.LogInformation($"Aqua Plugin File Copy from {appStartupPath}");
                var fromPath = Path.Combine(appStartupPath, "Aqua");
                if (Directory.Exists(fromPath))
                {
                    // install folder
                    var files = Directory.EnumerateFiles(fromPath);
                    foreach (var file in files)
                    {
                        try
                        {
                            File.Copy(file, appStartupPath, true);
                        }
                        catch (Exception e)
                        {
                            _logger.LogError("Aqua Plugin File Copy", e);
                        }
                    }

                    // en folder
                    fromPath = Path.Combine(appStartupPath, "Aqua", "en");
                    files = Directory.EnumerateFiles(fromPath);
                    foreach (var file in files)
                    {
                        try
                        {
                            File.Copy(file, Path.Combine(appStartupPath, "en"), true);
                        }
                        catch (Exception e)
                        {
                            _logger.LogError("Aqua Plugin File Copy", e);
                        }
                    }

                    // Services folder
                    fromPath = Path.Combine(appStartupPath, "Aqua", "Services");
                    files = Directory.EnumerateFiles(fromPath);
                    foreach (var file in files)
                    {
                        try
                        {
                            File.Copy(file, Path.Combine(appStartupPath, "Services"), true);
                        }
                        catch (Exception e)
                        {
                            _logger.LogError("Aqua Plugin File Copy", e);
                        }
                    }
                }

            }
        }

        #endregion // Methods

    }
}
