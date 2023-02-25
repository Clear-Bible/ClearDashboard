using Autofac;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.Wpf.Application.Services;

namespace ClearDashboard.Wpf.Application.ViewModels.Project.ParallelCorpusDialog;

public class SmtModelStepViewModel : DashboardApplicationWorkflowStepViewModel<IParallelCorpusDialogViewModel>
{
    private bool _canTrain;

    public SmtModelStepViewModel()
    {

    }

    public SmtModelStepViewModel( DialogMode dialogMode,  DashboardProjectManager projectManager,
        INavigationService navigationService, ILogger<SmtModelStepViewModel> logger, IEventAggregator eventAggregator,
        IMediator mediator, ILifetimeScope? lifetimeScope, TranslationSource translationSource, ILocalizationService localizationService)
        : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
    {
        DialogMode = dialogMode;
        CanMoveForwards = true;
        CanMoveBackwards = true;
        EnableControls = true;
        CanTrain = true;
    }

    private DialogMode _dialogMode;
    public DialogMode DialogMode
    {
        get => _dialogMode;
        set => Set(ref _dialogMode, value);
    }

    public bool CanTrain

    {
        get => _canTrain;
        set => Set(ref _canTrain, value);
    }

    protected override Task OnInitializeAsync(CancellationToken cancellationToken)
    {

        return base.OnInitializeAsync(cancellationToken);
    }

    protected override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        ParentViewModel.CurrentStepTitle =
            LocalizationService!.Get("ParallelCorpusDialog_TrainSmtModel");

        if (ParentViewModel.UseDefaults)
        {
            await Train(true);
        }

        base.OnActivateAsync(cancellationToken);
    }

    public async void Train()
    {
        await Train(true);
    }


    public async Task Train(object nothing)
    {
        CanTrain = false;
        _ = await Task.Factory.StartNew(async () =>
        {
            try
            {
                var processStatus = await ParentViewModel!.TrainSmtModel();

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
            catch (Exception ex)
            {
                ParentViewModel!.Cancel();
            }
        }, CancellationToken.None);
    }
}