using System;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.Wpf.ViewModels.Panes;
using Microsoft.Extensions.Logging;
using System.Windows;
using ClearDashboard.DataAccessLayer.Wpf;
using System.Threading.Tasks;
using System.Threading;

namespace ClearDashboard.Wpf.ViewModels
{
    public class NotesViewModel : ToolViewModel
    {

        #region Member Variables

       

        #endregion //Member Variables

        #region Public Properties


        #endregion //Public Properties

        #region Observable Properties

      

        #endregion //Observable Properties

        #region Constructor
        public NotesViewModel()
        {

        }

        public NotesViewModel(INavigationService navigationService, ILogger<NotesViewModel> logger, DashboardProjectManager projectManager, IEventAggregator eventAggregator):base(navigationService, logger, projectManager, eventAggregator)
        {
            this.Title = "🖉 NOTES";
            this.ContentId = "NOTES";

        }

        protected override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine();
            return base.OnActivateAsync(cancellationToken);
        }

        protected override void OnViewLoaded(object view)
        {
            Console.WriteLine();
            base.OnViewLoaded(view);
        }

        #endregion //Constructor

        #region Methods

        #endregion // Methods


    }
}
