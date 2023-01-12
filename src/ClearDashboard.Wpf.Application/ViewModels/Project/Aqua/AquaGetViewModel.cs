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

namespace ClearDashboard.Wpf.Application.ViewModels.Project.Aqua;

public class AquaGetViewModel : DashboardApplicationWorkflowStepViewModel<IAquaDialogViewModel>
{
    public AquaGetViewModel()
    {
    }
    public AquaGetViewModel( 
        DialogMode dialogMode,  
        DashboardProjectManager projectManager,
        INavigationService navigationService, 
        ILogger<AquaMakeRequestStepViewModel> logger, 
        IEventAggregator eventAggregator,
        IMediator mediator, 
        ILifetimeScope? lifetimeScope)
        : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope)
    {
        DialogMode = dialogMode;
        CanMoveForwards = true;
        CanMoveBackwards = true;
        EnableControls = true;
    }

    private DialogMode _dialogMode;
    public DialogMode DialogMode
    {
        get => _dialogMode;
        set => Set(ref _dialogMode, value);
    }
    protected override Task OnInitializeAsync(CancellationToken cancellationToken)
    {
        return base.OnInitializeAsync(cancellationToken);
    }

    protected override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        ParentViewModel!.CurrentStepTitle =
            LocalizationStrings.Get("AquaRequestCorpusAnalysisDialog_MakeRequestStep", Logger!);

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


    public LongRunningTask? CurrentTask { get; set; }
    public async void Get()
    {
        _ = await Task.Factory.StartNew(async () =>
        {
            try
            {
                var processStatus = await ParentViewModel!.RequestAnalysis();

                switch (processStatus)
                {
                    case LongRunningTaskStatus.Completed:
                        await MoveForwards();
                        break;
                    case LongRunningTaskStatus.Failed:
                    case LongRunningTaskStatus.Cancelled:
                        ParentViewModel.Cancel();
                        break;
                    case LongRunningTaskStatus.NotStarted:
                        break;
                    case LongRunningTaskStatus.Running:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception)
            {
                ParentViewModel!.Cancel();
            }
        }, CancellationToken.None);
    }
}