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
using MvvmHelpers;
using Newtonsoft.Json;

namespace ClearDashboard.Wpf.Views
{
    public partial class CreateNewProjects : Page
    {
        CreateNewProjectsViewModel _vm;

        protected bool m_IsDraging = false;
        protected Point m_DragStartPoint;

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

        private void listview_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            m_DragStartPoint = e.GetPosition(null);
        }

        private void listview_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            var lv = sender as ListView;

            if (lv.SelectedItem == null) return;

            if (e.LeftButton == MouseButtonState.Pressed && !m_IsDraging)
            {
                Point position = e.GetPosition(null);

                if (Math.Abs(position.X - m_DragStartPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(position.Y - m_DragStartPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    m_IsDraging = true;
                    DataObject data = new DataObject(typeof(ParatextProject), lv.SelectedItem);
                    DragDropEffects de = DragDrop.DoDragDrop(lv, data, DragDropEffects.Copy);
                    m_IsDraging = false;
                }
            }
        }

        private void Canvas_PreviewDrop(object sender, DragEventArgs e)
        {
            var data = e.Data;
            Point p = e.GetPosition(DrawCanvas);

            if (data.GetDataPresent(typeof(ParatextProject)))
            {
                var info = data.GetData(typeof(ParatextProject)) as ParatextProject;

                Rectangle rect = new Rectangle();
                rect.Width = rect.Height = 40;
                rect.Fill = Brushes.AliceBlue;
                Canvas.SetTop(rect, p.Y);
                Canvas.SetLeft(rect, p.X);
                DrawCanvas.Children.Add(rect);

                TextBlock text = new TextBlock();
                text.Text = info.Name;
                text.HorizontalAlignment = HorizontalAlignment.Center;
                text.VerticalAlignment = VerticalAlignment.Center;
                Canvas.SetTop(text, p.Y);
                Canvas.SetLeft(text, p.X);
                DrawCanvas.Children.Add(text);
            }
        }
    }
}
