using Autofac;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.DataAccessLayer.Wpf.Infrastructure;
using ClearDashboard.Wpf.Application.Helpers;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.ViewModels.Project.ParallelCorpusDialog;

public class TranslationSetStepViewModel : DashboardApplicationWorkflowStepViewModel<ParallelCorpusDialogViewModel>
{
    public TranslationSetStepViewModel(DashboardProjectManager projectManager,
        INavigationService navigationService, ILogger<TranslationSetStepViewModel> logger, IEventAggregator eventAggregator,
        IMediator mediator, ILifetimeScope? lifetimeScope, TranslationSource translationSource)
        : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope)
    {

        CanMoveForwards = true;
        CanMoveBackwards = true;
        EnableControls = true;

    }
}