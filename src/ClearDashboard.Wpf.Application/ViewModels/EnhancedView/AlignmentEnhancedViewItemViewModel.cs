using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.Services;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;


// ReSharper disable InconsistentNaming

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView
{
    public class AlignmentEnhancedViewItemViewModel : VerseAwareEnhancedViewItemViewModel
    {
        public AlignmentEnhancedViewItemViewModel(DashboardProjectManager? projectManager, IEnhancedViewManager enhancedViewManager, INavigationService? navigationService, ILogger<VerseAwareEnhancedViewItemViewModel>? logger, IEventAggregator? eventAggregator, IMediator? mediator, ILifetimeScope? lifetimeScope, IWindowManager windowManager, ILocalizationService localizationService) 
            : base(projectManager, enhancedViewManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, windowManager, localizationService)
        {
        }
    }
}
