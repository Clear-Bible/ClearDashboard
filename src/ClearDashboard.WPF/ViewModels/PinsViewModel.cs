using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.ViewModels.Panes;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.Wpf.ViewModels
{
    public class PinsViewModel : ToolViewModel
    {

        #region Member Variables

        #endregion //Member Variables

        #region Public Properties


        #endregion //Public Properties

        #region Observable Properties

        #endregion //Observable Properties

        #region Constructor

        public PinsViewModel()
        {
        }

        public PinsViewModel(INavigationService navigationService, ILogger<PinsViewModel> logger, DashboardProjectManager projectManager): base(navigationService,logger, projectManager)
        {
            this.Title = "⍒ PINS";
            this.ContentId = "PINS";
        }

        protected override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            Debug.WriteLine("");
            return base.OnActivateAsync(cancellationToken);
        }

        protected override async void OnViewAttached(object view, object context)
        {
            base.OnViewAttached(view, context);
        }


        protected override async void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
        }

        #endregion //Constructor

        #region Methods


        #endregion // Methods
    }
}
