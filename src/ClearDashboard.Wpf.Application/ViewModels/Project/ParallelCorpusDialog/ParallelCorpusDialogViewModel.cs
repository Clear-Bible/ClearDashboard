using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.Exceptions;
using ClearApplicationFoundation.Extensions;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface;
using ClearDashboard.Wpf.Application.Enums;
using ClearDashboard.Wpf.Application.Exceptions;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Models;
using ClearDashboard.Wpf.Application.Properties;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.Shell;
using MediatR;
using Microsoft.Extensions.Logging;
using SIL.Machine.Translation;
using SIL.Machine.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AlignmentSet = ClearDashboard.DAL.Alignment.Translation.AlignmentSet;
using TranslationSet = ClearDashboard.DAL.Alignment.Translation.TranslationSet;

namespace ClearDashboard.Wpf.Application.ViewModels.Project.ParallelCorpusDialog
{

    public class ParallelCorpusDialogViewModel : DashboardApplicationWorkflowShellViewModel, IParallelCorpusDialogViewModel
    {
        internal class TaskNames
        {
            public const string AlignmentSet = "AlignmentSet";
            public const string ParallelCorpus = "ParallelCorpus";
            public const string TrainingSmtModel = "TrainingSMTModel";
            public const string TranslationSet = "TranslationSet";
        }


        #region Member Variables   

        private readonly IEnumerable<TokenizedTextCorpusId> _tokenizedCorpora;
        private readonly BackgroundTasksViewModel _backgroundTasksViewModel;
        private readonly SystemPowerModes _systemPowerModes;
        private readonly LongRunningTaskManager _longRunningTaskManager;
        private CorpusNodeViewModel _sourceCorpusNodeViewModel;
        private CorpusNodeViewModel _targetCorpusNodeViewModel;
        private ParallelCorpusConnectionViewModel _parallelCorpusConnectionViewModel;
        public TopLevelProjectIds TopLevelProjectIds { get; set; }
        #endregion //Member Variables


        #region Public Properties

        public ParallelProjectType ProjectType { get; set; }

        public LongRunningTask CurrentLongRunningTask { get; set; }
        public IWordAlignmentModel WordAlignmentModel { get; set; }
        public ParallelCorpus ParallelTokenizedCorpus { get; set; }

        public bool CanCancel => (CurrentTask != null && CurrentTask.Status != LongRunningTaskStatus.Running) /* can always cancel */;
        public object? ExtraData { get; set; }
        public EngineParallelTextCorpus ParallelTextCorpus { get; set; }
        public LongRunningTask? CurrentTask { get; set; }
        public TranslationCommands TranslationCommandable { get; set; }


        public bool UseDefaults { get; set; } = false;

        public CorpusNodeViewModel SourceCorpusNodeViewModel
        {
            get => _sourceCorpusNodeViewModel;
            set => Set(ref _sourceCorpusNodeViewModel, value);
        }

        public CorpusNodeViewModel TargetCorpusNodeViewModel
        {
            get => _targetCorpusNodeViewModel;
            set => Set(ref _targetCorpusNodeViewModel, value);
        }

        public ParallelCorpusConnectionViewModel ParallelCorpusConnectionViewModel
        {
            get => _parallelCorpusConnectionViewModel;
            set => Set(ref _parallelCorpusConnectionViewModel, value);
        }

        public TranslationSet? TranslationSet { get; set; }
        public IEnumerable<AlignedTokenPairs> AlignedTokenPairs { get; set; }
        public AlignmentSet? AlignmentSet { get; set; }

        public bool IsTrainedSymmetrizedModel { get; set; }

        #endregion //Public Properties


        #region Observable Properties

        private SmtAlgorithm _selectedSmtAlgorithm;
        public SmtAlgorithm SelectedSmtAlgorithm
        {
            get => _selectedSmtAlgorithm;
            set => Set(ref _selectedSmtAlgorithm, value);
        }

        private bool _canOk;
        public bool CanOk
        {
            get => _canOk;
            set => Set(ref _canOk, value);
        }

        private string _message = string.Empty;
        public string Message
        {
            get => _message;
            set => Set(ref _message, value);
        }


        #endregion //Observable Properties


        #region Constructor

        public ParallelCorpusDialogViewModel()
        {
            // used by Caliburn Micro for design time 
        }


        public ParallelCorpusDialogViewModel(DialogMode dialogMode,
            ParallelCorpusConnectionViewModel parallelCorpusConnectionViewModel,
            CorpusNodeViewModel sourceCorpusNodeViewModel,
            CorpusNodeViewModel targetCorpusNodeViewModel,
            IEnumerable<TokenizedTextCorpusId> tokenizedCorpora,
            ParallelProjectType parallelProjectType,
            DashboardProjectManager? projectManager,
            INavigationService navigationService,
            ILogger<ParallelCorpusDialogViewModel> logger,
            BackgroundTasksViewModel backgroundTasksViewModel,
            IEventAggregator eventAggregator,
            SystemPowerModes systemPowerModes,
            IMediator mediator,
            ILifetimeScope lifetimeScope, LongRunningTaskManager longRunningTaskManager,
            ILocalizationService localizationService)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
        {
            _systemPowerModes = systemPowerModes;
            _tokenizedCorpora = tokenizedCorpora;
            ProjectType = parallelProjectType;
            _backgroundTasksViewModel = backgroundTasksViewModel;
            _longRunningTaskManager = longRunningTaskManager;
            CanOk = true;

            DisplayName = LocalizationService!.Get("ParallelCorpusDialog_ParallelCorpus");

            DialogMode = dialogMode;
            ParallelCorpusConnectionViewModel = parallelCorpusConnectionViewModel;
            SourceCorpusNodeViewModel = sourceCorpusNodeViewModel;
            TargetCorpusNodeViewModel = targetCorpusNodeViewModel;
        }

        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            var parameters = new List<Autofac.Core.Parameter> { new NamedParameter("dialogMode", DialogMode) };
            var views = LifetimeScope?.ResolveKeyedOrdered<IWorkflowStepViewModel>("ParallelCorpusDialog", parameters, "Order").ToArray();

            if (views == null || !views.Any())
            {
                throw new DependencyRegistrationMissingException(
                    "There are no dependency injection registrations of 'IWorkflowStepViewModel' with the key of 'ParallelCorpusDialog'.  Please check the dependency registration in your bootstrapper implementation.");
            }

            foreach (var view in views)
            {
                Steps!.Add(view);
            }

            CurrentStep = Steps![0];
            IsLastWorkflowStep = Steps.Count == 1;

            EnableControls = true;
            await ActivateItemAsync(Steps[0], cancellationToken);

            TopLevelProjectIds = await TopLevelProjectIds.GetTopLevelProjectIds(Mediator!);

            //var parallelCorpa = _topLevelProjectIds.ParallelCorpusIds.Where(x =>
            //    x.SourceTokenizedCorpusId.CorpusId.Id == SourceCorpusNodeViewModel.CorpusId
            //           && x.TargetTokenizedCorpusId.CorpusId.Id == TargetCorpusNodeViewModel.CorpusId
            //).ToList();

            //List<string> smts = new();
            //foreach (var parallelCorpusId in parallelCorpa)
            //{
            //    var alignment = _topLevelProjectIds.AlignmentSetIds.FirstOrDefault(x => x.ParallelCorpusId.Id == parallelCorpusId.Id);
            //    smts.Add(alignment.SmtModel);
            //}

            //if (smts.Contains("FastAlign"))
            //{
            //    SelectedSmtAlgorithm = SmtModelType.Hmm;
            //}
            //else
            //{
            //    SelectedSmtAlgorithm = SmtModelType.FastAlign;
            //}

            await base.OnInitializeAsync(cancellationToken);

        }

        #endregion //Constructor


        #region Methods

        public async void Cancel()
        {
            CancelCurrentTask();

            await TryCloseAsync(false);
        }

        private void CancelCurrentTask()
        {
            if (CurrentTask is { Status: LongRunningTaskStatus.Running })
            {
                Logger!.LogInformation($"Cancelling {CurrentTask.Name}");
                CurrentTask.Cancel();
            }
        }

        // The user clicked the close button for the dialog.
        public void OnClose(CancelEventArgs args)
        {
            if (args.Cancel)
            {
                Logger!.LogInformation("OnClose() called with 'Cancel' set to true");
                CancelCurrentTask();
            }
        }
        public async void Ok()
        {
            await TryCloseAsync(true);
        }

        public async Task<LongRunningTaskStatus> AddParallelCorpus(string parallelCorpusDisplayName)
        {
            var soundType = SoundType.Success;
            // var sourceNodeTokenization = SourceCorpusNodeViewModel.Tokenizations.FirstOrDefault();
            var sourceTokenizedTextCorpusId =
                _tokenizedCorpora.FirstOrDefault(tc => tc.CorpusId.Id == _sourceCorpusNodeViewModel.CorpusId);
            if (sourceTokenizedTextCorpusId == null)
            {
                throw new MissingTokenizedTextCorpusIdException(
                    $"Cannot find the source TokenizedTextCorpusId associated to Corpus with Id '{ParallelCorpusConnectionViewModel.SourceConnector.ParentNode.CorpusId}'.");
            }
            //var targetNodeTokenization = TargetCorpusNodeViewModel.Tokenizations.FirstOrDefault();
            var targetTokenizedTextCorpusId =
                _tokenizedCorpora.FirstOrDefault(tc => tc.CorpusId.Id == _targetCorpusNodeViewModel.CorpusId);
            if (targetTokenizedTextCorpusId == null)
            {
                throw new MissingTokenizedTextCorpusIdException(
                    $"Cannot find the target TokenizedTextCorpusId associated to Corpus with Id '{ParallelCorpusConnectionViewModel.DestinationConnector.ParentNode.CorpusId}'.");
            }


            IsBusy = true;

            // check to see if we want to run this in High Performance mode
            if (Settings.Default.EnablePowerModes && _systemPowerModes.IsLaptop)
            {
                await _systemPowerModes.TurnOnHighPerformanceMode();
            }

            var taskName = TaskNames.ParallelCorpus;
            CurrentTask = _longRunningTaskManager.Create(taskName, LongRunningTaskStatus.Running);
            var cancellationToken = CurrentTask!.CancellationTokenSource!.Token;

            try
            {

                Logger!.LogInformation(
                    $"Retrieving tokenized source and target corpora for '{parallelCorpusDisplayName}'.");
                await SendBackgroundStatus(taskName,
                    LongRunningTaskStatus.Running,
                    cancellationToken,
                    $"Retrieving tokenized source and target corpora for '{parallelCorpusDisplayName}'...");

                var sourceTokenizedTextCorpus = await TokenizedTextCorpus.Get(Mediator!, sourceTokenizedTextCorpusId);
                var targetTokenizedTextCorpus = await TokenizedTextCorpus.Get(Mediator!, targetTokenizedTextCorpusId);

                Logger!.LogInformation($"Parallelizing source and target corpora");
                await SendBackgroundStatus(taskName,
                    LongRunningTaskStatus.Running,
                    cancellationToken,
                    $"Parallelizing source and target corpora for '{parallelCorpusDisplayName}'...");

                ParallelTextCorpus =
                    await Task.Run(() => sourceTokenizedTextCorpus.EngineAlignRows(targetTokenizedTextCorpus,
                        new SourceTextIdToVerseMappingsFromVerseMappings(EngineParallelTextCorpus.VerseMappingsForAllVerses(
                            sourceTokenizedTextCorpus.Versification,
                            targetTokenizedTextCorpus.Versification))), 
                        cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                Logger!.LogInformation($"Saving parallelization '{parallelCorpusDisplayName}'");
                await SendBackgroundStatus(taskName,
                    LongRunningTaskStatus.Running,
                    cancellationToken,
                    $"Saving parallelization '{parallelCorpusDisplayName}'...");

                ParallelTokenizedCorpus = await ParallelTextCorpus.Create(parallelCorpusDisplayName, Mediator!, cancellationToken);

                await SendBackgroundStatus(taskName,
                    LongRunningTaskStatus.Completed,
                    cancellationToken,
                    $"Completed saving parallelization '{parallelCorpusDisplayName}'.");

                Logger!.LogInformation($"Completed saving parallelization '{parallelCorpusDisplayName}'");

                CurrentTask.Status = LongRunningTaskStatus.Completed;
            }
            catch (OperationCanceledException ex)
            {
                Logger!.LogInformation($"AddParallelCorpus - operation canceled.");

            }
            catch (MediatorErrorEngineException ex)
            {
                if (ex.Message.Contains("The operation was canceled."))
                {
                    Logger!.LogInformation($"AddParallelCorpus - operation canceled.");
                }
                else
                {
                    Logger!.LogError(ex, "An unexpected Engine exception was thrown.");
                }

            }
            catch (Exception ex)
            {
                Logger!.LogError(ex, $"An unexpected error occurred while creating the ParallelCorpus.");
                if (!cancellationToken.IsCancellationRequested)
                {
                    soundType = SoundType.Error;
                    await SendBackgroundStatus(taskName,
                        LongRunningTaskStatus.Failed,
                        cancellationToken,
                        exception: ex);

                }

                CurrentTask.Status = LongRunningTaskStatus.Failed;
            }
            finally
            {
                _longRunningTaskManager.TaskComplete(taskName);
                if (cancellationToken.IsCancellationRequested)
                {
                    CurrentTask.Status = LongRunningTaskStatus.Cancelled;

                    await SendBackgroundStatus(taskName,
                        LongRunningTaskStatus.Completed,
                        cancellationToken,
                        $"Parallelization was canceled.'{parallelCorpusDisplayName}'.");
                }

                // check to see if there are still High Performance Tasks still out there
                var numTasks = _backgroundTasksViewModel.GetNumberOfPerformanceTasksRemaining();
                if (numTasks == 0 && _systemPowerModes.IsHighPerformanceEnabled)
                {
                    // shut down high performance mode
                    await _systemPowerModes.TurnOffHighPerformanceMode();
                }

                IsBusy = false;
                Message = string.Empty;
            }

            if (UseDefaults == false)
            {
                PlaySound.PlaySoundFromResource(soundType);
            }

            return CurrentTask.Status;
        }

        public async Task<LongRunningTaskStatus> AddTranslationSet(string translationSetDisplayName)
        {
            IsBusy = true;

            // check to see if we want to run this in High Performance mode
            if (Settings.Default.EnablePowerModes && _systemPowerModes.IsLaptop)
            {
                await _systemPowerModes.TurnOnHighPerformanceMode();
            }

            var soundType = SoundType.Success;
            var taskName = TaskNames.TranslationSet;
            CurrentTask = _longRunningTaskManager.Create(taskName, LongRunningTaskStatus.Running);
            var cancellationToken = CurrentTask.CancellationTokenSource.Token;
            try
            {
                CurrentTask.Status = LongRunningTaskStatus.Running;
                await SendBackgroundStatus(taskName,
                    LongRunningTaskStatus.Running,
                    cancellationToken,
                    $"Creating the TranslationSet '{translationSetDisplayName}'...");

                var translationModel = WordAlignmentModel.GetTranslationTable();

                cancellationToken.ThrowIfCancellationRequested();

                // RUSSELL - code review
                TranslationSet = await TranslationSet.Create(null, AlignmentSet.AlignmentSetId,
                    translationSetDisplayName, new Dictionary<string, object>(),
                    ParallelTokenizedCorpus.ParallelCorpusId, Mediator, cancellationToken);



                await SendBackgroundStatus(taskName,
                    LongRunningTaskStatus.Completed,
                    cancellationToken,
                    $"Completed creation of the TranslationSet '{translationSetDisplayName}'.");
                Logger!.LogInformation($"Completed creating the TranslationSet '{translationSetDisplayName}'");

                CurrentTask.Status = LongRunningTaskStatus.Completed;
            }
            catch (OperationCanceledException ex)
            {
                Logger!.LogInformation($"AddTranslationSet - operation canceled.");
            }
            catch (MediatorErrorEngineException ex)
            {
                if (ex.Message.Contains("The operation was canceled."))
                {
                    Logger!.LogInformation($"AddTranslationSet - operation canceled.");
                }
                else
                {
                    Logger!.LogError(ex, "An unexpected Engine exception was thrown.");
                }

            }
            catch (Exception ex)
            {
                Logger!.LogError(ex, $"An unexpected error occurred while creating creating the TranslationSet.");
                if (!cancellationToken.IsCancellationRequested)
                {
                    soundType = SoundType.Error;
                    await SendBackgroundStatus(taskName,
                        LongRunningTaskStatus.Failed,
                        cancellationToken,
                        exception: ex);
                }

                CurrentTask.Status = LongRunningTaskStatus.Failed;
            }
            finally
            {
                _longRunningTaskManager.TaskComplete(taskName);
                if (cancellationToken.IsCancellationRequested)
                {
                    CurrentTask.Status = LongRunningTaskStatus.Cancelled;
                    await SendBackgroundStatus(taskName,
                        LongRunningTaskStatus.Completed,
                        cancellationToken,
                        $"Creation of the TranslationSet was canceled.'{translationSetDisplayName}'.");
                }
                IsBusy = false;
                Message = string.Empty;

                // check to see if there are still High Performance Tasks still out there
                var numTasks = _backgroundTasksViewModel.GetNumberOfPerformanceTasksRemaining();
                if (numTasks == 0 && _systemPowerModes.IsHighPerformanceEnabled)
                {
                    // shut down high performance mode
                    await _systemPowerModes.TurnOffHighPerformanceMode();
                }
            }

            PlaySound.PlaySoundFromResource(soundType);

            return CurrentTask.Status;

        }

        public async Task<LongRunningTaskStatus> AddAlignmentSet(string alignmentSetDisplayName)
        {
            IsBusy = true;

            // check to see if we want to run this in High Performance mode
            if (Settings.Default.EnablePowerModes && _systemPowerModes.IsLaptop)
            {
                await _systemPowerModes.TurnOnHighPerformanceMode();
            }

            var soundType = SoundType.Success;
            var taskName = TaskNames.AlignmentSet;
            CurrentTask = _longRunningTaskManager.Create(taskName, LongRunningTaskStatus.Running);
            var cancellationToken = CurrentTask.CancellationTokenSource.Token;
            try
            {

                CurrentTask.Status = LongRunningTaskStatus.Running;
                await SendBackgroundStatus(taskName,
                    LongRunningTaskStatus.Running,
                    cancellationToken,
                    $"Aligning corpora and creating the AlignmentSet '{alignmentSetDisplayName}'...");

                AlignedTokenPairs =
                    TranslationCommandable.PredictAllAlignedTokenIdPairs(WordAlignmentModel, ParallelTextCorpus);

                cancellationToken.ThrowIfCancellationRequested();


                if (ProjectType == ParallelProjectType.WholeProcess)
                {
                    Logger.LogInformation("Entering in AlignedTokenPairs.Create");
                    AlignmentSet = await AlignedTokenPairs.Create(displayName: alignmentSetDisplayName, 
                        smtModel: SelectedSmtAlgorithm.SmtName,
                        isSyntaxTreeAlignerRefined: false, 
                        isSymmetrized: IsTrainedSymmetrizedModel, 
                        metadata: new(), 
                        parallelCorpusId:  ParallelTokenizedCorpus.ParallelCorpusId, 
                        Mediator, 
                        cancellationToken);
                    Logger.LogInformation("Completed in AlignedTokenPairs.Create");

                }
                else
                {
                    // alignment only
                    Logger.LogInformation("Entering in AlignedTokenPairs.Create");
                    var parallelCorpus = ParallelTextCorpus as ParallelCorpus;
                    AlignmentSet = await AlignedTokenPairs.Create(displayName: alignmentSetDisplayName,
                        smtModel: SelectedSmtAlgorithm.SmtName,
                        isSyntaxTreeAlignerRefined: false,
                        isSymmetrized: IsTrainedSymmetrizedModel,
                        metadata: new(),
                        parallelCorpusId: parallelCorpus!.ParallelCorpusId, 
                        Mediator, 
                        cancellationToken);
                    Logger.LogInformation("Completed in AlignedTokenPairs.Create");
                }

                await SendBackgroundStatus(taskName,
                    LongRunningTaskStatus.Completed,
                    cancellationToken,
                    $"Completed creation of the AlignmentSet '{alignmentSetDisplayName}'.");

                Logger!.LogInformation($"Completed creating the AlignmentSet '{alignmentSetDisplayName}'");

                CurrentTask.Status = LongRunningTaskStatus.Completed;
            }
            catch (OperationCanceledException ex)
            {
                Logger!.LogInformation($"AddAlignmentSet - operation canceled.");
            }
            catch (MediatorErrorEngineException ex)
            {
                if (ex.Message.Contains("The operation was canceled."))
                {
                    Logger!.LogInformation($"AddAlignmentSet - operation canceled.");
                }
                else
                {
                    Logger!.LogError(ex, "An unexpected Engine exception was thrown.");
                }

            }
            catch (Exception ex)
            {
                Logger!.LogError(ex, $"An unexpected error occurred while creating creating the AlignmentSet.");
                if (!cancellationToken.IsCancellationRequested)
                {
                    soundType = SoundType.Error;
                    await SendBackgroundStatus(taskName,
                        LongRunningTaskStatus.Failed,
                        cancellationToken,
                        exception: ex);
                }

                CurrentTask.Status = LongRunningTaskStatus.Failed;
            }
            finally
            {
                _longRunningTaskManager.TaskComplete(taskName);

                if (cancellationToken.IsCancellationRequested)
                {
                    CurrentTask.Status = LongRunningTaskStatus.Cancelled;
                    await SendBackgroundStatus(taskName,
                        LongRunningTaskStatus.Completed,
                        cancellationToken,
                        $"Creation of the AlignmentSet was canceled.'{alignmentSetDisplayName}'.");
                }
                IsBusy = false;
                Message = string.Empty;

                // check to see if there are still High Performance Tasks still out there
                var numTasks = _backgroundTasksViewModel.GetNumberOfPerformanceTasksRemaining();
                if (numTasks == 0 && _systemPowerModes.IsHighPerformanceEnabled)
                {
                    // shut down high performance mode
                    await _systemPowerModes.TurnOffHighPerformanceMode();
                }
            }

            if (UseDefaults == false)
            {
                PlaySound.PlaySoundFromResource(soundType);
            }

            return CurrentTask.Status;
        }

        public async Task<LongRunningTaskStatus> TrainSmtModel(bool? isTrainedSymmetrizedModel)
        {
            IsBusy = true;

            // check to see if we want to run this in High Performance mode
            if (Settings.Default.EnablePowerModes && _systemPowerModes.IsLaptop)
            {
                await _systemPowerModes.TurnOnHighPerformanceMode();
            }

            var soundType = SoundType.Success;
            var taskName = TaskNames.TrainingSmtModel;
            CurrentTask = _longRunningTaskManager.Create(taskName, LongRunningTaskStatus.Running);
            var cancellationToken = CurrentTask.CancellationTokenSource.Token;
            try
            {
                CurrentTask.Status = LongRunningTaskStatus.Running;
                await SendBackgroundStatus(taskName,
                    LongRunningTaskStatus.Running,
                    cancellationToken,
                    $"Training SMT Model '{SelectedSmtAlgorithm}'.");

                Logger!.LogInformation($"Training SMT Model '{SelectedSmtAlgorithm}'.");

                CurrentTask.Status = LongRunningTaskStatus.Completed;

                TranslationCommandable = new TranslationCommands();

                var symmetrizationHeuristic = new SymmetrizationHeuristic();
                if (isTrainedSymmetrizedModel == true)
                {
                    symmetrizationHeuristic = SymmetrizationHeuristic.GrowDiagFinalAnd;
                }
                else
                {
                    symmetrizationHeuristic = SymmetrizationHeuristic.None;
                }

                if (SelectedSmtAlgorithm is null)
                {
                    SelectedSmtAlgorithm = new SmtAlgorithm
                    {
                        SmtName = "FastAlign",
                        IsEnabled = true
                    };
                }

                SmtModelType modelType = SmtModelType.FastAlign;

                Enum.TryParse<SmtModelType>(SelectedSmtAlgorithm.SmtName, out modelType);

                // 
                if (ProjectType == ParallelProjectType.AlignmentOnly)
                {
                    var sourceTokenizedTextCorpusId =
                        _tokenizedCorpora.FirstOrDefault(tc => tc.CorpusId.Id == _sourceCorpusNodeViewModel.CorpusId);
                    if (sourceTokenizedTextCorpusId == null)
                    {
                        throw new MissingTokenizedTextCorpusIdException(
                            $"Cannot find the source TokenizedTextCorpusId associated to Corpus with Id '{ParallelCorpusConnectionViewModel.SourceConnector.ParentNode.CorpusId}'.");
                    }
                    
                    var targetTokenizedTextCorpusId =
                        _tokenizedCorpora.FirstOrDefault(tc => tc.CorpusId.Id == _targetCorpusNodeViewModel.CorpusId);
                    if (targetTokenizedTextCorpusId == null)
                    {
                        throw new MissingTokenizedTextCorpusIdException(
                            $"Cannot find the target TokenizedTextCorpusId associated to Corpus with Id '{ParallelCorpusConnectionViewModel.DestinationConnector.ParentNode.CorpusId}'.");
                    }

                    ParallelCorpusId? ParallelCorpusId;
                    ParallelCorpusId = TopLevelProjectIds.ParallelCorpusIds.FirstOrDefault(x =>
                        x.SourceTokenizedCorpusId.IdEquals(sourceTokenizedTextCorpusId) && x.TargetTokenizedCorpusId.IdEquals(targetTokenizedTextCorpusId));

                    ParallelTextCorpus = await ParallelCorpus.Get(Mediator!,
                        new ParallelCorpusId(ParallelCorpusId.Id), useCache: true);
                    //ParallelTokenizedCorpus = ParallelTextCorpus;

                    //ParallelTextCorpus =
                    //    await Task.Run(() => sourceTokenizedTextCorpus.EngineAlignRows(targetTokenizedTextCorpus,
                    //        new List<ClearBible.Engine.Corpora.VerseMapping>()), cancellationToken);

                    //Logger!.LogInformation($"Saving parallelization '{parallelCorpusDisplayName}'");
                    //await SendBackgroundStatus(taskName,
                    //    LongRunningTaskStatus.Running,
                    //    cancellationToken,
                    //    $"Saving parallelization '{parallelCorpusDisplayName}'...");

                    //ParallelTokenizedCorpus = await ParallelTextCorpus.Create(parallelCorpusDisplayName, Mediator!, cancellationToken);

                    //await SendBackgroundStatus(taskName,
                    //    LongRunningTaskStatus.Completed,
                    //    cancellationToken,
                    //    $"Completed saving parallelization '{parallelCorpusDisplayName}'.");
                }


                WordAlignmentModel = await TranslationCommandable.TrainSmtModel(
                    modelType,
                    ParallelTextCorpus,
                    new DelegateProgress(async status =>
                    {
                        var message =
                            $"Training symmetrized {SelectedSmtAlgorithm.SmtName} model: {status.PercentCompleted:P}";
                        await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running, cancellationToken,
                            message);
                        Logger!.LogInformation(message);

                    }
                    ), symmetrizationHeuristic);

                await SendBackgroundStatus(taskName,
                    LongRunningTaskStatus.Completed,
                    cancellationToken,
                    $"Completed SMT Model '{SelectedSmtAlgorithm.SmtName}'.");

                Logger!.LogInformation($"Completed SMT Model '{SelectedSmtAlgorithm.SmtName}'.");


            }
            catch (OperationCanceledException ex)
            {
                Logger!.LogInformation($"TrainSmtModel - operation canceled.");
            }
            catch (MediatorErrorEngineException ex)
            {
                if (ex.Message.Contains("The operation was canceled."))
                {
                    Logger!.LogInformation($"TrainSmtModel - operation canceled.");
                }
                else
                {
                    Logger!.LogError(ex, "An unexpected Engine exception was thrown.");
                }

            }
            catch (Exception ex)
            {
                Logger!.LogError(ex, $"An unexpected error occurred while training the SMT model.");
                if (!cancellationToken.IsCancellationRequested)
                {
                    soundType = SoundType.Error;
                    await SendBackgroundStatus(taskName,
                        LongRunningTaskStatus.Failed,
                        cancellationToken,
                        exception: ex);
                }

                CurrentTask.Status = LongRunningTaskStatus.Failed;
            }
            finally
            {
                _longRunningTaskManager.TaskComplete(taskName);
                if (cancellationToken.IsCancellationRequested)
                {
                    CurrentTask.Status = LongRunningTaskStatus.Cancelled;
                    await SendBackgroundStatus(taskName,
                        LongRunningTaskStatus.Completed,
                        cancellationToken,
                        $"Training the SMT Model was canceled.'{SelectedSmtAlgorithm.SmtName}'.");
                }
                IsBusy = false;
                Message = string.Empty;

                // check to see if there are still High Performance Tasks still out there
                var numTasks = _backgroundTasksViewModel.GetNumberOfPerformanceTasksRemaining();
                if (numTasks == 0 && _systemPowerModes.IsHighPerformanceEnabled)
                {
                    // shut down high performance mode
                    await _systemPowerModes.TurnOffHighPerformanceMode();
                }
            }

            if (UseDefaults == false)
            {
                PlaySound.PlaySoundFromResource(soundType);
            }


            return CurrentTask.Status;
        }

        /// <summary>
        /// Button click for the background tasks on the status bar
        /// </summary>
        public async void BackgroundTasks()
        {
            await EventAggregator!.PublishOnUIThreadAsync(new ToggleBackgroundTasksVisibilityMessage());
        }

        #endregion // Methods
    }
}
