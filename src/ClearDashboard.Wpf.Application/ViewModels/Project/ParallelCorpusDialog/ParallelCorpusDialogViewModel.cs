using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.Exceptions;
using ClearApplicationFoundation.Extensions;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearDashboard.DataAccessLayer.Wpf;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf.Infrastructure;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Exceptions;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using System.Collections.Generic;
using System;
using ParallelCorpus = ClearDashboard.DataAccessLayer.Models.ParallelCorpus;
using ClearBible.Engine.Corpora;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.Wpf.Application.ViewModels.ProjectDesignSurface;
using SIL.Machine.Translation;
using SIL.Machine.Utils;
using AlignmentSet = ClearDashboard.DAL.Alignment.Translation.AlignmentSet;
using Translation = ClearDashboard.DAL.Alignment.Translation.Translation;
using TranslationSet = ClearDashboard.DAL.Alignment.Translation.TranslationSet;

namespace ClearDashboard.Wpf.Application.ViewModels.Project.ParallelCorpusDialog
{
    public class ParallelCorpusDialogViewModel : DashboardApplicationWorkflowShellViewModel, IParallelCorpusDialogViewModel
    {

        public ParallelCorpusDialogViewModel()
        {
            // used by Caliburn Micro for design time 
           // ParallelCorpus = new ParallelCorpus();
            LongRunningTaskStatus = LongRunningTaskStatus.NotStarted;
        }

        //parameters.Add(new NamedParameter("dialogMode", DialogMode.Add));
        //parameters.Add(new NamedParameter("connectionViewModel", newConnection));
        //parameters.Add(new NamedParameter("sourceCorpusNodeViewModel", sourceCorpusNode));
        //parameters.Add(new NamedParameter("targetCorpusNodeViewModel", targetCorpusNode));

        public ParallelCorpusDialogViewModel(DialogMode dialogMode, ConnectionViewModel connectionViewModel, CorpusNodeViewModel sourceCorpusNodeViewModel, CorpusNodeViewModel targetCorpusNodeViewModel, DashboardProjectManager? projectManager, INavigationService navigationService, ILogger<ParallelCorpusDialogViewModel> logger,
            IEventAggregator eventAggregator, IMediator mediator, ILifetimeScope lifetimeScope) : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope)
        {
            CanOk = true;
            DisplayName = LocalizationStrings.Get("ParallelCorpusDialog_ParallelCorpus", Logger);

            DialogMode = dialogMode;
            ConnectionViewModel = connectionViewModel;
            SourceCorpusNodeViewModel = sourceCorpusNodeViewModel;
            TargetCorpusNodeViewModel = targetCorpusNodeViewModel;

            LongRunningTaskStatus = LongRunningTaskStatus.NotStarted;
            SelectedSmtAlgorithm = SmtModelType.FastAlign;
        }

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

        public ConnectionViewModel ConnectionViewModel
        {
            get => _connectionViewModel;
            set => Set(ref _connectionViewModel, value);
        }

        private SmtModelType _selectedSmtAlgorithm;
        public SmtModelType SelectedSmtAlgorithm
        {
            get => _selectedSmtAlgorithm;
            set => Set(ref _selectedSmtAlgorithm, value);
        }

        public IWordAlignmentModel WordAlignmentModel { get; set; }
        public DAL.Alignment.Corpora.ParallelCorpus ParallelTokenizedCorpus { get; set; }

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


        public bool CanCancel => true /* can always cancel */;
        public async void Cancel()
        {
            await TryCloseAsync(false);
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

        // ReSharper disable once RedundantDefaultMemberInitializer
        public CancellationTokenSource? CancellationTokenSource { get; private set; }
        public bool LongProcessRunning;
        private ConnectionViewModel _connectionViewModel;

        public LongRunningTaskStatus LongRunningTaskStatus { get; set; }

        public CancellationTokenSource CreateCancellationTokenSource()
        {
            CancellationTokenSource = new CancellationTokenSource();
            return CancellationTokenSource;
        }

        public async Task<LongRunningTaskStatus> AddParallelCorpus(string parallelCorpusDisplayName)
        {
            var sourceNodeTokenization = SourceCorpusNodeViewModel.NodeTokenizations.FirstOrDefault();
            if (sourceNodeTokenization == null)
            {
                throw new MissingTokenizedTextCorpusIdException(
                    $"Cannot find the source TokenizedTextCorpusId associated to Corpus with Id '{ConnectionViewModel.SourceConnector.ParentNode.CorpusId}'.");
            }
            var targetNodeTokenization = TargetCorpusNodeViewModel.NodeTokenizations.FirstOrDefault();
            if (sourceNodeTokenization == null)
            {
                throw new MissingTokenizedTextCorpusIdException(
                    $"Cannot find the target TokenizedTextCorpusId associated to Corpus with Id '{ConnectionViewModel.DestinationConnector.ParentNode.CorpusId}'.");
            }


            IsBusy = true;
            CancellationTokenSource ??= new CancellationTokenSource();

            var cancellationToken = CancellationTokenSource.Token;

            var statusName = "ParallelCorpus";

            try
            {
                LongRunningTaskStatus = LongRunningTaskStatus.Running;
                Logger!.LogInformation($"Retrieving tokenized source and target corpora for '{parallelCorpusDisplayName}'.");
                await SendBackgroundStatus(statusName,
                    LongRunningTaskStatus.Running,
                    cancellationToken,
                    $"Retrieving tokenized source and target corpora for '{parallelCorpusDisplayName}'...");

                var sourceTokenizedTextCorpus = await TokenizedTextCorpus.Get(Mediator, new TokenizedTextCorpusId(sourceNodeTokenization.TokenizedTextCorpusId));
                var targetTokenizedTextCorpus = await TokenizedTextCorpus.Get(Mediator, new TokenizedTextCorpusId(targetNodeTokenization.TokenizedTextCorpusId));

                Logger!.LogInformation($"Parallelizing source and target corpora");
                await SendBackgroundStatus(statusName,
                    LongRunningTaskStatus.Running,
                    cancellationToken,
                    $"Parallelizing source and target corpora for '{parallelCorpusDisplayName}'...");

                // TODO:  Ask Chris/Russell how to go from models VerseMapping to Engine VerseMapping
                ParallelTextCorpus = await Task.Run(async () => sourceTokenizedTextCorpus.EngineAlignRows(targetTokenizedTextCorpus, new List<ClearBible.Engine.Corpora.VerseMapping>()), cancellationToken);


                Logger!.LogInformation($"Saving parallelization '{parallelCorpusDisplayName}'");
                await SendBackgroundStatus(statusName,
                    LongRunningTaskStatus.Running,
                    cancellationToken,
                    $"Saving parallelization '{parallelCorpusDisplayName}'...");

                ParallelTokenizedCorpus = await ParallelTextCorpus.Create(parallelCorpusDisplayName, Mediator!);

                await SendBackgroundStatus(statusName,
                    LongRunningTaskStatus.Completed,
                    cancellationToken,
                    $"Completed saving parallelization '{parallelCorpusDisplayName}'.");

                Logger.LogInformation($"Completed saving parallelization '{parallelCorpusDisplayName}'");

                LongRunningTaskStatus = LongRunningTaskStatus.Completed;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"An unexpected error occurred while creating the ParallelCorpus.");
                if (!cancellationToken.IsCancellationRequested)
                {
                    await SendBackgroundStatus(statusName,
                        LongRunningTaskStatus.Failed,
                        cancellationToken,
                        exception: ex);

                }

                LongRunningTaskStatus = LongRunningTaskStatus.Failed;
            }
            finally
            {
                CancellationTokenSource.Dispose();
                LongProcessRunning = false;
                IsBusy = false;
                Message = string.Empty;
            }

            PlaySound.PlaySoundFromResource(null, null);

            return LongRunningTaskStatus;
        }


        public TranslationCommands TranslationCommandable { get; set; }
        public async Task<LongRunningTaskStatus> TrainSmtModel()
        {
            IsBusy = true;
            CancellationTokenSource ??= new CancellationTokenSource();
            var cancellationToken = CancellationTokenSource.Token;
            var statusName = " TrainingSMTModel";
            try
            {
                LongRunningTaskStatus = LongRunningTaskStatus.Running;
                await SendBackgroundStatus(statusName,
                    LongRunningTaskStatus.Running,
                    cancellationToken,
                    $"Training SMT Model '{SelectedSmtAlgorithm}'.");

                Logger!.LogInformation($"Training SMT Model '{SelectedSmtAlgorithm}'.");

                LongRunningTaskStatus = LongRunningTaskStatus.Completed;

                TranslationCommandable = new TranslationCommands();

                WordAlignmentModel = await TranslationCommandable.TrainSmtModel(
                    SelectedSmtAlgorithm,
                    ParallelTextCorpus,
                    new DelegateProgress(async status =>
                        {
                            var message = $"Training symmetrized {SelectedSmtAlgorithm} model: {status.PercentCompleted:P}";
                            await SendBackgroundStatus(statusName, LongRunningTaskStatus.Running, cancellationToken, message);
                          Logger!.LogInformation(message);
                        }
                    ), SymmetrizationHeuristic.GrowDiagFinalAnd);

                await SendBackgroundStatus(statusName,
                    LongRunningTaskStatus.Completed,
                    cancellationToken,
                    $"Completed SMT Model '{SelectedSmtAlgorithm}'.");

                Logger!.LogInformation($"Completed SMT Model '{SelectedSmtAlgorithm}'.");


            }
            catch (Exception ex)
            {
                Logger!.LogError(ex, $"An unexpected error occurred while training the SMT model.");
                if (!cancellationToken.IsCancellationRequested)
                {
                    await SendBackgroundStatus(statusName,
                        LongRunningTaskStatus.Failed,
                        cancellationToken,
                        exception: ex);
                }

                LongRunningTaskStatus = LongRunningTaskStatus.Failed;
            }
            finally
            {
                CancellationTokenSource.Dispose();
                LongProcessRunning = false;
                IsBusy = false;
                Message = string.Empty;
            }

            PlaySound.PlaySoundFromResource(null, null);

            return LongRunningTaskStatus;
        }


        public TranslationSet TranslationSet { get; set; }
        public async Task<LongRunningTaskStatus> AddTranslationSet(string translationSetDisplayName)
        {
            IsBusy = true;
            CancellationTokenSource ??= new CancellationTokenSource();
            var cancellationToken = CancellationTokenSource.Token;
            var statusName = "TranslationSet";
            try
            {
                LongRunningTaskStatus = LongRunningTaskStatus.Running;
                await SendBackgroundStatus(statusName,
                    LongRunningTaskStatus.Running,
                    cancellationToken,
                    $"Creating the TranslationSet '{translationSetDisplayName}'...");

                var translationModel = WordAlignmentModel.GetTranslationTable();
                //TranslationSet = await translationModel.Create(translationSetDisplayName,
                //   SelectedSmtAlgorithm.ToString(), new(), ParallelTokenizedCorpus.ParallelCorpusId, Mediator);

                // RUSSELL - code review
                TranslationSet = await TranslationSet.Create(null, AlignmentSet.AlignmentSetId,
                    translationSetDisplayName, new Dictionary<string, object>(), ParallelTokenizedCorpus.ParallelCorpusId, Mediator);



                await SendBackgroundStatus(statusName,
                    LongRunningTaskStatus.Completed,
                    cancellationToken,
                    $"Completed creation of the TranslationSet '{translationSetDisplayName}'.");
                Logger!.LogInformation($"Completed creating the TranslationSet '{translationSetDisplayName}'");

                LongRunningTaskStatus = LongRunningTaskStatus.Completed;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"An unexpected error occurred while creating creating the TranslationSet.");
                if (!cancellationToken.IsCancellationRequested)
                {
                    await SendBackgroundStatus(statusName,
                        LongRunningTaskStatus.Failed,
                        cancellationToken,
                        exception: ex);
                }

                LongRunningTaskStatus = LongRunningTaskStatus.Failed;
            }
            finally
            {
                CancellationTokenSource.Dispose();
                LongProcessRunning = false;
                IsBusy = false;
                Message = string.Empty;
            }

            PlaySound.PlaySoundFromResource(null, null);

            return LongRunningTaskStatus;

        }


        public IEnumerable<AlignedTokenPairs> AlignedTokenPairs { get; set; }

        public AlignmentSet AlignmentSet { get; set; }
        public async Task<LongRunningTaskStatus> AddAlignmentSet(string alignmentSetDisplayName)
        {
            IsBusy = true;
            CancellationTokenSource ??= new CancellationTokenSource();
            var cancellationToken = CancellationTokenSource.Token;
            var statusName = "AlignmentSet";
            try
            {

                LongRunningTaskStatus = LongRunningTaskStatus.Running;
                await SendBackgroundStatus(statusName,
                    LongRunningTaskStatus.Running,
                    cancellationToken,
                    $"Aligning corpora and creating the AlignmentSet '{alignmentSetDisplayName}'...");

                AlignedTokenPairs = TranslationCommandable.PredictAllAlignedTokenIdPairs(WordAlignmentModel, ParallelTextCorpus);
                AlignmentSet = await AlignedTokenPairs.Create(alignmentSetDisplayName, SelectedSmtAlgorithm.ToString(),
                    false, new(), ParallelTokenizedCorpus.ParallelCorpusId, Mediator);
                await SendBackgroundStatus(statusName,
                    LongRunningTaskStatus.Completed,
                    cancellationToken,
                    $"Completed creation of the AlignmentSet '{alignmentSetDisplayName}'.");
               
                Logger!.LogInformation($"Completed creating the AlignmentSet '{alignmentSetDisplayName}'");

                LongRunningTaskStatus = LongRunningTaskStatus.Completed;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"An unexpected error occurred while creating creating the AlignmentSet.");
                if (!cancellationToken.IsCancellationRequested)
                {
                    await SendBackgroundStatus(statusName,
                        LongRunningTaskStatus.Failed,
                        cancellationToken,
                        exception:ex);
                }

                LongRunningTaskStatus = LongRunningTaskStatus.Failed;
            }
            finally
            {
                CancellationTokenSource.Dispose();
                LongProcessRunning = false;
                IsBusy = false;
                Message = string.Empty;
            }

            PlaySound.PlaySoundFromResource(null, null);

            return LongRunningTaskStatus;
        }


    }
}
