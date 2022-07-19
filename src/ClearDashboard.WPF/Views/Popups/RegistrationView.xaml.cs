using System.Windows.Controls;


namespace ClearDashboard.Wpf.Views.Popups
{
    /// <summary>
    /// Interaction logic for RegistrationPopupView.xaml
    /// </summary>
    public partial class RegistrationView : UserControl
    {
        public RegistrationView()
        {
            InitializeComponent();
        }
        
        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            //we can check if all three are done
            //we can tell when to turn on register button
        }
    }
}
