using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.Wpf.ViewModels.Panes;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.ViewModels
{
    public class TargetContextViewModel : ToolViewModel
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
        public TargetContextViewModel()
        {

        }

        public TargetContextViewModel(INavigationService navigationService, ILogger<TargetContextViewModel> logger, ProjectManager projectManager)
        {
            this.Title = "⬓ TARGET CONTEXT";
            this.ContentId = "TARGETCONTEXT";

            _logger = logger;
            _projectManager = projectManager;

            flowDirection = _projectManager.CurrentLanguageFlowDirection;

        }


        #endregion //Constructor

        #region Methods

        #endregion // Methods


    }
}
