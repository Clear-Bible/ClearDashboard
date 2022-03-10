using Caliburn.Micro;
using ClearDashboard.Wpf.Views;
using System.Collections.ObjectModel;
using System.Windows;

namespace ClearDashboard.Wpf.ViewModels
{
    public class SettingsViewModel: PropertyChangedBase
    {
        private readonly ILog _logger;
        public ObservableCollection<ItemInfo> observableCollection { get; set; } = new ObservableCollection<ItemInfo>();

        public SettingsViewModel()
        {
            _logger = ((App)Application.Current).Log;
            observableCollection.Add(new ItemInfo() { ImagePath = @"D:\Projects-GBI\ClearDashboard\src\ClearDashboard.Wpf\Resources\NewProject_Icon_96.png", ImageName = "NEW" });
            observableCollection.Add(new ItemInfo() { ImagePath = @"D:\Projects-GBI\ClearDashboard\src\ClearDashboard.Wpf\Resources\settings_logo_96.png", ImageName = "SETTINGS" });
        }
    }
}
