using Autofac;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.Wpf.Application.Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using ClearApplicationFoundation.Framework.Input;
using ClearDashboard.Wpf.Application;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Aqua.Module.Services;
using FluentValidation.Results;
using FluentValidation;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearBible.Engine.Exceptions;
using static ClearDashboard.Aqua.Module.Services.IAquaManager;
using System.Collections.Generic;
using ClearDashboard.Aqua.Module.Models;

namespace ClearDashboard.Aqua.Module.ViewModels.AquaDialog;

public class AquaAddVersionOrListAssessmentsStepViewModel : DashboardApplicationValidatingWorkflowStepViewModel<IAquaDialogViewModel, AquaAddVersionOrListAssessmentsStepViewModel>
{
    private readonly IAquaManager? aquaManager_;
    private readonly IEnhancedViewManager? _enhancedViewManager;

    public class AquaItem
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
    public AquaAddVersionOrListAssessmentsStepViewModel()
    {
        OkCommand = new RelayCommand(Ok);
    }
    public AquaAddVersionOrListAssessmentsStepViewModel(
        IAquaManager aquaManager,
        IEnhancedViewManager enhancedViewManager,

        DialogMode dialogMode,
        DashboardProjectManager projectManager,
        INavigationService navigationService,
        ILogger<AquaAddVersionOrListAssessmentsStepViewModel> logger,
        IEventAggregator eventAggregator,
        IMediator mediator,
        ILifetimeScope? lifetimeScope,
        IValidator<AquaAddVersionOrListAssessmentsStepViewModel> validator,
        ILocalizationService localizationService)
        : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, validator, localizationService)
    {
        aquaManager_ = aquaManager;
        _enhancedViewManager = enhancedViewManager;
        DialogMode = dialogMode;

        CanMoveForwards = true;
        CanMoveBackwards = true;
        EnableControls = true;

        OkCommand = new RelayCommand(Ok);

        BodyTitle = LocalizationService!.Get("AquaAddVersionOrListAssessmentsStep_BodyTitle");
        BodyText = LocalizationService!.Get("AquaAddVersionOrListAssessmentsStep_BodyText"); ;
    }
    protected override Task OnInitializeAsync(CancellationToken cancellationToken)
    {
        //ParentViewModel!.StatusBarVisibility = Visibility.Hidden;
        return base.OnInitializeAsync(cancellationToken);
    }

    protected override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        await Reload();
        await base.OnActivateAsync(cancellationToken);
        return;
    }

    private async Task Reload()
    {
        var tokenizedTextCorpus = await TokenizedTextCorpus.Get(
            Mediator!,
            ParentViewModel!.TokenizedTextCorpusId
                ?? throw new InvalidParameterEngineException(
                    name: "tokenizedTextCorpusId_",
                    value: "null"),
            false);

        ParentViewModel!.AquaTokenizedTextCorpusMetadata = AquaTokenizedTextCorpusMetadata.Get(tokenizedTextCorpus);

        HasVersionId = ParentViewModel!.AquaTokenizedTextCorpusMetadata!.VersionId == null ? false : true;

        if (hasVersionId_)
        {
            await GetVersion();
            await GetRevisions();
        }
    }

    public ObservableCollection<Revision> Items { get; set; } = new ObservableCollection<Revision>();

    private bool hasVersionId_;
    public bool HasVersionId
    {
        get => hasVersionId_;
        set
        {
            hasVersionId_= value;
            DoesntHaveVersionId = !hasVersionId_;
            NotifyOfPropertyChange(() => HasVersionId);
        }
    }

    public bool DoesntHaveVersionId
    {
        get => !hasVersionId_;
        set
        {
            NotifyOfPropertyChange(() => DoesntHaveVersionId);
        }
    }

    private DialogMode _dialogMode;
    public DialogMode DialogMode
    {
        get => _dialogMode;
        set => Set(ref _dialogMode, value);
    }

    private string? bodyTitle_;
    public string? BodyTitle
    {
        get => bodyTitle_;
        set
        {
            bodyTitle_ = value;
            NotifyOfPropertyChange(() => BodyTitle);
        }
    }

    private string? bodyText_;
    public string? BodyText
    {
        get => bodyText_;
        set => Set(ref bodyText_, value);
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

    private string? isoLanguage_;
    public string? IsoLanguage
    {
        get => isoLanguage_;
        set
        {
            Set(ref isoLanguage_, value);
            ValidationResult = Validate();
        }
    }
    private string? isoScript_;
    public string? IsoScript
    {
        get => isoScript_;
        set
        {
            Set(ref isoScript_, value);
            ValidationResult = Validate();
        }
    }
    private string? abbreviation_;
    public string? Abbreviation
    {
        get => abbreviation_;
        set
        {
            Set(ref abbreviation_, value);
            ValidationResult = Validate();
        }
    }

    private string? rights_;
    public string? Rights
    {
        get => rights_;
        set
        {
            Set(ref rights_, value);
        }
    }

    private string? forwardTranslationToVersionId_;
    public string? ForwardTranslationToVersionId
    {
        get => forwardTranslationToVersionId_;
        set
        {
            Set(ref forwardTranslationToVersionId_, value == "" ? null : value);
            ValidationResult = Validate();
        }
    }
    private string? backTranslationToVersionId_;
    public string? BackTranslationToVersionId
    {
        get => backTranslationToVersionId_;
        set
        {
            Set(ref backTranslationToVersionId_, value == "" ? null : value);
            ValidationResult = Validate();
        }
    }

    private bool machineTranslation_ = false;
    public bool MachineTranslation
    {
        get => machineTranslation_;
        set
        {
            Set(ref machineTranslation_, value);
        }
    }

    /*
    private string? validatedText_;
    public string? ValidatedText
    {
        get => validatedText_;
        set
        {
            Set(ref validatedText_, value);
            //ValidationResult = Validate();
        }
    }

    private string? unvalidatedText_;
    public string? UnvalidatedText
    {
        get => unvalidatedText_;
        set
        {
            Set(ref unvalidatedText_, value);
            //ValidationResult = Validate();
        }
    }

    private string? numericText_;
    public string? NumericText
    {
        get => numericText_;
        set
        {
            Set(ref numericText_, value);
            //ValidationResult = Validate();
        }
    }

    private string? lengthText_;
    public string? LengthText
    {
        get => lengthText_;
        set
        {
            Set(ref lengthText_, value);
            //ValidationResult = Validate();
        }
    }
    */
    public RelayCommand OkCommand { get; }

    public void AddItemToEnhancedView(Revision revision)
    {
        Logger!.LogInformation($"AddItemToEnhancedView - {revision.revisionId}");
        _enhancedViewManager!.AddMetadatumEnhancedView(
            new AquaCorpusAnalysisEnhancedViewItemMetadatum() 
            { 
                UrlString = $"RevisionId {revision.revisionId}" 
            }, 
            default); //FIXME: is this okay?
    }

    public async void DeleteItem(Revision revision)
    {
        Logger!.LogInformation($"DeleteItem - {revision.revisionId}");
        await DeleteRevision(revision);
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

    public async Task GetVersion()
    {
        try
        {
            var processStatus = await ParentViewModel!.RunLongRunningTask(
                "Getting corpus information from AQuA",
                (cancellationToken) => aquaManager_!.GetVersion(
                    ParentViewModel!.AquaTokenizedTextCorpusMetadata!.VersionId!,
                    cancellationToken),
                (version) =>
                {
                    Name = version.name;
                    IsoLanguage = version.isoLanguage;
                    IsoScript = version.isoScript; 
                    Abbreviation = version.abbreviation;
                    Rights = version.rights;
                    ForwardTranslationToVersionId = version.forwardTranslationToVersionId.ToString();
                    BackTranslationToVersionId = version?.backTranslationToVersionId.ToString();
                    MachineTranslation = version?.machineTranslation ?? 
                        throw new InvalidStateEngineException(name: "version.machineTranslation", value: "null");
                });

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
        catch (Exception)
        {
            //ParentViewModel!.Cancel();
        }
    }

    public async void AddVersion()
    {
        try
        {
            var processStatus = await ParentViewModel!.RunLongRunningTask(
                "Adding corpus information to AQuA",
                (cancellationToken) => aquaManager_!.AddVersion(
                    new IAquaManager.Version(
                        Name ?? throw new InvalidStateEngineException(name: "Name", value: "null"),
                        IsoLanguage ?? throw new InvalidStateEngineException(name: "IsoLanguage", value: "null"),
                        IsoScript ?? throw new InvalidStateEngineException(name: "IsoScript", value: "null"),
                        Abbreviation ?? throw new InvalidStateEngineException(name: "Abbreviation", value: "null"), 
                        Rights,
                        ForwardTranslationToVersionId != null ? int.Parse(ForwardTranslationToVersionId) : null, 
                        BackTranslationToVersionId != null ?  int.Parse(BackTranslationToVersionId) : null, 
                        MachineTranslation),
                    cancellationToken),
                async (versionId) =>
                {   
                    ParentViewModel!.AquaTokenizedTextCorpusMetadata.VersionId = versionId;
                    await ParentViewModel!.AquaTokenizedTextCorpusMetadata!.Save(ParentViewModel!.TokenizedTextCorpusId!, Mediator!);

                    await Reload();
                });

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
        catch (Exception)
        {
            //ParentViewModel!.Cancel();
        }
    }

    private void SetRevisionsList(IEnumerable<Revision>? revisions)
    {

        //fixme: remove.For texting.
        revisions = new List<Revision>() 
        {
            new Revision("revisionId234", "info 234"),
            new Revision("revisionId888", "info 888")
        };

        if (revisions != null)
            foreach (var revision in revisions)
                Items.Add(revision);
    }
    public async Task GetRevisions()
    {
        try
        {
            var processStatus = await ParentViewModel!.RunLongRunningTask(
                "Getting corpus revisions from AQuA",
                (cancellationToken) => aquaManager_!.ListRevisions(
                    ParentViewModel!.AquaTokenizedTextCorpusMetadata!.VersionId!,
                    cancellationToken),
                SetRevisionsList);

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
        catch (Exception)
        {
            //ParentViewModel!.Cancel();
        }
    }

    public async Task DeleteRevision(Revision revision)
    {
        try
        {
            var processStatus = await ParentViewModel!.RunLongRunningTask(
                "Deleting corpus revision in AQuA",
                (cancellationToken) => {
                    aquaManager_!.DeleteRevision(
                        revision.revisionId,
                        cancellationToken);
                    return Task.FromResult(""); // so RunLongRunningTask can infer bogus type string
                },
                (_) => Items.Remove(revision));

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
        catch (Exception)
        {
            //ParentViewModel!.Cancel();
        }
    }
    protected override ValidationResult? Validate()
    {
        return Validator!.Validate(this);
    }
}
