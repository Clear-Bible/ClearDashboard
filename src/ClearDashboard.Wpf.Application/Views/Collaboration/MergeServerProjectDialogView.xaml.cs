using System;
using System.Collections.Specialized;
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

            //((INotifyCollectionChanged)ProgressMessages.ItemsSource).CollectionChanged +=
            //    new NotifyCollectionChangedEventHandler(ProgressMessagesCollectionChanged);
        }

        //public void ProgressMessagesCollectionChanged(Object sender, NotifyCollectionChangedEventArgs e)
        //{
        //    if (ProgressMessages.Items.Count > 0)
        //    {
        //        // auto scroll last message into view
        //        ProgressMessages.ScrollIntoView(ProgressMessages.Items[^1]);
        //    }
        //}

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
