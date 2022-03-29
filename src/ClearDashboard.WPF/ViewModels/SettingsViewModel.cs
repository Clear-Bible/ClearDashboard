using System;
using Caliburn.Micro;
using ClearDashboard.Wpf.Views;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using ClearDashboard.DataAccessLayer;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.ViewModels
{
    public class SettingsViewModel : ApplicationScreen
    {

        private ILogger Logger { get; set; }
        private INavigationService NavigationService { get; set; }
        private ProjectManager ProjectManager;
        public ObservableCollection<ItemInfo> observableCollection { get; set; } = new ObservableCollection<ItemInfo>();

        /// <summary>
        /// Required for design-time support
        /// </summary>
        public SettingsViewModel()
        {

        }

        public SettingsViewModel(INavigationService navigationService, ILogger<SettingsViewModel> logger, ProjectManager projectManager) : base(navigationService, logger)
        {
            Logger = logger;
            NavigationService = navigationService;
            ProjectManager = projectManager;

            observableCollection.Add(new ItemInfo() { ImagePath = @"/Resources/NewProject_Icon_96.png", ImageName = "NEW" });
            observableCollection.Add(new ItemInfo() { ImagePath = @"/Resources/settings_logo_96.png", ImageName = "SETTINGS" });
        }
    }
}
