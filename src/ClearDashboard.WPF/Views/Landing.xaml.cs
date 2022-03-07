using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ClearDashboard.Wpf.Helpers;
using ClearDashboard.Wpf.ViewModels;

namespace ClearDashboard.Wpf.Views
{
    /// <summary>
    /// Interaction logic for LandingViewModel.xaml
    /// </summary>
    public partial class Landing : Page
    {
        public Landing()
        {
            InitializeComponent();
        }

        private void Grid_Click(object sender, RoutedEventArgs e)
        {
            var ClickedButton = e.OriginalSource as NavButton;

            //(Application.Current as ClearDashboard.Wpf.App).SelectedDashboardProject =
            //    new Common.Models.DashboardProject { Name = "TEST ME" };
            NavigationService.Navigate(ClickedButton.NavUri);


            //if (ClickedButton.DashboardProject != null)
            //{
            //    if (Application.Current is ClearDashboard.Wpf.App)
            //    {
            //        (Application.Current as ClearDashboard.Wpf.App).SelectedDashboardProject = ClickedButton.DashboardProject;
            //    }

            //    //NavigationService.Navigate(ClickedButton.NavUri, ClickedButton.DashboardProject);
            //    WorkSpace workspace = new WorkSpace(ClickedButton.DashboardProject);
            //    NavigationService.Navigate(workspace);
            //}
            //else
            //{
            //    (Application.Current as ClearDashboard.Wpf.App).SelectedDashboardProject =
            //        new Common.Models.DashboardProject {Name="TEST ME" };
            //    NavigationService.Navigate(ClickedButton.NavUri);
            //}
            
        }

    }
}
