using ClearDashboard.DataAccessLayer.Models.Common;
using ClearDashboard.Wpf.Application.Properties;
using ClearDashboard.Wpf.Application.ViewModels.Shell;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace ClearDashboard.Wpf.Application.Views.Shell
{
    /// <summary>
    /// Interaction logic for ShellView.xaml
    /// </summary>
    public partial class ShellView 
    {
        private readonly ILogger<ShellView> _logger;

        public ShellView(ILogger<ShellView> logger)
        {
            _logger = logger;
            InitializeComponent();

            Toggle.IsChecked = Settings.Default.Theme == MaterialDesignThemes.Wpf.BaseTheme.Dark; 

            RoundCorners();
        }
        
        private void OnWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
        {

            this.Closing -= OnWindowClosing;

            _logger.LogInformation("ShellView closing.");

        
            if (SelectedLanguage.SelectedItem != null)
            {
                var language = SelectedLanguage.SelectedItem.ToString();
                if (!string.IsNullOrEmpty(language))
                {
                    Settings.Default.language_code = language;
                    Settings.Default.Save();
                }
            }
        }


        
        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
          
            var vm = (ShellViewModel)this.DataContext;
            this.Title = "ClearDashboard " + vm.Version;

            _logger.LogInformation("ShellView loaded.");

            // force the background task window to be on top of the bootstrapper inserted frame
           // Panel.SetZIndex(this.BackgroundTasksView, 10);
        }

        private void Toggle_Checked(object sender, RoutedEventArgs e) => SetTheme();

        private void Toggle_Unchecked(object sender, RoutedEventArgs e) => SetTheme();

        private void SetTheme()
        {

            //TODO:  Complete theme integration
            
            //if (Toggle.IsChecked == true)
            //{
            //    Settings.Default.Theme = MaterialDesignThemes.Wpf.BaseTheme.Dark;
            //}
            //else
            //{
            //    Settings.Default.Theme = MaterialDesignThemes.Wpf.BaseTheme.Light;
            //}

            //Settings.Default.Save();
            //((App)DashboardApplication.Current).SetTheme(Settings.Default.Theme);
            //(DashboardApplication.Current as ClearDashboard.Wpf.Application.App).Theme = Settings.Default.Theme;
        }

        private void OnWindowInitialized(object? sender, EventArgs e)
        {
            _logger.LogInformation("ShellView initialized.");

            this.Closing += OnWindowClosing;
        }

        private async void ApplicationWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var shellViewModel = (ShellViewModel)this.DataContext;
            var windowSettings = new WindowSettings
            {
                Height = Height,
                Width = Width,
                Left = Left,
                Top = Top,
            };

            await shellViewModel.SetWindowsSettings(windowSettings);
        }
    }
}
