using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.Services;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.ViewModels.Startup.ProjectTemplate
{
    public class UsfmCheckStepViewModel : DashboardApplicationWorkflowStepViewModel<StartupDialogViewModel>
    {
        public UsfmCheckStepViewModel(DashboardProjectManager projectManager,
            INavigationService navigationService, ILogger<ProjectSetupViewModel> logger, IEventAggregator eventAggregator,
            IMediator mediator, ILifetimeScope? lifetimeScope, TranslationSource translationSource, ILocalizationService localizationService)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
        {
            CanMoveForwards = true;
            CanMoveBackwards = true;
            EnableControls = true;
        }
    }


}
