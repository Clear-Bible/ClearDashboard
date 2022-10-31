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
using FluentValidation;
using FluentValidation.Results;
using ClearDashboard.Wpf.Application.Threading;

namespace ClearDashboard.Wpf.Application.ViewModels.Project.ParallelCorpusDialog;

public class AlignmentSetStepViewModel : DashboardApplicationValidatingWorkflowStepViewModel<IParallelCorpusDialogViewModel, AlignmentSetStepViewModel>
{
   

    public AlignmentSetStepViewModel()
    {

    }

    public AlignmentSetStepViewModel(DialogMode dialogMode, DashboardProjectManager projectManager,
        INavigationService navigationService, ILogger<AlignmentSetStepViewModel> logger, IEventAggregator eventAggregator,
        IMediator mediator, ILifetimeScope? lifetimeScope, TranslationSource translationSource, IValidator<AlignmentSetStepViewModel> validator)
        : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, validator)
    {

        DialogMode = dialogMode;
        CanMoveForwards = true;
        CanMoveBackwards = true;
        CanOk = true;
        EnableControls = true;

        CanAdd = false;

    }

    private DialogMode _dialogMode;
    public DialogMode DialogMode
    {
        get => _dialogMode;
        set => Set(ref _dialogMode, value);
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
        set
        {
            Set(ref _alignmentSetDisplayName, value);
            ValidationResult = Validator.Validate(this);
            CanAdd = !string.IsNullOrEmpty(value) && ValidationResult.IsValid;
        }
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
        CanAdd = false;
        ParentViewModel!.CreateCancellationTokenSource();
        _ = await Task.Factory.StartNew(async () =>
        {
            try
            {
                var processStatus = await ParentViewModel!.AddAlignmentSet(AlignmentSetDisplayName);

                switch (processStatus)
                {
                    case LongRunningTaskStatus.Completed:
                        if (ParentViewModel.Steps.Count > 3)
                        {
                            await MoveForwards();
                        }
                        else
                        {
                            ParentViewModel.Ok();
                        }

                        break;
                    case LongRunningTaskStatus.Failed:
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
        }, ParentViewModel!.CancellationTokenSource!.Token);
    }

    protected override ValidationResult? Validate()
    {
        return (!string.IsNullOrEmpty(AlignmentSetDisplayName)) ? Validator.Validate(this) : null;
    }
}