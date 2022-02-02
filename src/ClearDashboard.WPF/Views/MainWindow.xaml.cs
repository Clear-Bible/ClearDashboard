using ClearDashboard.Wpf.Helpers;
using ClearDashboard.Wpf.Properties;
using System.Net.Mime;
using System.Windows;

namespace ClearDashboard.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();


            var userPrefs = new UserPreferences();

            this.Height = userPrefs.WindowHeight;
            this.Width = userPrefs.WindowWidth;
            this.Top = userPrefs.WindowTop;
            this.Left = userPrefs.WindowLeft;
            this.WindowState = userPrefs.WindowState;
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
            //var vm = (MainWindowViewModel)this.DataContext;

            //this.Title = "ClearDashboard " + vm.Version;
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
        }
    }
}
