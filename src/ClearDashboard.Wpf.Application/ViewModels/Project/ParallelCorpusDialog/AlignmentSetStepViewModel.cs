using Autofac;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.ViewModels.Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.ViewModels.Project.ParallelCorpusDialog;

public class AlignmentSetStepViewModel : DashboardApplicationWorkflowStepViewModel
{
    public AlignmentSetStepViewModel(DashboardProjectManager projectManager,
        INavigationService navigationService, ILogger<AlignmentSetStepViewModel> logger, IEventAggregator eventAggregator,
        IMediator mediator, ILifetimeScope? lifetimeScope, TranslationSource translationSource)
        : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope)
    {

        CanMoveForwards = true;
        CanMoveBackwards = true;
        EnableControls = true;

    }
}