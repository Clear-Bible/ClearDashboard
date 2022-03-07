using ClearDashboard.Wpf.Helpers;
using ClearDashboard.Wpf.Properties;
using System.Net.Mime;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ClearDashboard.Wpf.ViewModels;

namespace ClearDashboard.Wpf.Views
{
    /// <summary>
    /// Interaction logic for ShellView.xaml
    /// </summary>
    public partial class ShellView : Window
    {
        //NavigationCommands


        public ShellView()
        {
            InitializeComponent();


            var userPrefs = new UserPreferences();

            this.Height = userPrefs.WindowHeight;
            this.Width = userPrefs.WindowWidth;
            this.Top = userPrefs.WindowTop;
            this.Left = userPrefs.WindowLeft;
            this.WindowState = userPrefs.WindowState;

            if (Properties.Settings.Default.Theme == MaterialDesignThemes.Wpf.BaseTheme.Dark)
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
            var userPrefs = new UserPreferences();

            userPrefs.WindowHeight = this.Height;
            userPrefs.WindowWidth = this.Width;
            userPrefs.WindowTop = this.Top;
            userPrefs.WindowLeft = this.Left;
            userPrefs.WindowState = this.WindowState;

            userPrefs.Save();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = (ShellViewModel)this.DataContext;
            this.Title = "ClearDashboard " + vm.Version;
        }

        private void Toggle_Checked(object sender, RoutedEventArgs e) => SetTheme();

        private void Toggle_Unchecked(object sender, RoutedEventArgs e) => SetTheme();

        private void SetTheme()
        {
            if (Toggle.IsChecked == true)
            {
                Properties.Settings.Default.Theme = MaterialDesignThemes.Wpf.BaseTheme.Dark;
            }
            else
            {
                Properties.Settings.Default.Theme = MaterialDesignThemes.Wpf.BaseTheme.Light;
            }

            Properties.Settings.Default.Save();
            ((App)Application.Current).SetTheme(Properties.Settings.Default.Theme);
            (Application.Current as ClearDashboard.Wpf.App).Theme = Properties.Settings.Default.Theme;
        }
    }
}
