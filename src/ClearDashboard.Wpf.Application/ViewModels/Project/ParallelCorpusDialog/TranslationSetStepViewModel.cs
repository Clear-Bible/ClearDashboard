using Autofac;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Infrastructure;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using ClearApplicationFoundation.Services;
using ClearDashboard.Wpf.Application.Services;

namespace ClearDashboard.Wpf.Application.ViewModels.Project.ParallelCorpusDialog;

public class TranslationSetStepViewModel : DashboardApplicationValidatingWorkflowStepViewModel<IParallelCorpusDialogViewModel, TranslationSetStepViewModel>
{
    #region Member Variables   

    #endregion //Member Variables


    #region Public Properties

    #endregion //Public Properties


    #region Observable Properties

    private bool _canAdd;

    public bool CanAdd
    {
        get => _canAdd;
        set => Set(ref _canAdd, value);
    }

    private DialogMode _dialogMode;
    public DialogMode DialogMode
    {
        get => _dialogMode;
        set => Set(ref _dialogMode, value);
    }

    private string _translationSetDisplayName;

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

    #endregion //Observable Properties


    #region Constructor

    public TranslationSetStepViewModel()
    {

    }

    public TranslationSetStepViewModel(DialogMode dialogMode, DashboardProjectManager projectManager,
        INavigationService navigationService, ILogger<TranslationSetStepViewModel> logger, IEventAggregator eventAggregator,
        IMediator mediator, ILifetimeScope? lifetimeScope, TranslationSource translationSource, IValidator<TranslationSetStepViewModel> validator, ILocalizationService localizationService)
        : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, validator, localizationService)
    {
        DialogMode = dialogMode;
        CanMoveForwards = true;
        CanMoveBackwards = true;
        EnableControls = true;
        CanAdd = false;

    }

    protected override Task OnInitializeAsync(CancellationToken cancellationToken)
    {

        return base.OnInitializeAsync(cancellationToken);
    }

    protected override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        // skip this step if we are only adding an alignment
        if (ParentViewModel.ProjectType == Enums.ParallelProjectType.AlignmentOnly)
        {
            ParentViewModel.Ok();
            return;
        }



        ParentViewModel.CurrentStepTitle =
            LocalizationService!.Get("ParallelCorpusDialog_AddTranslationSet");

        var gloss = LocalizationService!.Get("AddParatextCorpusDialog_Interlinear");

        TranslationSetDisplayName =
            $"{ParentViewModel.SourceCorpusNodeViewModel.Name} - {ParentViewModel.TargetCorpusNodeViewModel.Name} {gloss}";

        if (ParentViewModel.UseDefaults)
        {
            await Add(true);
        }
        
        base.OnActivateAsync(cancellationToken);
    }

    #endregion //Constructor


    #region Methods

    protected override ValidationResult? Validate()
    {
        return (!string.IsNullOrEmpty(TranslationSetDisplayName)) ? Validator.Validate(this) : null;
    }

    public async void Add()
    {
        await Add(true);
    }

    public async Task Add(object nothing)
    {
        CanAdd = false;
        _ = await Task.Factory.StartNew(async () =>
        {
            try
            {
                var processStatus = await ParentViewModel!.AddTranslationSet(TranslationSetDisplayName);

                switch (processStatus)
                {
                    case LongRunningTaskStatus.Completed:
                        
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

    #endregion // Methods
}