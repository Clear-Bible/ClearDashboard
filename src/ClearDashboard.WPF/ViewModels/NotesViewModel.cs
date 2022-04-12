using Caliburn.Micro;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.Wpf.ViewModels.Panes;
using Microsoft.Extensions.Logging;
using System.Windows;

namespace ClearDashboard.Wpf.ViewModels
{
    public class NotesViewModel : ToolViewModel
    {

        #region Member Variables

        private readonly ILogger _logger;
        private readonly ProjectManager _projectManager;

        #endregion //Member Variables

        #region Public Properties


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
        public NotesViewModel()
        {

        }

        public NotesViewModel(INavigationService navigationService, ILogger<NotesViewModel> logger, ProjectManager projectManager)
        {
            _projectManager = projectManager;

            flowDirection = _projectManager.CurrentLanguageFlowDirection;

            this.Title = "🖉 NOTES";
            this.ContentId = "NOTES";

        }

        #endregion //Constructor

        #region Methods

        #endregion // Methods


    }
}
