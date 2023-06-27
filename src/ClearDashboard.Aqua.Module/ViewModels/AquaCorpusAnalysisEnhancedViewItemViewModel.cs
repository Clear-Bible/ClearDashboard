using Autofac;
using Autofac.Features.AttributeFilters;
using Caliburn.Micro;
using ClearBible.Engine.Exceptions;
using ClearDashboard.Aqua.Module.Models;
using ClearDashboard.Aqua.Module.Services;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.Wpf.Application;
using ClearDashboard.Wpf.Application.Infrastructure.EnhancedView;
using ClearDashboard.Wpf.Application.Messages;
using ClearDashboard.Wpf.Application.Models.EnhancedView;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using MediatR;
using Microsoft.Extensions.Logging;
using MimeKit;
using SIL.Scripture;
using SkiaSharp;
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
        public record TypeAnalysisConfiguration(
            string name,
            IEnumerable<VisualizationEnum> visualizations,
            int defaultVisualizationsIndex = 0,
            decimal? lowLimit = null,
            decimal? highLimit = null);

        public enum VisualizationEnum
        {
            CartesianChart,
            BarChart,
            MissingWords,
            HeatMap
        }

        public readonly List<string> Types = new()
        {
            "missing-words",
            "semantic-similarity",
            "sentence-length",
            "word-alignment"
        };

        private Dictionary<string, TypeAnalysisConfiguration> typeToTypeAnalysisConfigurations_ = new()
        {
            { "missing-words",  new TypeAnalysisConfiguration(
                "missing-words",
                new List<VisualizationEnum>(){
                    VisualizationEnum.MissingWords })},
            { "semantic-similarity", new TypeAnalysisConfiguration(
                "semantic-similarity",
                new List<VisualizationEnum>(){
                    VisualizationEnum.HeatMap,
                    VisualizationEnum.BarChart,
                    VisualizationEnum.CartesianChart },
                0,
                0M,
                1M) },
            {
                "sentence-length",
                new TypeAnalysisConfiguration(
                "sentence-length",
                new List<VisualizationEnum>(){
                    VisualizationEnum.BarChart,
                    VisualizationEnum.CartesianChart },
                1,
                15M,
                65M) },
            {
                "word-alignment",
                new TypeAnalysisConfiguration(
                "word-alignment",
                new List<VisualizationEnum>(){
                    VisualizationEnum.BarChart,
                    VisualizationEnum.CartesianChart,
                    VisualizationEnum.HeatMap},
                0,
                0.0M,
                0.9M) },
        };

        private readonly IAquaManager aquaManager_;
        private readonly LongRunningTaskManager longRunningTaskManager_;
        private LongRunningTask? currentLongRunningTask_;
        private int? assessmentId_;
        private int? versionId_;

        public string RandomString { get; } =  Guid.NewGuid().ToString();

        private Visibility? _progressBarVisibility = Visibility.Hidden;
        public Visibility? ProgressBarVisibility
        {
            get => _progressBarVisibility;
            set => Set(ref _progressBarVisibility, value);
        }

        private TypeAnalysisConfiguration? typeAnalsysis_ = null;
        public TypeAnalysisConfiguration? TypeAnalysis
        {
            get => typeAnalsysis_;
            set => Set(ref typeAnalsysis_, value);
        }

        public VisualizationEnum visualization_;
        public VisualizationEnum Visualization
        {
            get => visualization_;
            set 
            {
                Set(ref visualization_, value);
                RefreshData();
            }
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

        private BindableCollection<ISeries> seriesCollection_ = new();
        public BindableCollection<ISeries> SeriesCollection
        {
            get => seriesCollection_;
            //set => Set(ref seriesCollection_, value);
            set => seriesCollection_ = value;    
        }
        private BindableCollection<Axis> yAxis_ = new();
        public BindableCollection<Axis> YAxis
        {
            get => yAxis_;
            set => Set(ref yAxis_, value);
            //set => yAxis_ = value;
        }

        private BindableCollection<Axis> xAxis_ = new();
        public BindableCollection<Axis> XAxis
        {
            get => xAxis_;
            set => Set(ref xAxis_, value);
            //set => xAxis_ = value;
        }

        private IEnumerable<Result> sortedResultsForChapter_ = new List<Result>();

        private IEnumerable<Result>? allResults_;

        public AquaCorpusAnalysisEnhancedViewItemViewModel(
            DashboardProjectManager? projectManager,
            IEnhancedViewManager enhancedViewManager,
            INavigationService? navigationService,
            ILogger<AquaCorpusAnalysisEnhancedViewItemViewModel>? logger,
            IEventAggregator? eventAggregator,
            IMediator? mediator,
            ILifetimeScope? lifetimeScope,
            IWindowManager windowManager,
            [KeyFilter("Aqua")] ILocalizationService localizationService,
            IAquaManager aquaManager,
            LongRunningTaskManager longRunningTaskManager)
            : base(projectManager, enhancedViewManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, windowManager, localizationService)
        {
            aquaManager_ = aquaManager;
            longRunningTaskManager_ = longRunningTaskManager;

            var s = LocalizationService.Get("Pds_AquaDialogMenuId");
        }

        //public override Task GetData(EnhancedViewItemMetadatum metadatum, CancellationToken cancellationToken)
        //{
        //    assessmentId_ = ((AquaCorpusAnalysisEnhancedViewItemMetadatum)metadatum)?.AssessmentId;
        //    versionId_ = ((AquaCorpusAnalysisEnhancedViewItemMetadatum)metadatum)?.VersionId;
        //    return Task.CompletedTask; //return base.GetData(metadatum, cancellationToken);
        //}

        public override Task GetData(CancellationToken cancellationToken) 
        {
            assessmentId_ = ((AquaCorpusAnalysisEnhancedViewItemMetadatum)EnhancedViewItemMetadatum)?.AssessmentId;
            versionId_ = ((AquaCorpusAnalysisEnhancedViewItemMetadatum)EnhancedViewItemMetadatum)?.VersionId;
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
                    ?? throw new InvalidStateEngineException(name: "assessmentId_", value: "null"));
                await GetRevisions(Assessment
                    ?? throw new InvalidStateEngineException(name: "Assessment", value: "null"));
                await GetResults();
            }
            catch (Exception ex)
            {
                Logger!.LogError(ex, ex.Message);
                OnUIThread(() =>
                {
                    Message = ex.Message ?? ex.ToString();
                });
            }
        }


        private void SetSeries(TypeAnalysisConfiguration typeAnalysis, VisualizationEnum visualization, IEnumerable<Result> results)
        {
            if (visualization == VisualizationEnum.CartesianChart)
            {
                var min = sortedResultsForChapter_.Min(r => r.score) -
                    (sortedResultsForChapter_.Max(r => r.score) - sortedResultsForChapter_.Min(r => r.score)) * 0.2;
                var max = sortedResultsForChapter_.Max(r => r.score) +
                    (sortedResultsForChapter_.Max(r => r.score) - sortedResultsForChapter_.Min(r => r.score)) * 0.2;
                SeriesCollection.Clear();
                SeriesCollection.AddRange(new ISeries[]
                {
                    new LineSeries<Result>
                    {
                        Values = results.ToList(),
                        Mapping = (result, point) =>
                        {
                            point.PrimaryValue = result.score ?? 0;
                            point.SecondaryValue = (new VerseRef(result.vref)).VerseNum;
                        }
                    }
                });
                XAxis.Clear();
                YAxis.Clear();
                var labels = new List<string>() { "" };
                labels.AddRange(results
                            .Select(r => r.vref ?? "")
                            .ToArray());
                XAxis.AddRange(new Axis[]
                {
                    new Axis
                    {
                        Labels = labels,
                        LabelsRotation = 90
                    }
                });
                YAxis.AddRange(new Axis[] { new Axis {
                    MinLimit = min,//(double) (typeAnalysis.lowLimit ?? 0M), 
                    MaxLimit = max //(double) (typeAnalysis.highLimit ?? 1M)
                }});
                XAxis.NotifyOfPropertyChange(nameof(XAxis));
                YAxis.NotifyOfPropertyChange(nameof(YAxis));
            }
            else if (visualization == VisualizationEnum.BarChart)
            {
                var min = sortedResultsForChapter_.Min(r => r.score) -
                    (sortedResultsForChapter_.Max(r => r.score) - sortedResultsForChapter_.Min(r => r.score)) * 0.2;
                var max = sortedResultsForChapter_.Max(r => r.score) +
                    (sortedResultsForChapter_.Max(r => r.score) - sortedResultsForChapter_.Min(r => r.score)) * 0.2;
                SeriesCollection.Clear();
                SeriesCollection.AddRange(new ISeries[]
                {
                    //new ColumnSeries<double>
                    //{
                    //    IsHoverable = false, // disables the series from the tooltips 
                    //    Values = new double[] { 1, 1, 1, 1, 1, 1, 1 },
                    //    Stroke = null,
                    //    Fill = new SolidColorPaint(new SKColor(30, 30, 30, 30)),
                    //    IgnoresBarPosition = true
                    //},
                    new ColumnSeries<Result>
                    {
                        Values = sortedResultsForChapter_.ToList(),
                        //Stroke = null,
                        Fill = new SolidColorPaint(SKColors.CornflowerBlue),
                        //IgnoresBarPosition = true,
                        Mapping = (result, point) =>
                        {
                            point.PrimaryValue = result.score ?? 0;
                            point.SecondaryValue = (new VerseRef(result.vref)).VerseNum;
                        }
                    }
                });
                XAxis.Clear();
                YAxis.Clear();
                var labels = new List<string>() { "" };
                labels.AddRange(results
                            .Select(r => r.vref ?? "")
                            .ToArray());
                XAxis.AddRange(new Axis[]
                {
                    new Axis
                    {
                        Labels = labels,
                        LabelsRotation = 90
                    }
                });
                YAxis.AddRange(new Axis[] { new Axis {
                    MinLimit = min,//(double) (typeAnalysis.lowLimit ?? 0M),
                    MaxLimit = max //(double) (typeAnalysis.highLimit ?? 1M)
                }});
                XAxis.NotifyOfPropertyChange(nameof(XAxis));
                YAxis.NotifyOfPropertyChange(nameof(YAxis));
            }
            else if (visualization == VisualizationEnum.MissingWords)
            {

            }
            else if (visualization == VisualizationEnum.HeatMap)
            {
                var values = new BindableCollection<WeightedPoint>();
                values.AddRange(
                    results
                        .Select(r =>
                            new WeightedPoint((new VerseRef(r.vref)).VerseNum, 0, r.score))
                );


                SeriesCollection.Clear();
                SeriesCollection.AddRange(new ISeries[]
                {
                    new HeatSeries<WeightedPoint>
                    {
                        HeatMap = new[]
                        {
                            SKColors.Red.AsLvcColor(), // the first element is the "coldest"
                            SKColors.DarkSlateGray.AsLvcColor(),
                            SKColors.Green.AsLvcColor() // the last element is the "hottest"
                        },
                        Values = values
                    }
                }); ;
                XAxis.Clear(); 
                YAxis.Clear();
                var labels = new List<string>() { "" };
                labels.AddRange(results
                            .Select(r => r.vref ?? "")
                            .ToArray());
                XAxis.AddRange(new Axis[]
                {
                    new Axis 
                    {
                        Labels = labels,
                        LabelsRotation = 90
                    }
                });
                YAxis.AddRange(new Axis[] { new Axis 
                {
                    Labels = new List<string>() {""}    
                }
                });
                XAxis.NotifyOfPropertyChange(nameof(XAxis));
                YAxis.NotifyOfPropertyChange(nameof(YAxis));
            }
        }
        public override Task RefreshData(ReloadType reloadType = ReloadType.Refresh, CancellationToken cancellationToken = default)
        {
            if (ParentViewModel == null)
                return Task.CompletedTask;

            var currentVerseRef = new VerseRef(ParentViewModel.CurrentBcv.GetBBBCCCVVV());
            var chapterNum = currentVerseRef.ChapterNum;
            offset_ = ParentViewModel.VerseOffsetRange;
            verseNum_ = currentVerseRef.VerseNum;
            Verse = currentVerseRef.ToString();
            //if (chapterNum != chapterNum_ && allResults_ != null && allResults_.Count() > 0)
            //{
                sortedResultsForChapter_ = SortedResultsForCurrentChapter(
                    allResults_,
                    currentVerseRef) ?? new List<Result>();
                chapterNum_ = chapterNum;
            //}
            //if (
            //    (sortedResultsForChapter_ != null && sortedResultsForChapter_.Count() > 0) ||
            //    reloadType== ReloadType.Force)
            //{
                SetSeries(TypeAnalysis!, Visualization, sortedResultsForChapter_);
            //}
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
                (assessment) => {
                    Assessment = assessment;
                    try
                    {
                        TypeAnalysisConfiguration? typeAnalysisConfiguration;
                        var succeeded = typeToTypeAnalysisConfigurations_.TryGetValue(Assessment?.type ?? "", out typeAnalysisConfiguration);
                        if (succeeded)
                        {
                            TypeAnalysis = typeAnalysisConfiguration
                                ?? throw new InvalidStateEngineException(
                                name: "typeAnalysisConfiguration",
                                value: "null");
                        }
                        else
                            throw new InvalidStateEngineException(
                                name: "Assessment.type",
                                value: $"{Assessment?.type}");
                        try
                        {
                            Visualization = TypeAnalysis!.visualizations.ToArray()[TypeAnalysis.defaultVisualizationsIndex];
                        }
                        catch (IndexOutOfRangeException)
                        {
                            throw new InvalidStateEngineException(
                                name: "TypeAnalysis.visualizations[TypeAnalysis.defaultVisualizationsIndex]",
                                value: $"index not found");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger!.LogError(ex, ex.Message);
                        OnUIThread(() =>
                        {
                            Message = ex.Message ?? ex.ToString();
                        });
                        throw;
                    }
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
        public async Task GetResults()
        {
            var processStatus = await RunLongRunningTask(
                "AQuA-Get_Results",
                (cancellationToken) => aquaManager_!.ListResults(
                    Assessment!?.id ?? 
                        throw new InvalidStateEngineException(name: "Assessment", value: "null"),
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
