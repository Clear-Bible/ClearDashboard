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
using FluentValidation;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearBible.Engine.Exceptions;
using static ClearDashboard.Aqua.Module.Services.IAquaManager;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using Autofac.Features.AttributeFilters;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace ClearDashboard.Aqua.Module.ViewModels.AquaDialog;

public class AquaVersionStepViewModel : DashboardApplicationValidatingWorkflowStepViewModel<IAquaDialogViewModel, AquaVersionStepViewModel>
{
    private readonly IAquaManager? aquaManager_;
    private readonly IEnhancedViewManager? _enhancedViewManager;

    private bool isValidValuesLoaded = false;

    public BindableCollection<Language> IsoLanguages { get; set; } = new();

    public BindableCollection<Script> IsoScripts { get; set; } = new();
    public BindableCollection<Revision> Items { get; set; } = new();

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

    private int? dataLoadedForId_ = null;
    private int? DataLoadedForId
    {
        get => dataLoadedForId_;
        set
        {
            dataLoadedForId_ = value;
            NotifyOfPropertyChange(() => DataLoaded);
        }
    }
    public bool DataLoaded
    {
        get => dataLoadedForId_ != null ? true : false;
    }

    private DialogMode _dialogMode;
    public DialogMode DialogMode
    {
        get => _dialogMode;
        set => Set(ref _dialogMode, value);
    }

    private string? _versionId;
    public string? VersionId
    {
        get => _versionId;
        set => Set(ref _versionId, value);
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

    private Language? isoLanguage_;
    public Language? IsoLanguage
    {
        get => isoLanguage_;
        set
        {
            Set(ref isoLanguage_, value);
            ValidationResult = Validate();
        }
    }

    private Visibility isoLanguageErrorVisibility_;
    public Visibility IsoLanguageErrorVisibility
    {
        get => isoLanguageErrorVisibility_;
        set
        {
            Set(ref isoLanguageErrorVisibility_, value);
        }
    }

    private Script? isoScript_;
    public Script? IsoScript
    {
        get => isoScript_;
        set
        {
            Set(ref isoScript_, value);
            ValidationResult = Validate();
        }
    }

    private Visibility isoScriptErrorVisibility_;
    public Visibility IsoScriptErrorVisibility
    {
        get => isoScriptErrorVisibility_;
        set
        {
            Set(ref isoScriptErrorVisibility_, value);
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

    public RelayCommand OkCommand { get; }
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
        [KeyFilter("Aqua")] ILocalizationService localizationService)
        : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, validator, localizationService)
    {
        aquaManager_ = aquaManager;
        _enhancedViewManager = enhancedViewManager;
        DialogMode = dialogMode;

        CanMoveForwards = true;
        CanMoveBackwards = true;
        EnableControls = true;

        OkCommand = new RelayCommand(Ok);
    }
    protected override Task OnInitializeAsync(CancellationToken cancellationToken)
    {
        isValidValuesLoaded = false;
        DataLoadedForId = null;
        ParentViewModel!.StatusBarVisibility = Visibility.Visible;
        return base.OnInitializeAsync(cancellationToken);
    }

    protected override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        ParentViewModel!.DialogTitle = $"{LocalizationService!.Get("Aqua_DialogTitle")} - {LocalizationService!.Get("Aqua_Version_BodyTitle")}";
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

            var tokenizedTextCorpus = await TokenizedTextCorpus.Get(
                Mediator!,
                ParentViewModel!.TokenizedTextCorpusId
                    ?? throw new InvalidParameterEngineException(
                        name: "tokenizedTextCorpusId_",
                        value: "null"),
                false);

            ParentViewModel!.AquaTokenizedTextCorpusMetadata = AquaTokenizedTextCorpusMetadata.Get(tokenizedTextCorpus);

            var activeId = ParentViewModel!.AquaTokenizedTextCorpusMetadata!.id;
            HasId = activeId == null ? false : true;

            if (!HasId)
            {
                DataLoadedForId = null;
                ClearIdData();
            }
            else
            {
                if (DataLoadedForId == null || DataLoadedForId != activeId)
                {
                    ClearIdData();
                    await GetVersion();
                }
                else
                {
                    await GetRevisions();
                }
            }
        }
        catch (Exception ex)
        {
            OnUIThread(() =>
            {
                ParentViewModel!.Message = ex.Message ?? ex.ToString();
            });
        }

        ValidationResult = Validate();
    }

    private void ClearIdData()
    {
        Name = null;
        IsoLanguage = null;
        IsoScript = null;
        Abbreviation = null;
        Rights = null;
        ForwardTranslationToVersionId = null;
        BackTranslationToVersionId = null;
        MachineTranslation = false;
        Items.Clear();
    }
    private async Task LoadValidValues()
    {
        if (isValidValuesLoaded)
            return;

        await LoadLanguages();
        await LoadScripts();
        isValidValuesLoaded = true;
    }

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
        DataLoadedForId = null;

        var processStatus = await ParentViewModel!.RunLongRunningTask(
            "AQuA-Get_Version",
            (cancellationToken) => aquaManager_!.GetVersion(
                ParentViewModel!.AquaTokenizedTextCorpusMetadata!.id
                    ?? throw new InvalidStateEngineException(name: "AquaTokenizedTextCorpusMetadata!.VersionId", value: "null"),
                cancellationToken),
            async (version) =>
            {
                VersionId = version?.id?.ToString();
                Name = version?.name;
                IsoLanguage = IsoLanguages
                    .Where(l => l.iso639 == version?.isoLanguage)
                    .FirstOrDefault();
                IsoScript = IsoScripts
                    .Where(s => s.iso15924 == ParentViewModel!.AquaTokenizedTextCorpusMetadata.isoScript)
                    .FirstOrDefault();
                Abbreviation = version?.abbreviation;
                Rights = version?.rights;
                ForwardTranslationToVersionId = ParentViewModel!.AquaTokenizedTextCorpusMetadata.forwardTranslationToVersionId?.ToString();
                BackTranslationToVersionId = ParentViewModel!.AquaTokenizedTextCorpusMetadata.backTranslationToVersionId?.ToString();
                MachineTranslation = ParentViewModel!.AquaTokenizedTextCorpusMetadata.machineTranslation;

                ParentViewModel!.AquaTokenizedTextCorpusMetadata!.abbreviation = Abbreviation;
                await ParentViewModel!.AquaTokenizedTextCorpusMetadata!.Save(ParentViewModel!.TokenizedTextCorpusId!, Mediator!);
                DataLoadedForId = ParentViewModel!.AquaTokenizedTextCorpusMetadata!.id;
                await GetRevisions();
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

    public async void AddVersion()
    {
        var processStatus = await ParentViewModel!.RunLongRunningTask(
            "AQuA-Add_Version",
            (cancellationToken) => aquaManager_!.AddVersion(
                new IAquaManager.Version(
                    null,
                    Name 
                        ?? throw new InvalidStateEngineException(name: "Name", value: "null"),
                    IsoLanguage?.iso639
                        ?? throw new InvalidStateEngineException(name: "IsoLanguage", value: "null"),
                    IsoScript?.iso15924
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
                ParentViewModel!.AquaTokenizedTextCorpusMetadata.isoLanguage = IsoLanguage?.iso639
                    ?? throw new InvalidStateEngineException(name: "IsoLanguage", value: "null");
                ParentViewModel!.AquaTokenizedTextCorpusMetadata.isoScript = IsoScript?.iso15924
                    ?? throw new InvalidStateEngineException(name: "IsoScript", value: "null");
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

    public async Task DeleteVersion()
    {
        var processStatus = await ParentViewModel!.RunLongRunningTask(
            "AQuA-Delete_Version",
            (cancellationToken) => {
                aquaManager_!.DeleteVersion(
                    ParentViewModel!.AquaTokenizedTextCorpusMetadata!.id
                        ?? throw new InvalidStateEngineException(name: "AquaTokenizedTextCorpusMetadata!.VersionId", value: "null"),
                    cancellationToken);
                return Task.FromResult(""); // so RunLongRunningTask can infer bogus type string
            },
            (_) => {});

        switch (processStatus)
        {
            case LongRunningTaskStatus.Completed:
            case LongRunningTaskStatus.Failed:
                // want this to run even if delete on server failed because it's not on server so that the metadata is cleared.
                ParentViewModel!.AquaTokenizedTextCorpusMetadata.id = null;
                ParentViewModel!.AquaTokenizedTextCorpusMetadata.abbreviation = null;
                ParentViewModel!.AquaTokenizedTextCorpusMetadata.isoLanguage = null;
                ParentViewModel!.AquaTokenizedTextCorpusMetadata.isoScript = null;
                ParentViewModel!.AquaTokenizedTextCorpusMetadata.forwardTranslationToVersionId = null;
                ParentViewModel!.AquaTokenizedTextCorpusMetadata.backTranslationToVersionId = null;
                ParentViewModel!.AquaTokenizedTextCorpusMetadata.machineTranslation = MachineTranslation;
                await ParentViewModel!.AquaTokenizedTextCorpusMetadata!.Save(ParentViewModel!.TokenizedTextCorpusId!, Mediator!);

                ClearIdData();

                await Reload();
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
        if (revisions != null)
            foreach (var revision in revisions)
                Items.Add(revision);
    }
    public async Task GetRevisions()
    {
        var processStatus = await ParentViewModel!.RunLongRunningTask(
            "AQuA-Get_Revisions",
            (cancellationToken) => aquaManager_!.ListRevisions(
                ParentViewModel!.AquaTokenizedTextCorpusMetadata.id
                    ?? throw new InvalidStateEngineException(name: "AquaTokenizedTextCorpusMetadata.id", value: "null"),
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
                    .OrderBy(language => language.iso639)
                    .Select(language =>
                    {
                        IsoLanguages.Add(language);
                        return language;
                    })
                    .ToList();
                IsoLanguages.NotifyOfPropertyChange("IsoLanguages");
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
                IsoScripts.Clear();
                scripts?
                    .OrderBy(script => script.iso15924)
                    .Select(script =>
                    {
                        IsoScripts.Add(script);
                        return script;
                    })
                    .ToList();
                IsoScripts.NotifyOfPropertyChange("IsoScripts");
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
        var result = Validator!.Validate(this);

        IsoLanguageErrorVisibility = result.Errors.Any(e => e.PropertyName == "IsoLanguage") ? Visibility.Visible : Visibility.Hidden;
        IsoScriptErrorVisibility = result.Errors.Any(e => e.PropertyName == "IsoScript") ? Visibility.Visible : Visibility.Hidden;
        
        return result;
    }
}
