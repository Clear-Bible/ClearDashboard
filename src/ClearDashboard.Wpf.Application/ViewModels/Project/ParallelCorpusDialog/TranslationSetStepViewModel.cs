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
using System;
using ClearDashboard.Wpf.Application.Validators;
using FluentValidation;
using FluentValidation.Results;

namespace ClearDashboard.Wpf.Application.ViewModels.Project.ParallelCorpusDialog;

public class TranslationSetStepViewModel : DashboardApplicationValidatingWorkflowStepViewModel<IParallelCorpusDialogViewModel, TranslationSetStepViewModel>
{
    private bool _canAdd;
    private string _translationSetDisplayName;

    public TranslationSetStepViewModel()
    {

    }

    public TranslationSetStepViewModel(DialogMode dialogMode, DashboardProjectManager projectManager,
        INavigationService navigationService, ILogger<TranslationSetStepViewModel> logger, IEventAggregator eventAggregator,
        IMediator mediator, ILifetimeScope? lifetimeScope, TranslationSource translationSource, IValidator<TranslationSetStepViewModel> validator)
        : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, validator)
    {
        DialogMode = dialogMode;
        CanMoveForwards = true;
        CanMoveBackwards = true;
        EnableControls = true;
        CanAdd = false;

    }

    private DialogMode _dialogMode;
    public DialogMode DialogMode
    {
        get => _dialogMode;
        set => Set(ref _dialogMode, value);
    }

    public bool CanAdd
    {
        get => _canAdd;
        set => Set(ref _canAdd, value);
    }

    public string TranslationSetDisplayName
    {
        get => _translationSetDisplayName;
        set
        {
            Set(ref _translationSetDisplayName, value);
            ValidationResult = Validator.Validate(this);
            CanAdd = !string.IsNullOrEmpty(value) && ValidationResult.IsValid;
        }
    }


    public async void Add()
    {
        CanAdd = false;
        ParentViewModel!.CreateCancellationTokenSource();
        _ = await Task.Factory.StartNew(async () =>
        {
            try
            {
                var processStatus = await ParentViewModel!.AddTranslationSet(TranslationSetDisplayName);

                switch (processStatus)
                {
                    case ProcessStatus.Completed:
                        
                        //if (ParentViewModel.Steps.Count > 3)
                        //{
                        //    await MoveForwards();
                        //}
                        //else
                        //{
                        //    ParentViewModel.Ok();
                        //}

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
        }, ParentViewModel!.CancellationTokenSource!.Token);

    }

    protected override Task OnInitializeAsync(CancellationToken cancellationToken)
    {

        return base.OnInitializeAsync(cancellationToken);
    }

    protected override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        ParentViewModel.CurrentStepTitle =
            LocalizationStrings.Get("ParallelCorpusDialog_AddTranslationSet", Logger);
        return base.OnActivateAsync(cancellationToken);
    }

    protected override ValidationResult? Validate()
    {
        return (!string.IsNullOrEmpty(TranslationSetDisplayName)) ? Validator.Validate(this) : null;
    }
}