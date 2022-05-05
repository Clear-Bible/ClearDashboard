using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.ViewModels.Panes;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.ViewModels
{
    public class ConcordanceViewModel : PaneViewModel
    {
        #region Member Variables

       

        #endregion //Member Variables

        #region Public Properties

        public string ContentID => this.ContentID;

        #endregion //Public Properties

        #region Observable Properties
        #endregion //Observable Properties

        #region Constructor

        public ConcordanceViewModel()
        {

        }

        public ConcordanceViewModel(INavigationService navigationService, ILogger<ConcordanceViewModel> logger, DashboardProjectManager projectManager, IEventAggregator eventAggregator) 
            : base(navigationService, logger, projectManager, eventAggregator)
        {
            Title = "🆎 CONCORDANCE TOOL";
            ContentId = "CONCORDANCETOOL";
        }

        #endregion //Constructor

        #region Methods

        #endregion // Methods
    }
}
