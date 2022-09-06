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

namespace ClearDashboard.Wpf.Application.Views.Startup
{
    /// <summary>
    /// Interaction logic for ProjectSetupView.xaml
    /// </summary>
    public partial class ProjectSetupView : UserControl
    {
        public ProjectSetupView()
        {
            InitializeComponent();
        }

        private void TemplateListView_OnMouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is ListViewItem item)
            {
                item.Background = Brushes.LightBlue;
            }
        }

        private void TemplateListView_OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is ListViewItem item)
            {
                item.Background = Brushes.Transparent;
            }
        }
    }
}
