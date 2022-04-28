using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.ViewModels.Panes;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class StartPageViewModel : PaneViewModel
    {
        #region Member Variables

      
        #endregion //Member Variables

        #region Public Properties

        #endregion //Public Properties

        #region Observable Properties

        #endregion //Observable Properties

        #region Constructor

        public StartPageViewModel()
        {

        }

        public StartPageViewModel(INavigationService navigationService, ILogger<StartPageViewModel> logger, DashboardProjectManager projectManager):base(navigationService, logger, projectManager)
        {
            this.Title = "⌂ START PAGE";
            this.ContentId = "{StartPage_ContentId}";
        }

        #endregion //Constructor

        #region Methods

        #endregion // Methods

    }
}
