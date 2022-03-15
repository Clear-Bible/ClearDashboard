using System;
using Caliburn.Micro;
using ClearDashboard.Wpf.Views;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace ClearDashboard.Wpf.ViewModels
{
    public class SettingsViewModel : ApplicationScreen
    {
        public ObservableCollection<ItemInfo> observableCollection { get; set; } = new ObservableCollection<ItemInfo>();


        /// <summary>
        /// Required for design-time support
        /// </summary>
        public SettingsViewModel()
        {
            
        }

        public SettingsViewModel(ILog logger): base(logger)
        {
           observableCollection.Add(new ItemInfo() { ImagePath = @"/Resources/NewProject_Icon_96.png", ImageName = "NEW" });
            observableCollection.Add(new ItemInfo() { ImagePath = @"/Resources/settings_logo_96.png", ImageName = "SETTINGS" });
        }
    }
}
