using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace ClearDashboard.Wpf.Application.Views.Collaboration
{
    /// <summary>
    /// Interaction logic for MergeServerProjectDialogView.xaml
    /// </summary>
    public partial class MergeServerProjectDialogView : UserControl
    {
        public MergeServerProjectDialogView()
        {
            InitializeComponent();

        }

        private void MergeServerProjectDialogView_OnLoaded(object sender, RoutedEventArgs e)
        {
            ProgressMessages.Focus();

            //// listen for changes to the lower listview to make it scroll back 
            //// to the top
            INotifyPropertyChanged viewModel = (INotifyPropertyChanged)this.DataContext;
            viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName.Equals("MergeProgressUpdates"))
                {
                    if (ProgressMessages.Items.Count > 0)
                    {
                        // auto scroll last message into view
                        ProgressMessages.ScrollIntoView(ProgressMessages.Items[^1]);
                    }

                    return;
                }

            };

        }

        
    }
}
