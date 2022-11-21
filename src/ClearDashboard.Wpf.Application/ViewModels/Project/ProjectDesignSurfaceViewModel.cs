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
using ClearDashboard.Wpf.Application.Exceptions;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Models;
using ClearDashboard.Wpf.Application.Models.ProjectSerialization;
using ClearDashboard.Wpf.Application.ViewModels.Panes;
using ClearDashboard.Wpf.Application.ViewModels.Project.Interlinear;
using ClearDashboard.Wpf.Application.ViewModels.Project.ParallelCorpusDialog;
using ClearDashboard.Wpf.Application.Views.Project;
using MediatR;
using Microsoft.Extensions.Logging;
using SIL.Machine.Corpora;
using SIL.Machine.Tokenization;
using SIL.ObjectModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface;
using ClearDashboard.Wpf.Application.ViewModels.Main;
using ClearDashboard.Wpf.Controls;
using TranslationSet = ClearDashboard.DAL.Alignment.Translation.TranslationSet;
using Corpus = ClearDashboard.DAL.Alignment.Corpora.Corpus;
//using TopeLevelIds = ClearDashboard.DAL.Alignment.TopLevelIds;


// ReSharper disable once CheckNamespace
namespace ClearDashboard.Wpf.Application.ViewModels.Project
{

    public class ProjectDesignSurfaceViewModel : ToolViewModel
    {
        #region Member Variables

        //public record CorporaLoadedMessage(IEnumerable<DAL.Alignment.Corpora.Corpus> Copora);
        public record TokenizedTextCorpusLoadedMessage(TokenizedTextCorpus TokenizedTextCorpus, string TokenizationName, ParatextProjectMetadata? ProjectMetadata);

        private readonly IWindowManager? _windowManager;
        private readonly LongRunningTaskManager? _longRunningTaskManager;

        /// <summary>
        /// This is the network that is displayed in the window.
        /// It is the main part of the view-model.
        /// </summary>
        private DesignSurfaceViewModel _designSurface;

        ///
        /// The current scale at which the content is being viewed.
        /// 
        private double _contentScale = 1;

        ///
        /// The X coordinate of the offset of the viewport onto the content (in content coordinates).
        /// 
        private double _contentOffsetX;

        ///
        /// The Y coordinate of the offset of the viewport onto the content (in content coordinates).
        /// 
        private double _contentOffsetY;

        ///
        /// The width of the content (in content coordinates).
        /// 
        private double _contentWidth = 1000;

        ///
        /// The height of the content (in content coordinates).
        /// 
        private double _contentHeight = 1000;

        ///
        /// The width of the viewport onto the content (in content coordinates).
        /// The value for this is actually computed by the main window's ZoomAndPanControl and update in the
        /// view-model so that the value can be shared with the overview window.
        /// 
        private double _contentViewportWidth;

        ///
        /// The height of the viewport onto the content (in content coordinates).
        /// The value for this is actually computed by the main window's ZoomAndPanControl and update in the
        /// view-model so that the value can be shared with the overview window.
        /// 
        private double _contentViewportHeight;

        private Wpf.Controls.ProjectDesignSurface? ProjectDesignSurface { get; set; }


        #endregion //Member Variables

        #region Public Variables

        #endregion //Public Variables

        #region Observable Properties


         private BindableCollection<Corpus> Corpora { get; set; }

        /// <summary>
        /// This is the network that is displayed in the window.
        /// It is the main part of the view-model.
        /// </summary>
        public DesignSurfaceViewModel DesignSurface
        {
            get => _designSurface;
            private set => Set(ref _designSurface, value);
        }

        ///
        /// The current scale at which the content is being viewed.
        /// 
        public double ContentScale
        {
            get => _contentScale;
            set => Set(ref _contentScale, value);
        }

        ///
        /// The X coordinate of the offset of the viewport onto the content (in content coordinates).
        /// 
        public double ContentOffsetX
        {
            get => _contentOffsetX;
            set => Set(ref _contentOffsetX, value);
        }

        ///
        /// The Y coordinate of the offset of the viewport onto the content (in content coordinates).
        /// 
        public double ContentOffsetY
        {
            get => _contentOffsetY;
            set => Set(ref _contentOffsetY, value);
        }

        ///
        /// The width of the content (in content coordinates).
        /// 
        public double ContentWidth
        {
            get => _contentWidth;
            set => Set(ref _contentWidth, value);
        }

        ///
        /// The height of the content (in content coordinates).
        /// 
        public double ContentHeight
        {
            get => _contentHeight;
            set => Set(ref _contentHeight, value);
        }

        ///
        /// The width of the viewport onto the content (in content coordinates).
        /// The value for this is actually computed by the main window's ZoomAndPanControl and update in the
        /// view-model so that the value can be shared with the overview window.
        /// 
        public double ContentViewportWidth
        {
            get => _contentViewportWidth;
            set => Set(ref _contentViewportWidth, value);
        }

        ///
        /// The height of the viewport onto the content (in content coordinates).
        /// The value for this is actually computed by the main window's ZoomAndPanControl and update in the
        /// view-model so that the value can be shared with the overview window.
        /// 
        public double ContentViewportHeight
        {
            get => _contentViewportHeight;
            set => Set(ref _contentViewportHeight, value);
        }


        private object _selectedDesignSurfaceComponent;
        public object SelectedDesignSurfaceComponent
        {
            get
            {
                if (_selectedDesignSurfaceComponent is CorpusNodeViewModel node)
                {
                    foreach (var corpusNode in DesignSurface.CorpusNodes)
                    {
                        if (corpusNode.ParatextProjectId == node.ParatextProjectId)
                        {
                            return corpusNode;
                        }
                    }
                }
                else if (_selectedDesignSurfaceComponent is ConnectionViewModel conn)
                {
                    foreach (var connection in DesignSurface.Connections)
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

        private string _projectName;
        public string ProjectName
        {
            get => _projectName;
            set => Set(ref _projectName, value);
        }

        #endregion //Observable Properties

        #region Constructor
    

#pragma warning disable CS8618
        public ProjectDesignSurfaceViewModel()
#pragma warning restore CS8618

        {

        }

        // ReSharper disable once UnusedMember.Global
#pragma warning disable CS8618
        public ProjectDesignSurfaceViewModel(INavigationService navigationService, IWindowManager windowManager,
#pragma warning restore CS8618
            ILogger<ProjectDesignSurfaceViewModel> logger, DashboardProjectManager? projectManager,
            IEventAggregator? eventAggregator, IMediator mediator, ILifetimeScope lifetimeScope, LongRunningTaskManager longRunningTaskManager)
            : base(navigationService, logger, projectManager, eventAggregator, mediator, lifetimeScope)
        {

            _windowManager = windowManager;
            _longRunningTaskManager = longRunningTaskManager;

            Title = "🖧 PROJECT DESIGN SURFACE ****";
            ContentId = "PROJECTDESIGNSURFACETOOL";

            Corpora = new BindableCollection<Corpus>();
        }

        protected override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            EventAggregator.SubscribeOnUIThread(this);
            _busyState.CollectionChanged += BusyStateOnCollectionChanged;
            return base.OnActivateAsync(cancellationToken);
        }

        private void BusyStateOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            NotifyOfPropertyChange(() => IsBusy);
        }

        protected override async Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            _busyState.CollectionChanged -= BusyStateOnCollectionChanged;
            await SaveCanvas();
            await base.OnDeactivateAsync(close, cancellationToken);
        }

        protected override void OnViewAttached(object view, object context)
        {
         
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (ProjectDesignSurface == null)
            {
                if (view is ProjectDesignSurfaceView projectDesignSurfaceView)
                {
                    // ReSharper disable once AssignNullToNotNullAttribute
                    ProjectDesignSurface = (ProjectDesignSurface)projectDesignSurfaceView.FindName("ProjectDesignSurface");
                }
            }

            //
            // Create a network, the root of the view-model.
            //
#pragma warning disable CS8604
            // ReSharper disable once SuspiciousTypeConversion.Global
            DesignSurface = LifetimeScope.Resolve<DesignSurfaceViewModel>();
            // DesignSurface = new DesignSurfaceViewModel(NavigationService, Logger as ILogger<DesignSurfaceViewModel>,
            //    ProjectManager, EventAggregator);
#pragma warning restore CS8604

            base.OnViewAttached(view, context);
        }

      

     
        #endregion //Constructor

        #region Caliburn.Micro overrides

        //protected override async void OnViewLoaded(object view)
        //{
        //    Console.WriteLine();
        //    base.OnViewLoaded(view);
        //}

        //protected override async void OnViewReady(object view)
        //{
        //    Console.WriteLine();
        //    base.OnViewReady(view);
        //}
        #endregion //Constructor

        #region Methods

        public async Task SaveCanvas()
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
            foreach (var corpusNode in DesignSurface.CorpusNodes)
            {
                surface.TokenizedCorpora.Add(new SerializedTokenizedCorpus
                {
                    ParatextProjectId = corpusNode.ParatextProjectId,
                    CorpusType = corpusNode.CorpusType,
                    Name = corpusNode.Name,
                    X = corpusNode.X,
                    Y = corpusNode.Y,
                    Tokenizations = corpusNode.Tokenizations,
                    CorpusId = corpusNode.CorpusId,
                    IsRTL = corpusNode.IsRtl,
                    TranslationFontFamily = corpusNode.TranslationFontFamily,
                });
            }

            // save all the connections
            foreach (var connection in DesignSurface.Connections)
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

                surface.ParallelCorpora.Add(new SerializedParallelCorpuse
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

            // save out the corpora
            foreach (var corpus in this.Corpora)
            {
                surface.Corpora.Add(new SerializedCorpus
                {
                    CorpusId = corpus.CorpusId.Id.ToString(),
                    CorpusType = corpus.CorpusId.CorpusType,
                    Created = corpus.CorpusId.Created,
                    DisplayName = corpus.CorpusId.DisplayName,
                    IsRtl = corpus.CorpusId.IsRtl,
                    TranslationFontFamily = corpus.CorpusId.TranslationFontFamily ?? Corpus.DefaultTranslationFontFamily,
                    Language = corpus.CorpusId.Language,
                    Name = corpus.CorpusId.Name,
                    ParatextGuid = corpus.CorpusId.ParatextGuid,
                    UserId = corpus.CorpusId.UserId?.Id.ToString()
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

        public void LoadCanvas()
        {
            if (ProjectManager!.CurrentProject is null)
            {
                return;
            }

            ProjectName = ProjectManager.CurrentProject.ProjectName!;
            
            if (ProjectManager?.CurrentProject.DesignSurfaceLayout == "")
            {
                return;
            }

            var json = ProjectManager?.CurrentProject.DesignSurfaceLayout;

            if (json == null)
            {
                return;
            }

            Stopwatch sw = new();
            sw.Start();


            JsonSerializerOptions options = new()
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                IncludeFields = true,
                WriteIndented = true,
                NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
            };

            AddManuscriptGreekEnabled = true;
            AddManuscriptHebrewEnabled = true;
            var deserialized = JsonSerializer.Deserialize<ProjectDesignSurfaceSerializationModel>(json, options);


            //var toplevel = TopLevelProjectIds.Get

            // restore the nodes
            if (deserialized != null)
            {
                foreach (var corpusNode in deserialized.TokenizedCorpora)
                {
                    var corpus = new Corpus(
                        corpusId: new CorpusId(
                            corpusNode.CorpusId,
                            isRtl: corpusNode.IsRTL,
                            translationFontFamily: corpusNode.TranslationFontFamily,
                            name: corpusNode.Name,
                            displayName: "",
                            language: "",
                            paratextGuid: corpusNode.ParatextProjectId,
                            corpusType: corpusNode.CorpusType.ToString(),
                            metadata: new Dictionary<string, object>(),
                            created: new DateTimeOffset(),
                            userId: new UserId(ProjectManager!.CurrentUser.Id, ProjectManager.CurrentUser.FullName ?? string.Empty)));

                    var tokenization = corpusNode.Tokenizations[0].TokenizationName;
                    var tokenizer = (Tokenizers)Enum.Parse(typeof(Tokenizers), tokenization);

                    var node = CreateCorpusNode(corpus, new Point(corpusNode.X, corpusNode.Y), tokenizer);
                    node.Tokenizations = corpusNode.Tokenizations;

                    if (corpusNode.CorpusType == CorpusType.ManuscriptHebrew)
                    {
                        AddManuscriptHebrewEnabled = false;
                    }
                    else if (corpusNode.CorpusType == CorpusType.ManuscriptGreek)
                    {
                        AddManuscriptGreekEnabled = false;
                    }

                    // add in the menu
                    CreateCorpusNodeMenu(node);
                }

                // restore the connections
                foreach (var deserializedConnection in deserialized.ParallelCorpora)
                {
                    var sourceNode = DesignSurface.CorpusNodes.FirstOrDefault(p =>
                        p.ParatextProjectId == deserializedConnection.SourceConnectorId);
                    var targetNode = DesignSurface.CorpusNodes.FirstOrDefault(p =>
                        p.ParatextProjectId == deserializedConnection.TargetConnectorId);

                    if (sourceNode is not null && targetNode is not null)
                    {
                        var connection = new ConnectionViewModel
                        {
                            SourceConnector = sourceNode.OutputConnectors[0],
                            DestinationConnector = targetNode.InputConnectors[0],
                            TranslationSetInfo = deserializedConnection.TranslationSetInfo,
                            AlignmentSetInfo = deserializedConnection.AlignmentSetInfo,
                            ParallelCorpusDisplayName = deserializedConnection.ParallelCorpusDisplayName,
                            ParallelCorpusId = new ParallelCorpusId(Guid.Parse(deserializedConnection.ParallelCorpusId)),
                            SourceFontFamily = deserializedConnection.SourceFontFamily,
                            TargetFontFamily = deserializedConnection.TargetFontFamily,
                        };
                        DesignSurface.Connections.Add(connection);
                        // add in the context menu
                        CreateConnectionMenu(connection);
                    }
                }

                // restore the copora
                var corpora = deserialized.Corpora;
                foreach (var corpus in corpora)
                {
                    this.Corpora.Add(new Corpus(
                        corpusId: new CorpusId(
                            id: string.IsNullOrEmpty(corpus.CorpusId) ? Guid.NewGuid() : Guid.Parse(corpus.CorpusId),
                            isRtl: corpus.IsRtl,
                            translationFontFamily: corpus.TranslationFontFamily,
                            name: corpus.Name,
                            displayName: corpus.DisplayName,
                            language: corpus.Language,
                            paratextGuid: corpus.ParatextGuid,
                            corpusType: corpus.CorpusType,
                            metadata: new Dictionary<string, object>(),
                            created: corpus.Created ?? DateTimeOffset.Now,
                            userId: new UserId(corpus.UserId ?? Guid.NewGuid().ToString(), corpus.UserDisplayName ?? string.Empty))
                        ));
                }
            }

            sw.Stop();

            Debug.WriteLine($"LoadCanvas took {sw.ElapsedMilliseconds} ms ({sw.Elapsed.Seconds} seconds)");
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

            var task = _longRunningTaskManager.Create(taskName, LongRunningTaskStatus.Running);
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

                    corpus.CorpusId.TranslationFontFamily = ManuscriptIds.HebrewFontFamily;

                    OnUIThread(() => Corpora.Add(corpus));

                    OnUIThread(() =>
                    {
                        // figure out some offset based on the number of nodes already in the network
                        // so we don't overlap
                        var point = GetFreeSpot();
                        corpusNode = CreateCorpusNode(corpus, point, Tokenizers.WhitespaceTokenizer);
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
                        await UpdateNodeTokenization(corpusNode, corpus, tokenizedTextCorpus,
                            Tokenizers.WhitespaceTokenizer);
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
                        DeleteCorpusNode(corpusNode);
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
            var task = _longRunningTaskManager.Create(taskName, LongRunningTaskStatus.Running);
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

                CorpusNodeViewModel node = new();

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

                    corpus.CorpusId.TranslationFontFamily = ManuscriptIds.GreekFontFamily;

                    OnUIThread(() => Corpora.Add(corpus));

                    OnUIThread(() =>
                    {
                        // figure out some offset based on the number of nodes already in the network
                        // so we don't overlap
                        var point = GetFreeSpot();
                        node = CreateCorpusNode(corpus, point, Tokenizers.WhitespaceTokenizer);
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
                        await UpdateNodeTokenization(node, corpus, tokenizedTextCorpus, Tokenizers.WhitespaceTokenizer);
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
                        DeleteCorpusNode(node);
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
            var result = await _windowManager.ShowDialogAsync(dialogViewModel, null, DashboardProjectManager.NewProjectDialogSettings);

            if (result)
            {
                var metadata = dialogViewModel.SelectedProject;
                var taskName = $"{metadata.Name}";
                _busyState.Add(taskName, true);

                var task = _longRunningTaskManager.Create(taskName, LongRunningTaskStatus.Running);
                var cancellationToken = task.CancellationTokenSource!.Token;
                _ = await Task.Factory.StartNew(async () =>
                {
                    CorpusNodeViewModel node = new();
                    node.TranslationFontFamily = metadata.FontFamily;
                    
                    try
                    {
                        DAL.Alignment.Corpora.Corpus? corpus = null;
                       
                        // is this corpus already made for a different tokenization
                        foreach (var corpusNode in Corpora)
                        {
                            if (corpusNode.CorpusId.ParatextGuid == metadata.Id)
                            {
                                corpus = corpusNode;

                                // find the node on the design surface
                                foreach (var designSurfaceCorpusNode in DesignSurface.CorpusNodes)
                                {
                                    if (designSurfaceCorpusNode.ParatextProjectId == metadata.Id)
                                    {
                                        node = designSurfaceCorpusNode;
                                        break;
                                    }
                                }
                                break;
                            }
                        }

                        // first time for this corpus
                        if (corpus is null)
                        {
                            await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running,
                               description: $"Creating corpus '{metadata.Name}'...", cancellationToken: cancellationToken);
#pragma warning disable CS8604
                            corpus = await DAL.Alignment.Corpora.Corpus.Create(
                                 mediator: Mediator,
                                 IsRtl: metadata.IsRtl,

                                 Name: metadata.Name,

                                 Language: metadata.LanguageName,
                                 CorpusType: metadata.CorpusTypeDisplay,
                                 ParatextId: metadata.Id,
                                 token: cancellationToken);

                            corpus.CorpusId.TranslationFontFamily = metadata.FontFamily;
#pragma warning restore CS8604
                        }
                        OnUIThread(() =>
                        {
                                 Corpora.Add(corpus);
                                 var point = GetFreeSpot();
                                 node = CreateCorpusNode(corpus, point, dialogViewModel.SelectedTokenizers);
                             });


                        await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running,
                           description: $"Tokenizing and transforming '{metadata.Name}' corpus...", cancellationToken: cancellationToken);

                        var textCorpus = dialogViewModel.SelectedTokenizers switch
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
                           metadata.Name, dialogViewModel.SelectedTokenizers.ToString(), cancellationToken);
#pragma warning restore CS8604
                        await SendBackgroundStatus(taskName, LongRunningTaskStatus.Completed,
                           description: $"Creating tokenized text corpus for '{metadata.Name}' corpus...Completed", cancellationToken: cancellationToken);

                        Logger!.LogInformation("Sending TokenizedTextCorpusLoadedMessage via EventAggregator.");

                        OnUIThread(async () =>
                        {
                             await UpdateNodeTokenization(node, corpus, tokenizedTextCorpus, dialogViewModel.SelectedTokenizers);
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
                            DeleteCorpusNode(node);
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


        /// <summary>
        /// adds on the tokenizedtextid to the node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="corpus"></param>
        /// <param name="tokenizedTextCorpus"></param>
        /// <param name="viewModelSelectedTokenizers"></param>
        private async Task UpdateNodeTokenization(CorpusNodeViewModel node, DAL.Alignment.Corpora.Corpus corpus,
            TokenizedTextCorpus tokenizedTextCorpus, Tokenizers viewModelSelectedTokenizers)
        {
            var corpusNode = DesignSurface.CorpusNodes.FirstOrDefault(b => b.Id == node.Id);
            if (corpusNode is not null)
            {
                var nodeTokenization = corpusNode.Tokenizations.FirstOrDefault(b =>
                    b.TokenizationName == viewModelSelectedTokenizers.ToString());

                if (nodeTokenization is not null)
                {
                    nodeTokenization.IsSelected = false;
                    nodeTokenization.IsPopulated = true;
                    nodeTokenization.TokenizedTextCorpusId = tokenizedTextCorpus.TokenizedTextCorpusId.Id.ToString();
                    NotifyOfPropertyChange(() => DesignSurface.CorpusNodes);
                }
                else
                {
                    corpusNode.Tokenizations.Add(new SerializedTokenization
                    {
                        CorpusId = corpus.CorpusId.Id.ToString(),
                        TokenizationFriendlyName = EnumHelper.GetDescription(viewModelSelectedTokenizers),
                        IsSelected = false,
                        IsPopulated = true,
                        TokenizationName = viewModelSelectedTokenizers.ToString(),
                    });

                    // TODO the UI chip is not being updated with the new count...why?

                    //NotifyOfPropertyChange(() => corpusNode);
                    NotifyOfPropertyChange(() => DesignSurface.CorpusNodes);

                    // force a redraw
                    ProjectDesignSurface?.InvalidateVisual();
                }

                CreateCorpusNodeMenu(corpusNode);
                await SaveCanvas();
            }
        }


        /// <summary>
        /// creates the databound menu for the node
        /// </summary>
        /// <param name="corpusNode"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void CreateCorpusNodeMenu(CorpusNodeViewModel corpusNode)
        {
            // initiate the menu system
            corpusNode.MenuItems.Clear();

            BindableCollection<CorpusNodeMenuItemViewModel> nodeMenuItems = new();

            // restrict the ability of Manuscript to add new tokenizers
            if (corpusNode.CorpusType != CorpusType.ManuscriptHebrew || corpusNode.CorpusType != CorpusType.ManuscriptGreek)
            {
                // Add new tokenization
                nodeMenuItems.Add(new CorpusNodeMenuItemViewModel
                {
                    Header = LocalizationStrings.Get("Pds_AddNewTokenizationMenu", Logger),
                    Id = "AddTokenizationId",
                    IconKind = "BookTextAdd",
                    ProjectDesignSurfaceViewModel = this,
                    CorpusNodeViewModel = corpusNode,
                });
                nodeMenuItems.Add(new CorpusNodeMenuItemViewModel
                { Header = "", Id = "SeparatorId", ProjectDesignSurfaceViewModel = this, IsSeparator = true });
            }

            foreach (var nodeTokenization in corpusNode.Tokenizations)
            {
                nodeMenuItems.Add(new CorpusNodeMenuItemViewModel
                {
                    Header = nodeTokenization.TokenizationFriendlyName,
                    Id = nodeTokenization.TokenizedTextCorpusId,
                    IconKind = "Relevance",
                    MenuItems = new ObservableCollection<CorpusNodeMenuItemViewModel>
                    {
                        new CorpusNodeMenuItemViewModel
                        {
                            // Add Verses to focused enhanced view
                            Header = LocalizationStrings.Get("Pds_AddToEnhancedViewMenu", Logger),
                            Id = "AddToEnhancedViewId",
                            ProjectDesignSurfaceViewModel = this,
                            IconKind = "DocumentTextAdd",
                            CorpusNodeViewModel = corpusNode,
                            Tokenizer = nodeTokenization.TokenizationName,
                        },
                        new CorpusNodeMenuItemViewModel
                        {
                            // Show Verses in New Windows
                            Header = LocalizationStrings.Get("Pds_ShowVersesMenu", Logger),
                            Id = "ShowVerseId", ProjectDesignSurfaceViewModel = this,
                            IconKind = "DocumentText",
                            CorpusNodeViewModel = corpusNode,
                            Tokenizer = nodeTokenization.TokenizationName,
                        },
                        //new CorpusNodeMenuItemViewModel
                        //{
                        //    // Properties
                        //    Header = LocalizationStrings.Get("Pds_PropertiesMenu", Logger),
                        //    Id = "TokenizerPropertiesId",
                        //    ProjectDesignSurfaceViewModel = this,
                        //    IconKind = "Settings",
                        //    CorpusNodeViewModel = corpusNode,
                        //    Tokenizer = nodeTokenization.TokenizationName,
                        //}
                    }
                });
            }

            nodeMenuItems.Add(new CorpusNodeMenuItemViewModel
            {
                Header = "",
                Id = "SeparatorId",
                ProjectDesignSurfaceViewModel = this,
                IsSeparator = true
            });

            nodeMenuItems.Add(new CorpusNodeMenuItemViewModel
            {
                // Properties
                Header = LocalizationStrings.Get("Pds_PropertiesMenu", Logger),
                Id = "PropertiesId",
                IconKind = "Settings",
                CorpusNodeViewModel = corpusNode,
                ProjectDesignSurfaceViewModel = this
            });

            corpusNode.MenuItems = nodeMenuItems;
        }


        /// <summary>
        /// creates the data bound menu for the node
        /// </summary>
        /// <param name="connection"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void CreateConnectionMenu(ConnectionViewModel connection)
        {
            // initiate the menu system
            connection.MenuItems.Clear();

            BindableCollection<ParallelCorpusConnectionMenuItemViewModel> connectionMenuItems = new();

            // Add new alignment set
            connectionMenuItems.Add(new ParallelCorpusConnectionMenuItemViewModel
            {
                Header = LocalizationStrings.Get("Pds_CreateNewAlignmentSetMenu", Logger),
                Id = "CreateAlignmentSetId",
                IconKind = "BookTextAdd",
                ProjectDesignSurfaceViewModel = this,
                ConnectionId = connection.Id,
                ParallelCorpusId = connection.ParallelCorpusId.Id.ToString(),
                ParallelCorpusDisplayName = connection.ParallelCorpusDisplayName,
                IsRtl = connection.IsRtl,
                SourceParatextId = connection.SourceConnector.ParatextId,
                TargetParatextId = connection.DestinationConnector.ParatextId,
            }) ;
            connectionMenuItems.Add(new ParallelCorpusConnectionMenuItemViewModel
            { Header = "", Id = "SeparatorId", ProjectDesignSurfaceViewModel = this, IsSeparator = true });


            // ALIGNMENT SETS
            foreach (var alignmentSetInfo in connection.AlignmentSetInfo)
            {
                connectionMenuItems.Add(new ParallelCorpusConnectionMenuItemViewModel
                {
                    Header = alignmentSetInfo.DisplayName,
                    Id = alignmentSetInfo.AlignmentSetId,
                    IconKind = "Sitemap",
                    IsEnabled = true,
                    MenuItems = new ObservableCollection<ParallelCorpusConnectionMenuItemViewModel>
                    {
                        new ParallelCorpusConnectionMenuItemViewModel
                        {
                            // Add Verses to focused enhanced view
                            Header = LocalizationStrings.Get("Pds_AddConnectionToEnhancedViewMenu", Logger),
                            Id = "AddAlignmentToEnhancedViewId", 
                            ProjectDesignSurfaceViewModel = this,
                            IconKind = "DocumentTextAdd",
                            AlignmentSetId = alignmentSetInfo.AlignmentSetId,
                            DisplayName = alignmentSetInfo.DisplayName,
                            ParallelCorpusId = alignmentSetInfo.ParallelCorpusId,
                            ParallelCorpusDisplayName = alignmentSetInfo.ParallelCorpusDisplayName,
                            IsEnabled = true,
                            IsRtl = alignmentSetInfo.IsRtl,
                            IsTargetRTL = alignmentSetInfo.IsTargetRtl,
                            SourceParatextId = connection.SourceConnector.ParatextId,
                            TargetParatextId = connection.DestinationConnector.ParatextId,
                        },
                    }
                });
            }

            // TRANSLATION SET
            connectionMenuItems.Add(new ParallelCorpusConnectionMenuItemViewModel
            { Header = "", Id = "SeparatorId", ProjectDesignSurfaceViewModel = this, IsSeparator = true });
            connectionMenuItems.Add(new ParallelCorpusConnectionMenuItemViewModel
            { Header = "", Id = "SeparatorId", ProjectDesignSurfaceViewModel = this, IsSeparator = true });

            // Add new tokenization
            connectionMenuItems.Add(new ParallelCorpusConnectionMenuItemViewModel
            {
                Header = LocalizationStrings.Get("Pds_CreateNewInterlinear", Logger),
                Id = "CreateNewInterlinearId",
                IconKind = "BookTextAdd",
                ProjectDesignSurfaceViewModel = this,
                ConnectionId = connection.Id,
                Enabled = (connection.AlignmentSetInfo.Count > 0)
            });
            connectionMenuItems.Add(new ParallelCorpusConnectionMenuItemViewModel
            { Header = "", Id = "SeparatorId", ProjectDesignSurfaceViewModel = this, IsSeparator = true });


            foreach (var info in connection.TranslationSetInfo)
            {
                connectionMenuItems.Add(new ParallelCorpusConnectionMenuItemViewModel
                {
                    Header = info.DisplayName,
                    Id = info.TranslationSetId,
                    IconKind = "Relevance",
                    MenuItems = new ObservableCollection<ParallelCorpusConnectionMenuItemViewModel>
                        {
                            new ParallelCorpusConnectionMenuItemViewModel
                            {
                                // Add Verses to focused enhanced view
                                Header = LocalizationStrings.Get("Pds_AddConnectionToEnhancedViewMenu", Logger),
                                Id = "AddTranslationToEnhancedViewId", ProjectDesignSurfaceViewModel = this,
                                IconKind = "DocumentTextAdd",
                                TranslationSetId = info.TranslationSetId,
                                DisplayName = info.DisplayName,
                                ParallelCorpusId = info.ParallelCorpusId,
                                ParallelCorpusDisplayName = info.ParallelCorpusDisplayName,
                                IsRtl = info.IsRTL,
                                SourceParatextId = connection.SourceConnector.ParatextId,
                                TargetParatextId = connection.DestinationConnector.ParatextId,
                            }
                        }
                });
            }


            connectionMenuItems.Add(new ParallelCorpusConnectionMenuItemViewModel
            { Header = "", Id = "SeparatorId", ProjectDesignSurfaceViewModel = this, IsSeparator = true });

            //connectionMenuItems.Add(new ParallelCorpusConnectionMenuItemViewModel
            //{
            //    // Properties
            //    Header = LocalizationStrings.Get("Pds_PropertiesMenu", Logger),
            //    Id = "PropertiesId",
            //    IconKind = "Settings",
            //    ConnectionViewModel = connection,
            //    ProjectDesignSurfaceViewModel = this
            //});

            connection.MenuItems = connectionMenuItems;
        }

        public async Task ExecuteConnectionMenuCommand(ParallelCorpusConnectionMenuItemViewModel connectionMenuItem)
        {
            var connectionViewModel = connectionMenuItem.ConnectionViewModel;
            switch (connectionMenuItem.Id)
            {
                case "AddTranslationSetId":
                    // find the right connection to send
                    var connection = DesignSurface.Connections.First(c => c.Id == connectionMenuItem.ConnectionId);

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



        public async Task ExecuteCorpusNodeMenuCommand(CorpusNodeMenuItemViewModel corpusNodeMenuItem)
        {
            var corpusNodeViewModel = corpusNodeMenuItem.CorpusNodeViewModel;
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
                    // ShowTokenizationWindowMessage(string ParatextProjectId, string projectName, string TokenizationType, Guid corpusId, Guid tokenizedTextCorpusId);
                    var tokenization = corpusNodeViewModel.Tokenizations.FirstOrDefault(b => b.TokenizationName == corpusNodeMenuItem.Tokenizer);
                    if (tokenization == null)
                    {
                        return;
                    }

                    bool showInNewWindow = corpusNodeMenuItem.Id == "ShowVerseId";

                    var corpusId = Guid.Parse(tokenization.CorpusId);
                    var tokenizedTextCorpusId = Guid.Parse(tokenization.TokenizedTextCorpusId);
                    await EventAggregator.PublishOnUIThreadAsync(
                        new ShowTokenizationWindowMessage(
                            corpusNodeViewModel.ParatextProjectId,
                            ProjectName: corpusNodeViewModel.Name,
                            TokenizationType: corpusNodeMenuItem.Tokenizer,
                            CorpusId: corpusId,
                            TokenizedTextCorpusId: tokenizedTextCorpusId,
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
                    // get the selected tokenizer
                    var nodeTokenization =
                        corpusNodeViewModel.Tokenizations.FirstOrDefault(b =>
                            b.TokenizationName == corpusNodeMenuItem.Tokenizer);
#pragma warning disable CS8601
                    SelectedDesignSurfaceComponent = nodeTokenization;
#pragma warning restore CS8601
                    break;
            }
        }


        /// <summary>
        /// gets the position below the last node on the surface
        /// </summary>
        /// <returns></returns>
        private Point GetFreeSpot()
        {
            double x = 25;
            double y = 25;
            double yOffset = 0;

            foreach (var corpusNode in DesignSurface.CorpusNodes)
            {
                var positionX = corpusNode.X;
                var positionY = corpusNode.Y + corpusNode.Size.Height;
                yOffset = corpusNode.Size.Height;

                if (positionX > x && !double.IsNegativeInfinity(positionX) && !double.IsPositiveInfinity(positionX) && !double.IsNaN(positionX))
                {
                    x = positionX;
                }
                if (positionY > y && !double.IsNegativeInfinity(positionY) && !double.IsPositiveInfinity(positionY) && !double.IsNaN(positionY))
                {
                    y = positionY;
                }
            }

            if (double.IsNegativeInfinity(y) || double.IsPositiveInfinity(y) || double.IsNaN(y))
            {
                y = 150;
            }

            if (double.IsNegativeInfinity(x) || double.IsPositiveInfinity(x) || double.IsNaN(x))
            {
                x = 150;
            }

            return new Point(x, y + (yOffset * 0.5));
        }

        /// <summary>
        /// Called when the user has started to drag out a connector, thus creating a new connection.
        /// </summary>
        public ConnectionViewModel ConnectionDragStarted(ConnectorViewModel draggedOutConnector, Point curDragPoint)
        {
            //
            // Create a new connection to add to the view-model.
            //
            var connection = new ConnectionViewModel();

            if (draggedOutConnector.Type == ConnectorType.Output)
            {
                //
                // The user is dragging out a source connector (an output) and will connect it to a destination connector (an input).
                //
                connection.SourceConnector = draggedOutConnector;
                connection.DestConnectorHotspot = curDragPoint;
            }
            else
            {
                //
                // The user is dragging out a destination connector (an input) and will connect it to a source connector (an output).
                //
                connection.DestinationConnector = draggedOutConnector;
                connection.SourceConnectorHotspot = curDragPoint;
            }

            //
            // Add the new connection to the view-model.
            //
            DesignSurface.Connections.Add(connection);

            return connection;
        }

        /// <summary>
        /// Called to query the application for feedback while the user is dragging the connection.
        /// </summary>
        public void QueryConnectionFeedback(ConnectorViewModel draggedOutConnector, ConnectorViewModel draggedOverConnector, out object feedbackIndicator, out bool connectionOk)
        {
            if (draggedOutConnector == draggedOverConnector)
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
                var sourceConnector = draggedOutConnector;
                var destConnector = draggedOverConnector;

                //
                // Only allow connections from output connector to input connector (ie each
                // connector must have a different type).
                // Also only allocation from one node to another, never one node back to the same node.
                //
                connectionOk = sourceConnector.ParentNode != destConnector.ParentNode &&
                               sourceConnector.Type != destConnector.Type;

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
        public void ConnectionDragging(Point curDragPoint, ConnectionViewModel connection)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (connection is not null)
            {
                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                if (connection.DestinationConnector == null)
                {
                    connection.DestConnectorHotspot = curDragPoint;
                }
                else
                {
                    connection.SourceConnectorHotspot = curDragPoint;
                }
            }
        }

        /// <summary>
        /// Called when the user has finished dragging out the new connection.
        /// </summary>
        public async void ConnectionDragCompleted(ConnectionViewModel newConnection, ConnectorViewModel connectorDraggedOut, ConnectorViewModel connectorDraggedOver)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (connectorDraggedOver == null)
            {
                //
                // The connection was unsuccessful.
                // Maybe the user dragged it out and dropped it in empty space.
                //
                this.DesignSurface.Connections.Remove(newConnection);
                return;
            }

            //
            // Only allow connections from output connector to input connector (ie each
            // connector must have a different type).
            // Also only allocation from one node to another, never one node back to the same node.
            //
            var connectionOk = connectorDraggedOut.ParentNode != connectorDraggedOver.ParentNode &&
                               connectorDraggedOut.Type != connectorDraggedOver.Type;

            if (!connectionOk)
            {
                //
                // Connections between connectors that have the same type,
                // eg input -> input or output -> output, are not allowed,
                // Remove the connection.
                //
                DesignSurface.Connections.Remove(newConnection);
                return;
            }

            //
            // The user has dragged the connection on top of another valid connector.
            //

            //
            // Remove any existing connection between the same two connectors.
            //
            var existingConnection = FindConnection(connectorDraggedOut, connectorDraggedOver);
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (existingConnection != null)
            {
                DesignSurface.Connections.Remove(existingConnection);
            }

            //
            // Finalize the connection by attaching it to the connector
            // that the user dragged the mouse over.
            //
            bool added;
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (newConnection.DestinationConnector is null)
            {
                newConnection.DestinationConnector = connectorDraggedOver;
                added = true;
            }
            else
            {
                newConnection.SourceConnector = connectorDraggedOver;
                added = true;
            }

            if (added)
            {
                // check to see if we somehow didn't get a source/target id properly.  If so remove the line
                if (newConnection.SourceConnector.ParentNode.ParatextProjectId == "" || newConnection.SourceConnector.ParentNode.ParatextProjectId is null)
                {
                    DesignSurface.Connections.Remove(newConnection);
                    return;
                }

                if (newConnection.DestinationConnector.ParentNode.ParatextProjectId == "" || newConnection.DestinationConnector.ParentNode.ParatextProjectId is null)
                {
                    DesignSurface.Connections.Remove(newConnection);
                    return;
                }

                await EventAggregator.PublishOnUIThreadAsync(new ParallelCorpusAddedMessage(
                    SourceParatextId: newConnection.SourceConnector.ParentNode.ParatextProjectId,
                    TargetParatextId: newConnection.DestinationConnector.ParentNode.ParatextProjectId,
                    ConnectorGuid: newConnection.Id));
                
                var mainViewModel = IoC.Get<MainViewModel>();
                newConnection.SourceFontFamily = mainViewModel.GetFontFamilyFromParatextProjectId(newConnection.SourceConnector.ParentNode
                    .ParatextProjectId);

                newConnection.TargetFontFamily = mainViewModel.GetFontFamilyFromParatextProjectId(newConnection.DestinationConnector.ParentNode
                    .ParatextProjectId);

                await AddParallelCorpus(newConnection);
            }

            await SaveCanvas();
        }

      
        private async Task AddNewInterlinear(ParallelCorpusConnectionMenuItemViewModel connectionMenuItem)
        {
            var parameters = new List<Autofac.Core.Parameter>
            {
                new NamedParameter("parallelCorpusId", connectionMenuItem.ParallelCorpusId)
            };

            var dialogViewModel = LifetimeScope!.Resolve<InterlinearDialogViewModel>(parameters);
            var result = await _windowManager.ShowDialogAsync(dialogViewModel, null, DashboardProjectManager.NewProjectDialogSettings);

            if (result)
            {
                var translationSet = await TranslationSet.Create(null, dialogViewModel.SelectedAlignmentSet,
                        dialogViewModel.TranslationSetDisplayName, new Dictionary<string, object>(),
                        dialogViewModel.SelectedAlignmentSet.ParallelCorpusId, Mediator);

                if (translationSet != null)
                {
                    connectionMenuItem.ConnectionViewModel.TranslationSetInfo.Add(new TranslationSetInfo
                    {
                        DisplayName = translationSet.TranslationSetId.DisplayName,
                        TranslationSetId = translationSet.TranslationSetId.Id.ToString(),
                        ParallelCorpusDisplayName = translationSet.ParallelCorpusId.DisplayName,
                        ParallelCorpusId = translationSet.ParallelCorpusId.Id.ToString(),
                        AlignmentSetId = translationSet.AlignmentSetId.Id.ToString(),
                        AlignmentSetDisplayName = translationSet.AlignmentSetId.DisplayName
                    });

                    CreateConnectionMenu(connectionMenuItem.ConnectionViewModel);
                    await SaveCanvas();
                }
            }
        }

        public async Task AddParallelCorpus(ConnectionViewModel newConnection)
        {
            var sourceCorpusNode = DesignSurface.CorpusNodes.FirstOrDefault(b => b.Id == newConnection.SourceConnector.ParentNode.Id);
            if (sourceCorpusNode == null)
            {
                throw new MissingCorpusNodeException(
                    $"Cannot find the source Corpus node for the Corpus with Id '{newConnection.SourceConnector.ParentNode.CorpusId}'.");
            }
            var targetCorpusNode = DesignSurface.CorpusNodes.FirstOrDefault(b => b.Id == newConnection.DestinationConnector.ParentNode.Id);
            if (targetCorpusNode == null)
            {
                throw new MissingCorpusNodeException(
                    $"Cannot find the target Corpus node for the Corpus with Id '{newConnection.DestinationConnector.ParentNode.CorpusId}'.");
            }

            var sourceNodeTokenization = sourceCorpusNode.Tokenizations.FirstOrDefault();
            if (sourceNodeTokenization == null)
            {
                throw new MissingTokenizedTextCorpusIdException(
                    $"Cannot find the source TokenizedTextCorpusId associated to Corpus with Id '{newConnection.SourceConnector.ParentNode.CorpusId}'.");
            }
            var targetNodeTokenization = targetCorpusNode.Tokenizations.FirstOrDefault();
            if (targetNodeTokenization == null)
            {
                throw new MissingTokenizedTextCorpusIdException(
                    $"Cannot find the target TokenizedTextCorpusId associated to Corpus with Id '{newConnection.DestinationConnector.ParentNode.CorpusId}'.");
            }

            var parameters = new List<Autofac.Core.Parameter>
            {
                new NamedParameter("dialogMode", DialogMode.Add),
                new NamedParameter("connectionViewModel", newConnection),
                new NamedParameter("sourceCorpusNodeViewModel", sourceCorpusNode),
                new NamedParameter("targetCorpusNodeViewModel", targetCorpusNode)
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
                        newConnection.TranslationSetInfo.Add(new TranslationSetInfo
                        {
                            DisplayName = translationSet.TranslationSetId.DisplayName,
                            TranslationSetId = translationSet.TranslationSetId.Id.ToString(),
                            ParallelCorpusDisplayName = translationSet.ParallelCorpusId.DisplayName,
                            ParallelCorpusId = translationSet.ParallelCorpusId.Id.ToString(),
                            AlignmentSetId = translationSet.AlignmentSetId.Id.ToString(),
                            AlignmentSetDisplayName = translationSet.AlignmentSetId.DisplayName,
                            SourceFontFamily = newConnection.SourceFontFamily,
                            TargetFontFamily = newConnection.TargetFontFamily,
                        });
                    }

                    var alignmentSet = dialogViewModel.AlignmentSet;
                    if (alignmentSet != null)
                    {
                        newConnection.AlignmentSetInfo.Add(new AlignmentSetInfo
                        {
                            DisplayName = alignmentSet.AlignmentSetId.DisplayName,
                            AlignmentSetId = alignmentSet.AlignmentSetId.Id.ToString(),
                            ParallelCorpusDisplayName = alignmentSet.ParallelCorpusId.DisplayName,
                            ParallelCorpusId = alignmentSet.ParallelCorpusId.Id.ToString(),
                            IsRtl = newConnection.SourceConnector.ParentNode.IsRtl,
                            IsTargetRtl = newConnection.DestinationConnector.ParentNode.IsRtl
                        });
                    }

                    newConnection.ParallelCorpusId = dialogViewModel.ParallelTokenizedCorpus.ParallelCorpusId;
                    newConnection.ParallelCorpusDisplayName =
                        dialogViewModel.ParallelTokenizedCorpus.ParallelCorpusId.DisplayName;
                    CreateConnectionMenu(newConnection);

                }
                else
                {
                    DeleteConnection(newConnection);
                }
            }
            finally
            {
                await SaveCanvas();
            }

         
        }

        /// <summary>
        /// Retrieve a connection between the two connectors.
        /// Returns null if there is no connection between the connectors.
        /// </summary>
        private ConnectionViewModel FindConnection(ConnectorViewModel connector1, ConnectorViewModel connector2)
        {
            Trace.Assert(connector1.Type != connector2.Type);

            //
            // Figure out which one is the source connector and which one is the
            // destination connector based on their connector types.
            //
            var sourceConnector = connector1.Type == ConnectorType.Output ? connector1 : connector2;
            var destConnector = connector1.Type == ConnectorType.Output ? connector2 : connector1;

            //
            // Now we can just iterate attached connections of the source
            // and see if it each one is attached to the destination connector.
            //

            foreach (var connection in sourceConnector.AttachedConnections)
            {
                if (connection.DestinationConnector == destConnector)
                {
                    //
                    // Found a connection that is outgoing from the source connector
                    // and incoming to the destination connector.
                    //
                    return connection;
                }
            }

#pragma warning disable CS8603
            return null;
#pragma warning restore CS8603
        }

        /// <summary>
        /// Delete the currently selected nodes from the view-model.
        /// </summary>
        public void DeleteSelectedNodes()
        {
            // Take a copy of the selected nodes list so we can delete nodes while iterating.
            var nodesCopy = this.DesignSurface.CorpusNodes.ToArray();
            foreach (var node in nodesCopy)
            {
                if (node.IsSelected)
                {
                    DeleteCorpusNode(node);
                }
            }
        }

        /// <summary>
        /// Delete the node from the view-model.
        /// Also deletes any connections to or from the node.
        /// </summary>
        public void DeleteCorpusNode(CorpusNodeViewModel node)
        {
            OnUIThread(() =>
            {
                //
                // Remove all connections attached to the node.
                //
                DesignSurface.Connections.RemoveRange(node.AttachedConnections);

                //
                // Remove the node from the network.
                //
                DesignSurface.CorpusNodes.Remove(node);

                EventAggregator.PublishOnUIThreadAsync(new CorpusDeletedMessage(node.ParatextProjectId));
            });
            
        }

        /// <summary>
        /// Create a node and add it to the view-model.
        /// </summary>
        private CorpusNodeViewModel CreateCorpusNode(Corpus corpus, Point nodeLocation,
            Tokenizers tokenizers)
        {
            var node = new CorpusNodeViewModel(corpus.CorpusId.Name ?? string.Empty, EventAggregator, ProjectManager)
            {
                X = (double.IsNegativeInfinity(nodeLocation.X) || double.IsPositiveInfinity(nodeLocation.X) || double.IsNaN(nodeLocation.X)) ? 150 : nodeLocation.X,
                Y = (double.IsNegativeInfinity(nodeLocation.Y) || double.IsPositiveInfinity(nodeLocation.Y) || double.IsNaN(nodeLocation.Y)) ? 150 : nodeLocation.Y,
                CorpusType = (CorpusType)Enum.Parse(typeof(CorpusType), corpus.CorpusId.CorpusType),
                ParatextProjectId = corpus.CorpusId.ParatextGuid ?? string.Empty,
                CorpusId = corpus.CorpusId.Id,
                IsRtl = corpus.CorpusId.IsRtl,
                TranslationFontFamily = corpus.CorpusId.TranslationFontFamily ?? Corpus.DefaultTranslationFontFamily,
            };

            node.InputConnectors.Add(new ConnectorViewModel("Target", EventAggregator, node.ParatextProjectId)
            {
                Type = ConnectorType.Input
            });

            node.OutputConnectors.Add(new ConnectorViewModel("Source", EventAggregator, node.ParatextProjectId)
            {
                Type = ConnectorType.Output
            });


            node.Tokenizations.Add(new SerializedTokenization
            {
                CorpusId = corpus.CorpusId.Id.ToString(),
                TokenizationFriendlyName = EnumHelper.GetDescription(tokenizers),
                IsSelected = false,
                TokenizationName = tokenizers.ToString(),
                TokenizedTextCorpusId = corpus.CorpusId.TranslationFontFamily ?? Corpus.DefaultTranslationFontFamily,
            });

            //
            // Add the node to the view-model.
            //
            OnUIThread(() =>
            {
                DesignSurface.CorpusNodes.Add(node);
            });
          
            EventAggregator.PublishOnUIThreadAsync(new CorpusAddedMessage(node.ParatextProjectId));

            return node;
        }

        /// <summary>
        /// Utility method to delete a connection from the view-model.
        /// </summary>
        public void DeleteConnection(ConnectionViewModel connection)
        {
            EventAggregator.PublishOnUIThreadAsync(new ParallelCorpusDeletedMessage(
                SourceParatextId: connection.SourceConnector.ParentNode.ParatextProjectId,
                TargetParatextId: connection.DestinationConnector.ParentNode.ParatextProjectId,
                ConnectorGuid: connection.Id));

            DesignSurface.Connections.Remove(connection);
        }


   

        public void ShowCorpusProperties(object corpus)
        {
            SelectedDesignSurfaceComponent = corpus;
        }

        public void ShowConnectionProperties(ConnectionViewModel connection)
        {
            SelectedDesignSurfaceComponent = connection;
        }

        public void UiLanguageChangedMessage(UiLanguageChangedMessage message)
        {
            //var language = message.LanguageCode;

            // rerender the context menus
            foreach (var corpusNode in DesignSurface.CorpusNodes)
            {
                CreateCorpusNodeMenu(corpusNode);
            }
        }

        #endregion // Methods

      
    }
}
