using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.Views;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace ClearDashboard.Wpf.ViewModels
{
    public class SettingsViewModel : ApplicationScreen
    {
        #region Member Variables
      

        #endregion //Member Variables

        #region Public Properties

        #endregion //Public Properties

        #region Observable Properties

        public ObservableCollection<ItemInfo> observableCollection { get; set; } = new ObservableCollection<ItemInfo>();

        #endregion //Observable Properties

        #region Constructor

        /// <summary>
        /// Required for design-time support
        /// </summary>
        public SettingsViewModel()
        {

        }

        public SettingsViewModel(INavigationService navigationService, 
            ILogger<SettingsViewModel> logger, ProjectManager projectManager) 
            : base(navigationService, logger, projectManager)
        {
           
         
            observableCollection.Add(new ItemInfo() { ImagePath = @"/Resources/NewProject_Icon_96.png", ImageName = "NEW" });
            observableCollection.Add(new ItemInfo() { ImagePath = @"/Resources/settings_logo_96.png", ImageName = "SETTINGS" });
        }

        #endregion //Constructor

        #region Methods

        #endregion // Methods
    }
}
