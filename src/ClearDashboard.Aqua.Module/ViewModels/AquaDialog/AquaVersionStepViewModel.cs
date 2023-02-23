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
using ClearDashboard.DAL.Alignment.Corpora;
using ClearBible.Engine.Exceptions;
using static ClearDashboard.Aqua.Module.Services.IAquaManager;
using System.Collections.Generic;
using ClearDashboard.Aqua.Module.Models;
using System.Windows;
using System.Linq;
using SIL.Machine.DataStructures;

namespace ClearDashboard.Aqua.Module.ViewModels.AquaDialog;

public class AquaVersionStepViewModel : DashboardApplicationValidatingWorkflowStepViewModel<IAquaDialogViewModel, AquaVersionStepViewModel>
{
    private readonly IAquaManager? aquaManager_;
    private readonly IEnhancedViewManager? _enhancedViewManager;
    public AquaVersionStepViewModel()
    {
        OkCommand = new RelayCommand(Ok);
    }
    public AquaVersionStepViewModel(
        IAquaManager aquaManager,
        IEnhancedViewManager enhancedViewManager,

        DialogMode dialogMode,
        DashboardProjectManager projectManager,
        INavigationService navigationService,
        ILogger<AquaVersionStepViewModel> logger,
        IEventAggregator eventAggregator,
        IMediator mediator,
        ILifetimeScope? lifetimeScope,
        IValidator<AquaVersionStepViewModel> validator,
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

        BodyTitle = LocalizationService!.Get("Aqua_VersionStep_BodyTitle");
        BodyText = LocalizationService!.Get("Aqua_VersionStep_BodyText"); ;
    }
    protected override Task OnInitializeAsync(CancellationToken cancellationToken)
    {
        ParentViewModel!.StatusBarVisibility = Visibility.Visible;
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

        HasId = ParentViewModel!.AquaTokenizedTextCorpusMetadata!.id == null ? false : true;

        if (hasId_)
        {
            await LoadLanguages();
            await LoadScripts();
            await GetVersion(); //make sure the version is still on the server
            await GetRevisions();
        }
    }

    public BindableCollection<string> IsoLanguages { get; set; } = new();

    public BindableCollection<string> IsoScripts { get; set; } = new();
    public BindableCollection<Revision> Items { get; set; } = new ();

    private bool hasId_;
    public bool HasId
    {
        get => hasId_;
        set
        {
            hasId_= value;
            NotifyOfPropertyChange(() => HasId);
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

    public void AddRevision()
    {
        ParentViewModel!.ActiveRevision = null;
        MoveForwards();
    }

    public void ViewRevision(Revision revision)
    {
        ParentViewModel!.ActiveRevision = revision;
        MoveForwards();
    }
    public void AddItemToEnhancedView(Revision revision)
    {
        Logger!.LogInformation($"AddItemToEnhancedView - {revision.id}");
        _enhancedViewManager!.AddMetadatumEnhancedView(
            new AquaCorpusAnalysisEnhancedViewItemMetadatum() 
            { 
                UrlString = $"RevisionId {revision.id}" 
            }, 
            default); //FIXME: is this okay?
    }

    public async void DeleteItem(Revision revision)
    {
        Logger!.LogInformation($"DeleteItem - {revision.id}");
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
        var processStatus = await ParentViewModel!.RunLongRunningTask(
            "AQuA-Get_Version",
            (cancellationToken) => aquaManager_!.GetVersion(
                ParentViewModel!.AquaTokenizedTextCorpusMetadata!.id
                    ?? throw new InvalidStateEngineException(name: "AquaTokenizedTextCorpusMetadata!.VersionId", value: "null"),
                cancellationToken),
            async (version) =>
            {
                Name = version.name;
                IsoLanguage = version.language;
                IsoScript = ParentViewModel!.AquaTokenizedTextCorpusMetadata.isoScript;
                Abbreviation = version.abbreviation;
                Rights = version.rights;
                ForwardTranslationToVersionId = ParentViewModel!.AquaTokenizedTextCorpusMetadata.forwardTranslationToVersionId?.ToString();
                BackTranslationToVersionId = ParentViewModel!.AquaTokenizedTextCorpusMetadata.backTranslationToVersionId?.ToString();
                MachineTranslation = ParentViewModel!.AquaTokenizedTextCorpusMetadata.machineTranslation;

                ParentViewModel!.AquaTokenizedTextCorpusMetadata!.abbreviation = Abbreviation;
                await ParentViewModel!.AquaTokenizedTextCorpusMetadata!.Save(ParentViewModel!.TokenizedTextCorpusId!, Mediator!);
            }); ;

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

    public async void AddVersion()
    {
        var processStatus = await ParentViewModel!.RunLongRunningTask(
            "AQuA-Add_Version",
            (cancellationToken) => aquaManager_!.AddVersion(
                new IAquaManager.Version(
                    null,
                    Name 
                        ?? throw new InvalidStateEngineException(name: "Name", value: "null"),
                    IsoLanguage 
                        ?? throw new InvalidStateEngineException(name: "IsoLanguage", value: "null"),
                    IsoScript 
                        ?? throw new InvalidStateEngineException(name: "IsoScript", value: "null"),
                    Abbreviation 
                        ?? throw new InvalidStateEngineException(name: "Abbreviation", value: "null"), 
                    Rights,
                    ForwardTranslationToVersionId != null ? int.Parse(ForwardTranslationToVersionId) : null, 
                    BackTranslationToVersionId != null ?  int.Parse(BackTranslationToVersionId) : null,
                    MachineTranslation
                ),
                cancellationToken),
            async (version) =>
            {
                if (version == null)
                    throw new InvalidParameterEngineException(name: "version", value: "null");

                //commented items not persisted but instead re-obtained each time from GetVersion() (metadata
                // only saves state server not providing back, otherwise version state saved on server.
                ParentViewModel!.AquaTokenizedTextCorpusMetadata.id = version!.id;
                //ParentViewModel!.AquaTokenizedTextCorpusMetadata.name = Name;
                ParentViewModel!.AquaTokenizedTextCorpusMetadata.isoLanguage = IsoLanguage;
                ParentViewModel!.AquaTokenizedTextCorpusMetadata.isoScript = IsoScript;
                ParentViewModel!.AquaTokenizedTextCorpusMetadata.abbreviation = version.abbreviation;
                //ParentViewModel!.AquaTokenizedTextCorpusMetadata.rights = Rights;
                ParentViewModel!.AquaTokenizedTextCorpusMetadata.forwardTranslationToVersionId = 
                    ForwardTranslationToVersionId != null ? int.Parse(ForwardTranslationToVersionId!) : null;
                ParentViewModel!.AquaTokenizedTextCorpusMetadata.backTranslationToVersionId = 
                    BackTranslationToVersionId != null ? int.Parse(BackTranslationToVersionId) : null;
                ParentViewModel!.AquaTokenizedTextCorpusMetadata.machineTranslation = MachineTranslation;
                //ParentViewModel!.AquaTokenizedTextCorpusMetadata.language = version.language;
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

    private void SetRevisionsList(IEnumerable<Revision>? revisions)
    {
        Items.Clear();
        //fixme: remove.For texting.
        //revisions = new List<Revision>() 
        //{
        //    new Revision("revisionId234", "info 234"),
        //    new Revision("revisionId888", "info 888")
        //};

        if (revisions != null)
            foreach (var revision in revisions)
                Items.Add(revision);
    }
    public async Task GetRevisions()
    {
        var processStatus = await ParentViewModel!.RunLongRunningTask(
            "AQuA-Get_Revisions",
            (cancellationToken) => aquaManager_!.ListRevisions(
                Abbreviation 
                    ?? throw new InvalidStateEngineException(name: "Abbreviation", value: "null"),
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

    public async Task DeleteRevision(Revision revision)
    {
        var processStatus = await ParentViewModel!.RunLongRunningTask(
            "AQuA-Delete_Revision",
            (cancellationToken) => {
                aquaManager_!.DeleteRevision(
                    revision.id 
                        ?? throw new InvalidStateEngineException(name: "revision.id", value: "null"),
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

    /*
    public async void AddRevision()
    {
        var processStatus = await ParentViewModel!.RunLongRunningTask(
            "AQuA-Add_Revision",
            (cancellationToken) => aquaManager_!.AddRevision(
                ParentViewModel!.TokenizedTextCorpusId
                    ?? throw new InvalidStateEngineException(name: "ParentViewModel!.TokenizedTextCorpus", value: "null"),
                new Revision(
                    null,
                    Abbreviation
                        ?? throw new InvalidStateEngineException(name: "Abbreviation", value: "null"),
                    null,
                    false
                ),
                cancellationToken),
            (revision) => {
                var foo = revision;
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
    */


    private async Task LoadLanguages()
    {
        var processStatus = await ParentViewModel!.RunLongRunningTask(
            "AQuA-Get_IsoLanguages",
            (cancellationToken) => aquaManager_!.ListLanguages(
                cancellationToken),
            (languages) =>
            {
                IsoLanguages.Clear();
                languages?
                    .OrderBy(language => language.iso693)
                    .Select(language =>
                    {
                        IsoLanguages.Add(language.iso693);
                        return language;
                    })
                    .ToList();
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

    private async Task LoadScripts()
    {
        var processStatus = await ParentViewModel!.RunLongRunningTask(
            "AQuA-Get_IsoScripts",
            (cancellationToken) => aquaManager_!.ListScripts(
                cancellationToken),
            (scripts) =>
            {
                IsoLanguages.Clear();
                scripts?
                    .OrderBy(script => script.iso15924)
                    .Select(script =>
                    {
                        IsoScripts.Add(script.iso15924);
                        return script;
                    })
                    .ToList();
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
    protected override ValidationResult? Validate()
    {
        return Validator!.Validate(this);
    }
}
