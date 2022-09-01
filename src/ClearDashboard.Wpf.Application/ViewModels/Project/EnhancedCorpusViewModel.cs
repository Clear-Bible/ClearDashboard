using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.ViewModels.Panes;
using ClearDashboard.Wpf.Application.Views.ParatextViews;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.ViewModels.Project
{
    public class EnhancedCorpusViewModel : PaneViewModel, IHandle<VerseChangedMessage>
    {

        #region Member Variables      

        #endregion //Member Variables

        #region Public Properties
        
        public string ContentID => this.ContentID;
        
        #endregion //Public Properties

        #region Observable Properties

        #endregion //Observable Properties

        #region Constructor

        public EnhancedCorpusViewModel()
        {
            
        }

        public EnhancedCorpusViewModel(INavigationService navigationService, ILogger<EnhancedCorpusViewModel> logger,
            DashboardProjectManager projectManager, IEventAggregator eventAggregator, IMediator mediator, ILifetimeScope? lifetimeScope) :
            base(navigationService, logger, projectManager, eventAggregator, mediator, lifetimeScope)
        {
            this.Title = "⳼ ENHANCED CORPUS";
            this.ContentId = "ENHANCEDCORPUS";
        }

        #endregion //Constructor

        #region Methods

        public void LaunchMirrorView(double actualWidth, double actualHeight)
        {
            LaunchMirrorView<TextCollectionsView>.Show(this, actualWidth, actualHeight);
        }

        public Task HandleAsync(VerseChangedMessage message, CancellationToken cancellationToken)
        {
            // TODO

            return Task.CompletedTask;
        }

        #endregion // Methods
    }
}
