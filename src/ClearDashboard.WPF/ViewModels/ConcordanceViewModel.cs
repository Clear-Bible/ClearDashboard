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
        protected override void OnViewAttached(object view, object context)
        {
            Logger.LogInformation("OnViewAttached");
            base.OnViewAttached(view, context);
        }

        protected override void OnViewLoaded(object view)
        {
            Logger.LogInformation("OnViewLoaded");
            base.OnViewLoaded(view);
        }

        protected override void OnViewReady(object view)
        {
            Logger.LogInformation("OnViewReady");
            base.OnViewReady(view);
        }
        #endregion // Methods
    }
}
