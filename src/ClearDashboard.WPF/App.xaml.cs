using System;
using MaterialDesignThemes.Wpf;
using System.Windows;
using ClearDashboard.DataAccessLayer.Models;
using Action = System.Action;

namespace ClearDashboard.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
       // public ILog Log { get; set; }


        private DashboardProject _selectedDashboardProject = new DashboardProject();
        public DashboardProject SelectedDashboardProject
        {
            get { return _selectedDashboardProject; }
            set { _selectedDashboardProject = value; }
        }


        // Theme related
        public event Action ThemeChanged;

        private MaterialDesignThemes.Wpf.BaseTheme _theme;
        public MaterialDesignThemes.Wpf.BaseTheme Theme
        {
            get { return _theme; }
            set
            {
                _theme = value;
                OnThemeChanged();
            }
        }

        // trigger the change event
        private void OnThemeChanged()
        {
            ThemeChanged?.Invoke();
        }


        /// <summary>
        /// Gets the current theme for the application from the app settings and sets them
        /// </summary>
        /// <param name="theme"></param>
        public void SetTheme(BaseTheme theme)
        {
            //Copied from the existing ThemeAssist class
            //https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit/blob/master/MaterialDesignThemes.Wpf/ThemeAssist.cs

            string lightSource = "pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Light.xaml";
            string darkSource = "pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Dark.xaml";

            foreach (ResourceDictionary resourceDictionary in Resources.MergedDictionaries)
            {
                if (string.Equals(resourceDictionary.Source?.ToString(), lightSource, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(resourceDictionary.Source?.ToString(), darkSource, StringComparison.OrdinalIgnoreCase))
                {
                    Resources.MergedDictionaries.Remove(resourceDictionary);
                    break;
                }
            }

            if (theme == BaseTheme.Dark)
            {
                Resources.MergedDictionaries.Insert(0, new ResourceDictionary { Source = new Uri(darkSource) });
            }
            else
            {
                //This handles both Light and Inherit
                Resources.MergedDictionaries.Insert(0, new ResourceDictionary { Source = new Uri(lightSource) });
            }
        }

        public App()
        {
 
        }
    }
}
