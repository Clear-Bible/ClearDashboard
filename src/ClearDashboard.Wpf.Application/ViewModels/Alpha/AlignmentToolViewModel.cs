using Autofac;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.Application.ViewModels.Panes;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.ViewModels
{
    public class AlignmentToolViewModel : PaneViewModel
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

        public AlignmentToolViewModel(INavigationService navigationService, ILogger<AlignmentToolViewModel> logger,
            DashboardProjectManager projectManager, IEventAggregator eventAggregator, IMediator mediator, ILifetimeScope? lifetimeScope) :
            base(navigationService: navigationService, logger: logger, projectManager: projectManager, eventAggregator: eventAggregator, mediator: mediator, lifetimeScope: lifetimeScope)
        {
            this.Title = "⳼ ALIGNMENT TOOL";
            this.ContentId = "ALIGNMENTTOOL";
        }

        #endregion //Constructor

        #region Methods

        #endregion // Methods


    }
}
