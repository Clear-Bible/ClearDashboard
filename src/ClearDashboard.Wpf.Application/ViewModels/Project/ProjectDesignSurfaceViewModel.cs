using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.Application.ViewModels.Panes;
using ClearDashboard.Wpf.Application.ViewModels.Project;
using ClearDashboard.Wpf.Application.Views.Project;
using MediatR;
using Microsoft.Extensions.Logging;
using SIL.Machine.Corpora;
using SIL.Machine.Tokenization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ClearDashboard.Wpf.Application.Models;
using ClearDashboard.Wpf.Application.ViewModels.Shell;
using Corpus = ClearDashboard.DAL.Alignment.Corpora.Corpus;

namespace ClearDashboard.Wpf.Application.ViewModels
{
    #region Enums

    public enum Tokenizer
    {
        LatinSentenceTokenizer,
        LatinWordDetokenizer,
        LatinWordTokenizer,
        LineSegmentTokenizer,
        NullTokenizer,
        RegexTokenizer,
        StringDetokenizer,
        StringTokenizer,
        WhitespaceDetokenizer,
        WhitespaceTokenizer,
        ZwspWordDetokenizer,
        ZwspWordTokenizer
    }

    #endregion //Enums

    public class ProjectDesignSurfaceViewModel : ToolViewModel, IHandle<NodeSelectedChanagedMessage>,
        IHandle<ConnectionSelectedChanagedMessage>, IHandle<ProjectLoadCompleteMessage>
    {
        #region Member Variables
        CancellationTokenSource _cancellationTokenSource = null;
        private bool _addParatextCorpusRunning = false;

        public record CorporaLoadedMessage(IEnumerable<DAL.Alignment.Corpora.Corpus> Copora);
        public record TokenizedTextCorpusLoadedMessage(TokenizedTextCorpus TokenizedTextCorpus, ParatextProjectMetadata ProjectMetadata);

        private readonly INavigationService _navigationService;
        private readonly ILogger<ProjectDesignSurfaceViewModel> _logger;
        private readonly DashboardProjectManager _projectManager;
        private readonly IEventAggregator _eventAggregator;
        private readonly IMediator _mediator;
        private ObservableCollection<DAL.Alignment.Corpora.Corpus> _corpora;

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

        #endregion //Member Variables
        
        #region Public Variables

        public ProjectDesignSurfaceView View { get; set; }
        public Canvas DesignSurfaceCanvas { get; set; }

        #endregion //Public Variables
        
        #region Observable Properties

        public IWindowManager WindowManager { get; }

        public ObservableCollection<DAL.Alignment.Corpora.Corpus> Corpora
        {
            get => _corpora;
            set => _corpora = value;
        }

        /// <summary>
        /// This is the network that is displayed in the window.
        /// It is the main part of the view-model.
        /// </summary>
        public DesignSurfaceViewModel DesignSurface
        {
            get => _designSurface;
            set => Set(ref _designSurface, value);
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


        private ConnectionViewModel _selectedNode;
        public ConnectionViewModel SelectedConnection
        {
            get
            {
                foreach (var connection in DesignSurface.Connections)
                {
                    if (connection.IsSelected)
                    {
                        return connection;
                    }
                }
                return null;
            }
            set => Set(ref _selectedNode, value);
        }

        #endregion //Observable Properties
        
        #region Constructor
        public ProjectDesignSurfaceViewModel()
        {
            // Add some test data to the view-model.
            PopulateWithTestData();
        }

        public ProjectDesignSurfaceViewModel(IWindowManager windowManager, INavigationService navigationService,
            ILogger<ProjectDesignSurfaceViewModel> logger, DashboardProjectManager projectManager,
            IEventAggregator eventAggregator, IMediator mediator, ILifetimeScope lifetimeScope) 
            : base(navigationService, logger, projectManager, eventAggregator, mediator, lifetimeScope)
        {
            _navigationService = navigationService;
            _logger = logger;
            _projectManager = projectManager;
            _eventAggregator = eventAggregator;

            WindowManager = windowManager;
            _mediator = mediator;
            Title = "🖧 PROJECT DESIGN SURFACE";
            ContentId = "PROJECTDESIGNSURFACETOOL";

            Corpora = new ObservableCollection<DAL.Alignment.Corpora.Corpus>();
        }

        protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            //IsBusy = true;
            return base.OnInitializeAsync(cancellationToken);
        }

        protected override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            EventAggregator.SubscribeOnUIThread(this);

            //IsBusy = false;
            return base.OnActivateAsync(cancellationToken);
        }

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            SaveCanvas();
            
            //we need to cancel this process here
            //check a bool to see if it already cancelled or already completed
            if (_addParatextCorpusRunning)
            {
                _cancellationTokenSource.Cancel();
                EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
                {
                    Name = "Corpus",
                    Description = "Task was cancelled",
                    EndTime = DateTime.Now,
                    TaskStatus = StatusEnum.Completed
                }));
            }
            // todo - save the project
            return base.OnDeactivateAsync(close, cancellationToken);
        }

        protected override async void OnViewAttached(object view, object context)
        {
            if (View == null)
            {
                if (view is ProjectDesignSurfaceView projectDesignSurfaceView)
                {
                    View = (ProjectDesignSurfaceView)view;
                    DesignSurfaceCanvas = (Canvas)projectDesignSurfaceView.FindName("DesignSurfaceCanvas");
                }
            }

            //
            // Create a network, the root of the view-model.
            //
            DesignSurface = new DesignSurfaceViewModel(_navigationService, _logger as ILogger<DesignSurfaceViewModel>,
                _projectManager, _eventAggregator);

            if (_projectManager.CurrentProject.DesignSurfaceLayout != "" && _projectManager.CurrentProject.DesignSurfaceLayout is not null)
            {
                LoadCanvas();
            }

            base.OnViewAttached(view, context);
        }

        protected override async void OnViewLoaded(object view)
        {
            Console.WriteLine();
            base.OnViewLoaded(view);
        }

        protected override async void OnViewReady(object view)
        {
            Console.WriteLine();
            base.OnViewReady(view);
        }
        #endregion //Constructor

        #region Methods

        
        private async void SaveCanvas()
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

            JsonSerializerOptions options = new()
            {
                IncludeFields = true,
                WriteIndented = true
            };
            string jsonString = JsonSerializer.Serialize(surface, options);

            _projectManager.CurrentProject.DesignSurfaceLayout = jsonString;

            await _projectManager.UpdateProject(_projectManager.CurrentProject).ConfigureAwait(false);
        }

        private void LoadCanvas()
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
            ProjectDesignSurfaceSerializationModel deserialized = JsonSerializer.Deserialize<ProjectDesignSurfaceSerializationModel>(json, options);


            foreach (var corpusNode in deserialized.CorpusNodes)
            {
                CreateNode(corpusNode.Name, new Point(corpusNode.X, corpusNode.Y), false, corpusNode.CorpusType,
                    corpusNode.ParatextProjectId);
            }

            foreach (var deserializedConnection in deserialized.Connections)
            {
                var sourceNode = DesignSurface.CorpusNodes.FirstOrDefault(p =>
                    p.ParatextProjectId == deserializedConnection.SourceConnectorId);
                var targetNode = DesignSurface.CorpusNodes.FirstOrDefault(p =>
                    p.ParatextProjectId == deserializedConnection.TargetConnectorId);

                if (sourceNode is not null && targetNode is not null)
                {
                    var connection = new ConnectionViewModel();
                    connection.SourceConnector = sourceNode.OutputConnectors[0];
                    connection.DestinationConnector = targetNode.InputConnectors[0];
                    DesignSurface.Connections.Add(connection);
                }

            }

        }


        public void AddManuscriptCorpus()
        {
            Logger.LogInformation("AddParatextCorpus called.");
            CreateNode("Manuscript", new Point(25, 50), false, CorpusType.Manuscript, Guid.NewGuid().ToString());

        }

        public void AddUsfmCorpus()
        {
            Logger.LogInformation("AddParatextCorpus called.");
        }

        public async void AddParatextCorpus()
        {
            Logger.LogInformation("AddParatextCorpus called.");
            _addParatextCorpusRunning = true;
            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            await ProjectManager.InvokeDialog<AddParatextCorpusDialogViewModel, AddParatextCorpusDialogViewModel>(
                DashboardProjectManager.NewProjectDialogSettings, (Func<AddParatextCorpusDialogViewModel, Task<bool>>)Callback);

            async Task<bool> Callback(AddParatextCorpusDialogViewModel viewModel)
            {

                if (viewModel.SelectedProject != null)
                {
                    var metadata = viewModel.SelectedProject;
                    await Task.Factory.StartNew(async () =>
                    {
                        try
                        {
                            CopyOriginalDatabase();
                            //await EventAggregator.PublishOnCurrentThreadAsync(
                            //    new ProgressBarVisibilityMessage(true));


                            // if (viewModel.SelectedProject.HasProjectPath)
                            {

                                await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
                                {
                                    Name = "Corpus",
                                    Description = $"Creating corpus '{metadata.Name}'...",
                                    StartTime = DateTime.Now,
                                    TaskStatus = StatusEnum.Working
                                }));

                                var corpus = await DAL.Alignment.Corpora.Corpus.Create(ProjectManager.Mediator, metadata.IsRtl, metadata.Name, metadata.LanguageName,
                                    metadata.CorpusTypeDisplay, cancellationToken);
                                
                                OnUIThread(() => Corpora.Add(corpus));


                                OnUIThread(() =>
                                {
                                    //
                                    // Create some nodes and add them to the view-model.
                                    //
                                    CorpusType corpusType = CorpusType.Unknown;
                                    switch (viewModel.SelectedProject.CorpusType)
                                    {
                                        case CorpusType.Standard:
                                            corpusType = CorpusType.Standard;
                                            break;
                                        case CorpusType.BackTranslation:
                                            corpusType = CorpusType.BackTranslation;
                                            break;
                                        case CorpusType.Resource:
                                            corpusType = CorpusType.Resource;
                                            break;

                                        default:
                                            corpusType = CorpusType.Unknown;
                                            break;
                                    }

                                    corpus.ParatextGuid = viewModel.SelectedProject.Id;

                                    // figure out some offset based on the number of nodes already in the network
                                    // so we don't overlap
                                    var offset = DesignSurface.CorpusNodes.Count * 50;


                                    CreateNode(corpus.Name, new Point(150, 50 + offset), false, corpusType, corpus.ParatextGuid);
                                });

                                await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
                                {
                                    Name = "Corpus",
                                    Description = $"Tokenizing and transforming '{metadata.Name}' corpus...",
                                    StartTime = DateTime.Now,
                                    TaskStatus = StatusEnum.Working
                                }));

                                //var textCorpus = new ParatextTextCorpus(metadata.ProjectPath)
                                //    .Tokenize<LatinWordTokenizer>()
                                //    .Transform<IntoTokensTextRowProcessor>();

                                var textCorpus = (await ParatextProjectTextCorpus.Get(ProjectManager.Mediator, metadata.Id!, cancellationToken))
                                            .Tokenize<LatinWordTokenizer>()
                                            .Transform<IntoTokensTextRowProcessor>();

                                await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
                                {
                                    Name = "Corpus",
                                    Description = $"Creating tokenized text corpus for '{metadata.Name}' corpus...",
                                    StartTime = DateTime.Now,
                                    TaskStatus = StatusEnum.Working
                                }));

                                var tokenizedTextCorpus = await textCorpus.Create(ProjectManager.Mediator, corpus.CorpusId,
                                    metadata.Name, ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()", cancellationToken);


                                await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
                                {
                                    Name = "Corpus",
                                    Description = $"Creating tokenized text corpus for '{metadata.Name}' corpus...Completed",
                                    StartTime = DateTime.Now,
                                    TaskStatus = StatusEnum.Completed
                                }));

                                Logger.LogInformation("Sending TokenizedTextCorpusLoadedMessage via EventAggregator.");
                                await EventAggregator.PublishOnCurrentThreadAsync(
                                    new TokenizedTextCorpusLoadedMessage(tokenizedTextCorpus, metadata));
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex, $"An unexpected error occurred while creating the the corpus for {metadata.Name} ");
                            if (!cancellationToken.IsCancellationRequested)
                            {
                                await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(
                                    new BackgroundTaskStatus
                                    {
                                        Name = "Corpus",
                                        EndTime = DateTime.Now,
                                        ErrorMessage = $"{ex}",
                                        TaskStatus = StatusEnum.Error
                                    }));
                            }
                            else
                            {
                                RestoreOriginalDatabase();
                            }
                        }
                        finally
                        {
                            _cancellationTokenSource.Dispose();
                            DeleteOriginalDatabase();
                            _addParatextCorpusRunning = false;
                        }

                    });
                }
                // We don't want to navigate anywhere.
                return false;
            }
        }

        private void DeleteOriginalDatabase()
        {
            var projectName = ProjectManager.CurrentDashboardProject.ProjectName;
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var dashboardPath = Path.Combine(documentsPath, $"ClearDashboard_Projects");
            try
            {
                File.Delete(Path.Combine(dashboardPath, $"{projectName}_original.sqlite"));
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
        }

        private void RestoreOriginalDatabase()
        {
            var projectName = ProjectManager.CurrentDashboardProject.ProjectName;
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var dashboardPath = Path.Combine(documentsPath, $"ClearDashboard_Projects");
            var projectPath = Path.Combine(dashboardPath, projectName);
            try
            {
                if (ProjectManager != null)
                {
                    ProjectManager.ProjectNameDbContextFactory.ProjectAssets.ProjectDbContext.Database.EnsureDeleted();
                }

                File.Move(
                    Path.Combine(dashboardPath, $"{projectName}_original.sqlite"),
                    Path.Combine(projectPath, $"{projectName}.sqlite"));
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
        }

        private void CopyOriginalDatabase()
        {
            //make a copy of the database here named original_ProjectName.sqlite
            var projectName = ProjectManager.CurrentDashboardProject.ProjectName;
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var dashboardPath = Path.Combine(documentsPath, $"ClearDashboard_Projects");
            var projectPath = Path.Combine(dashboardPath, projectName);
            var filePath = Path.Combine(projectPath, $"{projectName}.sqlite");
            try
            {
                File.Copy(filePath, Path.Combine(dashboardPath, $"{projectName}_original.sqlite"));
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
        }

        public async Task HandleAsync(BackgroundTaskChangedMessage message, CancellationToken cancellationToken)
        {
            var incomingMessage = message.Status;

            if (incomingMessage.Name == "Corpus" && incomingMessage.TaskStatus == StatusEnum.CancelTaskRequested)
            {
                _cancellationTokenSource.Cancel();

                // return that your task was cancelled
                incomingMessage.EndTime = DateTime.Now;
                incomingMessage.TaskStatus = StatusEnum.Completed;
                incomingMessage.Description = "Task was cancelled";

                await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(incomingMessage));
            }

            await Task.CompletedTask;
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
            if (connection.DestinationConnector == null)
            {
                connection.DestConnectorHotspot = curDragPoint;
            }
            else
            {
                connection.SourceConnectorHotspot = curDragPoint;
            }
        }

        /// <summary>
        /// Called when the user has finished dragging out the new connection.
        /// </summary>
        public void ConnectionDragCompleted(ConnectionViewModel newConnection, ConnectorViewModel connectorDraggedOut, ConnectorViewModel connectorDraggedOver)
        {
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
            if (existingConnection != null)
            {
                DesignSurface.Connections.Remove(existingConnection);
            }

            //
            // Finalize the connection by attaching it to the connector
            // that the user dragged the mouse over.
            //
            bool added = false;
            if (newConnection.DestinationConnector == null)
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
                EventAggregator.PublishOnUIThreadAsync(new ParallelCorpusAddedMessage(
                    sourceParatextId: newConnection.SourceConnector.ParentNode.ParatextProjectId,
                    targetParatextId: newConnection.DestinationConnector.ParentNode.ParatextProjectId,
                    connectorGuid: newConnection.Id));
            }
        }

        /// <summary>
        /// Retrieve a connection between the two connectors.
        /// Returns null if there is no connection between the connectors.
        /// </summary>
        public ConnectionViewModel FindConnection(ConnectorViewModel connector1, ConnectorViewModel connector2)
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

            return null;
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
        public CorpusNodeViewModel CreateNode(string name, Point nodeLocation, bool centerNode,
            CorpusType corpusType, string projectId)
        {
            var node = new CorpusNodeViewModel(name, _eventAggregator, _projectManager)
            {
                X = nodeLocation.X,
                Y = nodeLocation.Y
            };

            node.CorpusType = corpusType;
            node.ParatextProjectId = projectId;

            node.InputConnectors.Add(new ConnectorViewModel("Target", _eventAggregator, _projectManager, node.ParatextProjectId)
            {
                Type = ConnectorType.Input
            });
            //node.InputConnectors.Add(new ConnectorViewModel("In2"));
            node.OutputConnectors.Add(new ConnectorViewModel("Source", _eventAggregator, _projectManager, node.ParatextProjectId)
            {
                Type = ConnectorType.Output
            });
            //node.OutputConnectors.Add(new ConnectorViewModel("Out2"));

            if (centerNode)
            {
                // 
                // We want to center the node.
                //
                // For this to happen we need to wait until the UI has determined the 
                // size based on the node's data-template.
                //
                // So we define an anonymous method to handle the SizeChanged event for a node.
                //
                // Note: If you don't declare sizeChangedEventHandler before initializing it you will get
                //       an error when you try and unsubscribe the event from within the event handler.
                //
                void SizeChangedEventHandler(object sender, EventArgs e)
                {
                    //
                    // This event handler will be called after the size of the node has been determined.
                    // So we can now use the size of the node to modify its position.
                    //
                    node.X -= node.Size.Width / 2;
                    node.Y -= node.Size.Height / 2;

                    //
                    // Don't forget to unhook the event, after the initial centering of the node
                    // we don't need to be notified again of any size changes.
                    //
                    node.SizeChanged -= SizeChangedEventHandler;
                }

                //
                // Now we hook the SizeChanged event so the anonymous method is called later
                // when the size of the node has actually been determined.
                //
                node.SizeChanged += SizeChangedEventHandler;
            }

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
                sourceParatextId: connection.SourceConnector.ParentNode.ParatextProjectId,
                targetParatextId: connection.DestinationConnector.ParentNode.ParatextProjectId,
                connectorGuid: connection.Id));

            DesignSurface.Connections.Remove(connection);
        }


        /// <summary>
        /// A function to conveniently populate the view-model with test data.
        /// </summary>
        private void PopulateWithTestData()
        {
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
        }

        public async Task HandleAsync(NodeSelectedChanagedMessage message, CancellationToken cancellationToken)
        {
            var node = message.Node as CorpusNodeViewModel;
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
        }

        public Task HandleAsync(ConnectionSelectedChanagedMessage message, CancellationToken cancellationToken)
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

        #endregion // Methods


    }
}
