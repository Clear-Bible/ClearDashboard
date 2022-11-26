using Autofac;
using Caliburn.Micro;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Exceptions;
using ClearBible.Engine.SyntaxTree.Corpora;
using ClearBible.Engine.Tokenization;
using ClearBible.Macula.PropertiesSources.Tokenization;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.DataAccessLayer.Wpf.Infrastructure;
using ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface;
using ClearDashboard.Wpf.Application.Exceptions;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Models;
using ClearDashboard.Wpf.Application.Models.ProjectSerialization;
using ClearDashboard.Wpf.Application.ViewModels.Main;
using ClearDashboard.Wpf.Application.ViewModels.Project.Interlinear;
using ClearDashboard.Wpf.Application.ViewModels.Project.ParallelCorpusDialog;
using ClearDashboard.Wpf.Application.Views.Project;
using ClearDashboard.Wpf.Controls;
using MediatR;
using Microsoft.Extensions.Logging;
using SIL.Machine.Corpora;
using SIL.Machine.Tokenization;
using SIL.ObjectModel;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Corpus = ClearDashboard.DAL.Alignment.Corpora.Corpus;
using TopLevelProjectIds = ClearDashboard.DAL.Alignment.TopLevelProjectIds;
using TranslationSet = ClearDashboard.DAL.Alignment.Translation.TranslationSet;


// ReSharper disable once CheckNamespace
namespace ClearDashboard.Wpf.Application.ViewModels.Project
{

    public class ProjectDesignSurfaceViewModel : DashboardApplicationScreen, IHandle<UiLanguageChangedMessage>
    {
        #region Member Variables

        //public record CorporaLoadedMessage(IEnumerable<DAL.Alignment.Corpora.Corpus> Copora);
        public record TokenizedTextCorpusLoadedMessage(TokenizedTextCorpus TokenizedTextCorpus, string TokenizationName, ParatextProjectMetadata? ProjectMetadata);

        private readonly IWindowManager? _windowManager;
        private readonly LongRunningTaskManager? _longRunningTaskManager;
        #endregion //Member Variables


        #region Observable Properties

        public bool LoadingDesignSurface { get; set; }

        public bool DesignSurfaceLoaded { get; set; }
        /// <summary>
        /// This is the design surface that is displayed in the window.
        /// It is the main part of the view-model.
        /// </summary>
        private DesignSurfaceViewModel? _designSurface;
        public DesignSurfaceViewModel? DesignSurface
        {
            get => _designSurface;
            private set => Set(ref _designSurface, value);
        }

        private object? _selectedDesignSurfaceComponent;
        public object? SelectedDesignSurfaceComponent
        {
            get
            {
                if (_selectedDesignSurfaceComponent is CorpusNodeViewModel node)
                {
                    foreach (var corpusNode in DesignSurface!.CorpusNodes)
                    {
                        if (corpusNode.ParatextProjectId == node.ParatextProjectId)
                        {
                            return corpusNode;
                        }
                    }
                }
                else if (_selectedDesignSurfaceComponent is ParallelCorpusConnectionViewModel conn)
                {
                    foreach (var connection in DesignSurface!.ParallelCorpusConnections)
                    {
                        if (connection.Id == conn.Id)
                        {
                            return connection;
                        }
                    }
                }
                else
                {
                    return _selectedDesignSurfaceComponent;
                }

#pragma warning disable CS8603
                return null;
#pragma warning restore CS8603
            }
            set => Set(ref _selectedDesignSurfaceComponent, value);
        }

        private bool _addManuscriptHebrewEnabled = true;
        public bool AddManuscriptHebrewEnabled
        {
            get => _addManuscriptHebrewEnabled;
            set => Set(ref _addManuscriptHebrewEnabled, value);
        }

        private bool _addManuscriptGreekEnabled = true;
        public bool AddManuscriptGreekEnabled
        {
            get => _addManuscriptGreekEnabled;
            set => Set(ref _addManuscriptGreekEnabled, value);
        }

        private string? _projectName;
        public string? ProjectName
        {
            get => _projectName;
            set => Set(ref _projectName, value);
        }

        #endregion //Observable Properties

        #region Constructor

        // Required for design-time binding
        public ProjectDesignSurfaceViewModel()
        {
            //no-op
        }

        public ProjectDesignSurfaceViewModel(INavigationService navigationService, IWindowManager windowManager,
            ILogger<ProjectDesignSurfaceViewModel> logger, DashboardProjectManager? projectManager,
            IEventAggregator? eventAggregator, IMediator mediator, ILifetimeScope lifetimeScope, LongRunningTaskManager longRunningTaskManager)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope)
        {
            _windowManager = windowManager;
            _longRunningTaskManager = longRunningTaskManager;


            EventAggregator.SubscribeOnUIThread(this);
            _busyState.CollectionChanged += BusyStateOnCollectionChanged;
        }

        protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            // NB:  This is currently not getting called, so we're doing the wire up in the 
            //       constructor and cleaning up in Dispose
            EventAggregator.SubscribeOnUIThread(this);
            _busyState.CollectionChanged += BusyStateOnCollectionChanged;
            return base.OnInitializeAsync(cancellationToken);
        }

        protected override void Dispose(bool disposing)
        {
            EventAggregator!.Unsubscribe(this);
            _busyState.CollectionChanged -= BusyStateOnCollectionChanged;
            base.Dispose(disposing);
        }

        //protected override Task OnActivateAsync(CancellationToken cancellationToken)
        //{
        //    EventAggregator.SubscribeOnUIThread(this);
        //    _busyState.CollectionChanged += BusyStateOnCollectionChanged;
        //    return base.OnActivateAsync(cancellationToken);
        //}

        private void BusyStateOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            NotifyOfPropertyChange(() => IsBusy);
        }

        protected override async Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            _busyState.CollectionChanged -= BusyStateOnCollectionChanged;
            EventAggregator!.Unsubscribe(this);
            await SaveDesignSurfaceData();
            await base.OnDeactivateAsync(close, cancellationToken);
        }

        protected override void OnViewAttached(object view, object context)
        {
            //
            // Create a design surface, the root of the view-model.
            //
            DesignSurface = LifetimeScope!.Resolve<DesignSurfaceViewModel>();

            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (DesignSurface.ProjectDesignSurface == null)
            {
                if (view is ProjectDesignSurfaceView projectDesignSurfaceView)
                {
                    // ReSharper disable once AssignNullToNotNullAttribute
                    DesignSurface.ProjectDesignSurface = (ProjectDesignSurface)projectDesignSurfaceView.FindName("ProjectDesignSurface");

                }
            }
            base.OnViewAttached(view, context);
        }


        #endregion //Constructor


        #region Methods

        public async Task SaveDesignSurfaceData()
        {
            _ = await Task.Factory.StartNew(async () =>
            {
                var json = SerializeDesignSurface();

                ProjectManager!.CurrentProject.DesignSurfaceLayout = json;

                Logger!.LogInformation($"DesignSurfaceLayout : {ProjectManager.CurrentProject.DesignSurfaceLayout}");

                try
                {
                    await ProjectManager.UpdateProject(ProjectManager.CurrentProject).ConfigureAwait(false);
                    await Task.Delay(250);
                }
                catch (Exception ex)
                {
                    Logger?.LogError(ex,
                        $"An unexpected error occurred while saving the project layout to the '{ProjectManager.CurrentProject.ProjectName} database.");
                }
            });

        }

        public string SerializeDesignSurface()
        {
            var surface = new ProjectDesignSurfaceSerializationModel();

            // save all the nodes
            foreach (var corpusNode in DesignSurface!.CorpusNodes)
            {
                surface.TokenizedCorpora.Add(new SerializedTokenizedCorpus
                {
                    ParatextProjectId = corpusNode.ParatextProjectId,
                    CorpusType = corpusNode.CorpusType,
                    Name = corpusNode.Name,
                    X = corpusNode.X,
                    Y = corpusNode.Y,
                    //Tokenizations = corpusNode.Tokenizations,
                    CorpusId = corpusNode.CorpusId,
                    IsRTL = corpusNode.IsRtl,
                    TranslationFontFamily = corpusNode.TranslationFontFamily,
                });
            }

            // save all the connections
            foreach (var connection in DesignSurface.ParallelCorpusConnections)
            {
                var serializedTranslationSet = connection.TranslationSetInfo.Select(translationSet => new TranslationSetInfo
                {
                    DisplayName = translationSet.DisplayName ?? string.Empty,
                    TranslationSetId = translationSet.TranslationSetId,
                    ParallelCorpusDisplayName = translationSet.ParallelCorpusDisplayName ?? string.Empty,
                    ParallelCorpusId = translationSet.ParallelCorpusId,
                    AlignmentSetDisplayName = translationSet.AlignmentSetDisplayName ?? string.Empty,
                    AlignmentSetId = translationSet.AlignmentSetId,
                    IsRTL = translationSet.IsRTL,
                    SourceFontFamily = translationSet.SourceFontFamily,
                    TargetFontFamily = translationSet.TargetFontFamily,
                })
                    .ToList();

                var serializedAlignmentSet = connection.AlignmentSetInfo.Select(alignmentSetInfo => new AlignmentSetInfo
                {
                    DisplayName = alignmentSetInfo.DisplayName ?? string.Empty,
                    AlignmentSetId = alignmentSetInfo.AlignmentSetId,
                    ParallelCorpusDisplayName = alignmentSetInfo.ParallelCorpusDisplayName ?? string.Empty,
                    ParallelCorpusId = alignmentSetInfo.ParallelCorpusId,
                    IsRtl = alignmentSetInfo.IsRtl,
                    IsTargetRtl = alignmentSetInfo.IsTargetRtl,
                    SourceFontFamily = alignmentSetInfo.SourceFontFamily,
                    TargetFontFamily = alignmentSetInfo.TargetFontFamily,
                })
                    .ToList();

                surface.ParallelCorpora.Add(new SerializedParallelCorpus
                {
                    SourceConnectorId = connection.SourceConnector.ParatextId,
                    TargetConnectorId = connection.DestinationConnector.ParatextId,
                    TranslationSetInfo = serializedTranslationSet,
                    AlignmentSetInfo = serializedAlignmentSet,
                    ParallelCorpusDisplayName = connection.ParallelCorpusDisplayName,
                    ParallelCorpusId = connection.ParallelCorpusId!.Id.ToString(),
                    SourceFontFamily = connection.SourceFontFamily,
                    TargetFontFamily = connection.TargetFontFamily,
                });
            }

            JsonSerializerOptions options = new()
            {
                IncludeFields = true,
                WriteIndented = false,
                NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
            };

            return JsonSerializer.Serialize(surface, options);

        }

        public async Task LoadDesignSurface()
        {
            AddManuscriptGreekEnabled = true;
            AddManuscriptHebrewEnabled = true;
            LoadingDesignSurface = true;
            DesignSurfaceLoaded = false;

            Stopwatch sw = new();
            sw.Start();

            try
            {
                var designSurfaceData = LoadDesignSurfaceData();
                var topLevelProjectIds = await TopLevelProjectIds.GetTopLevelProjectIds(Mediator!);

                // restore the nodes
                if (designSurfaceData != null)
                {
                    foreach (var corpusId in topLevelProjectIds.CorpusIds)
                    {
                        if (corpusId.CorpusType == CorpusType.ManuscriptHebrew.ToString())
                        {
                            AddManuscriptHebrewEnabled = false;
                        }
                        else if (corpusId.CorpusType == CorpusType.ManuscriptGreek.ToString())
                        {
                            AddManuscriptGreekEnabled = false;
                        }

                        var corpus = new Corpus(corpusId);
                        var corpusNode = designSurfaceData.TokenizedCorpora.FirstOrDefault(cn => cn.CorpusId == corpusId.Id);
                        var point = corpusNode != null ? new Point(corpusNode.X, corpusNode.Y) : new Point();
                        var node = DesignSurface!.CreateCorpusNode(corpus, point);


                        var tokenizedCorpora = topLevelProjectIds.TokenizedTextCorpusIds.Where(ttc => ttc.CorpusId!.Id == corpusId.Id);
                        DesignSurface!.CreateCorpusNodeMenu(node, tokenizedCorpora, this);
                    }

                    foreach (var parallelCorpusId in topLevelProjectIds.ParallelCorpusIds)
                    {

                        var sourceNode = DesignSurface!.CorpusNodes.FirstOrDefault(p =>
                            p.ParatextProjectId == parallelCorpusId.SourceTokenizedCorpusId?.CorpusId?.ParatextGuid);
                        var targetNode = DesignSurface!.CorpusNodes.FirstOrDefault(p =>
                            p.ParatextProjectId == parallelCorpusId.TargetTokenizedCorpusId?.CorpusId?.ParatextGuid);


                        if (sourceNode is not null && targetNode is not null)
                        {
                            var connection = new ParallelCorpusConnectionViewModel
                            {
                                SourceConnector = sourceNode.OutputConnectors[0],
                                DestinationConnector = targetNode.InputConnectors[0],
                                ParallelCorpusDisplayName = parallelCorpusId.DisplayName,
                                ParallelCorpusId = parallelCorpusId,
                                SourceFontFamily = parallelCorpusId.SourceTokenizedCorpusId?.CorpusId?.FontFamily,
                                TargetFontFamily = parallelCorpusId.TargetTokenizedCorpusId?.CorpusId?.FontFamily,
                            };
                            DesignSurface.ParallelCorpusConnections.Add(connection);
                            // add in the context menu
                            DesignSurface!.CreateConnectionMenu(connection, topLevelProjectIds, this);
                        }
                    }
                }

              
            }
            finally
            {
                LoadingDesignSurface = false;
                DesignSurfaceLoaded = true;
                sw.Stop();

                Debug.WriteLine($"LoadCanvas took {sw.ElapsedMilliseconds} ms ({sw.Elapsed.Seconds} seconds)");
            }
          
        }

        private ProjectDesignSurfaceSerializationModel? LoadDesignSurfaceData()
        {
            if (ProjectManager!.CurrentProject is null)
            {
                return null;
            }

            ProjectName = ProjectManager.CurrentProject.ProjectName!;

            if (ProjectManager?.CurrentProject.DesignSurfaceLayout == "")
            {
                return null;
            }

            var json = ProjectManager?.CurrentProject.DesignSurfaceLayout;

            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            JsonSerializerOptions options = new()
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                IncludeFields = true,
                WriteIndented = true,
                NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
            };

            return JsonSerializer.Deserialize<ProjectDesignSurfaceSerializationModel>(json, options);
        }


        private readonly ObservableDictionary<string, bool> _busyState = new();

        private bool _isBusy;
        public override bool IsBusy
        {
            get => _busyState.Count > 0;
            set => Set(ref _isBusy, _busyState.Count > 0);
        }


        // ReSharper disable once UnusedMember.Global
        public async void AddManuscriptHebrewCorpus()
        {
            Logger!.LogInformation("AddManuscriptHebrewCorpus called.");

            var taskName = "HebrewCorpus";

            AddManuscriptHebrewEnabled = false;

            var task = _longRunningTaskManager!.Create(taskName, LongRunningTaskStatus.Running);
            var cancellationToken = task.CancellationTokenSource!.Token;

            await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running,
                description: $"Transforming syntax trees...", cancellationToken: cancellationToken);

            var syntaxTree = new SyntaxTrees();
            var sourceCorpus = new SyntaxTreeFileTextCorpus(syntaxTree, ClearBible.Engine.Persistence.FileGetBookIds.LanguageCodeEnum.H)
                .Transform<SetTrainingByTrainingLowercase>()
                .Transform<AddPronominalReferencesToTokens>();

            var bookInfo = new BookInfo();
            var books = bookInfo.GenerateScriptureBookList()
                .Where(bi => sourceCorpus.Texts
                    .Select(t => t.Id)
                    .Contains(bi.Code))
                .ToList();

            var metadata = new ParatextProjectMetadata
            {
                Id = ManuscriptIds.HebrewManuscriptId,
                CorpusType = CorpusType.ManuscriptHebrew,
                Name = "Macula Hebrew",
                AvailableBooks = books,
            };

            _ = await Task.Factory.StartNew(async () =>
            {
                _busyState.Add(taskName, true);
                CorpusNodeViewModel corpusNode = new();

                try
                {

                    await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running,
                        description: $"Creating '{metadata.Name}' corpus...", cancellationToken: cancellationToken);

                    var corpus = await Corpus.Create(
                        mediator: Mediator,
                        IsRtl: true,
                        Name: "Macula Hebrew",
                        Language: "Hebrew",
                        CorpusType: CorpusType.ManuscriptHebrew.ToString(),
                        ParatextId: ManuscriptIds.HebrewManuscriptId,
                        token: cancellationToken);

                    corpus.CorpusId.FontFamily = ManuscriptIds.HebrewFontFamily;

                    OnUIThread(() =>
                    {
                        corpusNode = DesignSurface!.CreateCorpusNode(corpus, new Point());
                    });


                    await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running,
                    description: $"Creating tokenized text corpus for '{metadata.Name}' corpus...",
                    cancellationToken: cancellationToken);

                    var tokenizedTextCorpus = await sourceCorpus.Create(Mediator, corpus.CorpusId,
                        "Macula Hebrew",
                        Tokenizers.WhitespaceTokenizer.ToString(),
                        cancellationToken);

                    await SendBackgroundStatus(taskName, LongRunningTaskStatus.Completed,
                        description: $"Creating tokenized text corpus for '{metadata.Name}' corpus...Completed",
                        cancellationToken: cancellationToken);


                    Logger!.LogInformation("Sending TokenizedTextCorpusLoadedMessage via EventAggregator.");

                    OnUIThread(async () =>
                    {
                        await DesignSurface!.UpdateNodeTokenization(corpusNode, this);
                    });

                }
                catch (OperationCanceledException ex)
                {
                    Logger!.LogInformation($"AddManuscriptHebrewCorpus - operation canceled.");
                }
                catch (MediatorErrorEngineException ex)
                {
                    if (ex.Message.Contains("The operation was canceled."))
                    {
                        Logger!.LogInformation($"AddManuscriptHebrewCorpus - operation canceled.");
                    }
                    else
                    {
                        Logger!.LogError(ex, "An unexpected Engine exception was thrown.");
                    }

                }
                catch (Exception ex)
                {
                    Logger!.LogError(ex, $"An unexpected error occurred while creating the the corpus for {metadata.Name} ");
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        await SendBackgroundStatus(taskName, LongRunningTaskStatus.Failed,
                           exception: ex, cancellationToken: cancellationToken);

                    }
                }
                finally
                {
                    _longRunningTaskManager.TaskComplete(taskName);
                    _busyState.Remove(taskName);
                    if (cancellationToken.IsCancellationRequested)
                    {
                        DesignSurface!.DeleteCorpusNode(corpusNode);
                        // What other work needs to be done?  how do we know which steps have been executed?
                        AddManuscriptHebrewEnabled = true;
                    }
                    else
                    {
                        PlaySound.PlaySoundFromResource(null, null);
                    }

                }
            }, cancellationToken);


        }


        public async void AddManuscriptGreekCorpus()
        {
            Logger!.LogInformation("AddGreekCorpus called.");
            
            AddManuscriptGreekEnabled = false;

            var taskName = "GreekCorpus";
            var task = _longRunningTaskManager!.Create(taskName, LongRunningTaskStatus.Running);
            var cancellationToken = task.CancellationTokenSource!.Token;


            await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running,
                description: $"Transforming syntax trees...", cancellationToken: cancellationToken);


            var syntaxTree = new SyntaxTrees();
            var sourceCorpus = new SyntaxTreeFileTextCorpus(syntaxTree, ClearBible.Engine.Persistence.FileGetBookIds.LanguageCodeEnum.G)
                .Transform<SetTrainingByTrainingLowercase>()
                .Transform<AddPronominalReferencesToTokens>();

            var bookInfo = new BookInfo();
            var books = bookInfo.GenerateScriptureBookList()
                .Where(bi => sourceCorpus.Texts
                    .Select(t => t.Id)
                    .Contains(bi.Code))
                .ToList();

            var metadata = new ParatextProjectMetadata
            {
                Id = ManuscriptIds.GreekManuscriptId,
                CorpusType = CorpusType.ManuscriptGreek,
                Name = "Macula Greek",
                AvailableBooks = books,
            };

            _ = await Task.Factory.StartNew(async () =>
            {
                _busyState.Add(taskName, true);

                CorpusNodeViewModel corpusNode = new();

                try
                {
                    await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running,
                        description: $"Creating '{metadata.Name}' corpus...", cancellationToken: cancellationToken);

                    var corpus = await DAL.Alignment.Corpora.Corpus.Create(
                        mediator: Mediator!,
                        IsRtl: false,
                        Name: "Macula Greek",
                        Language: "Greek",
                        CorpusType: CorpusType.ManuscriptGreek.ToString(),
                        ParatextId: ManuscriptIds.GreekManuscriptId,
                        token: cancellationToken);

                    corpus.CorpusId.FontFamily = ManuscriptIds.GreekFontFamily;

                    OnUIThread(() =>
                    {
                        corpusNode = DesignSurface!.CreateCorpusNode(corpus, new Point());
                    });



                    await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running,
                        description: $"Creating tokenized text corpus for '{metadata.Name}' corpus...", cancellationToken: cancellationToken);

                    var tokenizedTextCorpus = await sourceCorpus.Create(Mediator, corpus.CorpusId,
                        "Macula Greek",
                        Tokenizers.WhitespaceTokenizer.ToString(),
                        cancellationToken);
                    
                    await SendBackgroundStatus(taskName, LongRunningTaskStatus.Completed,
                        description: $"Creating tokenized text corpus for '{metadata.Name}' corpus...Completed", cancellationToken: cancellationToken);

                    Logger!.LogInformation("Sending TokenizedTextCorpusLoadedMessage via EventAggregator.");

                    OnUIThread(async () =>
                    {
                        await DesignSurface!.UpdateNodeTokenization(corpusNode, this);
                    });

                }
                catch (OperationCanceledException ex)
                {
                    Logger!.LogInformation($"AddManuscriptGreekCorpus - operation canceled.");
                }
                catch (MediatorErrorEngineException ex)
                {
                    if (ex.Message.Contains("The operation was canceled."))
                    {
                        Logger!.LogInformation($"AddManuscriptGreekCorpus - operation canceled.");
                    }
                    else
                    {
                        Logger!.LogError(ex, "An unexpected Engine exception was thrown.");
                    }

                }
                catch (Exception ex)
                {
                    Logger!.LogError(ex, $"An unexpected error occurred while creating the the corpus for {metadata.Name} ");
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        await SendBackgroundStatus(taskName, LongRunningTaskStatus.Failed,
                            exception: ex, cancellationToken: cancellationToken);
                    }
                }
                finally
                {
                    _longRunningTaskManager.TaskComplete(taskName);
                    _busyState.Remove(taskName);
                    if (cancellationToken.IsCancellationRequested)
                    {
                        DesignSurface!.DeleteCorpusNode(corpusNode);
                        AddManuscriptGreekEnabled = true;
                    }
                    else
                    {
                        PlaySound.PlaySoundFromResource(null, null);
                    }
                }
            }, cancellationToken);


        }

        public async void AddParatextCorpus()
        {
            await AddParatextCorpus("");
        }


        // ReSharper restore UnusedMember.Global
        // ReSharper disable once UnusedMember.Global
        public async Task AddParatextCorpus(string selectedParatextProjectId)
        {
            Logger!.LogInformation("AddParatextCorpus called.");

            var dialogViewModel = LifetimeScope!.Resolve<AddParatextCorpusDialogViewModel>();
            var result = await _windowManager!.ShowDialogAsync(dialogViewModel, null, DashboardProjectManager.NewProjectDialogSettings);

            if (result)
            {
                var metadata = dialogViewModel.SelectedProject;
                var taskName = $"{metadata.Name}";
                _busyState.Add(taskName, true);

                var task = _longRunningTaskManager!.Create(taskName, LongRunningTaskStatus.Running);
                var cancellationToken = task.CancellationTokenSource!.Token;
                _ = await Task.Factory.StartNew(async () =>
                {
                    CorpusNodeViewModel node = new()
                    {
                        TranslationFontFamily = metadata.FontFamily
                    };

                    if (DesignSurface!.CorpusNodes.Any(cn => cn.ParatextProjectId == metadata.Id))
                    {
                        node = DesignSurface!.CorpusNodes.Single(cn => cn.ParatextProjectId == metadata.Id);
                    }

                    try
                    {
                        var topLevelProjectIds = await TopLevelProjectIds.GetTopLevelProjectIds(Mediator!);
                        var corpusId = topLevelProjectIds.CorpusIds.FirstOrDefault(c => c.ParatextGuid == metadata.Id);
                        var corpus = corpusId != null ? await Corpus.Get(Mediator!, corpusId) : null;

                        // first time for this corpus
                        if (corpus is null)
                        {
                            await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running,
                               description: $"Creating corpus '{metadata.Name}'...", cancellationToken: cancellationToken);
#pragma warning disable CS8604
                            corpus = await Corpus.Create(
                                  mediator: Mediator,
                                  IsRtl: metadata.IsRtl,

                                  Name: metadata.Name,

                                  Language: metadata.LanguageName,
                                  CorpusType: metadata.CorpusTypeDisplay,
                                  ParatextId: metadata.Id,
                                  token: cancellationToken);

                            corpus.CorpusId.FontFamily = metadata.FontFamily;
#pragma warning restore CS8604
                        }
                        OnUIThread(() =>
                        {
                            node = DesignSurface!.CreateCorpusNode(corpus, new Point());
                        });

                        await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running,
                           description: $"Tokenizing and transforming '{metadata.Name}' corpus...", cancellationToken: cancellationToken);

                        var textCorpus = dialogViewModel.SelectedTokenizer switch
                        {
                            Tokenizers.LatinWordTokenizer =>
                               (await ParatextProjectTextCorpus.Get(Mediator!, metadata.Id!, cancellationToken))
                               .Tokenize<LatinWordTokenizer>()
                               .Transform<IntoTokensTextRowProcessor>()
                               .Transform<SetTrainingBySurfaceLowercase>(),
                            Tokenizers.WhitespaceTokenizer =>
                               (await ParatextProjectTextCorpus.Get(Mediator!, metadata.Id!, cancellationToken))
                               .Tokenize<WhitespaceTokenizer>()
                               .Transform<IntoTokensTextRowProcessor>()
                               .Transform<SetTrainingBySurfaceLowercase>(),
                            Tokenizers.ZwspWordTokenizer => (await ParatextProjectTextCorpus.Get(Mediator!, metadata.Id!, cancellationToken))
                               .Tokenize<ZwspWordTokenizer>()
                               .Transform<IntoTokensTextRowProcessor>()
                               .Transform<SetTrainingBySurfaceLowercase>(),
                            _ => (await ParatextProjectTextCorpus.Get(Mediator!, metadata.Id!, cancellationToken))
                               .Tokenize<WhitespaceTokenizer>()
                               .Transform<IntoTokensTextRowProcessor>()
                               .Transform<SetTrainingBySurfaceLowercase>()
                        };

                        await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running,
                           description: $"Creating tokenized text corpus for '{metadata.Name}' corpus...", cancellationToken: cancellationToken);

#pragma warning disable CS8604
                        var tokenizedTextCorpus = await textCorpus.Create(Mediator, corpus.CorpusId,
                           metadata.Name, dialogViewModel.SelectedTokenizer.ToString(), cancellationToken);
#pragma warning restore CS8604

                        await SendBackgroundStatus(taskName, LongRunningTaskStatus.Completed,
                           description: $"Creating tokenized text corpus for '{metadata.Name}' corpus...Completed", cancellationToken: cancellationToken);

                        Logger!.LogInformation("Sending TokenizedTextCorpusLoadedMessage via EventAggregator.");

                        OnUIThread(async () =>
                        {
                            await DesignSurface!.UpdateNodeTokenization(node, this);
                        });

                        _longRunningTaskManager.TaskComplete(taskName);
                    }
                    catch (OperationCanceledException ex)
                    {
                        Logger!.LogInformation("AddParatextCorpus() - OperationCanceledException was thrown -> cancellation was requested.");
                    }
                    catch (MediatorErrorEngineException ex)
                    {
                        if (ex.Message.Contains("The operation was canceled"))
                        {
                            Logger!.LogInformation($"AddParatextCorpus() - OperationCanceledException was thrown -> cancellation was requested.");
                        }
                        else
                        {
                            Logger!.LogError(ex, "an unexpected Engine exception was thrown.");
                        }

                    }
                    catch (Exception ex)
                    {
                        Logger!.LogError(ex, $"An unexpected error occurred while creating the the corpus for {metadata.Name} ");
                        if (!cancellationToken.IsCancellationRequested)
                        {

                            await SendBackgroundStatus(taskName, LongRunningTaskStatus.Failed,
                               exception: ex, cancellationToken: cancellationToken);
                        }
                    }
                    finally
                    {

                        _longRunningTaskManager.TaskComplete(taskName);
                        _busyState.Remove(taskName);
                        if (cancellationToken.IsCancellationRequested)
                        {
                            DesignSurface!.DeleteCorpusNode(node);
                        }
                        else
                        {
                            PlaySound.PlaySoundFromResource(null, null);
                        }

                    }
                }, cancellationToken);
            }
        }

        public async Task SendBackgroundStatus(string name, LongRunningTaskStatus status, CancellationToken cancellationToken, string? description = null, Exception? exception = null)
        {
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

        //private async Task UpdateNodeTokenization(CorpusNodeViewModel node)
        //{
        //    var corpusNode = DesignSurface!.CorpusNodes.FirstOrDefault(b => b.Id == node.Id);
        //    if (corpusNode is not null)
        //    {
        //        var topLevelProjectIds = await TopLevelProjectIds.GetTopLevelProjectIds(Mediator!);
        //        var tokenizedCorpora = topLevelProjectIds.TokenizedTextCorpusIds.Where(ttc => ttc.CorpusId.Id == corpusNode.CorpusId);
        //        DesignSurface!.CreateCorpusNodeMenu(corpusNode, tokenizedCorpora, this);
        //    }
        //}

        ///// <summary>
        ///// creates the menu for the CorpusNode
        ///// </summary>
        ///// <param name="corpusNode"></param>
        ///// <exception cref="NotImplementedException"></exception>
        //private void CreateCorpusNodeMenu(CorpusNodeViewModel corpusNode, IEnumerable<TokenizedTextCorpusId> tokenizedCorpora)
        //{
        //    // initiate the menu system
        //    corpusNode.MenuItems.Clear();

        //    BindableCollection<CorpusNodeMenuItemViewModel> nodeMenuItems = new();

        //    // restrict the ability of Manuscript to add new tokenizers
        //    if (corpusNode.CorpusType != CorpusType.ManuscriptHebrew || corpusNode.CorpusType != CorpusType.ManuscriptGreek)
        //    {
        //        // Add new tokenization
        //        nodeMenuItems.Add(new CorpusNodeMenuItemViewModel
        //        {
        //            Header = LocalizationStrings.Get("Pds_AddNewTokenizationMenu", Logger),
        //            Id = "AddTokenizationId",
        //            IconKind = PackIconPicolIconsKind.BookTextAdd.ToString(),
        //            ProjectDesignSurfaceViewModel = this,
        //            CorpusNodeViewModel = corpusNode,
        //        });
        //        nodeMenuItems.Add(new CorpusNodeMenuItemViewModel { Header = "", Id = "SeparatorId", ProjectDesignSurfaceViewModel = this, IsSeparator = true });
        //    }

        //    corpusNode.TokenizationCount = 0;
        //    foreach (var tokenizedCorpus in tokenizedCorpora)
        //    {
        //        if (!string.IsNullOrEmpty(tokenizedCorpus.TokenizationFunction))
        //        {
        //            var tokenizer = (Tokenizers)Enum.Parse(typeof(Tokenizers),
        //                tokenizedCorpus.TokenizationFunction);
        //            nodeMenuItems.Add(new CorpusNodeMenuItemViewModel
        //            {
        //                Header = EnumHelper.GetDescription(tokenizer),
        //                Id = tokenizedCorpus.Id.ToString(),
        //                IconKind = PackIconPicolIconsKind.Relevance.ToString(),
        //                MenuItems = new BindableCollection<CorpusNodeMenuItemViewModel>
        //                {
        //                    new CorpusNodeMenuItemViewModel
        //                    {
        //                        // Add Verses to focused enhanced view
        //                        Header = LocalizationStrings.Get("Pds_AddToEnhancedViewMenu", Logger!),
        //                        Id = "AddToEnhancedViewId",
        //                        ProjectDesignSurfaceViewModel = this,
        //                        IconKind = PackIconPicolIconsKind.DocumentTextAdd.ToString(),
        //                        CorpusNodeViewModel = corpusNode,
        //                        Tokenizer = tokenizer.ToString(),
        //                    },
        //                    new CorpusNodeMenuItemViewModel
        //                    {
        //                        // Show Verses in New Windows
        //                        Header = LocalizationStrings.Get("Pds_ShowVersesMenu", Logger!),
        //                        Id = "ShowVerseId", ProjectDesignSurfaceViewModel = this,
        //                        IconKind = PackIconPicolIconsKind.DocumentText.ToString(),
        //                        CorpusNodeViewModel = corpusNode,
        //                        Tokenizer = tokenizer.ToString(),
        //                    },
        //                    //new CorpusNodeMenuItemViewModel
        //                    //{
        //                    //    // Properties
        //                    //    Header = LocalizationStrings.Get("Pds_PropertiesMenu", Logger),
        //                    //    Id = "TokenizerPropertiesId",
        //                    //    ProjectDesignSurfaceViewModel = this,
        //                    //    IconKind = "Settings",
        //                    //    CorpusNodeViewModel = corpusNode,
        //                    //    Tokenizer = nodeTokenization.TokenizationName,
        //                    //}
        //                }
        //            });
        //            corpusNode.TokenizationCount++;
        //        }
        //    }

        //    nodeMenuItems.Add(new CorpusNodeMenuItemViewModel
        //    {
        //        Header = "",
        //        Id = "SeparatorId",
        //        ProjectDesignSurfaceViewModel = this,
        //        IsSeparator = true
        //    });

        //    nodeMenuItems.Add(new CorpusNodeMenuItemViewModel
        //    {
        //        // Properties
        //        Header = LocalizationStrings.Get("Pds_PropertiesMenu", Logger),
        //        Id = "PropertiesId",
        //        IconKind = PackIconPicolIconsKind.Settings.ToString(),
        //        CorpusNodeViewModel = corpusNode,
        //        ProjectDesignSurfaceViewModel = this
        //    });

        //    corpusNode.MenuItems = nodeMenuItems;
        //}


        ///// <summary>
        ///// creates the data bound menu for the node
        ///// </summary>
        ///// <param name="parallelCorpusConnection"></param>
        ///// <param name="topLevelProjectIds"></param>
        ///// <exception cref="NotImplementedException"></exception>
        //public void CreateConnectionMenu(ParallelCorpusConnectionViewModel parallelCorpusConnection, TopLevelProjectIds topLevelProjectIds)
        //{
        //    // initiate the menu system
        //    parallelCorpusConnection.MenuItems.Clear();

        //    BindableCollection<ParallelCorpusConnectionMenuItemViewModel> connectionMenuItems = new()
        //    {
        //        // Add new alignment set
        //        new ParallelCorpusConnectionMenuItemViewModel
        //        {
        //            Header = LocalizationStrings.Get("Pds_CreateNewAlignmentSetMenu", Logger!),
        //            Id = "CreateAlignmentSetId",
        //            IconKind = PackIconPicolIconsKind.BookTextAdd.ToString(),
        //            ProjectDesignSurfaceViewModel = this,
        //            ConnectionId = parallelCorpusConnection.Id,
        //            ParallelCorpusId = parallelCorpusConnection.ParallelCorpusId?.Id.ToString(),
        //            ParallelCorpusDisplayName = parallelCorpusConnection.ParallelCorpusDisplayName,
        //            IsRtl = parallelCorpusConnection.IsRtl,
        //            SourceParatextId = parallelCorpusConnection.SourceConnector?.ParatextId,
        //            TargetParatextId = parallelCorpusConnection.DestinationConnector?.ParatextId,
        //        },
        //        new ParallelCorpusConnectionMenuItemViewModel
        //        {
        //            Header = "",
        //            Id = "SeparatorId",
        //            ProjectDesignSurfaceViewModel = this,
        //            IsSeparator = true
        //        }
        //    };

        //    var alignmentSets = topLevelProjectIds.AlignmentSetIds.Where(alignmentSet => alignmentSet.ParallelCorpusId == parallelCorpusConnection.ParallelCorpusId);
        //    // ALIGNMENT SETS
        //    foreach (var alignmentSetInfo in alignmentSets)
        //    {
        //        connectionMenuItems.Add(new ParallelCorpusConnectionMenuItemViewModel
        //        {
        //            Header = alignmentSetInfo.DisplayName,
        //            Id = alignmentSetInfo.Id.ToString(),
        //            IconKind = PackIconPicolIconsKind.Sitemap.ToString(),
        //            IsEnabled = true,
        //            MenuItems = new BindableCollection<ParallelCorpusConnectionMenuItemViewModel>
        //            {
        //                new ParallelCorpusConnectionMenuItemViewModel
        //                {
        //                    // Add Verses to focused enhanced view
        //                    Header = LocalizationStrings.Get("Pds_AddConnectionToEnhancedViewMenu", Logger),
        //                    Id = "AddAlignmentToEnhancedViewId",
        //                    ProjectDesignSurfaceViewModel = this,
        //                    IconKind = PackIconPicolIconsKind.DocumentTextAdd.ToString(),
        //                    AlignmentSetId = alignmentSetInfo.Id.ToString(),
        //                    DisplayName = alignmentSetInfo.DisplayName,
        //                    ParallelCorpusId = alignmentSetInfo.ParallelCorpusId.Id.ToString(),
        //                    ParallelCorpusDisplayName = alignmentSetInfo.ParallelCorpusId.DisplayName,
        //                    IsEnabled = true,
        //                    IsRtl = parallelCorpusConnection.SourceConnector!.ParentNode!.IsRtl,
        //                    IsTargetRTL = parallelCorpusConnection.DestinationConnector!.ParentNode!.IsRtl,
        //                    SourceParatextId = parallelCorpusConnection.SourceConnector.ParatextId,
        //                    TargetParatextId = parallelCorpusConnection.DestinationConnector.ParatextId,
        //                },
        //            }
        //        });
        //    }

        //    // TRANSLATION SET
        //    connectionMenuItems.Add(new ParallelCorpusConnectionMenuItemViewModel
        //    { Header = "", Id = "SeparatorId", ProjectDesignSurfaceViewModel = this, IsSeparator = true });
        //    connectionMenuItems.Add(new ParallelCorpusConnectionMenuItemViewModel
        //    { Header = "", Id = "SeparatorId", ProjectDesignSurfaceViewModel = this, IsSeparator = true });

        //    // Add new tokenization
        //    connectionMenuItems.Add(new ParallelCorpusConnectionMenuItemViewModel
        //    {
        //        Header = LocalizationStrings.Get("Pds_CreateNewInterlinear", Logger),
        //        Id = "CreateNewInterlinearId",
        //        IconKind = PackIconPicolIconsKind.BookTextAdd.ToString(),
        //        ProjectDesignSurfaceViewModel = this,
        //        ConnectionId = parallelCorpusConnection.Id,
        //        Enabled = (parallelCorpusConnection.AlignmentSetInfo.Count > 0)
        //    });
        //    connectionMenuItems.Add(new ParallelCorpusConnectionMenuItemViewModel
        //    { Header = "", Id = "SeparatorId", ProjectDesignSurfaceViewModel = this, IsSeparator = true });

        //    var translationSets = topLevelProjectIds.TranslationSetIds.Where(translationSet => translationSet.ParallelCorpusId == parallelCorpusConnection.ParallelCorpusId);
        //    foreach (var info in translationSets)
        //    {
        //        connectionMenuItems.Add(new ParallelCorpusConnectionMenuItemViewModel
        //        {
        //            Header = info.DisplayName,
        //            Id = info.Id.ToString(),
        //            IconKind = PackIconPicolIconsKind.Relevance.ToString(),
        //            MenuItems = new BindableCollection<ParallelCorpusConnectionMenuItemViewModel>
        //                {
        //                    new ParallelCorpusConnectionMenuItemViewModel
        //                    {
        //                        // Add Verses to focused enhanced view
        //                        Header = LocalizationStrings.Get("Pds_AddConnectionToEnhancedViewMenu", Logger),
        //                        Id = "AddTranslationToEnhancedViewId", ProjectDesignSurfaceViewModel = this,
        //                        IconKind = PackIconPicolIconsKind.DocumentTextAdd.ToString(),
        //                        TranslationSetId = info.Id.ToString(),
        //                        DisplayName = info.DisplayName,
        //                        ParallelCorpusId = info.ParallelCorpusId.Id.ToString(),
        //                        ParallelCorpusDisplayName =  info.ParallelCorpusId.DisplayName,

        //                        // TODO:  Where does IsRtl come from?
        //                        //IsRtl = info.ParallelCorpusId.SourceTokenizedCorpusId.,
        //                        SourceParatextId = parallelCorpusConnection.SourceConnector.ParatextId,
        //                        TargetParatextId = parallelCorpusConnection.DestinationConnector.ParatextId,
        //                    }
        //                }
        //        });
        //    }


        //    connectionMenuItems.Add(new ParallelCorpusConnectionMenuItemViewModel
        //    { Header = "", Id = "SeparatorId", ProjectDesignSurfaceViewModel = this, IsSeparator = true });

        //    //connectionMenuItems.Add(new ParallelCorpusConnectionMenuItemViewModel
        //    //{
        //    //    // Properties
        //    //    Header = LocalizationStrings.Get("Pds_PropertiesMenu", Logger),
        //    //    Id = "PropertiesId",
        //    //    IconKind = "Settings",
        //    //    ConnectionViewModel = connection,
        //    //    ProjectDesignSurfaceViewModel = this
        //    //});

        //    parallelCorpusConnection.MenuItems = connectionMenuItems;
        //}

        public async Task ExecuteConnectionMenuCommand(ParallelCorpusConnectionMenuItemViewModel connectionMenuItem)
        {
            var connectionViewModel = connectionMenuItem.ConnectionViewModel;
            switch (connectionMenuItem.Id)
            {
                case "AddTranslationSetId":
                    // find the right connection to send
                    var connection = DesignSurface.ParallelCorpusConnections.First(c => c.Id == connectionMenuItem.ConnectionId);

                    if (connection is not null)
                    {
                        // kick off the add new tokenization dialog
                        await AddParallelCorpus(connection);
                    }
                    else
                    {
                        Logger!.LogError("Could not find connection with id {0}", connectionMenuItem.ConnectionId);
                    }
                    break;
                case "SeparatorId":
                    // no-op
                    break;
                case "PropertiesId":
                    // node properties
                    SelectedDesignSurfaceComponent = connectionViewModel;
                    break;
                case "CreateNewInterlinearId":
                    await AddNewInterlinear(connectionMenuItem);
                    break;
                case "AddAlignmentToEnhancedViewId":
                    if (connectionMenuItem.IsEnabled)
                    {
                        await EventAggregator.PublishOnUIThreadAsync(
                            new ShowParallelTranslationWindowMessage(
                                null,
                                connectionMenuItem.AlignmentSetId,
                                connectionMenuItem.DisplayName,
                                connectionMenuItem.ParallelCorpusId ?? throw new InvalidDataEngineException(name: "ParallelCorpusId", value: "null"),
                                connectionMenuItem.ParallelCorpusDisplayName,
                                //FIXME:surface serialization new EngineStringDetokenizer(new LatinWordDetokenizer()),
                                connectionMenuItem.IsRtl,
                                //FIXME:surface serialization new EngineStringDetokenizer(new LatinWordDetokenizer()),
                                connectionMenuItem.IsTargetRTL,
                                IsNewWindow: false,
                                connectionMenuItem.SourceParatextId,
                                connectionMenuItem.TargetParatextId));
                    }
                    break;
                case "AddTranslationToEnhancedViewId":
                    if (connectionMenuItem.IsEnabled)
                    {
                        await EventAggregator.PublishOnUIThreadAsync(
                            new ShowParallelTranslationWindowMessage(
                                connectionMenuItem.TranslationSetId,
                                null,
                                connectionMenuItem.DisplayName,
                                connectionMenuItem.ParallelCorpusId ?? throw new InvalidDataEngineException(name: "ParallelCorpusId", value: "null"),
                                connectionMenuItem.ParallelCorpusDisplayName,
                                //FIXME:surface serialization new EngineStringDetokenizer(new LatinWordDetokenizer()),
                                connectionMenuItem.IsRtl,
                                //FIXME:surface serialization null,
                                null,
                                IsNewWindow: false,
                                connectionMenuItem.SourceParatextId,
                                connectionMenuItem.TargetParatextId));
                    }
                    break;
                default:

                    break;
            }
        }



        public async Task ExecuteCorpusNodeMenuCommand(CorpusNodeMenuItemViewModel? corpusNodeMenuItem)
        {
            var corpusNodeViewModel = corpusNodeMenuItem.CorpusNodeViewModel;

            //if (corpusNodeMenuItem == null)
            //{
            //    Logger!.LogInformation($"The CorpusNodeMenuItem is null.  Returning...");
            //    return;
            //}

            if (corpusNodeViewModel == null)
            {
                Logger!.LogInformation($"The CorpusNodeViewModel on the CorpusNodeMenuItem: '{corpusNodeMenuItem.Id}' is null., Returning...");
                return;
            }

            switch (corpusNodeMenuItem.Id)
            {
                case "AddTokenizationId":
                    // kick off the add new tokenization dialog
                    await AddParatextCorpus(corpusNodeViewModel.ParatextProjectId);
                    break;
                case "SeparatorId":
                    // no-op
                    break;

                case "AddToEnhancedViewId":
                case "ShowVerseId":

                    var topLevelProjectIds = await TopLevelProjectIds.GetTopLevelProjectIds(Mediator!);
                    var tokenizedCorpus = topLevelProjectIds.TokenizedTextCorpusIds.FirstOrDefault(tc => tc.TokenizationFunction == corpusNodeMenuItem.Tokenizer);

                    if (tokenizedCorpus == null)
                    {
                        Logger!.LogDebug($"Could not locate a TokenizedCorpus with a TokenizationFunction: '{corpusNodeMenuItem.Tokenizer}'.");
                        return;
                    }

                    var showInNewWindow = corpusNodeMenuItem.Id == "ShowVerseId";

                    await EventAggregator.PublishOnUIThreadAsync(
                        new ShowTokenizationWindowMessage(
                            corpusNodeViewModel!.ParatextProjectId,
                            ProjectName: corpusNodeViewModel.Name,
                            TokenizationType: corpusNodeMenuItem.Tokenizer!,
                            CorpusId: tokenizedCorpus.CorpusId!.Id,
                            TokenizedTextCorpusId: tokenizedCorpus.Id,
                            corpusNodeViewModel.CorpusType,
                            //FIXME:new EngineStringDetokenizer(new LatinWordDetokenizer()),
                            corpusNodeViewModel.IsRtl,
                            IsNewWindow: showInNewWindow));
                    break;
                case "PropertiesId":
                    // node properties
                    SelectedDesignSurfaceComponent = corpusNodeViewModel;
                    break;
                case "TokenizerPropertiesId":
                    //TODO:  Fix this

                    // get the selected tokenizer
                    //                    var nodeTokenization =
                    //                        corpusNodeViewModel.Tokenizations.FirstOrDefault(b =>
                    //                            b.TokenizationName == corpusNodeMenuItem.Tokenizer);
                    //#pragma warning disable CS8601
                    //SelectedDesignSurfaceComponent = nodeTokenization;
                    //#pragma warning restore CS8601
                    break;
            }
        }



        /// <summary>
        /// Called when the user has started to drag out a connector, thus creating a new connection.
        /// </summary>
        public ParallelCorpusConnectionViewModel ConnectionDragStarted(ParallelCorpusConnectorViewModel draggedOutParallelCorpusConnector, Point curDragPoint)
        {
            //
            // Create a new connection to add to the view-model.
            //
            var connection = new ParallelCorpusConnectionViewModel();

            if (draggedOutParallelCorpusConnector.ConnectorType == ConnectorType.Output)
            {
                //
                // The user is dragging out a source connector (an output) and will connect it to a destination connector (an input).
                //
                connection.SourceConnector = draggedOutParallelCorpusConnector;
                connection.DestConnectorHotspot = curDragPoint;
            }
            else
            {
                //
                // The user is dragging out a destination connector (an input) and will connect it to a source connector (an output).
                //
                connection.DestinationConnector = draggedOutParallelCorpusConnector;
                connection.SourceConnectorHotspot = curDragPoint;
            }

            //
            // Add the new connection to the view-model.
            //
            DesignSurface.ParallelCorpusConnections.Add(connection);

            return connection;
        }

        /// <summary>
        /// Event raised, to query for feedback, while the user is dragging a connection.
        /// </summary>
        public void OnProjectDesignSurfaceQueryConnectionFeedback(object sender, QueryConnectionFeedbackEventArgs e)
        {
            var draggedOutConnector = (ParallelCorpusConnectorViewModel)e.ConnectorDraggedOut;
            var draggedOverConnector = (ParallelCorpusConnectorViewModel)e.DraggedOverConnector;

            QueryConnectionFeedback(draggedOutConnector, draggedOverConnector, out var feedbackIndicator, out var connectionOk);

            //
            // Return the feedback object to ProjectDesignSurfaceView.
            // The object combined with the data-template for it will be used to create a 'feedback icon' to
            // display (in an adorner) to the user.
            //
            e.FeedbackIndicator = feedbackIndicator;

            //
            // Let ProjectDesignSurfaceView know if the connection is ok or not ok.
            //
            e.ConnectionOk = connectionOk;
        }

        /// <summary>
        /// Event raised when the user has started to drag out a connection.
        /// </summary>
        public void OnParallelCorpusConnectionDragStarted(object sender, ConnectionDragStartedEventArgs e)
        {
            if (IsBusy)
            {
                return;
            }

            var draggedOutConnector = (ParallelCorpusConnectorViewModel)e.ConnectorDraggedOut;
            var curDragPoint = Mouse.GetPosition(DesignSurface.ProjectDesignSurface);

            //
            // Delegate the real work to the view model.
            //
            var connection = ConnectionDragStarted(draggedOutConnector, curDragPoint);

            //
            // Must return the view-model object that represents the connection via the event args.
            // This is so that ProjectDesignSurfaceView can keep track of the object while it is being dragged.
            //
            e.Connection = connection;
        }

        /// <summary>
        /// Event raised while the user is dragging a connection.
        /// </summary>
        public void OnParallelCorpusConnectionDragging(object sender, ConnectionDraggingEventArgs e)
        {
            var curDragPoint = Mouse.GetPosition(DesignSurface.ProjectDesignSurface);
            var connection = (ParallelCorpusConnectionViewModel)e.Connection;
            ConnectionDragging(curDragPoint, connection);
        }

        /// <summary>
        /// Event raised when the user has finished dragging out a connection.
        /// </summary>
        public void OnParallelCorpusConnectionDragCompleted(object? sender, ConnectionDragCompletedEventArgs e)
        {
            var connectorDraggedOut = (ParallelCorpusConnectorViewModel)e.ConnectorDraggedOut;
            var connectorDraggedOver = (ParallelCorpusConnectorViewModel)e.ConnectorDraggedOver;
            var newConnection = (ParallelCorpusConnectionViewModel)e.Connection;
            ConnectionDragCompleted(newConnection, connectorDraggedOut, connectorDraggedOver);
        }

        public async void OnCorpusNodeDragCompleted(object? sender, NodeDragCompletedEventArgs? e)
        {
            Logger!.LogInformation("NodeDragCompleted");
            await SaveDesignSurfaceData();
         
        }

        /// <summary>
        /// Called to query the application for feedback while the user is dragging the connection.
        /// </summary>
        public void QueryConnectionFeedback(ParallelCorpusConnectorViewModel draggedOutParallelCorpusConnector, ParallelCorpusConnectorViewModel draggedOverParallelCorpusConnector, out object feedbackIndicator, out bool connectionOk)
        {
            if (draggedOutParallelCorpusConnector == draggedOverParallelCorpusConnector)
            {
                //
                // Can't connect to self!
                // Provide feedback to indicate that this connection is not valid!
                //
                feedbackIndicator = new ConnectionBadIndicator();
                connectionOk = false;
            }
            else
            {
                var sourceConnector = draggedOutParallelCorpusConnector;
                var destConnector = draggedOverParallelCorpusConnector;

                //
                // Only allow connections from output connector to input connector (ie each
                // connector must have a different type).
                // Also only allocation from one node to another, never one node back to the same node.
                //
                connectionOk = sourceConnector.ParentNode != destConnector.ParentNode &&
                               sourceConnector.ConnectorType != destConnector.ConnectorType;

                if (connectionOk)
                {
                    // 
                    // Yay, this is a valid connection!
                    // Provide feedback to indicate that this connection is ok!
                    //
                    feedbackIndicator = new ConnectionOkIndicator();
                }
                else
                {
                    //
                    // Connectors with the same connector type (eg input & input, or output & output)
                    // can't be connected.
                    // Only connectors with separate connector type (eg input & output).
                    // Provide feedback to indicate that this connection is not valid!
                    //
                    feedbackIndicator = new ConnectionBadIndicator();
                }
            }
        }

        /// <summary>
        /// Called as the user continues to drag the connection.
        /// </summary>
        public void ConnectionDragging(Point curDragPoint, ParallelCorpusConnectionViewModel parallelCorpusConnection)
        {

            Logger!.LogDebug($"Current drag point: {curDragPoint.X}, {curDragPoint.Y}");
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (parallelCorpusConnection is not null)
            {
                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                if (parallelCorpusConnection.DestinationConnector == null)
                {
                    parallelCorpusConnection.DestConnectorHotspot = curDragPoint;
                }
                else
                {
                    parallelCorpusConnection.SourceConnectorHotspot = curDragPoint;
                }
            }
        }

        /// <summary>
        /// Called when the user has finished dragging out the new connection.
        /// </summary>
        public async void ConnectionDragCompleted(ParallelCorpusConnectionViewModel newParallelCorpusConnection, ParallelCorpusConnectorViewModel parallelCorpusConnectorDraggedOut, ParallelCorpusConnectorViewModel parallelCorpusConnectorDraggedOver)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (parallelCorpusConnectorDraggedOver == null)
            {
                //
                // The connection was unsuccessful.
                // Maybe the user dragged it out and dropped it in empty space.
                //
                DesignSurface!.ParallelCorpusConnections.Remove(newParallelCorpusConnection);
                return;
            }

            //
            // Only allow connections from output connector to input connector (ie each
            // connector must have a different type).
            // Also only allocation from one node to another, never one node back to the same node.
            //
            var connectionOk = parallelCorpusConnectorDraggedOut.ParentNode != parallelCorpusConnectorDraggedOver.ParentNode &&
                               parallelCorpusConnectorDraggedOut.ConnectorType != parallelCorpusConnectorDraggedOver.ConnectorType;

            if (!connectionOk)
            {
                //
                // Connections between connectors that have the same type,
                // eg input -> input or output -> output, are not allowed,
                // Remove the connection.
                //
                DesignSurface!.ParallelCorpusConnections.Remove(newParallelCorpusConnection);
                return;
            }

            //
            // The user has dragged the connection on top of another valid connector.
            //

            //
            // Remove any existing connection between the same two connectors.
            //
            var existingConnection = DesignSurface!.FindConnection(parallelCorpusConnectorDraggedOut, parallelCorpusConnectorDraggedOver);
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (existingConnection != null)
            {
                DesignSurface!.ParallelCorpusConnections.Remove(existingConnection);
            }

            //
            // Finalize the connection by attaching it to the connector
            // that the user dragged the mouse over.
            //
            bool added;
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (newParallelCorpusConnection.DestinationConnector is null)
            {
                newParallelCorpusConnection.DestinationConnector = parallelCorpusConnectorDraggedOver;
                added = true;
            }
            else
            {
                newParallelCorpusConnection.SourceConnector = parallelCorpusConnectorDraggedOver;
                added = true;
            }

            if (added)
            {
                // check to see if we somehow didn't get a source/target id properly.  If so remove the line
                if (newParallelCorpusConnection.SourceConnector.ParentNode.ParatextProjectId == "" || newParallelCorpusConnection.SourceConnector.ParentNode.ParatextProjectId is null)
                {
                    DesignSurface!.ParallelCorpusConnections.Remove(newParallelCorpusConnection);
                    return;
                }

                if (newParallelCorpusConnection.DestinationConnector.ParentNode.ParatextProjectId == "" || newParallelCorpusConnection.DestinationConnector.ParentNode.ParatextProjectId is null)
                {
                    DesignSurface!.ParallelCorpusConnections.Remove(newParallelCorpusConnection);
                    return;
                }

                await EventAggregator.PublishOnUIThreadAsync(new ParallelCorpusAddedMessage(
                    SourceParatextId: newParallelCorpusConnection.SourceConnector.ParentNode.ParatextProjectId,
                    TargetParatextId: newParallelCorpusConnection.DestinationConnector.ParentNode.ParatextProjectId,
                    ConnectorGuid: newParallelCorpusConnection.Id));

                var mainViewModel = IoC.Get<MainViewModel>();
                newParallelCorpusConnection.SourceFontFamily = mainViewModel.GetFontFamilyFromParatextProjectId(newParallelCorpusConnection.SourceConnector.ParentNode
                    .ParatextProjectId);

                newParallelCorpusConnection.TargetFontFamily = mainViewModel.GetFontFamilyFromParatextProjectId(newParallelCorpusConnection.DestinationConnector.ParentNode
                    .ParatextProjectId);

                await AddParallelCorpus(newParallelCorpusConnection);
            }

            await SaveDesignSurfaceData();
        }


        private async Task AddNewInterlinear(ParallelCorpusConnectionMenuItemViewModel connectionMenuItem)
        {
            var parameters = new List<Autofac.Core.Parameter>
            {
                new NamedParameter("parallelCorpusId", connectionMenuItem.ParallelCorpusId!)
            };

            var dialogViewModel = LifetimeScope!.Resolve<InterlinearDialogViewModel>(parameters);
            var result = await _windowManager!.ShowDialogAsync(dialogViewModel, null, DashboardProjectManager.NewProjectDialogSettings);

            if (result)
            {
                var translationSet = await TranslationSet.Create(null, dialogViewModel.SelectedAlignmentSet!,
                        dialogViewModel.TranslationSetDisplayName, new Dictionary<string, object>(),
                        dialogViewModel.SelectedAlignmentSet!.ParallelCorpusId!, Mediator!);

                if (translationSet != null)
                {
                    connectionMenuItem.ConnectionViewModel!.TranslationSetInfo.Add(new TranslationSetInfo
                    {
                        DisplayName = translationSet.TranslationSetId.DisplayName,
                        TranslationSetId = translationSet.TranslationSetId.Id.ToString(),
                        ParallelCorpusDisplayName = translationSet.ParallelCorpusId.DisplayName,
                        ParallelCorpusId = translationSet.ParallelCorpusId.Id.ToString(),
                        AlignmentSetId = translationSet.AlignmentSetId.Id.ToString(),
                        AlignmentSetDisplayName = translationSet.AlignmentSetId.DisplayName
                    });

                    var topLevelProjectIds = await TopLevelProjectIds.GetTopLevelProjectIds(Mediator!);
                    DesignSurface!.CreateConnectionMenu(connectionMenuItem.ConnectionViewModel, topLevelProjectIds, this);
                    await SaveDesignSurfaceData();
                }
            }
        }

        public async Task AddParallelCorpus(ParallelCorpusConnectionViewModel newParallelCorpusConnection)
        {
            var sourceCorpusNode = DesignSurface.CorpusNodes.FirstOrDefault(b => b.Id == newParallelCorpusConnection.SourceConnector.ParentNode.Id);
            if (sourceCorpusNode == null)
            {
                throw new MissingCorpusNodeException(
                    $"Cannot find the source Corpus node for the Corpus with Id '{newParallelCorpusConnection.SourceConnector.ParentNode.CorpusId}'.");
            }
            var targetCorpusNode = DesignSurface.CorpusNodes.FirstOrDefault(b => b.Id == newParallelCorpusConnection.DestinationConnector.ParentNode.Id);
            if (targetCorpusNode == null)
            {
                throw new MissingCorpusNodeException(
                    $"Cannot find the target Corpus node for the Corpus with Id '{newParallelCorpusConnection.DestinationConnector.ParentNode.CorpusId}'.");
            }

            var topLevelProjectIds = await TopLevelProjectIds.GetTopLevelProjectIds(Mediator);
            var tokenizedCorpora = topLevelProjectIds.TokenizedTextCorpusIds;
            var parameters = new List<Autofac.Core.Parameter>
            {
                new NamedParameter("dialogMode", DialogMode.Add),
                new NamedParameter("connectionViewModel", newParallelCorpusConnection),
                new NamedParameter("sourceCorpusNodeViewModel", sourceCorpusNode),
                new NamedParameter("targetCorpusNodeViewModel", targetCorpusNode),
                new NamedParameter("tokenizedCorpora", tokenizedCorpora)
            };

            var dialogViewModel = LifetimeScope?.Resolve<ParallelCorpusDialogViewModel>(parameters);

            try
            {
                var success = await _windowManager.ShowDialogAsync(dialogViewModel, null, DashboardProjectManager.NewProjectDialogSettings);

                PlaySound.PlaySoundFromResource(null, null);

                if (success)
                {
                    // get TranslationSet , etc from the dialogViewModel
                    var translationSet = dialogViewModel!.TranslationSet;

                    if (translationSet != null)
                    {
                        newParallelCorpusConnection.TranslationSetInfo.Add(new TranslationSetInfo
                        {
                            DisplayName = translationSet.TranslationSetId.DisplayName,
                            TranslationSetId = translationSet.TranslationSetId.Id.ToString(),
                            ParallelCorpusDisplayName = translationSet.ParallelCorpusId.DisplayName,
                            ParallelCorpusId = translationSet.ParallelCorpusId.Id.ToString(),
                            AlignmentSetId = translationSet.AlignmentSetId.Id.ToString(),
                            AlignmentSetDisplayName = translationSet.AlignmentSetId.DisplayName,
                            SourceFontFamily = newParallelCorpusConnection.SourceFontFamily!,
                            TargetFontFamily = newParallelCorpusConnection.TargetFontFamily!,
                        });
                    }

                    var alignmentSet = dialogViewModel.AlignmentSet;
                    if (alignmentSet != null)
                    {
                        newParallelCorpusConnection.AlignmentSetInfo.Add(new AlignmentSetInfo
                        {
                            DisplayName = alignmentSet.AlignmentSetId.DisplayName,
                            AlignmentSetId = alignmentSet.AlignmentSetId.Id.ToString(),
                            ParallelCorpusDisplayName = alignmentSet.ParallelCorpusId.DisplayName,
                            ParallelCorpusId = alignmentSet.ParallelCorpusId.Id.ToString(),
                            IsRtl = newParallelCorpusConnection.SourceConnector!.ParentNode!.IsRtl,
                            IsTargetRtl = newParallelCorpusConnection.DestinationConnector!.ParentNode!.IsRtl
                        });
                    }

                    newParallelCorpusConnection.ParallelCorpusId = dialogViewModel.ParallelTokenizedCorpus.ParallelCorpusId;
                    newParallelCorpusConnection.ParallelCorpusDisplayName =
                        dialogViewModel.ParallelTokenizedCorpus.ParallelCorpusId.DisplayName;

                    topLevelProjectIds = await TopLevelProjectIds.GetTopLevelProjectIds(Mediator!);
                    DesignSurface!.CreateConnectionMenu(newParallelCorpusConnection, topLevelProjectIds, this);

                }
                else
                {
                    DeleteConnection(newParallelCorpusConnection);
                }
            }
            finally
            {
                await SaveDesignSurfaceData();
            }


        }

    

     

     

        ///// <summary>
        ///// Create a node and add it to the view-model.
        ///// </summary>
        //private CorpusNodeViewModel CreateCorpusNode(Corpus corpus, Point nodeLocation)
        //{
        //    if (nodeLocation.X == 0 && nodeLocation.Y == 0)
        //    {
        //        // figure out some offset based on the number of nodes already on the design surface
        //        // so we don't overlap
        //        nodeLocation = DesignSurface!.GetFreeSpot();
        //    }


        //    var parameters = new List<Autofac.Core.Parameter>
        //    {
        //        new NamedParameter("name", corpus.CorpusId.Name ?? string.Empty)
        //    };

        //    var node = LifetimeScope?.Resolve<CorpusNodeViewModel>(parameters);
        //    node.X = (double.IsNegativeInfinity(nodeLocation.X) || double.IsPositiveInfinity(nodeLocation.X) ||
        //              double.IsNaN(nodeLocation.X))
        //        ? 150
        //        : nodeLocation.X;
        //    node.Y = (double.IsNegativeInfinity(nodeLocation.Y) || double.IsPositiveInfinity(nodeLocation.Y) ||
        //              double.IsNaN(nodeLocation.Y))
        //        ? 150
        //        : nodeLocation.Y;
        //    node.CorpusType = (CorpusType)Enum.Parse(typeof(CorpusType), corpus.CorpusId.CorpusType);
        //    node.ParatextProjectId = corpus.CorpusId.ParatextGuid ?? string.Empty;
        //    node.CorpusId = corpus.CorpusId.Id;
        //    node.IsRtl = corpus.CorpusId.IsRtl;
        //    node.TranslationFontFamily = corpus.CorpusId.FontFamily ?? Corpus.DefaultFontFamily;

        //    var targetConnector = LifetimeScope!.Resolve<ParallelCorpusConnectorViewModel>(new List<Autofac.Core.Parameter>
        //    {
        //        new NamedParameter("name", "Target"),
        //        new NamedParameter("paratextProjectId", node.ParatextProjectId),
        //        new NamedParameter("connectorType", ConnectorType.Input)
        //    });
        //    node.InputConnectors.Add(targetConnector);


        //    var outputConnector = LifetimeScope!.Resolve<ParallelCorpusConnectorViewModel>(new List<Autofac.Core.Parameter>
        //    {
        //        new NamedParameter("name", "Source"),
        //        new NamedParameter("paratextProjectId", node.ParatextProjectId),
        //        new NamedParameter("connectorType", ConnectorType.Output)
        //    });
        //    node.OutputConnectors.Add(outputConnector);

        //    //
        //    // Add the node to the view-model.
        //    //
        //    OnUIThread(() =>
        //    {
        //        DesignSurface!.CorpusNodes.Add(node);
        //        // NB: Allow the newly added node to be drawn on the design surface - even if there are other long running background processes running
        //        //     This is the equivalent of calling Application.DoEvents() in a WinForms app and should only be used as a last resort.
        //        _ = App.Current.Dispatcher.Invoke(
        //            DispatcherPriority.Background,
        //            new ThreadStart(delegate { }));
        //    });

        //    EventAggregator.PublishOnUIThreadAsync(new CorpusAddedMessage(node.ParatextProjectId));

        //    return node;
        //}

        /// <summary>
        /// Utility method to delete a connection from the view-model.
        /// </summary>
        public void DeleteConnection(ParallelCorpusConnectionViewModel parallelCorpusConnection)
        {
            EventAggregator.PublishOnUIThreadAsync(new ParallelCorpusDeletedMessage(
                SourceParatextId: parallelCorpusConnection.SourceConnector.ParentNode.ParatextProjectId,
                TargetParatextId: parallelCorpusConnection.DestinationConnector.ParentNode.ParatextProjectId,
                ConnectorGuid: parallelCorpusConnection.Id));

            DesignSurface.ParallelCorpusConnections.Remove(parallelCorpusConnection);
        }




        public void ShowCorpusProperties(CorpusNodeViewModel corpus)
        {
            SelectedDesignSurfaceComponent = corpus;
        }

        public void ShowConnectionProperties(ParallelCorpusConnectionViewModel parallelCorpusConnection)
        {
            SelectedDesignSurfaceComponent = parallelCorpusConnection;
        }

        //public async Task UiLanguageChangedMessage(UiLanguageChangedMessage message)
        //{
            
        //}


        public async Task HandleAsync(UiLanguageChangedMessage message, CancellationToken cancellationToken)
        {
            if (LoadingDesignSurface && !DesignSurfaceLoaded)
            {
                return;
            }
            //var language = message.LanguageCode;
            var topLevelProjectIds = await TopLevelProjectIds.GetTopLevelProjectIds(Mediator!);
            // re-render the context menus
            foreach (var corpusNode in DesignSurface!.CorpusNodes)
            {
                var tokenizedCorpora = topLevelProjectIds.TokenizedTextCorpusIds.Where(ttc => ttc.CorpusId!.Id == corpusNode.CorpusId);
                DesignSurface!.CreateCorpusNodeMenu(corpusNode, tokenizedCorpora, this);
            }
        }

        #endregion // Methods


        }
}
