using Autofac;
using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ClearDashboard.Wpf.Application.ViewModels.Startup.ProjectTemplate
{
    public class ScopeSelectionStepViewModel : DashboardApplicationWorkflowStepViewModel<StartupDialogViewModel>
    {
        public ScopeSelectionStepViewModel(DashboardProjectManager projectManager,
            INavigationService navigationService, ILogger<ProjectSetupViewModel> logger, IEventAggregator eventAggregator,
            IMediator mediator, ILifetimeScope? lifetimeScope, TranslationSource translationSource, ILocalizationService localizationService)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
        {
            CanMoveForwards = true;
            CanMoveBackwards = true;
            EnableControls = true;
        }

        public async Task CreateProjectTemplate()
        {
            // FIXME
            await ParentViewModel!.GoToStep(1);
            //ParentViewModel?.Ok();
        }
    }


}
