using System.Windows;
using System.Windows.Controls;

namespace ClearDashboard.Wpf.Application.Views.ParatextViews
{
    /// <summary>
    /// Interaction logic for WordMeaningsView.xaml
    /// </summary>
    public partial class WordMeaningsView : UserControl
    {
        public WordMeaningsView()
        {
            InitializeComponent();
        }

        private void MainGrid_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width > 640)
            {
                LVSensesSlim.Visibility = Visibility.Collapsed;
                LVSenses.Visibility = Visibility.Visible;
            }
               
            else
            {
                LVSensesSlim.Visibility = Visibility.Visible;
                LVSenses.Visibility = Visibility.Collapsed;
            }
        }
    }
}
