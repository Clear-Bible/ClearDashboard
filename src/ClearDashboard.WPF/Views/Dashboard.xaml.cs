using ClearDashboard.Wpf.ViewModels;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace ClearDashboard.Wpf.Views
{
    /// <summary>
    /// Interaction logic for Dashboard.xaml
    /// </summary>
    public partial class Dashboard : UserControl
    {

        public Dashboard()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DashboardViewModel vm = this.DataContext as DashboardViewModel;

            Debug.WriteLine(vm.DashboardProject.Name);
        }
    }
}
