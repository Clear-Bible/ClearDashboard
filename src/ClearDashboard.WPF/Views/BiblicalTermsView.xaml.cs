using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using ClearDashboard.Wpf.ViewModels;

namespace ClearDashboard.Wpf.Views
{
    /// <summary>
    /// Interaction logic for BiblicalTerms.xaml
    /// </summary>
    public partial class BiblicalTermsView : UserControl
    {
        private BiblicalTermsViewModel _vm;

        public BiblicalTermsView()
        {
            InitializeComponent();
            
            
        }
        
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _vm = (BiblicalTermsViewModel)DataContext;

            // listen for changes to the lower listview to make it scroll back 
            // to the top
            INotifyPropertyChanged viewModel = (INotifyPropertyChanged)this.DataContext;
            viewModel.PropertyChanged += (sender, args) => {
                if (args.PropertyName.Equals("SelectedItemVerses"))
                {
                    if (SelectedItemVerses.Items.Count > 0)
                    {
                        SelectedItemVerses.ScrollIntoView(SelectedItemVerses.Items[0]);
                    }

                    return;
                    // execute code here.
                }
                if (args.PropertyName.Equals("WindowFlowDirection"))
                {
                    this.FlowDirection = _vm.WindowFlowDirection;
                }

            };
        }

        private void Duplicate_Click(object sender, RoutedEventArgs e)
        {
            // add in this usercontrol
            BiblicalTermsView main = new BiblicalTermsView();
            main.DataContext = _vm;

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
