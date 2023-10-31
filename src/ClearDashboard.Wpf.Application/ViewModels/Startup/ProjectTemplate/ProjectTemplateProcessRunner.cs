using Caliburn.Micro;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.SyntaxTree.Corpora;
using ClearBible.Engine.Tokenization;
using ClearBible.Macula.PropertiesSources.Tokenization;
using ClearDashboard.DAL.Alignment;
using ClearDashboard.DAL.Alignment.BackgroundServices;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Features.Denormalization;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.Project;
using MediatR;
using Microsoft.Extensions.Logging;
using SIL.Machine.Corpora;
using SIL.Machine.Tokenization;
using SIL.Machine.Translation;
using SIL.Machine.Utils;
using SIL.ObjectModel;
using SIL.Scripture;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.Wpf.Application.ViewModels.Startup.ProjectTemplate;
using static ClearBible.Engine.Persistence.FileGetBookIds;
using static ClearDashboard.DataAccessLayer.Threading.BackgroundTaskStatus;
using BookInfo = ClearDashboard.DataAccessLayer.Models.BookInfo;
using CorpusType = ClearDashboard.DataAccessLayer.Models.CorpusType;
using ParatextProjectMetadata = ClearDashboard.DataAccessLayer.Models.ParatextProjectMetadata;
using ClearDashboard.DataAccessLayer;

namespace ClearDashboard.Wpf.Application.ViewStartup.ProjectTemplate
{
    public class ProjectTemplateProcessRunner
    {

        #region Member Variables   
        
        private abstract record BackgroundTask
        {
            public BackgroundTask(Type taskReturnType, string? taskName = null) { TaskReturnType = taskReturnType; TaskName = taskName ?? Guid.NewGuid().ToString(); }
            public string TaskName { get; init; }
            public Type TaskReturnType { get; init; }
        }

        private abstract record CorpusBackgroundTask : BackgroundTask
        {
            public CorpusBackgroundTask(Type taskReturnType, string? taskName = null) : base(taskReturnType, taskName)
            {
            }

            public abstract string CorpusName { get; }
        }

        private record ManuscriptCorpusBackgroundTask : CorpusBackgroundTask
        {
            public ManuscriptCorpusBackgroundTask(string taskName, CorpusType corpusType) :
                base(typeof(TokenizedTextCorpus), taskName)
            {
                CorpusType = corpusType;
            }

            public CorpusType CorpusType { get; init; }
            public override string CorpusName => CorpusType.ToString();
        }

        private record ParatextProjectCorpusBackgroundTask : CorpusBackgroundTask
        {
            public ParatextProjectCorpusBackgroundTask(string taskName, ParatextProjectMetadata projectMetadata, Tokenizers tokenizer, IEnumerable<string> bookIds)
                : base(typeof(TokenizedTextCorpus), taskName)
            {
                if (!bookIds.Any())
                    throw new ArgumentException("empty", nameof(bookIds));

                ProjectMetadata = projectMetadata;
                Tokenizer = tokenizer;
                BookIds = bookIds;
            }

            public ParatextProjectMetadata ProjectMetadata { get; init; }
            public Tokenizers Tokenizer { get; init; }
            public IEnumerable<string> BookIds { get; init; }
            public override string CorpusName => ProjectMetadata.Name!;
        }

        private record ParallelCorpusBackgroundTask : BackgroundTask
        {
            public ParallelCorpusBackgroundTask(string taskName, string sourceBackgroundTaskName, string targetBackgroundTaskName)
                : base(typeof(ParallelCorpus), taskName)
            {
                SourceBackgroundTaskName = sourceBackgroundTaskName;
                TargetBackgroundTaskName = targetBackgroundTaskName;
            }
            public string SourceBackgroundTaskName { get; init; }
            public string TargetBackgroundTaskName { get; init; }
        }

        private record TrainSmtModelBackgroundTask : BackgroundTask
        {
            public TrainSmtModelBackgroundTask(string taskName, string parallelCorpusBackgroundTaskName, bool isTrainedSymmetrizedModel, string? smtModelName, bool generateAlignedTokenPairs) :
                base(typeof(TrainSmtModelSet), taskName)
            {
                ParallelCorpusBackgroundTaskName = parallelCorpusBackgroundTaskName;
                IsTrainedSymmetrizedModel = isTrainedSymmetrizedModel;
                SmtModelName = smtModelName;
                GenerateAlignedTokenPairs = generateAlignedTokenPairs;
            }
            public string ParallelCorpusBackgroundTaskName { get; init; }
            public bool IsTrainedSymmetrizedModel { get; init; }
            public string? SmtModelName { get; init; }
            public bool GenerateAlignedTokenPairs { get; init; }
        }

        private record AlignmentSetBackgroundTask : BackgroundTask
        {
            public AlignmentSetBackgroundTask(string taskName, string trainSmtModelBackgroundTaskName, string parallelCorpusBackgroundTaskName) :
                base(typeof(AlignmentSet), taskName)
            {
                TrainSmtModelBackgroundTaskName = trainSmtModelBackgroundTaskName;
                ParallelCorpusBackgroundTaskName = parallelCorpusBackgroundTaskName;
            }
            public string TrainSmtModelBackgroundTaskName { get; init; }
            public string ParallelCorpusBackgroundTaskName { get; init; }
        }

        private record TranslationSetBackgroundTask : BackgroundTask
        {
            public TranslationSetBackgroundTask(string taskName, string trainSmtModelBackgroundTaskName, string alignmentSetBackgroundTaskName) :
                base(typeof(TranslationSet), taskName)
            {
                TrainSmtModelBackgroundTaskName = trainSmtModelBackgroundTaskName;
                AlignmentSetBackgroundTaskName = alignmentSetBackgroundTaskName;
            }
            public string TrainSmtModelBackgroundTaskName { get; init; }
            public string AlignmentSetBackgroundTaskName { get; init; }
        }

        public record TrainSmtModelSet
        {
            public TrainSmtModelSet(IWordAlignmentModel wordAlignmentModel, SmtModelType smtModelType, bool isTrainedSymmetrizedModel, IEnumerable<AlignedTokenPairs>? alignedTokenPairs)
            {
                WordAlignmentModel = wordAlignmentModel;
                SmtModelType = smtModelType;
                IsTrainedSymmetrizedModel = isTrainedSymmetrizedModel;
                AlignedTokenPairs = alignedTokenPairs;
            }

            public IWordAlignmentModel WordAlignmentModel { get; set; }
            public SmtModelType SmtModelType { get; set; }
            public bool IsTrainedSymmetrizedModel { get; set; }
            public IEnumerable<AlignedTokenPairs>? AlignedTokenPairs { get; set; }
        }

        private readonly List<BackgroundTask> backgroundTasksToRun = new();
        private IMediator Mediator { get; init; }

        private ILogger Logger { get; init; }
        private IEventAggregator EventAggregator { get; init; }

        private TranslationCommands TranslationCommands { get; init; }

        private LongRunningTaskManager LongRunningTaskManager { get; init; }
        private SystemPowerModes SystemPowerModes { get; init; }

        static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
        private readonly object lockObject = new();

        private readonly ObservableDictionary<string, bool> _busyState = new();

        #endregion //Member Variables


        #region Public Properties

        public Tokenizers Tokenizers { get; init; }

        #endregion //Public Properties


        #region Observable Properties

        public bool IsBusy => _busyState.Count > 0;

        #endregion //Observable Properties


        #region Constructor

        public ProjectTemplateProcessRunner(
            ILogger<ProjectTemplateProcessRunner> logger,
            IMediator mediator,
            IEventAggregator eventAggregator,
            TranslationCommands translationCommands,
            LongRunningTaskManager longRunningTaskManager,
            SystemPowerModes systemPowerModes)
        {
            Logger = logger;
            Mediator = mediator;
            EventAggregator = eventAggregator;
            TranslationCommands = translationCommands;
            LongRunningTaskManager = longRunningTaskManager;
            SystemPowerModes = systemPowerModes;
        }

        #endregion //Constructor


        #region Methods

        public static string DefaultManuscriptCorpusTaskName(CorpusType corpusType) => $"Add{corpusType}Corpus";
        public static string DefaultParatextPojectCorpusTaskName(string paratextProjectName) => $"AddParatextProject{paratextProjectName}Corpus";

        public static IDictionary<Type, string> DefaultParallizationTaskNameSet(string sourceCorpusName, string targetCorpusName) => new Dictionary<Type, string> {
            { typeof(ParallelCorpus), $"AddParallelCorpus_{sourceCorpusName}_{targetCorpusName}" },
            { typeof(TrainSmtModelSet), $"TrainSmtModel_{sourceCorpusName}_{targetCorpusName}" },
            { typeof(AlignmentSet), $"AddAlignmentSet_{sourceCorpusName}_{targetCorpusName}" },
            { typeof(TranslationSet), $"AddTranslationSet_{sourceCorpusName}_{targetCorpusName}" } };

        public void Cancel()
        {
            LongRunningTaskManager.CancelAllTasks();
        }

        public void StartRegistration()
        {
            backgroundTasksToRun.Clear();
            LongRunningTaskManager.CancellationTokenSource = new CancellationTokenSource();
        }

        public string RegisterManuscriptCorpusTask(CorpusType corpusType, string? taskName = null)
        {
            taskName ??= DefaultManuscriptCorpusTaskName(corpusType);

            if (backgroundTasksToRun.Any(e => e.TaskName == taskName))
                throw new ArgumentException($"Background task with name '{taskName}' already exists", nameof(taskName));

            backgroundTasksToRun.Add(new ManuscriptCorpusBackgroundTask(
                taskName, 
                corpusType));

            return taskName;
        }

        public string RegisterParatextProjectCorpusTask(ParatextProjectMetadata projectMetadata, Tokenizers tokenizer, IEnumerable<string> bookIds, string? taskName = null)
        {
            if (string.IsNullOrEmpty(projectMetadata.Name))
                throw new ArgumentNullException(nameof(projectMetadata.Name));

            taskName ??= DefaultParatextPojectCorpusTaskName(projectMetadata.Name!);

            if (backgroundTasksToRun.Any(e => e.TaskName == taskName))
                throw new ArgumentException($"Background task with name '{taskName}' already exists", nameof(taskName));

            backgroundTasksToRun.Add(new ParatextProjectCorpusBackgroundTask(
                taskName,
                projectMetadata,
                tokenizer,
                bookIds));

            return taskName;
        }

        public IDictionary<Type, string> RegisterParallelizationTasks(
            string sourceCorpusTaskName,
            string targetCorpusTaskName,
            bool isTrainedSymmetrizedModel, 
            string? smtModelName)
        {
            var sourceTask = backgroundTasksToRun.FirstOrDefault(e => e.TaskName == sourceCorpusTaskName);

            if (sourceTask is null || sourceTask is not CorpusBackgroundTask)
                throw new ArgumentException($"Source corpus background task name '{sourceCorpusTaskName}' not found: ", nameof(sourceCorpusTaskName));

            var targetTask = backgroundTasksToRun.FirstOrDefault(e => e.TaskName == targetCorpusTaskName);

            if (targetTask is null || targetTask is not CorpusBackgroundTask)
                throw new ArgumentException($"Target corpus background task name '{targetCorpusTaskName}' not found: ", nameof(targetCorpusTaskName));

            var parallelizationTaskNameSet = DefaultParallizationTaskNameSet(
                (sourceTask as CorpusBackgroundTask)!.CorpusName, 
                (targetTask as CorpusBackgroundTask)!.CorpusName);

            RegisterParallelCorpusTask(
                parallelizationTaskNameSet[typeof(ParallelCorpus)],
                sourceTask.TaskName,
                targetTask.TaskName);
            RegisterTrainSmtModelTask(
                parallelizationTaskNameSet[typeof(TrainSmtModelSet)],
                parallelizationTaskNameSet[typeof(ParallelCorpus)],
                isTrainedSymmetrizedModel,
                smtModelName,
                false);
            RegisterAlignmentSetTask(
                parallelizationTaskNameSet[typeof(AlignmentSet)],
                parallelizationTaskNameSet[typeof(TrainSmtModelSet)],
                parallelizationTaskNameSet[typeof(ParallelCorpus)]
            );
            RegisterTranslationSetTask(
                parallelizationTaskNameSet[typeof(TranslationSet)],
                parallelizationTaskNameSet[typeof(TrainSmtModelSet)],
                parallelizationTaskNameSet[typeof(AlignmentSet)]
            );

            return parallelizationTaskNameSet;
        }

        public void RegisterParallelCorpusTask(string taskName, string sourceBackgroundTaskName, string targetBackgroundTaskName)
        {
            if (backgroundTasksToRun.Any(e => e.TaskName == taskName))
                throw new ArgumentException($"Background task with name '{taskName}' already exists", nameof(taskName));

            if (!backgroundTasksToRun.Any(e => e.TaskName == sourceBackgroundTaskName && e.TaskReturnType == typeof(TokenizedTextCorpus)))
                throw new ArgumentException($"BackgroundTaskName not found: {sourceBackgroundTaskName}", nameof(sourceBackgroundTaskName));

            if (!backgroundTasksToRun.Any(e => e.TaskName == targetBackgroundTaskName && e.TaskReturnType == typeof(TokenizedTextCorpus)))
                throw new ArgumentException($"BackgroundTaskName not found: {targetBackgroundTaskName}", nameof(targetBackgroundTaskName));

            backgroundTasksToRun.Add(new ParallelCorpusBackgroundTask(
                taskName,
                sourceBackgroundTaskName,
                targetBackgroundTaskName));
        }

        public void RegisterTrainSmtModelTask(string taskName, string parallelCorpusTaskName, bool isTrainedSymmetrizedModel, string? smtModelName, bool generateAlignedTokenPairs = true)
        {
            if (backgroundTasksToRun.Any(e => e.TaskName == taskName))
                throw new ArgumentException($"Background task with name '{taskName}' already exists", nameof(taskName));

            if (!backgroundTasksToRun.Any(e => e.TaskName == parallelCorpusTaskName && e.TaskReturnType == typeof(ParallelCorpus)))
                throw new ArgumentException($"BackgroundTaskId not found: {parallelCorpusTaskName}", nameof(parallelCorpusTaskName));

            backgroundTasksToRun.Add(new TrainSmtModelBackgroundTask(
                taskName,
                parallelCorpusTaskName,
                isTrainedSymmetrizedModel,
                smtModelName,
                generateAlignedTokenPairs));
        }

        public void RegisterAlignmentSetTask(string taskName, string trainSmtModelTaskName, string parallelCorpusTaskName)
        {
            if (backgroundTasksToRun.Any(e => e.TaskName == taskName))
                throw new ArgumentException($"Background task with name '{taskName}' already exists", nameof(taskName));

            if (!backgroundTasksToRun
                    .Any(e => 
                        e.TaskName == trainSmtModelTaskName && 
                        e.TaskReturnType == typeof(TrainSmtModelSet)))
                throw new ArgumentException($"BackgroundTaskId not found: {trainSmtModelTaskName}", nameof(trainSmtModelTaskName));

            if (!backgroundTasksToRun.Any(e => e.TaskName == parallelCorpusTaskName && e.TaskReturnType == typeof(ParallelCorpus)))
                throw new ArgumentException($"BackgroundTaskId not found: {parallelCorpusTaskName}", nameof(parallelCorpusTaskName));

            backgroundTasksToRun.Add(new AlignmentSetBackgroundTask(
                taskName,
                trainSmtModelTaskName,
                parallelCorpusTaskName));
        }

        public void RegisterTranslationSetTask(string taskName, string trainSmtModelTaskName, string alignmentSetTaskName)
        {
            if (backgroundTasksToRun.Any(e => e.TaskName == taskName))
                throw new ArgumentException($"Background task with name '{taskName}' already exists", nameof(taskName));

            if (!backgroundTasksToRun
                    .Any(e =>
                        e.TaskName == trainSmtModelTaskName &&
                        e.TaskReturnType == typeof(TrainSmtModelSet)))
                throw new ArgumentException($"BackgroundTaskId not found: {trainSmtModelTaskName}", nameof(trainSmtModelTaskName));

            if (!backgroundTasksToRun.Any(e => e.TaskName == alignmentSetTaskName && e.TaskReturnType == typeof(AlignmentSet)))
                throw new ArgumentException($"BackgroundTaskId not found: {alignmentSetTaskName}", nameof(alignmentSetTaskName));

            backgroundTasksToRun.Add(new TranslationSetBackgroundTask(
                taskName,
                trainSmtModelTaskName,
                alignmentSetTaskName));
        }

        public Task RunRegisteredTasks(Stopwatch sw)
        {
            var corpusTasksByName = new Dictionary<string, Task<TokenizedTextCorpus>>();
            var parallelCorpusTasksByName = new Dictionary<string, Task<ParallelCorpus>>();
            var trainSmtModelTasksByName = new Dictionary<string, Task<TrainSmtModelSet>>();
            var alignmentSetTasksByName = new Dictionary<string, Task<AlignmentSet>>();

            LongRunningTaskManager.Tasks.Clear();

            var endOfSequenceTasks = new Dictionary<string, Task>();

            foreach (var backgroundTaskToRun in backgroundTasksToRun)
            {
                switch (backgroundTaskToRun)
                {
                    case ManuscriptCorpusBackgroundTask task:
                        corpusTasksByName.Add(task.TaskName, RunBackgroundAddManuscriptCorpusAsync(task.TaskName, task.CorpusType));
                        endOfSequenceTasks.Add(task.TaskName, corpusTasksByName[task.TaskName]);
                        break;
                    case ParatextProjectCorpusBackgroundTask task:
                        corpusTasksByName.Add(task.TaskName, RunBackgroundAddParatextProjectCorpusAsync(task.TaskName, task.ProjectMetadata, task.Tokenizer, task.BookIds));
                        endOfSequenceTasks.Add(task.TaskName, corpusTasksByName[task.TaskName]);
                        break;
                    case ParallelCorpusBackgroundTask task:
                    {
                        var sourceTask = corpusTasksByName[task.SourceBackgroundTaskName];
                        var targetTask = corpusTasksByName[task.TargetBackgroundTaskName];
                        parallelCorpusTasksByName.Add(task.TaskName, AwaitRunAddParallelCorpusTask(sw, task.TaskName, sourceTask, targetTask));

                        //endOfSequenceTasks.Remove(task.SourceBackgroundTaskName);
                        //endOfSequenceTasks.Remove(task.TargetBackgroundTaskName);
                        endOfSequenceTasks.Add(task.TaskName, parallelCorpusTasksByName[task.TaskName]);
                    }
                        break;
                    case TrainSmtModelBackgroundTask task:
                    {
                        var parallelCorpusTask = parallelCorpusTasksByName[task.ParallelCorpusBackgroundTaskName];
                        trainSmtModelTasksByName.Add(task.TaskName, AwaitRunTrainSmtModelTask(
                            sw,
                            task.TaskName,
                            parallelCorpusTask,
                            task.IsTrainedSymmetrizedModel,
                            task.SmtModelName,
                            task.GenerateAlignedTokenPairs));

                        //endOfSequenceTasks.Remove(task.ParallelCorpusBackgroundTaskName);
                        endOfSequenceTasks.Add(task.TaskName, trainSmtModelTasksByName[task.TaskName]);
                    }
                        break;
                    case AlignmentSetBackgroundTask task:
                    {
                        var trainSmtModelTask = trainSmtModelTasksByName[task.TrainSmtModelBackgroundTaskName];
                        var parallelCorpusTask = parallelCorpusTasksByName[task.ParallelCorpusBackgroundTaskName];
                        alignmentSetTasksByName.Add(task.TaskName, AwaitRunAddAlignmentSetTask(
                            sw,
                            task.TaskName,
                            trainSmtModelTask,
                            parallelCorpusTask));

                        //endOfSequenceTasks.Remove(task.TrainSmtModelBackgroundTaskName);
                        //endOfSequenceTasks.Remove(task.ParallelCorpusBackgroundTaskName);
                        endOfSequenceTasks.Add(task.TaskName, alignmentSetTasksByName[task.TaskName]);
                    }
                        break;
                    case TranslationSetBackgroundTask task:
                    {
                        var trainSmtModelTask = trainSmtModelTasksByName[task.TrainSmtModelBackgroundTaskName];
                        var alignmentSetTask = alignmentSetTasksByName[task.AlignmentSetBackgroundTaskName];

                        //endOfSequenceTasks.Remove(task.TrainSmtModelBackgroundTaskName);
                        //endOfSequenceTasks.Remove(task.AlignmentSetBackgroundTaskName);
                        endOfSequenceTasks.Add(task.TaskName, AwaitRunAddTranslationSetTask(
                            sw,
                            task.TaskName,
                            trainSmtModelTask,
                            alignmentSetTask));
                    }
                        break;
                }
            }

            foreach (var value in endOfSequenceTasks.Values)
            {
                Debug.WriteLine($"EndOfSequenceTask: {value}");
            }

            return Task.WhenAll(endOfSequenceTasks.Values);
        }

        private async Task<ParallelCorpus> AwaitRunAddParallelCorpusTask(
            Stopwatch sw,
            string taskName,
            Task<TokenizedTextCorpus> sourceTokenizedTextCorpusTask, 
            Task<TokenizedTextCorpus> targetTokenizedTextCorpusTask)
        {
            if (!sourceTokenizedTextCorpusTask.IsCompleted || !targetTokenizedTextCorpusTask.IsCompleted)
            {
                await AwaitCancelAllIfErrorAsync(sw, sourceTokenizedTextCorpusTask, targetTokenizedTextCorpusTask);
            }

            var sourceTokenizedTextCorpusId = sourceTokenizedTextCorpusTask.Result.TokenizedTextCorpusId;
            var targetTokenizedTextCorpusId = targetTokenizedTextCorpusTask.Result.TokenizedTextCorpusId;
            var displayName = $"{sourceTokenizedTextCorpusId.DisplayName} to {targetTokenizedTextCorpusId.DisplayName}";

            Logger.LogInformation($"{nameof(AwaitRunAddParallelCorpusTask)} '{displayName}' before run  Elapsed={sw.Elapsed}");

            var parallelCorpus = await RunBackgroundLongRunningTaskAsync(
                (string taskName, CancellationToken cancellationToken) => AddParallelCorpusAsync(
                    taskName,
                    displayName,
                    sourceTokenizedTextCorpusId,
                    targetTokenizedTextCorpusId,
                    cancellationToken),
                taskName);

            Logger.LogInformation($"{nameof(AwaitRunAddParallelCorpusTask)} '{displayName}' after run  Elapsed={sw.Elapsed}");

            return parallelCorpus;
        }

        private async Task<TrainSmtModelSet> AwaitRunTrainSmtModelTask(
            Stopwatch sw,
            string taskName,
            Task<ParallelCorpus> parallelCorpusTask,
            bool isTrainedSymmetrizedModel,
            string? smtModelName,
            bool generateAlignedTokenPairs)
        {
            if (!parallelCorpusTask.IsCompleted)
            {
                await AwaitCancelAllIfErrorAsync(sw, parallelCorpusTask);
            }

            Logger.LogInformation($"{nameof(AwaitRunTrainSmtModelTask)} '{parallelCorpusTask.Result.ParallelCorpusId.DisplayName}' before run  Elapsed={sw.Elapsed}");

            var trainingResult = await RunBackgroundTrainSmtModelAsync(
                taskName,
                isTrainedSymmetrizedModel,
                smtModelName,
                generateAlignedTokenPairs,
                parallelCorpusTask.Result);

            Logger.LogInformation($"{nameof(AwaitRunTrainSmtModelTask)} '{parallelCorpusTask.Result.ParallelCorpusId.DisplayName}' after run  Elapsed={sw.Elapsed}");

            return trainingResult;
        }

        private async Task<AlignmentSet> AwaitRunAddAlignmentSetTask(
            Stopwatch sw,
            string taskName,
            Task<TrainSmtModelSet> trainSmtModelTask,
            Task<ParallelCorpus> parallelCorpusTask)
        {
            if (!trainSmtModelTask.IsCompleted || !parallelCorpusTask.IsCompleted)
            {
                await AwaitCancelAllIfErrorAsync(sw, trainSmtModelTask, parallelCorpusTask);
            }

            Logger.LogInformation($"{nameof(AwaitRunAddAlignmentSetTask)} '{parallelCorpusTask.Result.ParallelCorpusId.DisplayName}' before run  Elapsed={sw.Elapsed}");

            var alignmentSet = await RunBackgroundLongRunningTaskAsync(
                (string taskName, CancellationToken cancellationToken) => AddAlignmentSetAsync(
                    taskName,
                    $"AlignmentSet for {parallelCorpusTask.Result.ParallelCorpusId.DisplayName}",
                    trainSmtModelTask.Result.IsTrainedSymmetrizedModel,
                    trainSmtModelTask.Result.SmtModelType,
                    parallelCorpusTask.Result,
                    trainSmtModelTask.Result.WordAlignmentModel,
                    trainSmtModelTask.Result.AlignedTokenPairs,
                    cancellationToken),
                taskName);

            Logger.LogInformation($"{nameof(AwaitRunAddAlignmentSetTask)} '{parallelCorpusTask.Result.ParallelCorpusId.DisplayName}' after run  Elapsed={sw.Elapsed}");

            return alignmentSet;
        }

        private async Task<TranslationSet> AwaitRunAddTranslationSetTask(
            Stopwatch sw,
            string taskName,
            Task<TrainSmtModelSet> trainSmtModelTask,
            Task<AlignmentSet> alignmentSetTask)
        {
            if (!trainSmtModelTask.IsCompleted || !alignmentSetTask.IsCompleted)
            {
                await AwaitCancelAllIfErrorAsync(sw, trainSmtModelTask, alignmentSetTask);
            }

            Logger.LogInformation($"{nameof(AwaitRunAddTranslationSetTask)} '{alignmentSetTask.Result.ParallelCorpusId.DisplayName}' before run  Elapsed={sw.Elapsed}");

            var translationSet = await RunBackgroundLongRunningTaskAsync(
                (string taskName, CancellationToken cancellationToken) => AddTranslationSetAsync(
                    taskName,
                    $"TranslationSet for {alignmentSetTask.Result.ParallelCorpusId.DisplayName}",
                    alignmentSetTask.Result.AlignmentSetId,
                    alignmentSetTask.Result.ParallelCorpusId,
                    trainSmtModelTask.Result.WordAlignmentModel,
                    cancellationToken),
                taskName);

            Logger.LogInformation($"{nameof(AwaitRunAddTranslationSetTask)} '{alignmentSetTask.Result.ParallelCorpusId.DisplayName}' after run  Elapsed={sw.Elapsed}");

            return translationSet;
        }

        private async Task<T> AwaitCancelAllIfErrorAsync<T>(Stopwatch sw, Task<T> task)
        {
            Logger.LogInformation($"{nameof(AwaitCancelAllIfErrorAsync)} before WhenAll  Elapsed={sw.Elapsed}");

            try
            {
                return await task;
            }
            catch (Exception ex)
            {
                Logger.LogInformation($"Exception thrown by {nameof(AwaitCancelAllIfErrorAsync)} WhenAll: {ex.GetType().Name} {ex.Message}  Elapsed={sw.Elapsed}");
                LongRunningTaskManager.CancelAllTasks();
                throw;
            }
        }

        private async Task<T[]> AwaitCancelAllIfErrorAsync<T>(Stopwatch sw, params Task<T>[] tasks)
        {
            Logger.LogInformation($"{nameof(AwaitCancelAllIfErrorAsync)} before WhenAll  Elapsed={sw.Elapsed}");
            var awaitTask = Task.WhenAll(tasks);

            try
            {
                return await awaitTask;
            }
            catch (Exception ex)
            {
                Logger.LogInformation($"Exception thrown by {nameof(AwaitCancelAllIfErrorAsync)} WhenAll: {ex.GetType().Name} {ex.Message}  Elapsed={sw.Elapsed}");
                LongRunningTaskManager.CancelAllTasks();
                throw;
            }
        }

        private async Task AwaitCancelAllIfErrorAsync(Stopwatch sw, params Task[] tasks)
        {
            Logger.LogInformation($"{nameof(AwaitCancelAllIfErrorAsync)} before WhenAll  Elapsed={sw.Elapsed}");
            var awaitTask = Task.WhenAll(tasks);

            try
            {
                await awaitTask;
            }
            catch (Exception ex)
            {
                Logger.LogInformation($"Exception thrown by {nameof(AwaitCancelAllIfErrorAsync)} WhenAll: {ex.GetType().Name} {ex.Message}  Elapsed={sw.Elapsed}");
                LongRunningTaskManager.CancelAllTasks();
                throw;
            }
        }

        public async Task<T> RunBackgroundLongRunningTaskAsync<T>(Func<string, CancellationToken, Task<T>> taskToRunInBackground, string taskName)
        {
            var task = LongRunningTaskManager.Create(taskName, LongRunningTaskStatus.Running);
            var cancellationToken = task.CancellationTokenSource!.Token;

            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                _busyState.Add(taskName, true);
                task.Status = LongRunningTaskStatus.Running;

                T result = await Task.Run(
                    () => taskToRunInBackground(taskName, cancellationToken),
                    cancellationToken);

                return result;
            }
            catch (OperationCanceledException ex)
            {
                Logger!.LogInformation($"{taskName} - operation canceled.");
                await SendBackgroundStatus(taskName, LongRunningTaskStatus.Cancelled,
                    exception: ex, cancellationToken: cancellationToken,
                    backgroundTaskMode: BackgroundTaskMode.PerformanceMode);

                task.Status = LongRunningTaskStatus.Cancelled;

                throw;
            }
            catch (Exception ex)
            {
                Logger!.LogError(ex, $"{taskName} - An unexpected error occurred: {ex.Message}");
                await SendBackgroundStatus(taskName, LongRunningTaskStatus.Failed,
                    cancellationToken, exception: ex);

                task.Status = LongRunningTaskStatus.Failed;

                throw;
            }
            finally
            {
                task.Status = LongRunningTaskStatus.Completed;
                LongRunningTaskManager.TaskComplete(taskName);
                _busyState.Remove(taskName);
            }
        }

        public async Task<TokenizedTextCorpus> RunBackgroundAddManuscriptCorpusAsync(string taskName, CorpusType corpusType)
        {
            ParatextProjectMetadata metadata;
            LanguageCodeEnum languageCode;

            if (corpusType == CorpusType.ManuscriptHebrew)
            {
                metadata = new()
                {
                    Id = DataAccessLayer.ManuscriptIds.HebrewManuscriptId,
                    Name = MaculaCorporaNames.HebrewCorpusName,
                    CorpusType = corpusType,
                    IsRtl = true,
                    FontFamily = DataAccessLayer.FontNames.HebrewFontFamily,
                    LanguageId = ManuscriptIds.HebrewManuscriptLanguageId
                };
                languageCode = LanguageCodeEnum.H;
            }
            else if (corpusType == CorpusType.ManuscriptGreek)
            {
                metadata = new()
                {
                    Id = DataAccessLayer.ManuscriptIds.GreekManuscriptId,
                    Name = MaculaCorporaNames.GreekCorpusName,
                    CorpusType = corpusType,
                    IsRtl = false,
                    FontFamily = DataAccessLayer.FontNames.GreekFontFamily,
                    LanguageId = ManuscriptIds.GreekManuscriptLanguageId
                };
                languageCode = LanguageCodeEnum.G;
            }
            else
            {
                throw new ArgumentException(nameof(AddManuscriptCorpusAsync) + " only supports ManuscriptHebrew and ManuscriptGreek CorpusTypes", nameof(corpusType));
            }

            return await RunBackgroundLongRunningTaskAsync(
                (string taskName, CancellationToken cancellationToken) => AddManuscriptCorpusAsync(taskName, metadata, languageCode, cancellationToken),
                taskName);
        }

        public async Task<TokenizedTextCorpus> AddManuscriptCorpusAsync(string taskName, ParatextProjectMetadata metadata, LanguageCodeEnum languageCode, CancellationToken cancellationToken)
        {
            Logger!.LogInformation($"{nameof(AddManuscriptCorpusAsync)} '{metadata.CorpusType}' called.");

            await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running,
                description: $"Transforming syntax trees.", cancellationToken: cancellationToken,
                backgroundTaskMode: BackgroundTaskMode.PerformanceMode);

            var syntaxTree = new SyntaxTrees();
            var sourceCorpus = new SyntaxTreeFileTextCorpus(syntaxTree, languageCode)
                .Transform<SetTrainingByTrainingLowercase>()
                .Transform<AddPronominalReferencesToTokens>();

            cancellationToken.ThrowIfCancellationRequested();

            var books = BookInfo.GenerateScriptureBookList()
                .Where(bi => sourceCorpus.Texts
                    .Select(t => t.Id)
                    .Contains(bi.Code))
                .ToList();

            await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running,
                description: $"Creating '{metadata.Name}' corpus.", cancellationToken: cancellationToken,
                backgroundTaskMode: BackgroundTaskMode.PerformanceMode);

            var corpus = await Corpus.Create(
                mediator: Mediator!,
                IsRtl: metadata.IsRtl,
                Name: metadata.Name!,
                Language: metadata.LanguageId!,
                CorpusType: metadata.CorpusType.ToString(),
                ParatextId: metadata.Id!,
                token: cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            corpus.CorpusId.FontFamily = DataAccessLayer.FontNames.GreekFontFamily;

            await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running,
                description: $"Creating tokenized text corpus for '{metadata.Name}' corpus",
                cancellationToken: cancellationToken, backgroundTaskMode: BackgroundTaskMode.PerformanceMode);

            // ReSharper disable once UnusedVariable
            var tokenizedTextCorpus = await sourceCorpus.Create(Mediator!, corpus.CorpusId,
                metadata.Name!,
                Tokenizers.WhitespaceTokenizer.ToString(),
                ScrVers.Original,
                cancellationToken,
                true);

            await SendBackgroundStatus(taskName, LongRunningTaskStatus.Completed,
                description: $"Completed creation of tokenized text corpus for '{metadata.Name}' corpus.",
                cancellationToken: cancellationToken, backgroundTaskMode: BackgroundTaskMode.PerformanceMode);

            return tokenizedTextCorpus;
        }

        public async Task<TokenizedTextCorpus> RunBackgroundAddParatextProjectCorpusAsync(string taskName, ParatextProjectMetadata metadata, Tokenizers tokenizer, IEnumerable<string> bookIds)
        {
            if (!bookIds.Any())
            {
                throw new ArgumentException("BookIds is empty", nameof(bookIds));
            }

            return await RunBackgroundLongRunningTaskAsync(
                (string taskName, CancellationToken cancellationToken) => AddParatextProjectCorpusAsync(taskName, metadata, tokenizer, bookIds, cancellationToken),
                taskName);
        }

        public async Task<TokenizedTextCorpus> AddParatextProjectCorpusAsync(string taskName, ParatextProjectMetadata metadata, Tokenizers tokenizer, IEnumerable<string> bookIds, CancellationToken cancellationToken)
        {
            Logger!.LogInformation($"{nameof(AddParatextProjectCorpusAsync)} '{metadata.Name}' called.");

            var topLevelProjectIds = await TopLevelProjectIds.GetTopLevelProjectIds(Mediator);
            var corpusId = topLevelProjectIds.CorpusIds.FirstOrDefault(c => c.ParatextGuid == metadata.Id);
            var corpus = corpusId != null ? await Corpus.Get(Mediator, corpusId) : null;

            // first time for this corpus
            if (corpus is null)
            {
                await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running,
                    description: $"Creating corpus '{metadata.Name}'.", cancellationToken: cancellationToken,
                    backgroundTaskMode: BackgroundTaskMode.PerformanceMode);

                corpus = await Corpus.Create(
                    mediator: Mediator,
                    IsRtl: metadata.IsRtl,
                    FontFamily: metadata.FontFamily,
                    Name: metadata.Name!,
                    Language: metadata.LanguageId!,
                    CorpusType: metadata.CorpusTypeDisplay,
                    ParatextId: metadata.Id!,
                    token: cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                corpus.CorpusId.FontFamily = metadata.FontFamily;
            }

            await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running,
                description: $"Tokenizing and transforming '{metadata.Name}' corpus.", cancellationToken: cancellationToken,
                backgroundTaskMode: BackgroundTaskMode.PerformanceMode);

            var textCorpus = tokenizer switch
            {
                Tokenizers.LatinWordTokenizer =>
                    (await ParatextProjectTextCorpus.Get(Mediator!, metadata.Id!, bookIds, cancellationToken))
                    .Tokenize<LatinWordTokenizer>()
                    .Transform<IntoTokensTextRowProcessor>()
                    .Transform<SetTrainingBySurfaceLowercase>(),
                Tokenizers.WhitespaceTokenizer =>
                    (await ParatextProjectTextCorpus.Get(Mediator!, metadata.Id!, bookIds, cancellationToken))
                    .Tokenize<WhitespaceTokenizer>()
                    .Transform<IntoTokensTextRowProcessor>()
                    .Transform<SetTrainingBySurfaceLowercase>(),
                Tokenizers.ZwspWordTokenizer => (await ParatextProjectTextCorpus.Get(Mediator!, metadata.Id!, bookIds, cancellationToken))
                    .Tokenize<ZwspWordTokenizer>()
                    .Transform<IntoTokensTextRowProcessor>()
                    .Transform<SetTrainingBySurfaceLowercase>(),
                _ => (await ParatextProjectTextCorpus.Get(Mediator!, metadata.Id!, null, cancellationToken))
                    .Tokenize<WhitespaceTokenizer>()
                    .Transform<IntoTokensTextRowProcessor>()
                    .Transform<SetTrainingBySurfaceLowercase>()
            };

            cancellationToken.ThrowIfCancellationRequested();

            await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running,
                description: $"Creating tokenized text corpus for '{metadata.Name}' corpus.",
                cancellationToken: cancellationToken, backgroundTaskMode: BackgroundTaskMode.PerformanceMode);

            // ReSharper disable once UnusedVariable
            var tokenizedTextCorpus = await textCorpus.Create(Mediator, corpus.CorpusId,
                metadata.Name!, tokenizer.ToString(), metadata.ScrVers, cancellationToken);

            await SendBackgroundStatus(taskName, LongRunningTaskStatus.Completed,
                description: $"Completed creation of tokenized text corpus for '{metadata.Name}' corpus.",
                cancellationToken: cancellationToken, backgroundTaskMode: BackgroundTaskMode.PerformanceMode);

            return tokenizedTextCorpus;
        }

        public async Task<ParallelCorpus> AddParallelCorpusAsync(string taskName, string parallelCorpusDisplayName, TokenizedTextCorpusId sourceTokenizedTextCorpusId, TokenizedTextCorpusId targetTokenizedTextCorpusId, CancellationToken cancellationToken)
        {
            Logger!.LogInformation($"{nameof(AddParallelCorpusAsync)} '{sourceTokenizedTextCorpusId.DisplayName}' to '{targetTokenizedTextCorpusId.DisplayName}' called.");

            await SendBackgroundStatus(taskName,
                LongRunningTaskStatus.Running,
                cancellationToken,
                $"Retrieving tokenized source and target corpora for '{parallelCorpusDisplayName}'.");

            var sourceTokenizedTextCorpus = await TokenizedTextCorpus.Get(Mediator!, sourceTokenizedTextCorpusId);
            var targetTokenizedTextCorpus = await TokenizedTextCorpus.Get(Mediator!, targetTokenizedTextCorpusId);

            await SendBackgroundStatus(taskName,
                LongRunningTaskStatus.Running,
                cancellationToken,
                $"Parallelizing source and target corpora for '{parallelCorpusDisplayName}'.");

            var engineParallelTextCorpus =
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
                $"Saving parallelization '{parallelCorpusDisplayName}'.");

            var parallelCorpus = await engineParallelTextCorpus.Create(parallelCorpusDisplayName, Mediator!, cancellationToken);

            await SendBackgroundStatus(taskName,
                LongRunningTaskStatus.Completed,
                cancellationToken,
                $"Completed saving parallelization '{parallelCorpusDisplayName}'.");

            Logger!.LogInformation($"Completed saving parallelization '{parallelCorpusDisplayName}'.");

            return parallelCorpus;
        }

        public async Task<TrainSmtModelSet> RunBackgroundTrainSmtModelAsync(string taskName, bool isTrainedSymmetrizedModel, string? smtModelName, bool generateAlignedTokenPairs, ParallelCorpus parallelCorpus)
        {
            SmtModelType smtModelType = SmtModelType.FastAlign;

            if (smtModelName is not null && !Enum.TryParse(smtModelName, out smtModelType))
            {
                throw new ArgumentException(smtModelName, nameof(smtModelName));
            }

            return await RunBackgroundLongRunningTaskAsync(
                (string taskName, CancellationToken cancellationToken) => TrainSmtModelAsync(taskName, isTrainedSymmetrizedModel, smtModelType, generateAlignedTokenPairs, parallelCorpus, cancellationToken),
                taskName);
        }

        public async Task<TrainSmtModelSet> TrainSmtModelAsync(string taskName, bool isTrainedSymmetrizedModel, SmtModelType smtModelType, bool generateAlignedTokenPairs, ParallelCorpus parallelCorpus, CancellationToken cancellationToken)
        {
            Logger!.LogInformation($"{nameof(TrainSmtModelAsync)} '{smtModelType}' on '{parallelCorpus.ParallelCorpusId.DisplayName}' called.");

            await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running,
                cancellationToken, $"Training SMT Model type '{smtModelType}'.");

            var symmetrizationHeuristic = isTrainedSymmetrizedModel 
                ? SymmetrizationHeuristic.GrowDiagFinalAnd 
                : SymmetrizationHeuristic.None;

            TrainSmtModelSet? trainSmtModelSet = null;

            await semaphoreSlim.WaitAsync(cancellationToken);
            try
            {
                var wordAlignmentModel = await TranslationCommands.TrainSmtModel(
                    smtModelType,
                    parallelCorpus,
                    new ClearBible.Engine.Utils.DelegateProgress(async status =>
                    {
                        var message =
                            $"Training symmetrized {smtModelType} model: {status.PercentCompleted:P}";
                        await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running, cancellationToken,
                            message);
                        Logger!.LogInformation(message);

                    }), symmetrizationHeuristic);

                await SendBackgroundStatus(taskName, LongRunningTaskStatus.Completed,
                    cancellationToken, $"Completed SMT Model '{smtModelType}'.");

                IEnumerable<AlignedTokenPairs>? alignedTokenPairs = null;
                if (generateAlignedTokenPairs)
                {
                    alignedTokenPairs =
                        TranslationCommands.PredictAllAlignedTokenIdPairs(wordAlignmentModel, parallelCorpus)
                            .ToList();

                    await SendBackgroundStatus(taskName, LongRunningTaskStatus.Completed,
                        cancellationToken, $"Generated AlignedTokenPairs '{smtModelType}'.");
                }

                trainSmtModelSet = new TrainSmtModelSet(wordAlignmentModel, smtModelType, isTrainedSymmetrizedModel, alignedTokenPairs);
            }
            finally
            {
                semaphoreSlim.Release();
            }

            return trainSmtModelSet;
        }

        public async Task<AlignmentSet> AddAlignmentSetAsync(string taskName, string displayName, bool isTrainedSymmetrizedModel, SmtModelType smtModelType, ParallelCorpus parallelCorpus, IWordAlignmentModel wordAlignmentModel, IEnumerable<AlignedTokenPairs>? alignedTokenPairs, CancellationToken cancellationToken)
        {
            Logger!.LogInformation($"{nameof(AddAlignmentSetAsync)} '{displayName}' on '{parallelCorpus.ParallelCorpusId.DisplayName}' called.");

            await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running,
                cancellationToken, $"Aligning corpora and creating the AlignmentSet '{displayName}'.");

            //IEnumerable<AlignedTokenPairs>? alignedTokenPairs = null;
            if (alignedTokenPairs == null)
            {
                //await semaphoreSlim.WaitAsync(cancellationToken);
                //try
                //{
                // Accessing alignedTokenPairs later, during alignedTokenPairs.Create (i.e. without
                // doing a ToList() here) would periodically throw an exception when creating alignment
                // sets from multiple threads:
                //  Index was out of range. Must be non-negative and less than the size of the collection. (Parameter 'index')
                //  at System.Collections.Generic.List`1.get_Item(Int32 index)
                //  at ClearBible.Engine.Translation.Extensions.GetAlignedTokenPairs(EngineParallelTextRow engineParallelTextRow, WordAlignmentMatrix alignment) + MoveNext() in D:\dev\Clients\ClearBible\ClearEngine\src\ClearBible.Engine\Translation\Extensions.cs:line 15
                //  at System.Linq.Enumerable.SelectManySingleSelectorIterator`2.MoveNext()
                //  at System.Linq.Enumerable.SelectEnumerableIterator`2.GetCount(Boolean onlyIfCheap)
                //  at System.Linq.Enumerable.Count[TSource](IEnumerable`1 source)
                // So, doing a ToList() here to manifest the result and inside of a Monitor.Lock
                // to hopefully prevent the exception above.  
                alignedTokenPairs =
                    TranslationCommands.PredictAllAlignedTokenIdPairs(wordAlignmentModel, parallelCorpus)
                        .ToList();
                //}
                //finally
                //{
                //    semaphoreSlim.Release();
                //}
            }

            cancellationToken.ThrowIfCancellationRequested();

            AlignmentSet alignmentSet = await alignedTokenPairs.Create(displayName: displayName,
                smtModel: smtModelType.ToString(),
                isSyntaxTreeAlignerRefined: false,
                isSymmetrized: isTrainedSymmetrizedModel,
                metadata: new(),
                parallelCorpusId: parallelCorpus.ParallelCorpusId,
                Mediator,
                cancellationToken);

            await SendBackgroundStatus(taskName, LongRunningTaskStatus.Completed,
                cancellationToken, $"Completed creation of the AlignmentSet '{displayName}'.");

            cancellationToken.ThrowIfCancellationRequested();

            await Mediator.Send(
                new DenormalizeAlignmentTopTargetsCommand(
                    alignmentSet.AlignmentSetId.Id, 
                    new LongRunningProgressReporter(nameof(DenormalizeAlignmentTopTargetsCommand), this, cancellationToken)),
                cancellationToken);

            await SendBackgroundStatus(taskName, LongRunningTaskStatus.Completed,
                cancellationToken, $"Completed denormalization of the AlignmentSet '{displayName}'.");

            return alignmentSet;
        }

        public async Task<TranslationSet> AddTranslationSetAsync(string taskName, string displayName, AlignmentSetId alignmentSetId, ParallelCorpusId parallelCorpusId, IWordAlignmentModel wordAlignmentModel, CancellationToken cancellationToken)
        {
            Logger!.LogInformation($"{nameof(AddTranslationSetAsync)} '{displayName}' on '{parallelCorpusId.DisplayName}' called.");

            await SendBackgroundStatus(taskName,
                LongRunningTaskStatus.Running,
                cancellationToken,
                $"Creating the TranslationSet '{displayName}'.");

            var translationModel = wordAlignmentModel.GetTranslationTable();

            cancellationToken.ThrowIfCancellationRequested();

            // RUSSELL - code review
            var translationSet = await TranslationSet.Create(null, alignmentSetId,
                displayName, new Dictionary<string, object>(),
                parallelCorpusId, Mediator, cancellationToken);

            await SendBackgroundStatus(taskName,
                LongRunningTaskStatus.Completed,
                cancellationToken,
                $"Completed creation of the TranslationSet '{displayName}'.");

            Logger!.LogInformation($"Completed creating the TranslationSet '{displayName}'");

            return translationSet;
        }

        public async Task SendBackgroundStatus(string name, LongRunningTaskStatus status,
            CancellationToken cancellationToken, string? description = null, Exception? exception = null,
            BackgroundTaskMode backgroundTaskMode = BackgroundTaskMode.Normal)
        {
            if (exception is not null || description is not null)
            {
                Logger.LogInformation("Task '{name}' reports status '{status}' with message '{message}'", name, status, exception?.Message ?? description);
            }
            else
            {
                Logger.LogInformation("Task '{name}' reports status '{status}'", name, status);
            }

            var backgroundTaskStatus = new BackgroundTaskStatus
            {
                Name = name,
                EndTime = DateTime.Now,
                Description = !string.IsNullOrEmpty(description) ? description : null,
                ErrorMessage = exception != null ? $"{exception}" : null,
                TaskLongRunningProcessStatus = status,
                BackgroundTaskType = backgroundTaskMode,
            };
            await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(backgroundTaskStatus), cancellationToken);
        }

        #endregion // Methods
    }
}
