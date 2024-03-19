using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Models;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.PopUps;
using System.Dynamic;
using System.Windows;

namespace ClearDashboard.Wpf.Application.Helpers
{
    public static class AccountWindow
    {
        public static void ShowAccountInfoWindow(ILocalizationService localizationService, IWindowManager manager)
        {
            var localizedString = localizationService!["MainView_AccountInfo"];

            dynamic settings = new ExpandoObject();
            settings.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            settings.ResizeMode = ResizeMode.NoResize;
            settings.Title = $"{localizedString}";

            // Keep the window on top
            //settings.Topmost = true;
            settings.Owner = System.Windows.Application.Current.MainWindow;

            var viewModel = IoC.Get<AccountInfoViewModel>();

            manager.ShowDialogAsync(viewModel, null, settings);
        }
    }
}
