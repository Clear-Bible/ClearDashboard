using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace ClearDashboard.Wpf.Views
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : Page
    {

        protected bool m_IsDraging = false;
        protected Point m_DragStartPoint;


        public SettingsView()
        {
            InitializeComponent();

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
                    DataObject data = new DataObject(typeof(ItemInfo), lv.SelectedItem);
                    DragDropEffects de = DragDrop.DoDragDrop(lv, data, DragDropEffects.Copy);
                    m_IsDraging = false;
                }

            }
        }

        private void Canvas_PreviewDrop(object sender, DragEventArgs e)
        {
            var data = e.Data;
            Point p = e.GetPosition(DrawCanvas);

            if (data.GetDataPresent(typeof(ItemInfo)))
            {
                var info = data.GetData(typeof(ItemInfo)) as ItemInfo;

                Image image = new Image();
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri(info.ImagePath, UriKind.Relative);
                bitmapImage.EndInit();
                image.Source = bitmapImage;

                Canvas.SetLeft(image, p.X);
                Canvas.SetTop(image, p.Y);
                DrawCanvas.Children.Add(image);

                Rectangle rect = new Rectangle();
                if (info.ImageName == "NEW")
                {
                    rect.Fill = Brushes.Blue;
                }
                else
                {
                    rect.Fill = Brushes.Orange;
                }
                rect.Width = rect.Height = 40;
                Canvas.SetTop(rect, p.Y);
                Canvas.SetLeft(rect, p.X);
                DrawCanvas.Children.Add(rect);

                TextBlock text = new TextBlock();
                text.Text = info.ImageName;
                text.HorizontalAlignment = HorizontalAlignment.Center;
                text.VerticalAlignment = VerticalAlignment.Center;
                Canvas.SetTop(text, p.Y);
                Canvas.SetLeft(text, p.X);
                DrawCanvas.Children.Add(text);
            }
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }
    }

    public class ItemInfo
    {
        public string ImagePath { get; set; }
        public string ImageName { get; set; }
    }
}
