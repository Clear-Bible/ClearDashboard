using System;
using Autofac;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.DataAccessLayer.Wpf.Infrastructure;
using ClearDashboard.Wpf.Application.Helpers;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using ClearDashboard.DataAccessLayer.Threading;
using System.Windows;

namespace ClearDashboard.Wpf.Application.ViewModels.Project.Aqua;

public class AquaInfoStepViewModel : DashboardApplicationWorkflowStepViewModel<IAquaDialogViewModel>
{
    public AquaInfoStepViewModel()
    {
    }
    public AquaInfoStepViewModel( 
        DialogMode dialogMode,  
        DashboardProjectManager projectManager,
        INavigationService navigationService, 
        ILogger<AquaMakeRequestStepViewModel> logger, 
        IEventAggregator eventAggregator,
        IMediator mediator, 
        ILifetimeScope? lifetimeScope,
        LongRunningTaskManager longRunningTaskManager)
        : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope)
    {
        DialogMode = dialogMode;
        LongRunningTaskManager = longRunningTaskManager;
        CanMoveForwards = true;
        CanMoveBackwards = true;
        EnableControls = true;

        OkCommand = new RelayCommand(Ok);
    }

    public RelayCommand OkCommand { get; }

    private DialogMode _dialogMode;
    public DialogMode DialogMode
    {
        get => _dialogMode;
        set => Set(ref _dialogMode, value);
    }
    public LongRunningTaskManager LongRunningTaskManager { get; }
    protected override Task OnInitializeAsync(CancellationToken cancellationToken)
    {
        ParentViewModel.StatusBarVisibility = Visibility.Hidden;
        return base.OnInitializeAsync(cancellationToken);
    }

    protected override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        ParentViewModel!.CurrentStepTitle =
            LocalizationStrings.Get("AquaRequestCorpusAnalysisDialog_InfoStep", Logger!);

        return base.OnActivateAsync(cancellationToken);
    }
    public void Ok(object obj)
    {
        ParentViewModel?.Ok();
    }

    public void Cancel(object obj)
    {
        ParentViewModel?.Cancel();
    }
    public async void MoveForwards(object obj)
    {
        await MoveForwards();
    }
    public async void MoveBackwards(object obj)
    {
        await MoveBackwards();
    }
}