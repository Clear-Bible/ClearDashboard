using Caliburn.Micro;
using ClearDashboard.Wpf.Views;
using System.Collections.ObjectModel;

namespace ClearDashboard.Wpf.ViewModels
{
    public class SettingsViewModel: PropertyChangedBase
    {

        public ObservableCollection<ItemInfo> observableCollection { get; set; } = new ObservableCollection<ItemInfo>();

        public SettingsViewModel()
        {

            observableCollection.Add(new ItemInfo() { ImagePath = @"D:\Projects-GBI\ClearDashboard\src\ClearDashboard.Wpf\Resources\NewProject_Icon_96.png", ImageName = "NEW" });
            observableCollection.Add(new ItemInfo() { ImagePath = @"D:\Projects-GBI\ClearDashboard\src\ClearDashboard.Wpf\Resources\settings_logo_96.png", ImageName = "SETTINGS" });
        }
    }
}
