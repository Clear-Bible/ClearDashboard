using System;
using System.Collections.Generic;
using System.IO;
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
using ClearDashboard.DataAccessLayer.Models;
using Path = System.Windows.Shapes.Path;

namespace ClearDashboard.Wpf.Application.Views.Startup
{
    /// <summary>
    /// Interaction logic for ProjectPickerView.xaml
    /// </summary>
    public partial class ProjectPickerView : UserControl
    {
        public ProjectPickerView()
        {
            InitializeComponent();
        }
        private void MoveForwards_OnMouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Button button)
            {
                button.Background = Brushes.LightBlue;
            }
        }

        private void MoveForwards_OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Button button)
            {
                button.Background = Brushes.LightGray;
            }
        }

        private void UIElement_OnMouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is StackPanel element)
            {
                element.Background = Brushes.LightBlue;
            }
        }

        private void UIElement_OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is StackPanel element)
            {
                element.Background = Brushes.White;
            }
        }
    }
}
