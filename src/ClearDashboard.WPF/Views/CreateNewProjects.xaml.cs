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
using ClearDashboard.Common.Models;
using ClearDashboard.Wpf.ViewModels;
using Newtonsoft.Json;

namespace ClearDashboard.Wpf.Views
{
    public partial class CreateNewProjects : Page
    {
        CreateNewProjectsViewModel _vm;


        public CreateNewProjects()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is CreateNewProjectsViewModel)
            {
                _vm = (CreateNewProjectsViewModel)this.DataContext;

                _vm.Init();
            }
        }
    }
}
