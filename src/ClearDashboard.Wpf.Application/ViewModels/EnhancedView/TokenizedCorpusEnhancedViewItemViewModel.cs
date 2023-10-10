using Autofac;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;


// ReSharper disable InconsistentNaming

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView
{
    public class TokenizedCorpusEnhancedViewItemViewModel : VerseAwareEnhancedViewItemViewModel
    {
        public TokenizedCorpusEnhancedViewItemViewModel(
            DashboardProjectManager? projectManager, 
            IEnhancedViewManager enhancedViewManager, 
            INavigationService? navigationService, 
            ILogger<VerseAwareEnhancedViewItemViewModel>? logger, 
            IEventAggregator? eventAggregator, 
            IMediator? mediator, ILifetimeScope? lifetimeScope, 
            IWindowManager windowManager, 
            ILocalizationService localizationService, 
            NoteManager noteManager) : base(
                projectManager, 
                enhancedViewManager, 
                navigationService, 
                logger, 
                eventAggregator, 
                mediator, 
                lifetimeScope, 
                windowManager, 
                localizationService, 
                noteManager)
        {
        }
    }
}
