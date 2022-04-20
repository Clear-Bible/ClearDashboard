using System.Windows;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.ViewModels.Panes;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.ViewModels
{
    public class ConcordanceViewModel : PaneViewModel
    {
        #region Member Variables

        private readonly ILogger _logger;
        private readonly ProjectManager _projectManager;

        #endregion //Member Variables

        #region Public Properties

        public string ContentID => this.ContentID;

        #endregion //Public Properties

        #region Observable Properties

        private FlowDirection _flowDirection = FlowDirection.LeftToRight;
        public FlowDirection flowDirection
        {
            get => _flowDirection;
            set
            {
                _flowDirection = value;
                NotifyOfPropertyChange(() => flowDirection);
            }
        }

        #endregion //Observable Properties

        #region Constructor

        public ConcordanceViewModel()
        {

        }

        public ConcordanceViewModel(INavigationService navigationService, ILogger<CreateNewProjectsViewModel> logger, ProjectManager projectManager)
        {
            this.Title = "🆎 CONCORDANCE TOOL";
            this.ContentId = "CONCORDANCETOOL";
            _logger = logger;

            _projectManager = projectManager;

            flowDirection = _projectManager.CurrentLanguageFlowDirection;

        }

        #endregion //Constructor

        #region Methods

        #endregion // Methods
    }
}
