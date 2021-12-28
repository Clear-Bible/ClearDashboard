using ClearDashboard.Core.ViewModels;
using MvvmCross.Navigation;
using MvvmCross.Platforms.Wpf.Presenters.Attributes;
using MvvmCross.Platforms.Wpf.Views;
using MvvmCross.ViewModels;

namespace ClearDashboard.Wpf.Views
{
    /// <summary>
    /// Interaction logic for StartupView.xaml
    /// </summary>
    [MvxContentPresentation]
    [MvxViewFor(typeof(StartupViewModel))]
    public partial class StartupView : MvxWpfView
    {

        public StartupView()
        {
            InitializeComponent();
        }
    }
}
