using Autofac;
using Autofac.Features.AttributeFilters;
using Caliburn.Micro;
using ClearDashboard.Wpf.Application;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;
using MediatR;
using Microsoft.Extensions.Logging;


// ReSharper disable InconsistentNaming

namespace ClearDashboard.Sample.Module.ViewModels
{
    public class SampleCorpusAnalysisEnhancedViewItemViewModel : VerseAwareEnhancedViewItemViewModel
    {
        public SampleCorpusAnalysisEnhancedViewItemViewModel(DashboardProjectManager? projectManager,  IEnhancedViewManager enhancedViewManager, INavigationService? navigationService, ILogger<VerseAwareEnhancedViewItemViewModel>? logger, IEventAggregator? eventAggregator, IMediator? mediator, ILifetimeScope? lifetimeScope, IWindowManager windowManager, [KeyFilter("Sample")] ILocalizationService localizationService)
            : base(projectManager, enhancedViewManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, windowManager, localizationService)
        {
        }
    }
}
