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

namespace ClearDashboard.Wpf.Application.ViewModels.Project.ParallelCorpusDialog;

public class AlignmentSetStepViewModel : DashboardApplicationWorkflowStepViewModel<ParallelCorpusDialogViewModel>
{
   

    public AlignmentSetStepViewModel()
    {

    }

    public AlignmentSetStepViewModel(DashboardProjectManager projectManager,
        INavigationService navigationService, ILogger<AlignmentSetStepViewModel> logger, IEventAggregator eventAggregator,
        IMediator mediator, ILifetimeScope? lifetimeScope, TranslationSource translationSource)
        : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope)
    {

        CanMoveForwards = true;
        CanMoveBackwards = true;
        CanOk = true;
        EnableControls = true;

        CanAdd = true;

    }

    private bool _canAdd;
    public bool CanAdd
    {
        get => _canAdd;
        set => Set(ref _canAdd, value);
    }

    private string _alignmentSetDisplayName;
    public string AlignmentSetDisplayName
    {
        get => _alignmentSetDisplayName;
        set => Set(ref _alignmentSetDisplayName, value);
    }

    protected override Task OnInitializeAsync(CancellationToken cancellationToken)
    {
        
        return base.OnInitializeAsync(cancellationToken);
    }

    protected override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        ParentViewModel.CurrentStepTitle =
            LocalizationStrings.Get("ParallelCorpusDialog_AddAlignmentSet", Logger);
        return base.OnActivateAsync(cancellationToken);
    }

    private bool _canOk;
    public bool CanOk
    {
        get => _canOk;
        set => Set(ref _canOk, value);
    }

    public void Ok()
    {
       ParentViewModel?.Ok();
    }

    public async void Add()
    {
        try {
            var processStatus = await ParentViewModel!.AddAlignmentSet(AlignmentSetDisplayName);

            switch (processStatus)
            {
                case ProcessStatus.Completed:
                    ParentViewModel.Ok();
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