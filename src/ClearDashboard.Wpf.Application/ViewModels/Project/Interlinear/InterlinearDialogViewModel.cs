using Autofac;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.DataAccessLayer.Wpf.Infrastructure;
using ClearDashboard.Wpf.Application.Helpers;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.ViewModels.Project.Interlinear
{
    public class InterlinearDialogViewModel : DashboardApplicationScreen
    {

        private TranslationSource? _translationSource;

        public InterlinearDialogViewModel()
        {

        }

        public InterlinearDialogViewModel(TranslationSource? translationSource, INavigationService navigationService,
            ILogger<InterlinearDialogViewModel> logger, DashboardProjectManager? projectManager, IEventAggregator eventAggregator,
            IWindowManager windowManager, IMediator mediator, ILifetimeScope lifetimeScope)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope)
        {
            _translationSource = translationSource;

            Logger.LogInformation("'ShellViewModel' ctor called.");
        }
    }
}
