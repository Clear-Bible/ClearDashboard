using System.Windows;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.ViewModels.Panes;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.ViewModels
{
    public class AlignmentToolViewModel: PaneViewModel
    {
        #region Member Variables

        #endregion //Member Variables

        #region Public Properties

        public string ContentID => this.ContentID;

        #endregion //Public Properties

        #region Observable Properties

       

        #endregion //Observable Properties

        #region Constructor
        public AlignmentToolViewModel()
        {

        }

        public AlignmentToolViewModel(INavigationService navigationService, ILogger<AlignmentToolViewModel> logger, DashboardProjectManager projectManager, IEventAggregator eventAggregator) :
            base(navigationService, logger, projectManager, eventAggregator)
        {
            this.Title = "⳼ ALIGNMENT TOOL";
            this.ContentId = "ALIGNMENTTOOL";
        }

        #endregion //Constructor

        #region Methods

        #endregion // Methods


    }
}
