using System.Runtime.InteropServices;
using System;
using System.Windows;
using System.Windows.Controls;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Properties;
using ClearDashboard.Wpf.Application.ViewModels.Shell;
using DashboardApplication = System.Windows.Application;
using System.Windows.Interop;
using Caliburn.Micro;
using ClearDashboard.Wpf.Application.ViewModels.Main;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.Views.Shell
{
    /// <summary>
    /// Interaction logic for ShellView.xaml
    /// </summary>
    public partial class ShellView 
    {
        private readonly ILogger<ShellView> _logger;

        // The enum flag for DwmSetWindowAttribute's second parameter, which tells the function what attribute to set.
        // Copied from dwmapi.h
        public enum Dwmwindowattribute
        {
            DwmwaWindowCornerPreference = 33
        }

        // The DWM_WINDOW_CORNER_PREFERENCE enum for DwmSetWindowAttribute's third parameter, which tells the function
        // what value of the enum to set.
        // Copied from dwmapi.h
        public enum DwmWindowCornerPreference
        {
            DwmwcpDefault = 0,
            DwmwcpDoNotRound = 1,
            DwmwcpRound = 2,
            DwmwcpRoundSmall = 3
        }

        // Import dwmapi.dll and define DwmSetWindowAttribute in C# corresponding to the native function.
        [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        internal static extern void DwmSetWindowAttribute(IntPtr hwnd,
            Dwmwindowattribute attribute,
            ref DwmWindowCornerPreference pvAttribute,
            uint cbAttribute);

        public ShellView(ILogger<ShellView> logger)
        {
            _logger = logger;
            InitializeComponent();


            var userPrefs = new UserPreferences();

            this.Height = userPrefs.WindowHeight;
            this.Width = userPrefs.WindowWidth;
            this.Top = userPrefs.WindowTop;
            this.Left = userPrefs.WindowLeft;
            //this.WindowState = userPrefs.WindowState;

            if (Settings.Default.Theme == MaterialDesignThemes.Wpf.BaseTheme.Dark)
            {
                Toggle.IsChecked = true;
            }
            else
            {
                Toggle.IsChecked = false;
            }
            RoundCorners();
        }

        private void RoundCorners()
        {
            var hWnd = new WindowInteropHelper(GetWindow(this)!).EnsureHandle();
            var preference = DwmWindowCornerPreference.DwmwcpRound;
            DwmSetWindowAttribute(hWnd, Dwmwindowattribute.DwmwaWindowCornerPreference, ref preference, sizeof(uint));
        }

        private async  void OnWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
        {

            this.Closing -= OnWindowClosing;

            _logger.LogInformation("ShellView closing.");

            var userPrefs = new UserPreferences
            {
                WindowHeight = this.Height,
                WindowWidth = this.Width,
                WindowTop = this.Top,
                WindowLeft = this.Left,
                WindowState = this.WindowState
            };

            userPrefs.Save();

            if (SelectedLanguage.SelectedItem != null)
            {
                var language = this.SelectedLanguage.SelectedItem.ToString();
                if (language != "")
                {
                    Settings.Default.language_code = language.ToString();
                    Settings.Default.Save();
                }
            }
        }


        
        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
          
            var vm = (ShellViewModel)this.DataContext;
            this.Title = "ClearDashboard " + vm.Version;

            _logger.LogInformation("ShellView loaded.");

            // force the background task window to be on top of the bootstraper inserted frame
            Panel.SetZIndex(this.TaskView, 10);
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
    }
}
