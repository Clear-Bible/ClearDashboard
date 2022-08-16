using ClearDashboard.Wpf.Helpers;
using ClearDashboard.Wpf.Properties;
using ClearDashboard.Wpf.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace ClearDashboard.Wpf.Views
{
    /// <summary>
    /// Interaction logic for ShellView.xaml
    /// </summary>
    public partial class ShellView : Window
    {
        public ShellView()
        {
            InitializeComponent();


            var userPrefs = new UserPreferences();

            this.Height = userPrefs.WindowHeight;
            this.Width = userPrefs.WindowWidth;
            this.Top = userPrefs.WindowTop;
            this.Left = userPrefs.WindowLeft;
            this.WindowState = userPrefs.WindowState;

            if (Settings.Default.Theme == MaterialDesignThemes.Wpf.BaseTheme.Dark)
            {
                Toggle.IsChecked = true;
            }
            else
            {
                Toggle.IsChecked = false;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var userPrefs = new UserPreferences
            {
                WindowHeight = this.Height,
                WindowWidth = this.Width,
                WindowTop = this.Top,
                WindowLeft = this.Left,
                WindowState = this.WindowState
            };

            userPrefs.Save();

            var language = this.SelectedLanguage.SelectedItem.ToString();
            if (language != "")
            {
                Settings.Default.language_code = language.ToString();
                Settings.Default.Save();
            }

        }


        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = (ShellViewModel)this.DataContext;
            this.Title = "ClearDashboard " + vm.Version;

            // force the statusbar to be on top of the bootstraper inserted frame
            Panel.SetZIndex(this.StatusBar, 10);
        }

        private void Toggle_Checked(object sender, RoutedEventArgs e) => SetTheme();

        private void Toggle_Unchecked(object sender, RoutedEventArgs e) => SetTheme();

        private void SetTheme()
        {
            if (Toggle.IsChecked == true)
            {
                Settings.Default.Theme = MaterialDesignThemes.Wpf.BaseTheme.Dark;
            }
            else
            {
                Settings.Default.Theme = MaterialDesignThemes.Wpf.BaseTheme.Light;
            }

            Settings.Default.Save();
            ((App)Application.Current).SetTheme(Settings.Default.Theme);
            (Application.Current as ClearDashboard.Wpf.App).Theme = Settings.Default.Theme;
        }
        
    }
}
