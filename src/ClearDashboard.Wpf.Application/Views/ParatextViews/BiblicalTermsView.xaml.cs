using ClearDashboard.Wpf.Application.ViewModels.ParatextViews;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace ClearDashboard.Wpf.Application.Views.ParatextViews
{
    /// <summary>
    /// Interaction logic for BiblicalTermsView.xaml
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
            viewModel.PropertyChanged += (sender, args) =>
            {
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
    }
}
