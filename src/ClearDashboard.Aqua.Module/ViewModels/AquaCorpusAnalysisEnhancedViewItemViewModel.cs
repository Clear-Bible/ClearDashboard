using Autofac;
using Caliburn.Micro;
using ClearBible.Engine.Exceptions;
using ClearDashboard.Aqua.Module.Models;
using ClearDashboard.Aqua.Module.Services;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.Wpf.Application;
using ClearDashboard.Wpf.Application.Messages;
using ClearDashboard.Wpf.Application.Models.EnhancedView;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using MediatR;
using Microsoft.Extensions.Logging;
using SIL.Scripture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using static ClearDashboard.Aqua.Module.Services.IAquaManager;


namespace ClearDashboard.Aqua.Module.ViewModels
{
    public class AquaCorpusAnalysisEnhancedViewItemViewModel : 
        VerseAwareEnhancedViewItemViewModel
    {
        private readonly IAquaManager aquaManager_;
        private readonly LongRunningTaskManager longRunningTaskManager_;
        private LongRunningTask? currentLongRunningTask_;
        private int? assessmentId_;
        private int? versionId_;

        private Visibility? _progressBarVisibility = Visibility.Hidden;
        public Visibility? ProgressBarVisibility
        {
            get => _progressBarVisibility;
            set => Set(ref _progressBarVisibility, value);
        }

        private Assessment? assessment_ = null;
        public Assessment? Assessment
        {
            get => assessment_;
            set => Set(ref assessment_, value);
        }


        private Revision? referenceRevision_ = null;
        public Revision? ReferenceRevision
        {
            get => referenceRevision_;
            set => Set(ref referenceRevision_, value);
        }

        private Revision? revision_ = null;
        public Revision? Revision
        {
            get => revision_;
            set => Set(ref revision_, value);
        }


        private string? message_ = "";
        public string? Message
        {
            get => message_;
            set => Set(ref message_, value);
        }

        private string? bodyText_ = "";
        public string? BodyText
        {
            get => bodyText_;
            set => Set(ref bodyText_, value);
        }

        private string verse_ = string.Empty;
        public string Verse
        {
            get => verse_;
            set => Set(ref verse_, value);
        }

        private int chapterNum_ = -1;
        private int verseNum_ = -1;
        private int offset_ = -1;

        private ISeries[] seriesCollection_;
        public ISeries[] SeriesCollection
        {
            get => seriesCollection_;
            set => Set(ref seriesCollection_, value);
        }

        private IEnumerable<Result> sortedResultsForChapter_ = new List<Result>();

        private IEnumerable<Result>? allResults_;

        public AquaCorpusAnalysisEnhancedViewItemViewModel(
            DashboardProjectManager? projectManager,
            INavigationService? navigationService,
            ILogger<AquaCorpusAnalysisEnhancedViewItemViewModel>? logger,
            IEventAggregator? eventAggregator,
            IMediator? mediator,
            ILifetimeScope? lifetimeScope,
            IWindowManager windowManager,
            ILocalizationService localizationService,
            IAquaManager aquaManager,
            LongRunningTaskManager longRunningTaskManager)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, windowManager, localizationService)
        {
            aquaManager_ = aquaManager;
            longRunningTaskManager_ = longRunningTaskManager;
        }

        public override Task GetData(EnhancedViewItemMetadatum metadatum, CancellationToken cancellationToken)
        {
            assessmentId_ = ((AquaCorpusAnalysisEnhancedViewItemMetadatum)metadatum)?.AssessmentId;
            versionId_ = ((AquaCorpusAnalysisEnhancedViewItemMetadatum)metadatum)?.VersionId;
            return Task.CompletedTask; //return base.GetData(metadatum, cancellationToken);
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
                await GetAssessment(assessmentId_
                    ?? throw new InvalidStateEngineException(name:"assessmentId_", value: "null"));
                await GetRevisions(Assessment
                    ?? throw new InvalidStateEngineException(name: "Assessment", value: "null"));
                await GetResults(Assessment?.id
                    ?? throw new InvalidStateEngineException(name: "Assessment", value: "null"));
            }
            catch (Exception ex)
            {
                OnUIThread(() =>
                {
                    Message = ex.Message ?? ex.ToString();
                });
            }
        }
        public override Task RefreshData(ReloadType reloadType = ReloadType.Refresh, CancellationToken cancellationToken = default)
        {
            var currentVerseRef = new VerseRef(ParentViewModel.CurrentBcv.GetBBBCCCVVV());
            var chapterNum = currentVerseRef.ChapterNum;
            offset_ = ParentViewModel.VerseOffsetRange;
            verseNum_ = currentVerseRef.VerseNum;
            Verse = currentVerseRef.ToString();
            if (chapterNum != chapterNum_)
            {
                sortedResultsForChapter_ = SortedResultsForCurrentChapter(
                    allResults_,
                    currentVerseRef) ?? new List<Result>();
                SeriesCollection = new ISeries[]
                {
                    new LineSeries<Result>
                    {
                        Values = sortedResultsForChapter_.ToList(),
                        Mapping = (result, point) =>
                        {
                            point.PrimaryValue = result.score ?? 0;
                            point.SecondaryValue = (new VerseRef(result.vref)).VerseNum;
                        }
                    }
                };
            }            
            return Task.CompletedTask;
        }
        private IEnumerable<Result>? SortedResultsForCurrentChapter(IEnumerable<Result>? results, VerseRef currentVerseRef)
        {
            return results?
                .Where(r =>
                {
                    var vref = new VerseRef(r.vref);
                    return vref.BookNum == currentVerseRef.BookNum &&
                        vref.ChapterNum == currentVerseRef.ChapterNum;
                })
                .OrderBy(r => new VerseRef(r.vref));
        }
        public async Task GetRevisions(Assessment assessment)
        {
            var processStatus = await RunLongRunningTask(
                "AQuA-List_Revisions",
                (cancellationToken) => aquaManager_!.ListRevisions(
                    versionId_,
                    cancellationToken),
                (revisions) => 
                {
                    Revision = revisions?
                        .Where(r => r.id == assessment.revision_id)
                        .FirstOrDefault();
                    ReferenceRevision = revisions?
                        .Where(r => r.id == assessment.reference_id)
                        .FirstOrDefault();
                });

            switch (processStatus)
            {
                case LongRunningTaskStatus.Completed:
                    //await MoveForwards();
                    break;
                case LongRunningTaskStatus.Failed:
                    break;
                case LongRunningTaskStatus.Cancelled:
                    break;
                case LongRunningTaskStatus.NotStarted:
                    break;
                case LongRunningTaskStatus.Running:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public async Task GetAssessment(int assessmentId)
        {
            var processStatus = await RunLongRunningTask(
                "AQuA-Get_Assessment",
                (cancellationToken) => aquaManager_!.GetAssessment(
                    assessmentId,
                    cancellationToken),
                (assessment) => Assessment = assessment);

            switch (processStatus)
            {
                case LongRunningTaskStatus.Completed:
                    //await MoveForwards();
                    break;
                case LongRunningTaskStatus.Failed:
                    break;
                case LongRunningTaskStatus.Cancelled:
                    break;
                case LongRunningTaskStatus.NotStarted:
                    break;
                case LongRunningTaskStatus.Running:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        public async Task GetResults(int assessmentId)
        {
            var processStatus = await RunLongRunningTask(
                "AQuA-Get_Results",
                (cancellationToken) => aquaManager_!.ListResults(
                    assessmentId,
                    cancellationToken),
                (results) =>
                {
                    allResults_ = results;
                    RefreshData();

                    BodyText = JsonSerializer.Serialize(results);
                });

            switch (processStatus)
            {
                case LongRunningTaskStatus.Completed:
                    //await MoveForwards();
                    break;
                case LongRunningTaskStatus.Failed:
                    break;
                case LongRunningTaskStatus.Cancelled:
                    break;
                case LongRunningTaskStatus.NotStarted:
                    break;
                case LongRunningTaskStatus.Running:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        //fixme: push to base class?
        public async Task SendBackgroundStatus(string name, LongRunningTaskStatus status, CancellationToken cancellationToken, string? description = null, Exception? exception = null)
        {
            Message = !string.IsNullOrEmpty(description) ? description : null;
            var backgroundTaskStatus = new BackgroundTaskStatus
            {
                Name = name,
                EndTime = DateTime.Now,
                Description = !string.IsNullOrEmpty(description) ? description : null,
                ErrorMessage = exception != null ? $"{exception}" : null,
                TaskLongRunningProcessStatus = status
            };
            await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(backgroundTaskStatus), cancellationToken);
        }
        public async Task<LongRunningTaskStatus> RunLongRunningTask<TResult>(
            string taskName,
            Func<CancellationToken, Task<TResult>> awaitableFunction,
            Action<TResult> ProcessResult)
        {
            Random rnd = new Random();
            int num = rnd.Next(1, 999);
            taskName = $"{num}: {taskName}";

            IsBusy = true;
            ProgressBarVisibility = Visibility.Visible;
            currentLongRunningTask_ = longRunningTaskManager_!.Create(taskName, LongRunningTaskStatus.Running);
            var cancellationToken = currentLongRunningTask_!.CancellationTokenSource?.Token
                ?? throw new Exception("Cancellation source is not set.");
            try
            {
                currentLongRunningTask_.Status = LongRunningTaskStatus.Running;
                await SendBackgroundStatus(
                    taskName,
                    currentLongRunningTask_.Status,
                    cancellationToken,
                    $"{taskName} running");
                Logger!.LogDebug($"{taskName} started");

                ProcessResult(await awaitableFunction(cancellationToken));

                if (cancellationToken.IsCancellationRequested)
                {
                    currentLongRunningTask_.Status = LongRunningTaskStatus.Cancelled;
                    await SendBackgroundStatus(taskName,
                        currentLongRunningTask_.Status,
                        cancellationToken,
                        $"{taskName} canceled.");
                    Logger!.LogDebug($"{taskName} cancelled.");
                }
                else
                {
                    currentLongRunningTask_.Status = LongRunningTaskStatus.Completed;
                    await SendBackgroundStatus(
                        taskName,
                        currentLongRunningTask_.Status,
                        cancellationToken,
                        $"{taskName} complete");
                    Logger!.LogDebug($"{taskName} complete.");
                }
            }
            catch (OperationCanceledException)
            {
                currentLongRunningTask_.Status = LongRunningTaskStatus.Cancelled;
                await SendBackgroundStatus(
                    taskName,
                    currentLongRunningTask_.Status,
                    cancellationToken,
                    $"{taskName} cancelled.");
                Logger!.LogDebug($"{taskName}: cancelled.");
            }
            catch (MediatorErrorEngineException ex)
            {
                if (ex.Message.Contains("The operation was canceled."))
                {
                    currentLongRunningTask_.Status = LongRunningTaskStatus.Cancelled;
                    await SendBackgroundStatus(
                        taskName,
                        currentLongRunningTask_.Status,
                        cancellationToken,
                        $"{taskName} cancelled.");
                    Logger!.LogDebug($"{taskName}: cancelled.");

                }
                else
                {
                    currentLongRunningTask_.Status = LongRunningTaskStatus.Failed;
                    await SendBackgroundStatus(
                       taskName,
                       currentLongRunningTask_.Status,
                       cancellationToken,
                       $"{taskName} failed: {ex.Message}.",
                       ex);
                    Logger!.LogError(ex, $"{taskName}: failed: {ex}.");
                }
            }
            catch (Exception ex)
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    currentLongRunningTask_.Status = LongRunningTaskStatus.Failed;
                    await SendBackgroundStatus(
                        taskName,
                        currentLongRunningTask_.Status,
                        cancellationToken,
                        $"{taskName} failed: {ex.Message}.",
                        ex);
                    Logger!.LogError(ex, $"{taskName}: failed: {ex}.");
                }
            }
            finally
            {
                longRunningTaskManager_.TaskComplete(taskName);

                IsBusy = false;
                ProgressBarVisibility = Visibility.Hidden;
            }
            return currentLongRunningTask_.Status;

        }
    }
}
