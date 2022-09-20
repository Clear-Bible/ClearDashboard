using Autofac;
using Caliburn.Micro;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.SyntaxTree.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Models;
using ClearDashboard.Wpf.Application.ViewModels.Panes;
using ClearDashboard.Wpf.Application.ViewModels.ProjectDesignSurface;
using ClearDashboard.Wpf.Application.Views.Project;
using MediatR;
using Microsoft.Extensions.Logging;
using Serilog;
using SIL.Machine.Corpora;
using SIL.Machine.Tokenization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ClearDashboard.DataAccessLayer.Wpf.Infrastructure;
using ClearDashboard.Wpf.Application.Exceptions;
using ClearDashboard.Wpf.Application.ViewModels.Project.ParallelCorpusDialog;
using VerseMapping = ClearBible.Engine.Corpora.VerseMapping;

// ReSharper disable once CheckNamespace
namespace ClearDashboard.Wpf.Application.ViewModels.Project
{
    #region Enums

    public enum Tokenizer
    {
        //[Description("Latin Sentence Tokenizer")]
        //LatinSentenceTokenizer = 0,

        //[Description("Latin Word Detokenizer")]
        //LatinWordDetokenizer,

        [Description("Latin Word Tokenization")]
        LatinWordTokenizer,

        //[Description("Line Segment Tokenizer")]
        //LineSegmentTokenizer,

        //[Description("Null Tokenizer")]
        //NullTokenizer,

        //[Description("Regex Tokenizer")]
        //RegexTokenizer,

        //[Description("String Detokenizer")]
        //StringDetokenizer,

        //[Description("String Tokenizer")]
        //StringTokenizer,

        //[Description("Whitespace Detokenizer")]
        //WhitespaceDetokenizer,

        [Description("Whitespace Tokenization")]
        // ReSharper disable once UnusedMember.Global
        WhitespaceTokenizer,

        //[Description("Zwsp Word Detokenizer")]
        //ZwspWordDetokenizer,

        // ReSharper disable once UnusedMember.Global
        [Description("Zwsp Word Tokenization")]
        ZwspWordTokenizer
    }

    #endregion //Enums

    public class ProjectDesignSurfaceViewModel : ToolViewModel, IHandle<NodeSelectedChangedMessage>,
        IHandle<ConnectionSelectedChangedMessage>, IHandle<ProjectLoadCompleteMessage>, IHandle<CorpusDeletedMessage>,
        IHandle<UiLanguageChangedMessage>
    {
        #region Member Variables

        // ReSharper disable once RedundantDefaultMemberInitializer
        public CancellationTokenSource? CancellationTokenSource = null;
        public bool LongProcessRunning;

        //public record CorporaLoadedMessage(IEnumerable<DAL.Alignment.Corpora.Corpus> Copora);
        public record TokenizedTextCorpusLoadedMessage(TokenizedTextCorpus TokenizedTextCorpus, string TokenizationName, ParatextProjectMetadata? ProjectMetadata);

        private readonly INavigationService _navigationService;
        private readonly ILogger<ProjectDesignSurfaceViewModel> _logger;
        private readonly DashboardProjectManager? _projectManager;
        private readonly IEventAggregator? _eventAggregator;
        private readonly IMediator _mediator;
        private readonly IWindowManager _windowManager;
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

        private ProjectDesignSurfaceView View { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        private Canvas DesignSurfaceCanvas { get; set; }
        private Wpf.Controls.ProjectDesignSurface? ProjectDesignSurface { get; set; }


        #endregion //Member Variables


        #region Public Variables

        #endregion //Public Variables


        #region Observable Properties

        private ObservableCollection<DAL.Alignment.Corpora.Corpus> Corpora { get; set; }

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


        private object _selectedConnection;
        public object SelectedConnection
        {
            get
            {
                if (_selectedConnection is CorpusNodeViewModel node)
                {
                    foreach (var corpusNode in DesignSurface.CorpusNodes)
                    {
                        if (corpusNode.ParatextProjectId == node.ParatextProjectId)
                        {
                            return corpusNode;
                        }
                    }
                }
                else if (_selectedConnection is ConnectionViewModel conn)
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
                    return _selectedConnection;
                }

#pragma warning disable CS8603
                return null;
#pragma warning restore CS8603
            }
            set => Set(ref _selectedConnection, value);
        }

        private bool _addManuscriptEnabled = true;
        public bool AddManuscriptEnabled
        {
            get => _addManuscriptEnabled;
            set
            {
                _addManuscriptEnabled = value;
                NotifyOfPropertyChange(() => AddManuscriptEnabled);
            }
        }

        #endregion //Observable Properties

        #region Constructor
        // ReSharper disable once UnusedMember.Global
#pragma warning disable CS8618
        public ProjectDesignSurfaceViewModel()
#pragma warning restore CS8618
        {
            // Add some test data to the view-model.
            PopulateWithTestData();
        }

        // ReSharper disable once UnusedMember.Global
#pragma warning disable CS8618
        public ProjectDesignSurfaceViewModel(INavigationService navigationService,IWindowManager windowManager,
#pragma warning restore CS8618
            ILogger<ProjectDesignSurfaceViewModel> logger, DashboardProjectManager? projectManager,
            IEventAggregator? eventAggregator, IMediator mediator, ILifetimeScope lifetimeScope)
            : base(navigationService, logger, projectManager, eventAggregator, mediator, lifetimeScope)
        {
            _navigationService = navigationService;
            _windowManager = windowManager;
            _logger = logger;
            _projectManager = projectManager;
            _eventAggregator = eventAggregator;
            _mediator = mediator;

            Title = "🖧 PROJECT DESIGN SURFACE";
            ContentId = "PROJECTDESIGNSURFACETOOL";

            Corpora = new ObservableCollection<DAL.Alignment.Corpora.Corpus>();
        }

        //protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        //{
        //    //IsBusy = true;
        //    return base.OnInitializeAsync(cancellationToken);
        //}

        protected override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            EventAggregator.SubscribeOnUIThread(this);

            //IsBusy = false;
            return base.OnActivateAsync(cancellationToken);
        }

        protected override void OnViewAttached(object view, object context)
        {
            // NEVER IS CALLED NOW THAT WE ARE USING THIS AS A COMPONENT

            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (View == null)
            {
                if (view is ProjectDesignSurfaceView projectDesignSurfaceView)
                {
                    View = projectDesignSurfaceView;
                    // ReSharper disable once AssignNullToNotNullAttribute
                    DesignSurfaceCanvas = (Canvas)projectDesignSurfaceView.FindName("DesignSurfaceCanvas");

                    // ReSharper disable once AssignNullToNotNullAttribute
                    ProjectDesignSurface = (Wpf.Controls.ProjectDesignSurface)projectDesignSurfaceView.FindName("ProjectDesignSurface");
                }
            }

            //
            // Create a network, the root of the view-model.
            //
#pragma warning disable CS8604
            // ReSharper disable once SuspiciousTypeConversion.Global
            DesignSurface = new DesignSurfaceViewModel(_navigationService, _logger as ILogger<DesignSurfaceViewModel>,
#pragma warning restore CS8604
                _projectManager, _eventAggregator);

            base.OnViewAttached(view, context);
        }

        //protected override void OnViewLoaded(object view)
        //{
        //    // NEVER IS CALLED NOW THAT WE ARE USING THIS AS A COMPONENT
        //    if (_projectManager.CurrentProject.DesignSurfaceLayout != "" && _projectManager.CurrentProject.DesignSurfaceLayout is not null)
        //    {
        //        LoadCanvas();
        //    }
            
        //    base.OnViewLoaded(view);
        //}

        //protected override void OnViewReady(object view)
        //{
        //    // NEVER IS CALLED NOW THAT WE ARE USING THIS AS A COMPONENT
        //    if (_projectManager.CurrentProject.DesignSurfaceLayout != "" && _projectManager.CurrentProject.DesignSurfaceLayout is not null)
        //    {
        //        LoadCanvas();
        //    }

        //    base.OnViewReady(view);
        //}

        #endregion //Constructor

        #region Methods



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

        public async  Task SaveCanvas()
        {
            var surface = new ProjectDesignSurfaceSerializationModel();

            // save all the nodes
            foreach (var corpusNode in DesignSurface.CorpusNodes)
            {
                surface.CorpusNodes.Add(new SerializedNodes
                {
                    ParatextProjectId = corpusNode.ParatextProjectId,
                    CorpusType = corpusNode.CorpusType,
                    Name = corpusNode.Name,
                    X = corpusNode.X,
                    Y = corpusNode.Y,
                    NodeTokenizations = corpusNode.NodeTokenizations,
                });
            }

            // save all the connections
            foreach (var connection in DesignSurface.Connections)
            {
                surface.Connections.Add(new SerializedConnections
                {
                    SourceConnectorId = connection.SourceConnector.ParatextID,
                    TargetConnectorId = connection.DestinationConnector.ParatextID
                });
            }

            // save out the corpora
            foreach (var corpus in this.Corpora)
            {
                surface.Corpora.Add(new SerializedCopora
                {
                    CorpusId = corpus.CorpusId.Id.ToString(),
                    CorpusType = corpus.CorpusType,
                    Created = corpus.Created,
                    DisplayName = corpus.DisplayName,
                    IsRtl = corpus.IsRtl,
                    Language = corpus.Language,
                    Name = corpus.Name,
                    ParatextGuid = corpus.ParatextGuid,
                    UserId = corpus.UserId?.Id.ToString()
                });
            }

            JsonSerializerOptions options = new()
            {
                IncludeFields = true,
                WriteIndented = true
            };
            _projectManager.CurrentProject.DesignSurfaceLayout = JsonSerializer.Serialize(surface, options);

            try
            {
                await _projectManager.UpdateProject(_projectManager.CurrentProject).ConfigureAwait(false);
                await Task.Delay(250);
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, $"An unexpected error occurred while saving the project layout to the '{_projectManager.CurrentProject.ProjectName} database.");
            }
        }

        public void LoadCanvas()
        {
            // we have already loaded once
            if (DesignSurface.CorpusNodes.Count > 0)
            {
                return;
            }

            if (_projectManager.CurrentProject.DesignSurfaceLayout == "")
            {
                return;
            }

            var json = _projectManager.CurrentProject.DesignSurfaceLayout;

            if (json == null)
            {
                return;
            }

            JsonSerializerOptions options = new()
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                IncludeFields = true,
                WriteIndented = true
            };
            ProjectDesignSurfaceSerializationModel? deserialized = JsonSerializer.Deserialize<ProjectDesignSurfaceSerializationModel>(json, options);

            // restore the nodes
            if (deserialized != null)
            {
                foreach (var corpusNode in deserialized.CorpusNodes)
                {
                    var corpus = new DAL.Alignment.Corpora.Corpus(
                        corpusId: new CorpusId(Guid.NewGuid()),
                        mediator: _mediator,
                        isRtl: false,
                        name: corpusNode.Name,
                        displayName: "",
                        language: "",
                        paratextGuid: corpusNode.ParatextProjectId,
                        corpusType: corpusNode.CorpusType.ToString(),
                        metadata: new Dictionary<string, object>(),
                        created: new DateTimeOffset(),
                        userId: new UserId(_projectManager.CurrentUser.Id, _projectManager.CurrentUser.FullName ?? string.Empty));

                    var tokenization = corpusNode.NodeTokenizations[0].TokenizationName;
                    var tokenizer = (Tokenizer)Enum.Parse(typeof(Tokenizer), tokenization);

                    var node =CreateNode(corpus, new Point(corpusNode.X, corpusNode.Y), tokenizer);
                    node.NodeTokenizations = corpusNode.NodeTokenizations;
                    
                    if (corpusNode.CorpusType == CorpusType.Manuscript)
                    {
                        AddManuscriptEnabled = false;
                    }
                    
                    // add in the menu
                    CreateNodeMenu(node);
                }

                // restore the connections
                foreach (var deserializedConnection in deserialized.Connections)
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
                            DestinationConnector = targetNode.InputConnectors[0]
                        };
                        DesignSurface.Connections.Add(connection);
                    }
                }

                // restore the copora
                var corpora = deserialized.Corpora;
                foreach (var corpus in corpora)
                {
                    this.Corpora.Add(new DAL.Alignment.Corpora.Corpus(
                        corpusId: new CorpusId(corpus.CorpusId ?? Guid.NewGuid().ToString()),
                        mediator: _mediator,
                        isRtl: corpus.IsRtl,
                        name: corpus.Name,
                        displayName: corpus.DisplayName,
                        language: corpus.Language,
                        paratextGuid: corpus.ParatextGuid,
                        corpusType: corpus.CorpusType,
                        metadata: new Dictionary<string, object>(),
                        created: corpus.Created,
                        userId: new UserId(corpus.UserId ?? Guid.NewGuid().ToString(), corpus.UserDisplayName ?? string.Empty)
                    ));
                }
            }
        }

        // ReSharper disable once UnusedMember.Global
        public async void AddManuscriptCorpus()
        {
            _logger.LogInformation("AddParatextCorpus called.");

            //var corpus = new DAL.Alignment.Corpora.Corpus(corpusId: new CorpusId(Guid.NewGuid()), mediator: null,
            //    isRtl: false, name: "Manuscript", language: "Manuscript", paratextGuid: _projectManager.ManuscriptGuid,
            //    CorpusType.Manuscript, new Dictionary<string, object>());


            AddManuscriptEnabled = false;


            CancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = CancellationTokenSource.Token;


            var syntaxTree = new SyntaxTrees();
            var sourceCorpus = new SyntaxTreeFileTextCorpus(syntaxTree);

            BookInfo bookInfo = new BookInfo();
            var books = bookInfo.GenerateScriptureBookList();

            var metadata = new ParatextProjectMetadata
            {
                Id = _projectManager.ManuscriptGuid.ToString(),
                CorpusType = CorpusType.Manuscript,
                Name = "Manuscript",
                AvailableBooks = books,
            };


            _ = await Task.Factory.StartNew(async () =>
            {
               
                IsBusy = true;

                try
                {
                    var corpus = await DAL.Alignment.Corpora.Corpus.Create(
                        mediator: _projectManager.Mediator, 
                        IsRtl: false, 
                        Name: "Manuscript", 
                        Language: "Manuscript",
                        CorpusType: CorpusType.Manuscript.ToString(), 
                        ParatextId: _projectManager.ManuscriptGuid.ToString(), 
                        token: cancellationToken);

                    OnUIThread(() => Corpora.Add(corpus));

                    CorpusNodeViewModel node = new();

                    OnUIThread(() =>
                    {
                        // figure out some offset based on the number of nodes already in the network
                        // so we don't overlap
                        var point = GetFreeSpot();
                        node = CreateNode(corpus, point, Tokenizer.WhitespaceTokenizer);
                    });

                    await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
                    {
                        Name = "Corpus",
                        Description = $"Tokenizing and transforming '{metadata.Name}' corpus...",
                        StartTime = DateTime.Now,
                        TaskStatus = StatusEnum.Working
                    }), cancellationToken);

                    await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
                    {
                        Name = "Corpus",
                        Description = $"Creating tokenized text corpus for '{metadata.Name}' corpus...",
                        StartTime = DateTime.Now,
                        TaskStatus = StatusEnum.Working
                    }), cancellationToken);

                    var tokenizedTextCorpus = await sourceCorpus.Create(_projectManager.Mediator, corpus.CorpusId,
                        "Manuscript",
                        Tokenizer.WhitespaceTokenizer.ToString(),
                        cancellationToken);

                    await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
                    {
                        Name = "Corpus",
                        Description = $"Creating tokenized text corpus for '{metadata.Name}' corpus...Completed",
                        StartTime = DateTime.Now,
                        TaskStatus = StatusEnum.Completed
                    }), cancellationToken);

                    _logger.LogInformation("Sending TokenizedTextCorpusLoadedMessage via EventAggregator.");
                    await EventAggregator.PublishOnCurrentThreadAsync(
                        new TokenizedTextCorpusLoadedMessage(tokenizedTextCorpus, Tokenizer.WhitespaceTokenizer.ToString(), metadata), cancellationToken);

                    OnUIThread(() =>
                    {
                        UpdateNodeTokenization(node, corpus, tokenizedTextCorpus, Tokenizer.WhitespaceTokenizer);
                    });

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"An unexpected error occurred while creating the the corpus for {metadata.Name} ");
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(
                            new BackgroundTaskStatus
                            {
                                Name = "Corpus",
                                EndTime = DateTime.Now,
                                ErrorMessage = $"{ex}",
                                TaskStatus = StatusEnum.Error
                            }), cancellationToken);
                    }
                }
                finally
                {
                    CancellationTokenSource.Dispose();
                    LongProcessRunning = false;
                    IsBusy = false;
                }
            }, cancellationToken);


        }


        // ReSharper disable once UnusedMember.Global
        public void AddParatextCorpus()
        {
            AddParatextCorpus("");
        }

        // ReSharper disable once UnusedMember.Global
        private async void AddParatextCorpus(string selectedParatextProjectId = "")
        {
            _logger.LogInformation("AddParatextCorpus called.");
            LongProcessRunning = true;
            CancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = CancellationTokenSource.Token;

           
            
            await _projectManager.InvokeDialog<AddParatextCorpusDialogViewModel>(
                DashboardProjectManager.NewProjectDialogSettings, (Func<AddParatextCorpusDialogViewModel, Task<bool>>)Callback);

            async Task<bool> Callback(AddParatextCorpusDialogViewModel viewModel)
            {
                IsBusy = true;
                var metadata = viewModel.SelectedProject;

                _ = await Task.Factory.StartNew(async () =>
                {
                    try
                    {
                        DAL.Alignment.Corpora.Corpus? corpus = null;
                        CorpusNodeViewModel node = new();
                        // is this corpus already made for a different tokenization
                        foreach (var corpusNode in Corpora)
                        {
                            if (corpusNode.ParatextGuid == metadata.Id)
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
                            await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
                            {
                                Name = "Corpus",
                                Description = $"Creating corpus '{metadata.Name}'...",
                                StartTime = DateTime.Now,
                                TaskStatus = StatusEnum.Working
                            }), cancellationToken);


#pragma warning disable CS8604
                            corpus = await DAL.Alignment.Corpora.Corpus.Create(
                                mediator: _projectManager.Mediator,
                                IsRtl: metadata.IsRtl, 
                                Name: metadata.Name, 
                                Language: metadata.LanguageName,
                                CorpusType: metadata.CorpusTypeDisplay, 
                                ParatextId: metadata.Id, 
                                token: cancellationToken);
#pragma warning restore CS8604
                            OnUIThread(() => Corpora.Add(corpus));


                            OnUIThread(() =>
                            {
                                // figure out some offset based on the number of nodes already in the network
                                // so we don't overlap
                                var point = GetFreeSpot();
                                node = CreateNode(corpus, point, viewModel.SelectedTokenizer);
                            });
                        }

                        await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
                        {
                            Name = "Corpus",
                            Description = $"Tokenizing and transforming '{metadata.Name}' corpus...",
                            StartTime = DateTime.Now,
                            TaskStatus = StatusEnum.Working
                        }), cancellationToken);

                        ITextCorpus textCorpus;

                        switch (viewModel.SelectedTokenizer)
                        {
                            case Tokenizer.LatinWordTokenizer:
                                textCorpus = (await ParatextProjectTextCorpus.Get(_projectManager.Mediator, metadata.Id!, cancellationToken))
                                    .Tokenize<LatinWordTokenizer>()
                                    .Transform<IntoTokensTextRowProcessor>();
                                break;
                            case Tokenizer.WhitespaceTokenizer:
                                textCorpus = (await ParatextProjectTextCorpus.Get(_projectManager.Mediator, metadata.Id!, cancellationToken))
                                    .Tokenize<WhitespaceTokenizer>()
                                    .Transform<IntoTokensTextRowProcessor>();
                                break;
                            case Tokenizer.ZwspWordTokenizer:
                                textCorpus = (await ParatextProjectTextCorpus.Get(_projectManager.Mediator, metadata.Id!, cancellationToken))
                                    .Tokenize<ZwspWordTokenizer>()
                                    .Transform<IntoTokensTextRowProcessor>();
                                break;
                            default:
                                textCorpus = (await ParatextProjectTextCorpus.Get(_projectManager.Mediator, metadata.Id!, cancellationToken))
                                    .Tokenize<WhitespaceTokenizer>()
                                    .Transform<IntoTokensTextRowProcessor>();
                                break;
                        }

                        await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
                        {
                            Name = "Corpus",
                            Description = $"Creating tokenized text corpus for '{metadata.Name}' corpus...",
                            StartTime = DateTime.Now,
                            TaskStatus = StatusEnum.Working
                        }), cancellationToken);

#pragma warning disable CS8604
                        var tokenizedTextCorpus = await textCorpus.Create(_projectManager.Mediator, corpus.CorpusId,
                            metadata.Name, viewModel.SelectedTokenizer.ToString(), cancellationToken);
#pragma warning restore CS8604


                        await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
                        {
                            Name = "Corpus",
                            Description = $"Creating tokenized text corpus for '{metadata.Name}' corpus...Completed",
                            StartTime = DateTime.Now,
                            TaskStatus = StatusEnum.Completed
                        }), cancellationToken);

                        _logger.LogInformation("Sending TokenizedTextCorpusLoadedMessage via EventAggregator.");
                        await EventAggregator.PublishOnCurrentThreadAsync(
                            new TokenizedTextCorpusLoadedMessage(tokenizedTextCorpus, viewModel.SelectedTokenizer.ToString(), metadata), cancellationToken);

                        OnUIThread(() =>
                        {
                            UpdateNodeTokenization(node, corpus, tokenizedTextCorpus, viewModel.SelectedTokenizer);
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"An unexpected error occurred while creating the the corpus for {metadata.Name} ");
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(
                                new BackgroundTaskStatus
                                {
                                    Name = "Corpus",
                                    EndTime = DateTime.Now,
                                    ErrorMessage = $"{ex}",
                                    TaskStatus = StatusEnum.Error
                                }), cancellationToken);
                        }
                    }
                    finally
                    {
                        CancellationTokenSource.Dispose();
                        LongProcessRunning = false;
                        IsBusy = false;
                    }
                }, cancellationToken);


                // We don't want to navigate anywhere.
                return false;
            }
        }


        /// <summary>
        /// adds on the tokenizedtextid to the node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="corpus"></param>
        /// <param name="tokenizedTextCorpus"></param>
        /// <param name="viewModelSelectedTokenizer"></param>
        private void UpdateNodeTokenization(CorpusNodeViewModel node, DAL.Alignment.Corpora.Corpus corpus,
            TokenizedTextCorpus tokenizedTextCorpus, Tokenizer viewModelSelectedTokenizer)
        {
            var corpusNode = DesignSurface.CorpusNodes.FirstOrDefault(b => b.Id == node.Id);
            if (corpusNode is not null)
            {
                var nodeTokenization = corpusNode.NodeTokenizations.FirstOrDefault(b =>
                    b.TokenizationName == viewModelSelectedTokenizer.ToString());

                if (nodeTokenization is not null)
                {
                    nodeTokenization.IsSelected = false;
                    nodeTokenization.IsPopulated = true;
                    nodeTokenization.TokenizedTextCorpusId = tokenizedTextCorpus.TokenizedTextCorpusId.Id.ToString();
                    NotifyOfPropertyChange(() => DesignSurface.CorpusNodes);
                }
                else
                {
                    corpusNode.NodeTokenizations.Add(new NodeTokenization
                    {
                        CorpusId = corpus.CorpusId.Id.ToString(),
                        TokenizationFriendlyName = EnumHelper.GetDescription(viewModelSelectedTokenizer),
                        IsSelected = false,
                        IsPopulated = true,
                        TokenizationName = viewModelSelectedTokenizer.ToString(),
                    });
                    
                    // TODO the UI chip is not being updated with the new count...why?

                    //NotifyOfPropertyChange(() => corpusNode);
                    NotifyOfPropertyChange(() => DesignSurface.CorpusNodes);

                    // force a redraw
                    ProjectDesignSurface?.InvalidateVisual();
                }
                
                CreateNodeMenu(corpusNode);
            }
        }


        /// <summary>
        /// creates the databound menu for the node
        /// </summary>
        /// <param name="corpusNode"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void CreateNodeMenu(CorpusNodeViewModel corpusNode)
        {
            // initiate the menu system
            corpusNode.MenuItems.Clear();

            ObservableCollection<CorpusNodeMenuItemViewModel> nodeMenuItems = new();

            // restrict the ability of Manuscript to add new tokenizers
            if (corpusNode.CorpusType != CorpusType.Manuscript)
            {
                // Add new tokenization
                nodeMenuItems.Add(new CorpusNodeMenuItemViewModel { Header = LocalizationStrings.Get("Pds_AddNewTokenizationMenu", _logger), Id = "AddTokenizationId", IconKind = "BookTextAdd", ProjectDesignSurfaceViewModel = this, CorpusNodeViewModel = corpusNode, });
                nodeMenuItems.Add(new CorpusNodeMenuItemViewModel { Header = "", Id = "SeparatorId", ProjectDesignSurfaceViewModel = this, IsSeparator = true });
            }

            foreach (var nodeTokenization in corpusNode.NodeTokenizations)
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
                            Header = LocalizationStrings.Get("Pds_AddToEnhancedViewMenu", _logger), Id = "AddToEnhancedViewId", ProjectDesignSurfaceViewModel = this,
                            IconKind = "DocumentTextAdd", CorpusNodeViewModel = corpusNode,
                            Tokenizer = nodeTokenization.TokenizationName,
                        },
                        new CorpusNodeMenuItemViewModel
                        {
                            // Show Verses in New Windows
                            Header = LocalizationStrings.Get("Pds_ShowVersesMenu", _logger), Id = "ShowVerseId", ProjectDesignSurfaceViewModel = this, IconKind = "DocumentText",
                            CorpusNodeViewModel = corpusNode, Tokenizer = nodeTokenization.TokenizationName,
                        },
                        new CorpusNodeMenuItemViewModel
                        {
                            // Properties
                            Header = LocalizationStrings.Get("Pds_PropertiesMenu", _logger), Id = "TokenizerPropertiesId", ProjectDesignSurfaceViewModel = this, IconKind = "Settings",
                            CorpusNodeViewModel = corpusNode, Tokenizer = nodeTokenization.TokenizationName,
                        }
                    }
                });
            }

            nodeMenuItems.Add(new CorpusNodeMenuItemViewModel { Header = "", Id = "SeparatorId", ProjectDesignSurfaceViewModel = this, IsSeparator = true });

            nodeMenuItems.Add(new CorpusNodeMenuItemViewModel
            {
                // Properties
                Header = LocalizationStrings.Get("Pds_PropertiesMenu", _logger), 
                Id = "PropertiesId", 
                IconKind = "Settings",
                CorpusNodeViewModel = corpusNode,
                ProjectDesignSurfaceViewModel = this
            });
                
            corpusNode.MenuItems = nodeMenuItems;
        }

        public async Task ExecuteCorpusNodeMenuCommand(CorpusNodeMenuItemViewModel corpusNodeMenuItem)
        {
            var corpusNodeViewModel = corpusNodeMenuItem.CorpusNodeViewModel;
            switch (corpusNodeMenuItem.Id)
            {
                case "AddTokenizationId":
                    // kick off the add new tokenization dialog
                    AddParatextCorpus(corpusNodeViewModel.ParatextProjectId);
                    break;
                case "SeparatorId":
                    // no-op
                    break;
                
                case "AddToEnhancedViewId":
                case "ShowVerseId":
                    // ShowTokenizationWindowMessage(string ParatextProjectId, string projectName, string TokenizationType, Guid corpusId, Guid tokenizedTextCorpusId);
                    var tokenization = corpusNodeViewModel.NodeTokenizations.FirstOrDefault(b => b.TokenizationName == corpusNodeMenuItem.Tokenizer);
                    if (tokenization == null)
                    {
                        return;
                    }

                    bool showInNewWindow = corpusNodeMenuItem.Id == "ShowVerseId";

                    var corpusId = Guid.Parse(tokenization.CorpusId);
                    var tokenizationId = Guid.Parse(tokenization.TokenizedTextCorpusId);
                    await EventAggregator.PublishOnUIThreadAsync(
                        new ShowTokenizationWindowMessage(ParatextProjectId: corpusNodeViewModel.ParatextProjectId,
                            ProjectName: corpusNodeViewModel.Name,
                            TokenizationType: corpusNodeMenuItem.Tokenizer,
                            CorpusId: corpusId,
                            TokenizedTextCorpusId: tokenizationId,
                            IsNewWindow: showInNewWindow));
                    break;
                case "PropertiesId":
                    // node properties
                    SelectedConnection = corpusNodeViewModel;
                    break;
                case "TokenizerPropertiesId":
                    // get the selected tokenizer
                    var nodeTokenization =
                        corpusNodeViewModel.NodeTokenizations.FirstOrDefault(b =>
                            b.TokenizationName == corpusNodeMenuItem.Tokenizer);
#pragma warning disable CS8601
                    SelectedConnection = nodeTokenization;
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

                if (positionX > x)
                {
                    x = positionX;
                }
                if (positionY > y)
                {
                    y = positionY;
                }
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
                await EventAggregator.PublishOnUIThreadAsync(new ParallelCorpusAddedMessage(
                    SourceParatextId: newConnection.SourceConnector.ParentNode.ParatextProjectId,
                    TargetParatextId: newConnection.DestinationConnector.ParentNode.ParatextProjectId,
                    ConnectorGuid: newConnection.Id));

                // TODO:  
                await AddParallelCorpus(newConnection);
            }
        }

        public async Task AddParallelCorpus(ConnectionViewModel newConnection)
        {
            //await _projectManager.InvokeDialog<ParallelCorpusDialogViewModel>(
            //    DashboardProjectManager.NewProjectDialogSettings, (Func<ParallelCorpusDialogViewModel, Task>)Callback, DialogMode.Add);
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

            var sourceNodeTokenization = sourceCorpusNode.NodeTokenizations.FirstOrDefault();
            if (sourceNodeTokenization == null)
            {
                throw new MissingTokenizedTextCorpusIdException(
                    $"Cannot find the source TokenizedTextCorpusId associated to Corpus with Id '{newConnection.SourceConnector.ParentNode.CorpusId}'.");
            }
            var targetNodeTokenization = targetCorpusNode.NodeTokenizations.FirstOrDefault();
            if (sourceNodeTokenization == null)
            {
                throw new MissingTokenizedTextCorpusIdException(
                    $"Cannot find the target TokenizedTextCorpusId associated to Corpus with Id '{newConnection.DestinationConnector.ParentNode.CorpusId}'.");
            }

            var dialogViewModel = IoC.Get<ParallelCorpusDialogViewModel>();

            dialogViewModel.ParallelCorpus.DisplayName = $"{sourceCorpusNode.Name} - {targetCorpusNode.Name}";

            if (dialogViewModel is IDialog dialog)
            {
                dialog.DialogMode = DialogMode.Add;
            }

            var success = await _windowManager.ShowDialogAsync(dialogViewModel, null, DashboardProjectManager.NewProjectDialogSettings);

            if (success)
            {

                IsBusy = true;
                CancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = CancellationTokenSource.Token;

                var parallelCorpus = dialogViewModel.ParallelCorpus;

                _ = await Task.Factory.StartNew(async () =>
                {
                    try
                    {
                          _logger.LogInformation($"Retrieving tokenized source and target corpora for '{parallelCorpus.DisplayName}'.");
                        await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
                        {
                            Name = "ParallelCorpus",
                            Description = $"Retrieving tokenized source and target corpora for '{parallelCorpus.DisplayName}'...",
                            StartTime = DateTime.Now,
                            TaskStatus = StatusEnum.Working
                        }), cancellationToken);


                        var sourceTokenizedTextCorpus = await TokenizedTextCorpus.Get(Mediator, new TokenizedTextCorpusId(sourceNodeTokenization.TokenizedTextCorpusId));
                        var targetTokenizedTextCorpus = await TokenizedTextCorpus.Get(Mediator, new TokenizedTextCorpusId(targetNodeTokenization.TokenizedTextCorpusId));

                        _logger.LogInformation($"Aligning rows between target and source corpora");
                        await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
                        {
                            Name = "ParallelCorpus",
                            Description = $"Aligning rows for '{parallelCorpus.DisplayName}' between target and source corpora...",
                            StartTime = DateTime.Now,
                            TaskStatus = StatusEnum.Working
                        }), cancellationToken);
                        var parallelTextCorpus = await Task.Run(async () => sourceTokenizedTextCorpus.EngineAlignRows(targetTokenizedTextCorpus, new List<VerseMapping>()), cancellationToken);


                        _logger.LogInformation($"Creating the ParallelCorpus '{parallelCorpus.DisplayName}'");
                        await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
                        {
                            Name = "ParallelCorpus",
                            Description = $"Creating  ParallelCorpus '{parallelCorpus.DisplayName}'...",
                            StartTime = DateTime.Now,
                            TaskStatus = StatusEnum.Working
                        }), cancellationToken);
                        var parallelTokenizedCorpus = await parallelTextCorpus.Create(parallelCorpus.DisplayName, Mediator!);


                        await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
                        {
                            Name = "ParallelCorpus",
                            Description =  $"Completed creation of  ParallelCorpus '{parallelCorpus.DisplayName}'.",
                            StartTime = DateTime.Now,
                            TaskStatus = StatusEnum.Completed
                        }), cancellationToken);
                        _logger.LogInformation($"Completed creating the ParallelCorpus '{parallelCorpus.DisplayName}'");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"An unexpected error occurred while creating the ParallelCorpus.");
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
                    }
                    finally
                    {
                        CancellationTokenSource.Dispose();
                        LongProcessRunning = false;
                        IsBusy = false;
                    }

                }, cancellationToken);
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
                    DeleteNode(node);
                }
            }
        }

        /// <summary>
        /// Delete the node from the view-model.
        /// Also deletes any connections to or from the node.
        /// </summary>
        public void DeleteNode(CorpusNodeViewModel node)
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
        }

        /// <summary>
        /// Create a node and add it to the view-model.
        /// </summary>
        private CorpusNodeViewModel CreateNode(DAL.Alignment.Corpora.Corpus corpus, Point nodeLocation, 
            Tokenizer tokenizer)
        {
            var node = new CorpusNodeViewModel(corpus.Name ?? string.Empty, _eventAggregator, _projectManager)
            {
                X = nodeLocation.X,
                Y = nodeLocation.Y,
                CorpusType = (CorpusType)Enum.Parse(typeof(CorpusType), corpus.CorpusType),
                ParatextProjectId = corpus.ParatextGuid ?? string.Empty,
                CorpusId = corpus.CorpusId.Id
            };

            node.InputConnectors.Add(new ConnectorViewModel("Target", _eventAggregator, _projectManager, node.ParatextProjectId)
            {
                Type = ConnectorType.Input
            });

            node.OutputConnectors.Add(new ConnectorViewModel("Source", _eventAggregator, _projectManager, node.ParatextProjectId)
            {
                Type = ConnectorType.Output
            });


            node.NodeTokenizations.Add(new NodeTokenization
            {
                CorpusId = corpus.CorpusId.Id.ToString(),
                TokenizationFriendlyName = EnumHelper.GetDescription(tokenizer),
                IsSelected = false,
                TokenizationName = tokenizer.ToString(),
            });

            //
            // Add the node to the view-model.
            //
            DesignSurface.CorpusNodes.Add(node);
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


        /// <summary>
        /// A function to conveniently populate the view-model with test data.
        /// </summary>
        private void PopulateWithTestData()
        {
            /*
            
            //
            // Create a network, the root of the view-model.
            //
            DesignSurface = new DesignSurfaceViewModel(_navigationService, _logger as ILogger<DesignSurfaceViewModel>,
                _projectManager, _eventAggregator);

            //
            // Create some nodes and add them to the view-model.
            //
            var node1 = CreateNode("zz_SUR", new Point(100, 60), false, CorpusType.Standard, Guid.NewGuid().ToString());
            var node2 = CreateNode("zz_SURBT", new Point(350, 40), false, CorpusType.BackTranslation, Guid.NewGuid().ToString());
            var node3 = CreateNode("NIV", new Point(350, 120), false, CorpusType.Resource, Guid.NewGuid().ToString());



            //
            // Create a connection between the standard / back translation.
            //
            var connection = new ConnectionViewModel
            {
                SourceConnector = node1.OutputConnectors[0],
                DestinationConnector = node2.InputConnectors[0]
            };

            //
            // Add the connection to the view-model.
            //
            DesignSurface.Connections.Add(connection);


            connection = new ConnectionViewModel
            {
                SourceConnector = node1.OutputConnectors[0],
                DestinationConnector = node3.InputConnectors[0]
            };
            DesignSurface.Connections.Add(connection);

            */
        }

        public void ShowCorpusProperties(object corpus)
        {
            SelectedConnection = corpus;
        }

        public void ShowConnectionProperties(ConnectionViewModel connection)
        {
            SelectedConnection = connection;
        }

        public void UiLanguageChangedMessage(UiLanguageChangedMessage message)
        {
            var language = message.LanguageCode;

            // rerender the context menus
            foreach (var corpusNode in DesignSurface.CorpusNodes)
            {
                CreateNodeMenu(corpusNode);
            }
        }



        public async Task HandleAsync(BackgroundTaskChangedMessage message, CancellationToken cancellationToken)
        {
            var incomingMessage = message.Status;

            if (incomingMessage.Name == "Corpus" && incomingMessage.TaskStatus == StatusEnum.CancelTaskRequested)
            {
                CancellationTokenSource?.Cancel();

                // return that your task was cancelled
                incomingMessage.EndTime = DateTime.Now;
                incomingMessage.TaskStatus = StatusEnum.Completed;
                incomingMessage.Description = "Task was cancelled";

                await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(incomingMessage), cancellationToken);
            }

            await Task.CompletedTask;
        }

        public Task HandleAsync(NodeSelectedChangedMessage message, CancellationToken cancellationToken)
        {
            //var node = message.Node as CorpusNodeViewModel;

            //if (node is null)
            //{
            //    return;
            //}

            //var connection = node.AttachedConnections.Where(c => c.IsSelected).ToList();
            //if (connection.Count > 0)
            //{
            //    SelectedConnection = connection[0];
            //}
            //else
            //{
            //    SelectedConnection = null;
            //}

            return Task.CompletedTask;
        }

        public Task HandleAsync(ConnectionSelectedChangedMessage message, CancellationToken cancellationToken)
        {
            var guid = message.ConnectorId;

            foreach (var node in DesignSurface.CorpusNodes)
            {
                foreach (var connection in node.AttachedConnections)
                {
                    if (connection.Id == guid)
                    {
                        node.IsSelected = true;
                        connection.IsSelected = true;
                        SelectedConnection = connection;
                    }
                    else
                    {
                        connection.IsSelected = false;
                    }
                }
            }


            //var nodes = DesignSurface.CorpusNodes.Where(b => b.IsSelected).ToList();
            //for (int i = 0; i < nodes.Count; i++)
            //{
            //    Debug.WriteLine($"{i} {nodes[i].Name}");
            //}


            return Task.CompletedTask;
        }

        public Task HandleAsync(ProjectLoadCompleteMessage message, CancellationToken cancellationToken)
        {
            if (_projectManager.CurrentProject is not null)
            {
                LoadCanvas();
            }
            return Task.CompletedTask;
        }


        /// <summary>
        /// A corpus has been removed from the database - check to see if it is the Manuscript so
        /// we can enable the UI button.  Also delete the corpus for the database
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task HandleAsync(CorpusDeletedMessage message, CancellationToken cancellationToken)
        {
            //var paratextId = message.paratextId;
            // TODO delete database corpus using the paratextId


            foreach (var node in DesignSurface.CorpusNodes)
            {
                if (node.CorpusType == CorpusType.Manuscript)
                {
                    AddManuscriptEnabled = false;
                    return Task.CompletedTask;
                }
            }

            AddManuscriptEnabled = true;
            return Task.CompletedTask;
        }

        
        /// <summary>
        /// The UI language has changed
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task HandleAsync(UiLanguageChangedMessage message, CancellationToken cancellationToken)
        {
            // TODO - update the UI language
            var language = message.LanguageCode;

            // rerender the context menus
            foreach (var corpusNode in DesignSurface.CorpusNodes)
            {
                CreateNodeMenu(corpusNode);
            }



            return Task.CompletedTask;
        }

        #endregion // Methods
    }
}
