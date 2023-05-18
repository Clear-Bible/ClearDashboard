
using Autofac;
using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView
{
    public class AlignmentsBatchReviewViewModel : EnhancedViewItemViewModel
    {
        public AlignmentsBatchReviewViewModel(DashboardProjectManager? projectManager,
            IEnhancedViewManager enhancedViewManager,
            INavigationService? navigationService,
            ILogger<AlignmentsBatchReviewViewModel>? logger,
            IEventAggregator? eventAggregator,
            IMediator? mediator,
            ILifetimeScope? lifetimeScope,
            ILocalizationService localizationService) :
            base(projectManager, enhancedViewManager, navigationService, logger, eventAggregator, mediator,
                lifetimeScope, localizationService)
        {
        }
    }
}
