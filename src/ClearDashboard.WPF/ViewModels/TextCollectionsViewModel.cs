using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.ViewModels.Panes;
using ClearDashboard.Wpf.Views;
using Helpers;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.Wpf.ViewModels
{
    public class TextCollectionsViewModel : ToolViewModel, IHandle<TextCollectionChangedMessage>
    {

        #region Member Variables

     
        #endregion //Member Variables

        #region Public Properties


        #endregion //Public Properties

        #region Observable Properties


        #endregion //Observable Properties

        #region Constructor
        public TextCollectionsViewModel()
        {

        }

        public TextCollectionsViewModel(INavigationService navigationService, ILogger<TextCollectionsViewModel> logger, DashboardProjectManager projectManager, IEventAggregator eventAggregator) 
            : base(navigationService, logger, projectManager, eventAggregator)
        {
            this.Title = "🗐 TEXT COLLECTION";
            this.ContentId = "TEXTCOLLECTION";
        }



        #endregion //Constructor

        #region Methods

        public void LaunchMirrorView(double actualWidth, double actualHeight)
        {
            LaunchMirrorView<TextCollectionsView>.Show(this, actualWidth, actualHeight);
        }

        public Task HandleAsync(TextCollectionChangedMessage message, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        #endregion // Methods


    }
}
