using Caliburn.Micro;
using ClearDashboard.Wpf.Application.ViewModels.PopUps;
using System;
using System.Diagnostics;
using System.Dynamic;
using System.Net;
using System.Net.Mail;
using System.Windows;
using System.Windows.Controls;

namespace ClearDashboard.Wpf.Application.Views.Startup
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

        private async void SendLicenseEmailButton_OnClick(object sender, RoutedEventArgs e)
        {
            //var localizedString = _localizationService!["MainView_About"];

            dynamic settings = new ExpandoObject();
            settings.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            settings.ResizeMode = ResizeMode.NoResize;
            settings.MinWidth = 500;
            settings.MinHeight = 200;
            //settings.Title = $"{localizedString}";

            // Keep the window on top
            settings.Topmost = true;
            settings.Owner = System.Windows.Application.Current.MainWindow;

            var viewModel = IoC.Get<SendEmailViewModel>();

            IWindowManager manager = new WindowManager();
            await manager.ShowDialogAsync(viewModel, null, settings);
        }
    }
}
