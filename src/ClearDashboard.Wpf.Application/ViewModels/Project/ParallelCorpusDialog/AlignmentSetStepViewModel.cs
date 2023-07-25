using Autofac;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Services;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using ClearApplicationFoundation.Services;


namespace ClearDashboard.Wpf.Application.ViewModels.Project.ParallelCorpusDialog;

public class AlignmentSetStepViewModel : DashboardApplicationValidatingWorkflowStepViewModel<IParallelCorpusDialogViewModel, AlignmentSetStepViewModel>
{

    #region Member Variables



    #endregion //Member Variables


    #region Public Properties

    #endregion //Public Properties


    #region Observable Properties

    private DialogMode _dialogMode;
    public DialogMode DialogMode
    {
        get => _dialogMode;
        set => Set(ref _dialogMode, value);
    }

    private bool _canOk;
    public bool CanOk
    {
        get => _canOk;
        set => Set(ref _canOk, value);
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

    #endregion //Observable Properties


    #region Constructor

    public AlignmentSetStepViewModel()
    {

    }

    public AlignmentSetStepViewModel(DialogMode dialogMode, DashboardProjectManager projectManager,
        INavigationService navigationService, ILogger<AlignmentSetStepViewModel> logger, IEventAggregator eventAggregator,
        IMediator mediator, ILifetimeScope? lifetimeScope, TranslationSource translationSource, IValidator<AlignmentSetStepViewModel> validator, ILocalizationService localizationService)
        : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, validator,localizationService)
    {

        DialogMode = dialogMode;
        CanMoveForwards = true;
        CanMoveBackwards = true;
        CanOk = true;
        EnableControls = true;

        CanAdd = false;

    }

    protected override Task OnInitializeAsync(CancellationToken cancellationToken)
    {

        return base.OnInitializeAsync(cancellationToken);
    }

    protected async override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        ParentViewModel.CurrentStepTitle =
            LocalizationService!.Get("ParallelCorpusDialog_AddAlignmentSet");

        var alignment = LocalizationService!.Get("AddParatextCorpusDialog_Alignment"); 

        AlignmentSetDisplayName =
            $"{ParentViewModel.SourceCorpusNodeViewModel.Name} - {ParentViewModel.TargetCorpusNodeViewModel.Name} {alignment}";

        if (ParentViewModel.UseDefaults)
        {
            await Add(true);
        }

        base.OnActivateAsync(cancellationToken);
    }

    #endregion //Constructor


    #region Methods

    public void Ok()
    {
        ParentViewModel?.Ok();
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

        if (ParentViewModel.ProjectType == Enums.ParallelProjectType.AlignmentOnly)
        {
            // bail out
            Console.WriteLine();
        }
    }

    protected override ValidationResult? Validate()
    {
        return (!string.IsNullOrEmpty(AlignmentSetDisplayName)) ? Validator.Validate(this) : null;
    }

    #endregion // Methods
}