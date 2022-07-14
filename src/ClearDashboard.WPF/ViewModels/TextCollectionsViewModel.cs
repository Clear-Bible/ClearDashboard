using System;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.Helpers;
using ClearDashboard.Wpf.ViewModels.Panes;
using ClearDashboard.Wpf.Views;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.ParatextPlugin.CQRS.Features.BiblicalTerms;
using ClearDashboard.ParatextPlugin.CQRS.Features.TextCollections;

namespace ClearDashboard.Wpf.ViewModels
{
    public class TextCollectionsViewModel : ToolViewModel, IHandle<TextCollectionChangedMessage>, IHandle<VerseChangedMessage>
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

        protected async override void OnViewAttached(object view, object context)
        {
            try
            {
                var result = await ExecuteRequest(new GetTextCollectionsQuery(), CancellationToken.None)
                    .ConfigureAwait(false);
                if (result.Success)
                {
                    var data = result.Data;

                    await EventAggregator.PublishOnUIThreadAsync(new LogActivityMessage($"{this.DisplayName}: TextCollections read"));
                }

            }
            catch (Exception e)
            {
                Logger.LogError($"BiblicalTermsViewModel Deserialize BiblicalTerms: {e.Message}");
            }

            base.OnViewAttached(view, context);
        }


        #endregion //Constructor

        #region Methods

        public void LaunchMirrorView(double actualWidth, double actualHeight)
        {
            LaunchMirrorView<TextCollectionsView>.Show(this, actualWidth, actualHeight);
        }

        public Task HandleAsync(TextCollectionChangedMessage message, CancellationToken cancellationToken)
        {
            // TODO
            return Task.CompletedTask;
        }

        public Task HandleAsync(VerseChangedMessage message, CancellationToken cancellationToken)
        {
            // TODO
            return Task.CompletedTask;
        }

        #endregion // Methods


    }
}
