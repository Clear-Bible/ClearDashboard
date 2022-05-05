using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.ViewModels.Panes;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.ViewModels
{
    public class TreeDownViewModel : PaneViewModel
    {
        #region Member Variables

        #endregion //Member Variables

        #region Public Properties

        #endregion //Public Properties

        #region Observable Properties


        #endregion //Observable Properties

        #region Constructor

        public TreeDownViewModel()
        {

        }

        public TreeDownViewModel(INavigationService navigationService, ILogger<TreeDownViewModel> logger, DashboardProjectManager projectManager, IEventAggregator eventAggregator)
            : base(navigationService, logger, projectManager, eventAggregator)
        {
            this.Title = "⯭ TREEDOWN";
            this.ContentId = "TREEDOWN";
        }

        #endregion //Constructor

        #region Methods

        #endregion // Methods
    }
}
