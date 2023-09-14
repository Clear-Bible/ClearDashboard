using Autofac;
using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.Panes;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.ViewModels.Lexicon
{
    public class LexiconViewModel : ToolViewModel
    {
        public LexiconViewModel()
        {
            
        }
        public LexiconViewModel(INavigationService navigationService, 
            ILogger<LexiconViewModel> logger, 
            DashboardProjectManager dashboardProjectManager, 
            IEventAggregator eventAggregator, 
            IMediator mediator, 
            ILifetimeScope lifetimeScope, 
            ILocalizationService localizationService) :
            base(navigationService, logger, dashboardProjectManager, eventAggregator, mediator, lifetimeScope, localizationService)
        {
            
        }
    }
}
