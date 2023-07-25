using Autofac;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.Wpf.Application.Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using ClearApplicationFoundation.Framework.Input;
using ClearDashboard.Wpf.Application;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Aqua.Module.Services;
using FluentValidation.Results;
using FluentValidation;
using ClearBible.Engine.Exceptions;
using static ClearDashboard.Aqua.Module.Services.IAquaManager;
using System.Collections.Generic;
using ClearDashboard.Aqua.Module.Models;
using System.Windows;
using Autofac.Features.AttributeFilters;
using ClearApplicationFoundation.Services;

namespace ClearDashboard.Aqua.Module.ViewModels.AquaDialog;

public class AquaRevisionStepViewModel : 
    DashboardApplicationValidatingWorkflowStepViewModel<IAquaDialogViewModel, AquaRevisionStepViewModel>
{
    private readonly IAquaManager? aquaManager_;
    private readonly IEnhancedViewManager? enhancedViewManager_;

    private bool isValidValuesLoaded = false;
    private int? dataLoadedForId_ = null;
    public BindableCollection<Assessment> Items { get; set; } = new();

    private bool hasId_;
    public bool HasId
    {
        get => hasId_;
        set
        {
            hasId_ = value;
            NotifyOfPropertyChange(() => HasId);
        }
    }

    private DialogMode _dialogMode;
    public DialogMode DialogMode
    {
        get => _dialogMode;
        set => Set(ref _dialogMode, value);
    }

    private string? name_;
    public string? Name
    {
        get => name_;
        set
        {
            Set(ref name_, value);
            ValidationResult = Validate();
        }
    }

    private bool published_ = false;
    public bool Published
    {
        get => published_;
        set
        {
            Set(ref published_, value);
        }
    }

    public AquaRevisionStepViewModel()
    {
        OkCommand = new RelayCommand(Ok);
    }
    public AquaRevisionStepViewModel(
        IAquaManager aquaManager,
        IEnhancedViewManager enhancedViewManager,

        DialogMode dialogMode,
        DashboardProjectManager projectManager,
        INavigationService navigationService,
        ILogger<AquaRevisionStepViewModel> logger,
        IEventAggregator eventAggregator,
        IMediator mediator,
        ILifetimeScope? lifetimeScope,
        IValidator<AquaRevisionStepViewModel> validator,
        [KeyFilter("Aqua")] ILocalizationService localizationService)
        : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, validator, localizationService)
    {
        aquaManager_ = aquaManager;
        enhancedViewManager_ = enhancedViewManager;
        DialogMode = dialogMode;

        CanMoveForwards = true;
        CanMoveBackwards = true;
        EnableControls = true;

        OkCommand = new RelayCommand(Ok);
    }
    protected override Task OnInitializeAsync(CancellationToken cancellationToken)
    {
        isValidValuesLoaded = false;
        dataLoadedForId_ = null;
        ParentViewModel!.StatusBarVisibility = Visibility.Visible;
        return base.OnInitializeAsync(cancellationToken);
    }

    protected override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        ParentViewModel!.DialogTitle = $"{LocalizationService!.Get("Aqua_DialogTitle")} - {LocalizationService!.Get("Aqua_Revision_BodyTitle")}";
        await base.OnActivateAsync(cancellationToken);
        return;
    }
    protected override void OnViewReady(object view)
    {
        _ = Reload();
        base.OnViewReady(view);
    }
    private async Task Reload()
    {
        try
        {
            await LoadValidValues();

            var activeId = ParentViewModel!.ActiveRevision?.id;
            HasId = ParentViewModel!.ActiveRevision == null ? false : true;

            if (!HasId) //new
            {
                dataLoadedForId_ = null;
                ClearIdData();
            }
            else
            {
                if (dataLoadedForId_ == null || dataLoadedForId_ != activeId)
                {
                    ClearIdData();
                    GetRevision();
                    dataLoadedForId_ = activeId;
                }
                await GetAssessments();
             }
        }
        catch (Exception ex)
        {
            OnUIThread(() =>
            {
                ParentViewModel!.Message = ex.Message ?? ex.ToString();
            });
        }
    }

    private void ClearIdData()
    {
        Name = null;
        Published= false;
        Items.Clear();
    }
    private Task LoadValidValues()
    {
        if (isValidValuesLoaded)
            return Task.CompletedTask;

        //load valid values
        isValidValuesLoaded = true;
        return Task.CompletedTask;
    }
    public RelayCommand OkCommand { get; }

    public void AddAssessment()
    {
        ParentViewModel!.ActiveAssessment = null;
        MoveForwards();
    }

    public void ViewAssessment(Assessment assessment)
    {
        ParentViewModel!.ActiveAssessment = assessment;
        MoveForwards();
    }
    public void AddItemToEnhancedView(Assessment assessment)
    {
        Logger!.LogInformation($"AddItemToEnhancedView - {assessment.id}");
        enhancedViewManager_!.AddMetadatumEnhancedView(
            new AquaCorpusAnalysisEnhancedViewItemMetadatum()
            {
                AssessmentId = assessment.id,
                VersionId = ParentViewModel!.AquaTokenizedTextCorpusMetadata!.id,
                IsNewWindow = false
            },
            default); 
    }

    public async void DeleteItem(Assessment assessment)
    {
        Logger!.LogInformation($"DeleteItem - {assessment.id}");
        await DeleteAssessment(assessment);
    }

    public void Ok(object obj)
    {
        ParentViewModel!.Ok();
    }

    public void Cancel(object obj)
    {
        ParentViewModel!.Cancel();
    }
    public async void MoveForwards(object obj)
    {
        await MoveForwards();
    }
    public async void MoveBackwards(object obj)
    {
        await MoveBackwards();
    }

    private void GetRevision()
    {
        Name = ParentViewModel!.ActiveRevision?.name;
        Published = ParentViewModel!.ActiveRevision?.published ?? false;
    }
    private void SetAssessmentsList(IEnumerable<Assessment>? assessments)
    {
        Items.Clear();
        if (assessments != null)
            foreach (var assessment in assessments)
                Items.Add(assessment);
    }
    public async Task GetAssessments()
    {
        var processStatus = await ParentViewModel!.RunLongRunningTask(
            "AQuA-Get_Assessments",
            (cancellationToken) => aquaManager_!.ListAssessments(
                ParentViewModel!.ActiveRevision!.id
                    ?? throw new InvalidStateEngineException(name: "ActiveRevision.id", value: "null"),
                cancellationToken),
            SetAssessmentsList);

        switch (processStatus)
        {
            case LongRunningTaskStatus.Completed:
                //await MoveForwards();
                break;
            case LongRunningTaskStatus.Failed:
                break;
            case LongRunningTaskStatus.Cancelled:
                ParentViewModel!.Cancel();
                break;
            case LongRunningTaskStatus.NotStarted:
                break;
            case LongRunningTaskStatus.Running:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public async Task DeleteAssessment(Assessment assessment)
    {
        var processStatus = await ParentViewModel!.RunLongRunningTask(
            "AQuA-Delete_Assessment",
            (cancellationToken) => {
                aquaManager_!.DeleteAssessment(
                    assessment.id 
                        ?? throw new InvalidStateEngineException(name: "assessment.id", value: "null"),
                    cancellationToken);
                return Task.FromResult(""); // so RunLongRunningTask can infer bogus type string
            },
            (_) => Items.Remove(assessment));

        switch (processStatus)
        {
            case LongRunningTaskStatus.Completed:
                //await MoveForwards();
                break;
            case LongRunningTaskStatus.Failed:
                break;
            case LongRunningTaskStatus.Cancelled:
                ParentViewModel!.Cancel();
                break;
            case LongRunningTaskStatus.NotStarted:
                break;
            case LongRunningTaskStatus.Running:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public async void AddRevision()
    {
        var processStatus = await ParentViewModel!.RunLongRunningTask(
            "AQuA-Add_Revision",
            (cancellationToken) => aquaManager_!.AddRevision(
                ParentViewModel!.TokenizedTextCorpusId
                    ?? throw new InvalidStateEngineException(name: "ParentViewModel!.TokenizedTextCorpus", value: "null"),
                new Revision(
                    null,
                    ParentViewModel!.AquaTokenizedTextCorpusMetadata.id
                        ?? throw new InvalidStateEngineException(name: "Abbreviation", value: "null"),
                    Name,
                    null,
                    Published
                ),
                cancellationToken),
            async (revision) => {
                ParentViewModel!.ActiveRevision = revision;

                await Reload();
            });

        switch (processStatus)
        {
            case LongRunningTaskStatus.Completed:
                break;
            case LongRunningTaskStatus.Failed:
                break;
            case LongRunningTaskStatus.Cancelled:
                ParentViewModel!.Cancel();
                break;
            case LongRunningTaskStatus.NotStarted:
                break;
            case LongRunningTaskStatus.Running:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    protected override ValidationResult? Validate()
    {
        return Validator!.Validate(this);
    }
}
