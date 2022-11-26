using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.Exceptions;
using ClearApplicationFoundation.Extensions;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.DataAccessLayer.Wpf.Infrastructure;
using ClearDashboard.Wpf.Application.Exceptions;
using ClearDashboard.Wpf.Application.Helpers;
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
using ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface;
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




        public ParallelCorpusDialogViewModel()
        {
            // used by Caliburn Micro for design time 
        }


        public ParallelCorpusDialogViewModel(DialogMode dialogMode,
                                ParallelCorpusConnectionViewModel parallelCorpusConnectionViewModel,
                                CorpusNodeViewModel sourceCorpusNodeViewModel,
                                CorpusNodeViewModel targetCorpusNodeViewModel,
                                IEnumerable<TokenizedTextCorpusId> tokenizedCorpora,
                                DashboardProjectManager? projectManager,
                                INavigationService navigationService,
                                ILogger<ParallelCorpusDialogViewModel> logger,
                                IEventAggregator eventAggregator,
                                IMediator mediator,
                                ILifetimeScope lifetimeScope, LongRunningTaskManager longRunningTaskManager)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope)
        {
            _tokenizedCorpora = tokenizedCorpora;
            _longRunningTaskManager = longRunningTaskManager;
            CanOk = true;

            DisplayName = LocalizationStrings.Get("ParallelCorpusDialog_ParallelCorpus", Logger!);

            DialogMode = dialogMode;
            ParallelCorpusConnectionViewModel = parallelCorpusConnectionViewModel;
            SourceCorpusNodeViewModel = sourceCorpusNodeViewModel;
            TargetCorpusNodeViewModel = targetCorpusNodeViewModel;
            SelectedSmtAlgorithm = SmtModelType.FastAlign;
        }

        private readonly IEnumerable<TokenizedTextCorpusId> _tokenizedCorpora;
        private readonly LongRunningTaskManager _longRunningTaskManager;
        public LongRunningTask CurrentLongRunningTask { get; set; }

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

        private SmtModelType _selectedSmtAlgorithm;
        public SmtModelType SelectedSmtAlgorithm
        {
            get => _selectedSmtAlgorithm;
            set => Set(ref _selectedSmtAlgorithm, value);
        }

        public IWordAlignmentModel WordAlignmentModel { get; set; }
        public ParallelCorpus ParallelTokenizedCorpus { get; set; }

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

            await base.OnInitializeAsync(cancellationToken);

        }


        public bool CanCancel => (CurrentTask != null && CurrentTask.Status != LongRunningTaskStatus.Running) /* can always cancel */;
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


        private bool _canOk;
        private CorpusNodeViewModel _sourceCorpusNodeViewModel;
        private CorpusNodeViewModel _targetCorpusNodeViewModel;

        public bool CanOk
        {
            get => _canOk;
            set => Set(ref _canOk, value);
        }

        public async void Ok()
        {
            await TryCloseAsync(true);
        }

        public object? ExtraData { get; set; }

        public EngineParallelTextCorpus ParallelTextCorpus { get; set; }

        private ParallelCorpusConnectionViewModel _parallelCorpusConnectionViewModel;

        public LongRunningTask? CurrentTask { get; set; }


        public bool UseDefaults { get; set; } = false;

        public async Task<LongRunningTaskStatus> AddParallelCorpus(string parallelCorpusDisplayName)
        {
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

                // TODO:  Ask Chris/Russell how to go from models VerseMapping to Engine VerseMapping
                ParallelTextCorpus =
                    await Task.Run(() => sourceTokenizedTextCorpus.EngineAlignRows(targetTokenizedTextCorpus,
                            new List<ClearBible.Engine.Corpora.VerseMapping>()), cancellationToken);

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

                IsBusy = false;
                Message = string.Empty;
            }

            if (UseDefaults == false)
            {
                PlaySound.PlaySoundFromResource();
            }

            return CurrentTask.Status;
        }


        public TranslationCommands TranslationCommandable { get; set; }
        public async Task<LongRunningTaskStatus> TrainSmtModel()
        {
            IsBusy = true;
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

                WordAlignmentModel = await TranslationCommandable.TrainSmtModel(
                    SelectedSmtAlgorithm,
                    ParallelTextCorpus,
                    new DelegateProgress(async status =>
                        {
                            var message =
                                $"Training symmetrized {SelectedSmtAlgorithm} model: {status.PercentCompleted:P}";
                            await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running, cancellationToken,
                                message);
                            Logger!.LogInformation(message);

                        }
                    ), SymmetrizationHeuristic.GrowDiagFinalAnd);

                await SendBackgroundStatus(taskName,
                    LongRunningTaskStatus.Completed,
                    cancellationToken,
                    $"Completed SMT Model '{SelectedSmtAlgorithm}'.");

                Logger!.LogInformation($"Completed SMT Model '{SelectedSmtAlgorithm}'.");


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
                        $"Training the SMT Model was canceled.'{SelectedSmtAlgorithm}'.");
                }
                IsBusy = false;
                Message = string.Empty;
            }

            if (UseDefaults == false)
            {
                PlaySound.PlaySoundFromResource();
            }


            return CurrentTask.Status;
        }


        public TranslationSet? TranslationSet { get; set; }
        public async Task<LongRunningTaskStatus> AddTranslationSet(string translationSetDisplayName)
        {
            IsBusy = true;
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
            }

            PlaySound.PlaySoundFromResource();

            return CurrentTask.Status;

        }


        public IEnumerable<AlignedTokenPairs> AlignedTokenPairs { get; set; }

        public AlignmentSet? AlignmentSet { get; set; }
        public async Task<LongRunningTaskStatus> AddAlignmentSet(string alignmentSetDisplayName)
        {
            IsBusy = true;

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

                AlignmentSet = await AlignedTokenPairs.Create(alignmentSetDisplayName, SelectedSmtAlgorithm.ToString(),
                    false, new(), ParallelTokenizedCorpus.ParallelCorpusId, Mediator, cancellationToken);
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
            }

            if (UseDefaults == false)
            {
                PlaySound.PlaySoundFromResource();
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
    }
}
