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
using ClearDashboard.Wpf.Application.ViewModels.ProjectDesignSurface;
using SIL.Machine.Translation;
using SIL.Machine.Utils;
using AlignmentSet = ClearDashboard.DAL.Alignment.Translation.AlignmentSet;
using TranslationSet = ClearDashboard.DAL.Alignment.Translation.TranslationSet;

namespace ClearDashboard.Wpf.Application.ViewModels.Project.ParallelCorpusDialog
{

    public enum ProcessStatus
    {
        NotStarted,
        Running,
        Failed,
        Completed,
    }

    public interface IParallelCorpusDialogViewModel
    {
        CorpusNodeViewModel SourceCorpusNodeViewModel { get; set; }
        CorpusNodeViewModel TargetCorpusNodeViewModel { get; set; }
        ConnectionViewModel ConnectionViewModel { get; set; }
        SmtModelType SelectedSmtAlgorithm { get; set; }
        IWordAlignmentModel WordAlignmentModel { get; set; }
        DAL.Alignment.Corpora.ParallelCorpus ParallelTokenizedCorpus { get; set; }
        EngineParallelTextCorpus ParallelTextCorpus { get; set; }
        TranslationSet TranslationSet { get; set; }
        IEnumerable<AlignedTokenPairs> AlignedTokenPairs { get; set; }
        AlignmentSet AlignmentSet { get; set; }
        string? CurrentStepTitle { get; set; }
        CancellationTokenSource CreateCancellationTokenSource();
        Task<ProcessStatus> AddParallelCorpus(string parallelCorpusDisplayName);
        Task<ProcessStatus> TrainSmtModel();
        Task<ProcessStatus> AddTranslationSet(string translationSetDisplayName);
        Task<ProcessStatus> AddAlignmentSet(string alignmentSetDisplayName);

        Task SendBackgroundStatus(string name, LongRunningProcessStatus status, CancellationToken cancellationToken,
            string? description = null, Exception? ex = null);
        List<IWorkflowStepViewModel> Steps { get; }

        void Ok();
        void Cancel();
        CancellationTokenSource? CancellationTokenSource { get; }

    }

    public class ParallelCorpusDialogViewModel : DashboardApplicationWorkflowShellViewModel, IParallelCorpusDialogViewModel
    {

        public ParallelCorpusDialogViewModel()
        {
            // used by Caliburn Micro for design time 
           // ParallelCorpus = new ParallelCorpus();
            ProcessStatus = ProcessStatus.NotStarted;
        }

        public ParallelCorpusDialogViewModel(DashboardProjectManager? projectManager, INavigationService navigationService, ILogger<ParallelCorpusDialogViewModel> logger,
            IEventAggregator eventAggregator, IMediator mediator, ILifetimeScope lifetimeScope) : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope)
        {
            CanOk = true;
            DisplayName = LocalizationStrings.Get("ParallelCorpusDialog_ParallelCorpus", Logger);

            //ParallelCorpus = new ParallelCorpus();
            ProcessStatus = ProcessStatus.NotStarted;
            SelectedSmtAlgorithm = SmtModelType.FastAlign;
        }

        //private ParallelCorpus _parallelCorpus;
        //public ParallelCorpus ParallelCorpus
        //{
        //    get => _parallelCorpus;
        //    private init => Set(ref _parallelCorpus, value);
        //}

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
            var views = LifetimeScope?.ResolveKeyedOrdered<IWorkflowStepViewModel>("ParallelCorpusDialog", "Order").ToArray();

            if (views == null || !views.Any())
            {
                throw new DependencyRegistrationMissingException(
                    "There are no dependency inject registrations of 'IWorkflowStepViewModel' with the key of 'ParallelCorpusDialog'.  Please check the dependency registration in your bootstrapper implementation.");
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

        public ProcessStatus ProcessStatus { get; set; }

        public CancellationTokenSource CreateCancellationTokenSource()
        {
            CancellationTokenSource = new CancellationTokenSource();
            return CancellationTokenSource;
        }

        public async Task<ProcessStatus> AddParallelCorpus(string parallelCorpusDisplayName)
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
                ProcessStatus = ProcessStatus.Running;
                Logger!.LogInformation($"Retrieving tokenized source and target corpora for '{parallelCorpusDisplayName}'.");
                await SendBackgroundStatus(statusName,
                    LongRunningProcessStatus.Working,
                    cancellationToken,
                    $"Retrieving tokenized source and target corpora for '{parallelCorpusDisplayName}'...");
                //await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
                //{
                //    Name = "ParallelCorpus",
                //    Description = $"Retrieving tokenized source and target corpora for '{ParallelCorpus.DisplayName}'...",
                //    StartTime = DateTime.Now,
                //    TaskLongRunningProcessStatus = LongRunningProcessStatus.Working
                //}), cancellationToken);


                var sourceTokenizedTextCorpus = await TokenizedTextCorpus.Get(Mediator, new TokenizedTextCorpusId(sourceNodeTokenization.TokenizedTextCorpusId));
                var targetTokenizedTextCorpus = await TokenizedTextCorpus.Get(Mediator, new TokenizedTextCorpusId(targetNodeTokenization.TokenizedTextCorpusId));

                Logger!.LogInformation($"Aligning rows between target and source corpora");
                await SendBackgroundStatus(statusName,
                    LongRunningProcessStatus.Working,
                    cancellationToken,
                    $"Aligning rows for '{parallelCorpusDisplayName}' between target and source corpora...");
                //await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
                //{
                //    Name = "ParallelCorpus",
                //    Description = $"Aligning rows for '{parallelCorpusDisplayName}' between target and source corpora...",
                //    StartTime = DateTime.Now,
                //    TaskLongRunningProcessStatus = LongRunningProcessStatus.Working
                //}), cancellationToken);

                // TODO:  Ask Chris/Russell how to go from models VerseMapping to Engine VerseMapping
                ParallelTextCorpus = await Task.Run(async () => sourceTokenizedTextCorpus.EngineAlignRows(targetTokenizedTextCorpus, new List<ClearBible.Engine.Corpora.VerseMapping>()), cancellationToken);


                Logger!.LogInformation($"Creating the ParallelCorpus '{parallelCorpusDisplayName}'");
                await SendBackgroundStatus(statusName,
                    LongRunningProcessStatus.Working,
                    cancellationToken,
                    $"Creating  ParallelCorpus '{parallelCorpusDisplayName}'...");
                //await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
                //{
                //    Name = "ParallelCorpus",
                //    Description = $"Creating  ParallelCorpus '{parallelCorpusDisplayName}'...",
                //    StartTime = DateTime.Now,
                //    TaskLongRunningProcessStatus = LongRunningProcessStatus.Working
                //}), cancellationToken);
                ParallelTokenizedCorpus = await ParallelTextCorpus.Create(parallelCorpusDisplayName, Mediator!);

                await SendBackgroundStatus(statusName,
                    LongRunningProcessStatus.Completed,
                    cancellationToken,
                    $"Completed creation of  ParallelCorpus '{parallelCorpusDisplayName}'.");
                //await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
                //{
                //    Name = "ParallelCorpus",
                //    Description = $"Completed creation of  ParallelCorpus '{parallelCorpusDisplayName}'.",
                //    StartTime = DateTime.Now,
                //    TaskLongRunningProcessStatus = LongRunningProcessStatus.Completed
                //}), cancellationToken);
                Logger.LogInformation($"Completed creating the ParallelCorpus '{parallelCorpusDisplayName}'");

                ProcessStatus = ProcessStatus.Completed;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"An unexpected error occurred while creating the ParallelCorpus.");
                if (!cancellationToken.IsCancellationRequested)
                {
                    await SendBackgroundStatus(statusName,
                        LongRunningProcessStatus.Error,
                        cancellationToken,
                        exception: ex);
                    //await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(
                    //    new BackgroundTaskStatus
                    //    {
                    //        Name = "ParallelCorpus",
                    //        EndTime = DateTime.Now,
                    //        ErrorMessage = $"{ex}",
                    //        TaskLongRunningProcessStatus = LongRunningProcessStatus.Error
                    //    }), cancellationToken);

                }

                ProcessStatus = ProcessStatus.Failed;
            }
            finally
            {
                CancellationTokenSource.Dispose();
                LongProcessRunning = false;
                IsBusy = false;
                Message = string.Empty;
            }

            return ProcessStatus;
        }


        public TranslationCommands TranslationCommandable { get; set; }
        public async Task<ProcessStatus> TrainSmtModel()
        {
            IsBusy = true;
            CancellationTokenSource ??= new CancellationTokenSource();
            var cancellationToken = CancellationTokenSource.Token;
            var statusName = " TrainingSMTModel";
            try
            {
                ProcessStatus = ProcessStatus.Running;
                await SendBackgroundStatus(statusName,
                    LongRunningProcessStatus.Working,
                    cancellationToken,
                    $"Training SMT Model '{SelectedSmtAlgorithm}'.");
                //await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
                //{
                //    Name = "TrainingSmtModel",
                //    Description = $"Training SMT Model '{SelectedSmtAlgorithm}'.",
                //    StartTime = DateTime.Now,
                //    TaskLongRunningProcessStatus = LongRunningProcessStatus.Working
                //}), cancellationToken);
                Logger!.LogInformation($"Training SMT Model '{SelectedSmtAlgorithm}'.");

                ProcessStatus = ProcessStatus.Completed;

                TranslationCommandable = new TranslationCommands();

                WordAlignmentModel = await TranslationCommandable.TrainSmtModel(
                    SelectedSmtAlgorithm,
                    ParallelTextCorpus,
                    new DelegateProgress(status =>
                       Logger.LogInformation($"Training symmetrized {SelectedSmtAlgorithm} model: {status.PercentCompleted:P}")),
                    SymmetrizationHeuristic.GrowDiagFinalAnd);

                await SendBackgroundStatus(statusName,
                    LongRunningProcessStatus.Completed,
                    cancellationToken,
                    $"Completed SMT Model '{SelectedSmtAlgorithm}'.");
                //await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
                //{
                //    Name = "TrainingSmtModel",
                //    Description = $"Completed SMT Model '{SelectedSmtAlgorithm}'.",
                //    StartTime = DateTime.Now,
                //    TaskLongRunningProcessStatus = LongRunningProcessStatus.Completed
                //}), cancellationToken);
                Logger!.LogInformation($"Completed SMT Model '{SelectedSmtAlgorithm}'.");


            }
            catch (Exception ex)
            {
                Logger!.LogError(ex, $"An unexpected error occurred while training the SMT model.");
                if (!cancellationToken.IsCancellationRequested)
                {
                    await SendBackgroundStatus(statusName,
                        LongRunningProcessStatus.Error,
                        cancellationToken,
                        exception: ex);
                    //await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(
                    //    new BackgroundTaskStatus
                    //    {
                    //        Name = "TrainingSmtModel",
                    //        EndTime = DateTime.Now,
                    //        ErrorMessage = $"{ex}",
                    //        TaskLongRunningProcessStatus = LongRunningProcessStatus.Error
                    //    }), cancellationToken);
                }

                ProcessStatus = ProcessStatus.Failed;
            }
            finally
            {
                CancellationTokenSource.Dispose();
                LongProcessRunning = false;
                IsBusy = false;
                Message = string.Empty;
            }

            return ProcessStatus;
        }


        public TranslationSet TranslationSet { get; set; }
        public async Task<ProcessStatus> AddTranslationSet(string translationSetDisplayName)
        {
            IsBusy = true;
            CancellationTokenSource ??= new CancellationTokenSource();
            var cancellationToken = CancellationTokenSource.Token;
            var statusName = "TranslationSet";
            try
            {
                ProcessStatus = ProcessStatus.Running;
                await SendBackgroundStatus(statusName,
                    LongRunningProcessStatus.Working,
                    cancellationToken,
                    $"Creating the TranslationSet '{translationSetDisplayName}'...");
                //await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
                //{
                //    Name = "TranslationSet",
                //    Description = $"Creating the TranslationSet '{translationSetDisplayName}'...",
                //    StartTime = DateTime.Now,
                //    TaskLongRunningProcessStatus = LongRunningProcessStatus.Working
                //}), cancellationToken);

                var translationModel = WordAlignmentModel.GetTranslationTable();
                TranslationSet = await translationModel.Create(translationSetDisplayName,
                    SelectedSmtAlgorithm.ToString(), new(), ParallelTokenizedCorpus.ParallelCorpusId, Mediator);

                await SendBackgroundStatus(statusName,
                    LongRunningProcessStatus.Completed,
                    cancellationToken,
                    $"Completed creation of the TranslationSet '{translationSetDisplayName}'.");
                //await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
                //{
                //    Name = "TranslationSet",
                //    Description = $"Completed creation of the TranslationSet '{translationSetDisplayName}'.",
                //    StartTime = DateTime.Now,
                //    TaskLongRunningProcessStatus = LongRunningProcessStatus.Completed
                //}), cancellationToken);
                Logger!.LogInformation($"Completed creating the TranslationSet '{translationSetDisplayName}'");

                ProcessStatus = ProcessStatus.Completed;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"An unexpected error occurred while creating creating the TranslationSet.");
                if (!cancellationToken.IsCancellationRequested)
                {
                    await SendBackgroundStatus(statusName,
                        LongRunningProcessStatus.Error,
                        cancellationToken,
                        exception: ex);
                    //await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(
                    //    new BackgroundTaskStatus
                    //    {
                    //        Name = "TranslationSet",
                    //        EndTime = DateTime.Now,
                    //        ErrorMessage = $"{ex}",
                    //        TaskLongRunningProcessStatus = LongRunningProcessStatus.Error
                    //    }), cancellationToken);
                }

                ProcessStatus = ProcessStatus.Failed;
            }
            finally
            {
                CancellationTokenSource.Dispose();
                LongProcessRunning = false;
                IsBusy = false;
                Message = string.Empty;
            }

            return ProcessStatus;

        }


        public IEnumerable<AlignedTokenPairs> AlignedTokenPairs { get; set; }

        public AlignmentSet AlignmentSet { get; set; }
        public async Task<ProcessStatus> AddAlignmentSet(string alignmentSetDisplayName)
        {
            IsBusy = true;
            CancellationTokenSource ??= new CancellationTokenSource();
            var cancellationToken = CancellationTokenSource.Token;
            var statusName = "AlignmentSet";
            try
            {

                ProcessStatus = ProcessStatus.Running;
                await SendBackgroundStatus(statusName,
                    LongRunningProcessStatus.Working,
                    cancellationToken,
                    $"Creating the AlignmentSet '{alignmentSetDisplayName}'...");
                //await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
                //{
                //    Name = "AlignmentSet",
                //    Description = $"Creating the AlignmentSet '{alignmentSetDisplayName}'...",
                //    StartTime = DateTime.Now,
                //    TaskLongRunningProcessStatus = LongRunningProcessStatus.Working
                //}), cancellationToken);

                AlignedTokenPairs = TranslationCommandable.PredictAllAlignedTokenIdPairs(WordAlignmentModel, ParallelTextCorpus);
                AlignmentSet = await AlignedTokenPairs.Create(alignmentSetDisplayName, SelectedSmtAlgorithm.ToString(),
                    false, new(), ParallelTokenizedCorpus.ParallelCorpusId, Mediator);
                await SendBackgroundStatus(statusName,
                    LongRunningProcessStatus.Completed,
                    cancellationToken,
                    $"Completed creation of the AlignmentSet '{alignmentSetDisplayName}'.");
                //await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
                //{
                //    Name = "AlignmentSet",
                //    Description = $"Completed creation of the AlignmentSet '{alignmentSetDisplayName}'.",
                //    StartTime = DateTime.Now,
                //    TaskLongRunningProcessStatus = LongRunningProcessStatus.Completed
                //}), cancellationToken);
                Logger!.LogInformation($"Completed creating the AlignmentSet '{alignmentSetDisplayName}'");

                ProcessStatus = ProcessStatus.Completed;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"An unexpected error occurred while creating creating the AlignmentSet.");
                if (!cancellationToken.IsCancellationRequested)
                {
                    await SendBackgroundStatus(statusName,
                        LongRunningProcessStatus.Error,
                        cancellationToken,
                        exception:ex);
                    //await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(
                    //    new BackgroundTaskStatus
                    //    {
                    //        Name = "AlignmentSet",
                    //        EndTime = DateTime.Now,
                    //        ErrorMessage = $"{ex}",
                    //        TaskLongRunningProcessStatus = LongRunningProcessStatus.Error
                    //    }), cancellationToken);
                }

                ProcessStatus = ProcessStatus.Failed;
            }
            finally
            {
                CancellationTokenSource.Dispose();
                LongProcessRunning = false;
                IsBusy = false;
                Message = string.Empty;
            }

            return ProcessStatus;
        }


    }
}
