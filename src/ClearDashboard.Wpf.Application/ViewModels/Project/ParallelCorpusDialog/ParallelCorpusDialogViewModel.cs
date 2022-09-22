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
using System.Collections.Generic;
using System;
using ParallelCorpus = ClearDashboard.DataAccessLayer.Models.ParallelCorpus;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using SIL.Machine.Translation;
using SIL.Machine.Utils;

namespace ClearDashboard.Wpf.Application.ViewModels.Project.ParallelCorpusDialog
{

    public enum ProcessStatus
    {
        NotStarted,
        Running,
        Failed,
        Completed,
    }
    public class ParallelCorpusDialogViewModel : DashboardApplicationWorkflowShellViewModel
    {

        public ParallelCorpusDialogViewModel()
        {
            // used by Caliburn Micro for design time 
            ParallelCorpus = new ParallelCorpus();
            ProcessStatus = ProcessStatus.NotStarted;
        }

        public ParallelCorpusDialogViewModel(DashboardProjectManager? projectManager, INavigationService navigationService, ILogger<ParallelCorpusDialogViewModel> logger,
            IEventAggregator eventAggregator, IMediator mediator, ILifetimeScope lifetimeScope) : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope)
        {
            CanOk = true;
            DisplayName = LocalizationStrings.Get("ParallelCorpusDialog_ParallelCorpus", Logger);

            ParallelCorpus = new ParallelCorpus();
            ProcessStatus = ProcessStatus.NotStarted;
            SelectedSmtAlgorithm = SmtModelType.FastAlign;
        }

        private ParallelCorpus _parallelCorpus;
        public ParallelCorpus ParallelCorpus
        {
            get => _parallelCorpus;
            private init => Set(ref _parallelCorpus, value);
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

            ParallelCorpus.DisplayName = $"{SourceCorpusNodeViewModel.Name} - {TargetCorpusNodeViewModel.Name}";
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
        public CancellationTokenSource? CancellationTokenSource = null;
        public bool LongProcessRunning;
        private ConnectionViewModel _connectionViewModel;

        public ProcessStatus ProcessStatus { get; set; }

        public async Task<ProcessStatus> AddParallelCorpus()
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
            CancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = CancellationTokenSource.Token;


            //_ = await Task.Factory.StartNew(async () =>
            //{
                try
                {
                    ProcessStatus = ProcessStatus.Running;
                    Logger.LogInformation($"Retrieving tokenized source and target corpora for '{ParallelCorpus.DisplayName}'.");
                    await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
                    {
                        Name = "ParallelCorpus",
                        Description = $"Retrieving tokenized source and target corpora for '{ParallelCorpus.DisplayName}'...",
                        StartTime = DateTime.Now,
                        TaskStatus = StatusEnum.Working
                    }), cancellationToken);


                    var sourceTokenizedTextCorpus = await TokenizedTextCorpus.Get(Mediator, new TokenizedTextCorpusId(sourceNodeTokenization.TokenizedTextCorpusId));
                    var targetTokenizedTextCorpus = await TokenizedTextCorpus.Get(Mediator, new TokenizedTextCorpusId(targetNodeTokenization.TokenizedTextCorpusId));

                    Logger.LogInformation($"Aligning rows between target and source corpora");
                    await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
                    {
                        Name = "ParallelCorpus",
                        Description = $"Aligning rows for '{ParallelCorpus.DisplayName}' between target and source corpora...",
                        StartTime = DateTime.Now,
                        TaskStatus = StatusEnum.Working
                    }), cancellationToken);

                // TODO:  Ask Chris/Russell how to go from models VerseMapping to Engine VerseMapping
                ParallelTextCorpus = await Task.Run(async () => sourceTokenizedTextCorpus.EngineAlignRows(targetTokenizedTextCorpus, new List<ClearBible.Engine.Corpora.VerseMapping>()), cancellationToken);


                    Logger.LogInformation($"Creating the ParallelCorpus '{ParallelCorpus.DisplayName}'");
                    await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
                    {
                        Name = "ParallelCorpus",
                        Description = $"Creating  ParallelCorpus '{ParallelCorpus.DisplayName}'...",
                        StartTime = DateTime.Now,
                        TaskStatus = StatusEnum.Working
                    }), cancellationToken);
                    ParallelTokenizedCorpus = await ParallelTextCorpus.Create(ParallelCorpus.DisplayName, Mediator!);
                

                    await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
                    {
                        Name = "ParallelCorpus",
                        Description = $"Completed creation of  ParallelCorpus '{ParallelCorpus.DisplayName}'.",
                        StartTime = DateTime.Now,
                        TaskStatus = StatusEnum.Completed
                    }), cancellationToken);
                    Logger.LogInformation($"Completed creating the ParallelCorpus '{ParallelCorpus.DisplayName}'");

                    ProcessStatus = ProcessStatus.Completed;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"An unexpected error occurred while creating the ParallelCorpus.");
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(
                            new BackgroundTaskStatus
                            {
                                Name = "ParallelCorpus",
                                EndTime = DateTime.Now,
                                ErrorMessage = $"{ex}",
                                TaskStatus = StatusEnum.Error
                            }), cancellationToken);
                       
                    }

                    ProcessStatus = ProcessStatus.Failed;
                }
                finally
                {
                    CancellationTokenSource.Dispose();
                    LongProcessRunning = false;
                    IsBusy = false;
                }

            //}, cancellationToken);

            return ProcessStatus;
        }


        public async Task TrainSmtModel()
        {
            var translationCommandable = new TranslationCommands();

            using  var smtWordAlignmentModel = await translationCommandable.TrainSmtModel(
                SelectedSmtAlgorithm,
                ParallelTextCorpus,
                new DelegateProgress(status =>
                   Logger.LogInformation($"Training symmetrized Fastalign model: {status.PercentCompleted:P}")),
                SymmetrizationHeuristic.GrowDiagFinalAnd);

            WordAlignmentModel = smtWordAlignmentModel;
        }

        public async Task AddTranslationSet(string translationSetDisplayName)
        {
            var translationModel = WordAlignmentModel.GetTranslationTable();

            var translationSet = await translationModel.Create(translationSetDisplayName, SelectedSmtAlgorithm.ToString(), new(), ParallelTokenizedCorpus.ParallelCorpusId, Mediator);

        }

        public async Task AddAlignmentSet(string alignmentSetDisplayName)
        {

            // WordAlignmentModel

            //var translationModel = WordAlignmentModel.GetTranslationTable();

            //var translationSet = await translationModel.Create(translationSetDisplayName, SelectedSmtAlgorithm.ToString(), new(), ParallelTokenizedCorpus.ParallelCorpusId, Mediator);

        }
    }
}
