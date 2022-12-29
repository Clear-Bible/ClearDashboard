using Autofac;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Wpf;

using MediatR;
using Microsoft.Extensions.Logging;


// ReSharper disable InconsistentNaming

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView
{
    public class TokenizedCorpusEnhancedViewItemViewModel : VerseAwareEnhancedViewItemViewModel
    {
        public TokenizedCorpusEnhancedViewItemViewModel(DashboardProjectManager? projectManager, INavigationService? navigationService, ILogger<VerseAwareEnhancedViewItemViewModel>? logger, IEventAggregator? eventAggregator, IMediator? mediator, ILifetimeScope? lifetimeScope, IWindowManager windowManager) : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, windowManager)
        {
        }
    }
}
