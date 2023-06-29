using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Autofac.Features.AttributeFilters;
using Caliburn.Micro;
using ClearBible.Engine.Exceptions;
using ClearDashboard.Aqua.Module.Services;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.Wpf.Application;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Services;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using static ClearDashboard.Aqua.Module.Services.IAquaManager;

namespace ClearDashboard.Aqua.Module.ViewModels.AquaDialog;

public class AquaAssessmentStepViewModel : 
    DashboardApplicationValidatingWorkflowStepViewModel<IAquaDialogViewModel, AquaAssessmentStepViewModel>
{
    private readonly IAquaManager? aquaManager_;

    private bool isValidValuesLoaded = false;
    private int? DataLoadedForId_ = null;

    public BindableCollection<string> Types { get; set; } = new()
    {
        "missing-words",
        "semantic-similarity",
        "sentence-length",
        "word-alignment"
    };

    public BindableCollection<Revision> Revisions { get; set; } = new();

    private Revision? revision_;
    public Revision? Revision
    {
        get => revision_;
        set
        {
            revision_ = value;
            NotifyOfPropertyChange(() => Revision);
        }
    }

    private bool revisionEnabled_ = false;
    public bool RevisionEnabled
    {
        get => revisionEnabled_;
        set => Set(ref revisionEnabled_, value);
    }
    private string? type_ = null;
    public string? Type
    {
        get => type_;
        set
        {
            type_ = value;
            if (type_ == "sentence-length")
            {
                RevisionEnabled = false;
                Revision = null;
            }
            else
            {
                RevisionEnabled = true;
                Revision = null;
            }

            NotifyOfPropertyChange(() => Type);
        }
    }

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

    private bool published_ = false;
    public bool Published
    {
        get => published_;
        set
        {
            Set(ref published_, value);
        }
    }

    private string? modalSuffix_ = null;
    public string? ModalSuffix
    {
        get => modalSuffix_;
        set
        {
            Set(ref modalSuffix_, value);
        }
    }

    public AquaAssessmentStepViewModel()
    {
    }
    public AquaAssessmentStepViewModel(
        IAquaManager aquaManager,

        DialogMode dialogMode,  
        DashboardProjectManager projectManager,
        INavigationService navigationService, 
        ILogger<AquaAssessmentStepViewModel> logger, 
        IEventAggregator eventAggregator,
        IMediator mediator, 
        ILifetimeScope? lifetimeScope,
        [KeyFilter("Aqua")] ILocalizationService localizationService,
        IValidator<AquaAssessmentStepViewModel> validator)
        : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, validator, localizationService)
    {
        aquaManager_ = aquaManager;
        DialogMode = dialogMode;
        CanMoveForwards = true;
        CanMoveBackwards = true;
        EnableControls = true;
    }
    protected override Task OnInitializeAsync(CancellationToken cancellationToken)
    {
        isValidValuesLoaded = false;
        DataLoadedForId_ = null;
        return base.OnInitializeAsync(cancellationToken);
    }
    protected override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        ParentViewModel!.StatusBarVisibility = Visibility.Visible;
        ParentViewModel!.DialogTitle = $"{LocalizationService!.Get("Aqua_DialogTitle")} - {LocalizationService!.Get("Aqua_Assessment_BodyTitle")}";
        return base.OnActivateAsync(cancellationToken);
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

            var activeId = ParentViewModel!.ActiveAssessment?.id;
            HasId = activeId == null ? false : true;

            if (!HasId)
            {
                DataLoadedForId_ = null;
                ClearIdData();
            }
            else
            {
                //GetAssessment();  not used to view assessment
                DataLoadedForId_ = activeId;
                throw new InvalidStateEngineException(name: "ParentViewModel!.ActiveAssessment", value: "not null", "ParentViewModel!.ActiveAssessment not null");
            }
        }
        catch (Exception ex)
        {
            OnUIThread(() =>
            {
                ParentViewModel!.Message = ex.Message ?? ex.ToString();
            });
            throw;
        }
    }

    private void ClearIdData()
    {
        Type = null;
        Revision = null;
    }
    private async Task LoadValidValues()
    {
        if (isValidValuesLoaded)
            return;

        await GetAllProjectRevisions();
        isValidValuesLoaded = true;
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
    public async void AddAssessment()
    {
        var processStatus = await ParentViewModel!.RunLongRunningTask(
            "AQuA-Add_Assessment",
            (cancellationToken) => aquaManager_!.AddAssessment(
                new Assessment(
                    null,
                    ParentViewModel?.ActiveRevision?.id,
                    Revision?.id ?? null,
                    Type,
                    ModalSuffix,
                    null,
                    null,
                    null,
                    null),
                cancellationToken),
            (assessment) => {
                 MoveBackwards();
            });

        switch (processStatus)
        {
            case LongRunningTaskStatus.Completed:
                //await MoveForwards();
                await MoveBackwards();
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

    private void SetRevisionsList(IEnumerable<Revision>? revisions)
    {
        Execute.OnUIThread(() =>
        {
            Revisions.Clear();
            if (revisions != null)
                foreach (var revision in revisions)
                    if (revision.id != ParentViewModel!.ActiveRevision?.id)
                        Revisions.Add(revision);
        });
    }

    private async Task<IEnumerable<Revision>> GetAllProjectRevisions(CancellationToken cancellationToken)
    {
        var allCorpus = await Corpus.GetAllCorpusIds(Mediator!);
        List<TokenizedTextCorpusId> tokenizedCorpusIds = new();
        foreach (var corpusId in allCorpus)
        {
            tokenizedCorpusIds.AddRange(await TokenizedTextCorpus.GetAllTokenizedCorpusIds(
                Mediator!,
                corpusId
            ));
        }

        BlockingCollection<Revision> revisionsInProject = new();
        await Parallel.ForEachAsync(tokenizedCorpusIds, new ParallelOptions(), async (tokenizedCorpusId, cancellationToken) =>
        {
            var tokenizedTextCorpus = await TokenizedTextCorpus.Get(
                Mediator!,
                tokenizedCorpusId,
                false);

            var aquaTokenizedTextCorpusMetadata = AquaTokenizedTextCorpusMetadata.Get(tokenizedTextCorpus);

            var versionId = aquaTokenizedTextCorpusMetadata?.id ?? null;

            if (versionId != null)
            {
                var versionRevisions = await aquaManager_!.ListRevisions(
                    versionId,
                    cancellationToken);
                if (versionRevisions != null)
                {
                    foreach(var revision in versionRevisions)
                    {
                        revisionsInProject.Add(revision);
                    }
                }
            }
        });
        return revisionsInProject;
    }

    public async Task GetAllProjectRevisions()
    {
        var processStatus = await ParentViewModel!.RunLongRunningTask(
            "AQuA-Assessments-Get_All_Project_Revisions",
            GetAllProjectRevisions,
            SetRevisionsList,
            () => { },
            () => { });

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
    protected override ValidationResult? Validate()
    {
        return Validator!.Validate(this);
    }
}