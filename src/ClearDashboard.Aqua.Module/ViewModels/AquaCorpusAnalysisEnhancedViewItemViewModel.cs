using Autofac;
using Caliburn.Micro;
using ClearDashboard.Wpf.Application;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;
using MediatR;
using Microsoft.Extensions.Logging;


// ReSharper disable InconsistentNaming

namespace ClearDashboard.Aqua.Module.ViewModels
{
    public class AquaCorpusAnalysisEnhancedViewItemViewModel : VerseAwareEnhancedViewItemViewModel
    {
        public AquaCorpusAnalysisEnhancedViewItemViewModel(DashboardProjectManager? projectManager, INavigationService? navigationService, ILogger<VerseAwareEnhancedViewItemViewModel>? logger, IEventAggregator? eventAggregator, IMediator? mediator, ILifetimeScope? lifetimeScope, IWindowManager windowManager, ILocalizationService localizationService)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, windowManager, localizationService)
        {
        }
    }
}
