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
using SIL.Machine.Utils;

namespace ClearDashboard.Wpf.Application.ViewModels.Project.ParallelCorpusDialog;

public class SmtModelStepViewModel : DashboardApplicationWorkflowStepViewModel<ParallelCorpusDialogViewModel>
{
    private bool _canTrain;

    public SmtModelStepViewModel()
    {

    }

    public SmtModelStepViewModel(DashboardProjectManager projectManager,
        INavigationService navigationService, ILogger<SmtModelStepViewModel> logger, IEventAggregator eventAggregator,
        IMediator mediator, ILifetimeScope? lifetimeScope, TranslationSource translationSource)
        : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope)
    {

        CanMoveForwards = true;
        CanMoveBackwards = true;
        EnableControls = true;
        CanTrain = true;
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

    protected override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        ParentViewModel.CurrentStepTitle =
            LocalizationStrings.Get("ParallelCorpusDialog_TrainSmtModel", Logger);
        return base.OnActivateAsync(cancellationToken);
    }

    public async void Train()
    {
        try
        {
            var processStatus =  await ParentViewModel!.TrainSmtModel();

            switch (processStatus)
            {
                case ProcessStatus.Completed:
                    await MoveForwards();
                    break;
                case ProcessStatus.Failed:
                    ParentViewModel.Cancel();
                    break;
                case ProcessStatus.NotStarted:
                    break;
                case ProcessStatus.Running:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        catch (Exception ex)
        {
            ParentViewModel!.Cancel();
        }
    }
}