using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.Infrastructure.HelpOverlay
{

    public abstract class HelpOverlayShellViewModel<T> : Conductor<T>.Collection.OneActive where T : class //  Conductor<T>.Collection.OneActive
    {
    }


    public abstract class HelpOverlayStepViewModel<TParentViewModel> : WorkflowStepViewModel
        where TParentViewModel : class
    {
       
        protected ILocalizationService? LocalizationService { get; }
        public TParentViewModel? ParentViewModel => Parent as TParentViewModel;

        protected HelpOverlayStepViewModel()
        {
            //no op
        }

        protected HelpOverlayStepViewModel(INavigationService navigationService, 
            ILogger logger, 
            IEventAggregator eventAggregator,
            IMediator mediator, 
            ILifetimeScope? lifetimeScope, 
            ILocalizationService localizationService)
            : base(navigationService, logger, eventAggregator, mediator, lifetimeScope)
        {
          
            LocalizationService = localizationService;
        }

    }
}
