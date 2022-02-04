using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClearDashboard.Wpf.Views;
using MvvmHelpers;

namespace ClearDashboard.Wpf.ViewModels
{
    public class SettingsViewModel: ObservableObject
    {

        public ObservableCollection<ItemInfo> observableCollection { get; set; } = new ObservableCollection<ItemInfo>();

        public SettingsViewModel()
        {

            observableCollection.Add(new ItemInfo() { ImagePath = @"D:\Projects-GBI\ClearDashboard\src\ClearDashboard.Wpf\Resources\NewProject_Icon_96.png", ImageName = "NEW" });

            observableCollection.Add(new ItemInfo() { ImagePath = @"D:\Projects-GBI\ClearDashboard\src\ClearDashboard.Wpf\Resources\settings_logo_96.png", ImageName = "SETTINGS" });
        }
    }
}
