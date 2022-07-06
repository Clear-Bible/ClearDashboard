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

namespace ClearDashboard.Wpf.Views
{
    /// <summary>
    /// Interaction logic for WordMeanings.xaml
    /// </summary>
    public partial class WordMeaningsView : UserControl
    {
        public WordMeaningsView()
        {
            InitializeComponent();
        }

        private void lvSenses_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            Console.WriteLine();
        }

        private void Duplicate_Click(object sender, RoutedEventArgs e)
        {
            // add in this usercontrol
            var main = new WordMeaningsView();
            main.DataContext = this.DataContext;

            // get current size and compare to maximized size
            double width = this.MainGrid.ActualWidth;
            double height = this.MainGrid.ActualHeight;

            var mirror = new MirrorView();
            mirror.MirrorViewRoot.Children.Add(main);
            mirror.WindowState = WindowState.Maximized;
            mirror.Show();
            double newWidth = main.MainGrid.ActualWidth;
            double newHeight = main.MainGrid.ActualHeight;

            // calculate new zoom sizes
            double widthZoom = (width + newWidth) / newWidth;
            double heightZoom = (height + newHeight) / newHeight;
            mirror.MirrorViewRoot.LayoutTransform = new ScaleTransform(widthZoom, heightZoom);
        }
    }
}
