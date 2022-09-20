using Autofac;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.ViewModels.Infrastructure;
using ClearDashboard.Wpf.Application.ViewModels.Startup;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.ViewModels.Project.ParallelCorpusDialog;

public class AlignmentSetStepViewModel : DashboardApplicationWorkflowStepViewModel<ParallelCorpusDialogViewModel>
{
    private bool _canOk;

    public AlignmentSetStepViewModel(DashboardProjectManager projectManager,
        INavigationService navigationService, ILogger<AlignmentSetStepViewModel> logger, IEventAggregator eventAggregator,
        IMediator mediator, ILifetimeScope? lifetimeScope, TranslationSource translationSource)
        : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope)
    {

        CanMoveForwards = true;
        CanMoveBackwards = true;
        CanOk = true;
        EnableControls = true;

    }

    public bool CanOk
    {
        get => _canOk;
        set => Set(ref _canOk, value);
    }

    public void Ok()
    {
        var dialogViewModel = Parent as ParallelCorpusDialogViewModel;
       // startupDialogViewModel!.ExtraData = ProjectManager.CurrentDashboardProject;
        dialogViewModel?.Ok();
    }
}