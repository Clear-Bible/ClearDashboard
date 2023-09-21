using Autofac;
using Caliburn.Micro;
using ClearBible.Engine.Exceptions;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Paratext;
using ClearDashboard.ParatextPlugin.CQRS.Features.AllProjects;
using ClearDashboard.ParatextPlugin.CQRS.Features.Project;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Messages;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.Project;
using ClearDashboard.Wpf.Controls;
using ClearDashboard.Wpf.Controls.Utils;
using MahApps.Metro.IconPacks;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Corpus = ClearDashboard.DAL.Alignment.Corpora.Corpus;
using TopLevelProjectIds = ClearDashboard.DAL.Alignment.TopLevelProjectIds;

namespace ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface
{


    /// <summary>
    /// Defines a design surface with nodes and connections between the nodes.
    /// </summary>
    public partial class DesignSurfaceViewModel : Screen
    {

        #region Internal Data Members

        /// <summary>
        /// The collection of nodes in the network.
        /// </summary>
        private ImpObservableCollection<CorpusNodeViewModel>? _nodes;

        /// <summary>
        /// The collection of connections in the network.
        /// </summary>
        private ImpObservableCollection<ParallelCorpusConnectionViewModel>? _connections;

        #endregion Internal Data Members


        protected ILogger<DesignSurfaceViewModel>? Logger { get; }

        protected ILifetimeScope LifetimeScope { get; }
        protected IEventAggregator? EventAggregator { get; }
        protected IMediator Mediator { get; }
        private readonly ParatextProxy _paratextProxy;
        protected readonly ILocalizationService LocalizationService;


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


        public IProjectDesignSurfaceViewModel ProjectDesignSurfaceViewModel => (IProjectDesignSurfaceViewModel)Parent;
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

        /// <summary>
        /// This is the design surface that is displayed in the window.
        /// It is the main part of the view-model.
        /// </summary>
        ///
        public Wpf.Controls.ProjectDesignSurface? ProjectDesignSurface { get; set; }

        /// <summary>
        /// The collection of nodes in the network.
        /// </summary>
        public ImpObservableCollection<CorpusNodeViewModel> CorpusNodes => _nodes ??= new ImpObservableCollection<CorpusNodeViewModel>();

        /// <summary>
        /// The collection of connections in the network.
        /// </summary>
        public ImpObservableCollection<ParallelCorpusConnectionViewModel> ParallelCorpusConnections
        {
            get
            {
                if (_connections == null)
                {
                    _connections = new ImpObservableCollection<ParallelCorpusConnectionViewModel>();
                    _connections.ItemsRemoved += OnConnectionsItemsRemoved;
                    _connections.ItemsSelected += OnConnectionsItemsSelected;
                }

                return _connections;
            }
        }

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            Logger!.LogInformation("DesignSurfaceViewModel - OnDeactivateAsync called.");
            return base.OnDeactivateAsync(close, cancellationToken);
        }

        protected override void OnViewLoaded(object view)
        {
            if (ProjectDesignSurface == null)
            {
                if (view is UserControl projectDesignSurfaceView)
                {
                    // ReSharper disable once AssignNullToNotNullAttribute
                    ProjectDesignSurface = (Wpf.Controls.ProjectDesignSurface)projectDesignSurfaceView.FindName("ProjectDesignSurface");

                }
            }
            base.OnViewLoaded(view);
        }

        protected override void OnViewReady(object view)
        {
            if (ProjectDesignSurface == null)
            {
                if (view is UserControl projectDesignSurfaceView)
                {
                    // ReSharper disable once AssignNullToNotNullAttribute
                    ProjectDesignSurface = (Wpf.Controls.ProjectDesignSurface)projectDesignSurfaceView.FindName("ProjectDesignSurface");

                }
            }
            base.OnViewReady(view);
        }

        protected override void OnViewAttached(object view, object context)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (ProjectDesignSurface == null)
            {
                if (view is UserControl projectDesignSurfaceView)
                {
                    // ReSharper disable once AssignNullToNotNullAttribute
                    ProjectDesignSurface = (Wpf.Controls.ProjectDesignSurface)projectDesignSurfaceView.FindName("ProjectDesignSurface");

                }
            }
            base.OnViewAttached(view, context);
        }


        #region ctor

        public DesignSurfaceViewModel(
            ILogger<DesignSurfaceViewModel>? logger,
            IEventAggregator? eventEventAggregator,
            ILifetimeScope lifetimeScope,
            IMediator mediator,
            ParatextProxy paratextProxy,
            ILocalizationService localizationService)
        {
            Logger = logger;
            EventAggregator = eventEventAggregator;
            LifetimeScope = lifetimeScope;
            Mediator = mediator;
            _paratextProxy = paratextProxy;
            LocalizationService = localizationService;
        }
        #endregion


        /// <summary>
        /// Retrieve a connection between the two connectors.
        /// Returns null if there is no connection between the connectors.
        /// </summary>
        public ParallelCorpusConnectionViewModel FindConnection(ParallelCorpusConnectorViewModel connector1, ParallelCorpusConnectorViewModel connector2)
        {
            Trace.Assert(connector1.ConnectorType != connector2.ConnectorType);

            //
            // Figure out which one is the source connector and which one is the
            // destination connector based on their connector types.
            //
            var sourceConnector = connector1.ConnectorType == ConnectorType.Output ? connector1 : connector2;
            var destConnector = connector1.ConnectorType == ConnectorType.Output ? connector2 : connector1;

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
        /// gets the position below the last node on the surface
        /// </summary>
        /// <returns></returns>
        private Point DetermineCorpusNodeLocation()
        {
            double x = 25;
            double y = 25;

            var index = 0;
            foreach (var corpusNode in CorpusNodes)
            {
                var nodeHeight = !double.IsNegativeInfinity(corpusNode.Size.Height) ? corpusNode.Size.Height : 75;
                index++;
                y = ((corpusNode.Y * index) / CorpusNodes.Count) + nodeHeight;
            }

            return new Point(x, y);
        }


        /// <summary>
        /// Create a node and add it to the view-model.
        /// </summary>
        public CorpusNodeViewModel CreateCorpusNode(Corpus corpus, Point nodeLocation)
        {
            if (nodeLocation.X == 0 && nodeLocation.Y == 0)
            {
                // figure out some offset based on the number of nodes already on the design surface
                // so we don't overlap
                nodeLocation = DetermineCorpusNodeLocation();
            }

            var parameters = new List<Autofac.Core.Parameter>
            {
                new NamedParameter("name", corpus.CorpusId.Name ?? string.Empty)
            };

            var node = LifetimeScope.Resolve<CorpusNodeViewModel>(parameters);

            if (node == null)
            {
                throw new NullReferenceException(
                    "Could not resolve 'CorpusNodeViewModel'.  Please ensure the type has been registered with the DI container.");
            }
            node.X = (double.IsNegativeInfinity(nodeLocation.X) || double.IsPositiveInfinity(nodeLocation.X) ||
                      double.IsNaN(nodeLocation.X))
                ? 150
                : nodeLocation.X;
            node.Y = (double.IsNegativeInfinity(nodeLocation.Y) || double.IsPositiveInfinity(nodeLocation.Y) ||
                      double.IsNaN(nodeLocation.Y))
                ? 150
                : nodeLocation.Y;

            node.CorpusType = (CorpusType)Enum.Parse(typeof(CorpusType), corpus.CorpusId.CorpusType);

            // check to see if this is a resource and not a Standard
            if (node.CorpusType == CorpusType.Standard)
            {
                var resourceList = GetParatextResourceNames();
                var resource = resourceList.FirstOrDefault(x => x == corpus.CorpusId.Name);
                if (resource != null)
                {
                    node.CorpusType = CorpusType.Resource;
                }
            }

            node.ParatextProjectId = corpus.CorpusId.ParatextGuid ?? string.Empty;
            node.CorpusId = corpus.CorpusId.Id;
            node.IsRtl = corpus.CorpusId.IsRtl;
            node.TranslationFontFamily = corpus.CorpusId.FontFamily ?? Corpus.DefaultFontFamily;

            var targetConnector = LifetimeScope.Resolve<ParallelCorpusConnectorViewModel>(new List<Autofac.Core.Parameter>
            {
                new NamedParameter("name", "Target"),
                new NamedParameter("paratextProjectId", node.ParatextProjectId),
                new NamedParameter("connectorType", ConnectorType.Input),
                new NamedParameter("corpusType", corpus.CorpusId.CorpusType)
            });
            node.InputConnectors.Add(targetConnector);


            var outputConnector = LifetimeScope.Resolve<ParallelCorpusConnectorViewModel>(new List<Autofac.Core.Parameter>
            {
                new NamedParameter("name", "Source"),
                new NamedParameter("paratextProjectId", node.ParatextProjectId),
                new NamedParameter("connectorType", ConnectorType.Output),
                new NamedParameter("corpusType", corpus.CorpusId.CorpusType)
            });
            node.OutputConnectors.Add(outputConnector);

            //
            // Add the node to the view-model.
            //
            OnUIThread(() =>
            {
                CorpusNodes.Add(node);
                ProjectDesignSurface!.InvalidateArrange();
                //// NB: Allow the newly added node to be drawn on the design surface - even if there are other long running background processes running
                ////     This is the equivalent of calling Application.DoEvents() in a WinForms app and should only be used as a last resort.
                //_ = App.Current.Dispatcher.Invoke(
                //    DispatcherPriority.Background,
                //    new ThreadStart(delegate { }));

            });

            EventAggregator.PublishOnUIThreadAsync(new CorpusAddedMessage(node.ParatextProjectId));

            return node;
        }


        public async Task UpdateNodeTokenization(CorpusNodeViewModel node)
        {
            // ReSharper disable once AsyncVoidLambda
            //await Execute.OnUIThreadAsync(async () =>
            OnUIThread(async () =>
            {
                //var corpusNode = CorpusNodes.FirstOrDefault(b => b.Id == node.Id);
                //if (node is not null)
                //{
                var topLevelProjectIds = await TopLevelProjectIds.GetTopLevelProjectIds(Mediator);
                var tokenizedCorpora =
                    topLevelProjectIds.TokenizedTextCorpusIds.Where(ttc => ttc.CorpusId!.Id == node.CorpusId);
                await CreateCorpusNodeMenu(node, tokenizedCorpora);
                //}
            });

            await Task.CompletedTask;
        }

        /// <summary>
        /// creates the data bound menu for the node
        /// </summary>
        /// <param name="parallelCorpusConnection"></param>
        /// <param name="topLevelProjectIds"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void CreateParallelCorpusConnectionMenu(ParallelCorpusConnectionViewModel parallelCorpusConnection, TopLevelProjectIds topLevelProjectIds)
        {
            // initiate the menu system
            parallelCorpusConnection.MenuItems.Clear();

            var connectionMenuItems = new BindableCollection<ParallelCorpusConnectionMenuItemViewModel>();

            AddAlignmentSetMenu(parallelCorpusConnection, topLevelProjectIds, ProjectDesignSurfaceViewModel, connectionMenuItems);
            AddMenuSeparator(connectionMenuItems);
            AddInterlinearMenu(parallelCorpusConnection, topLevelProjectIds, ProjectDesignSurfaceViewModel, connectionMenuItems);
            AddMenuSeparator(connectionMenuItems);
            AddResetVerseMappings(parallelCorpusConnection, ProjectDesignSurfaceViewModel, connectionMenuItems);
        

            parallelCorpusConnection.MenuItems = connectionMenuItems;

            try
            {
                // PLUG-IN REVIEW
                var menuBuilders = LifetimeScope.Resolve<IEnumerable<IDesignSurfaceMenuBuilder>>();

                foreach (var menuBuilder in menuBuilders)
                {
                    menuBuilder.CreateParallelCorpusConnectionMenu(parallelCorpusConnection, topLevelProjectIds);
                }
            }
            catch (Exception ex)
            {
                Logger!.LogError(ex, "An unexpected error occurred while creating plug-in menus.");
            }
        }

        private static void AddMenuSeparator(BindableCollection<ParallelCorpusConnectionMenuItemViewModel> connectionMenuItems)
        {
            connectionMenuItems.Add(new ParallelCorpusConnectionMenuItemViewModel
            {
                Header = "",
                Id = "SeparatorId",
                IsSeparator = true
            });
        }

        private void AddResetVerseMappings(ParallelCorpusConnectionViewModel parallelCorpusConnection,
            IProjectDesignSurfaceViewModel projectDesignSurfaceViewModel,
            BindableCollection<ParallelCorpusConnectionMenuItemViewModel> connectionMenuItems)
        {
            connectionMenuItems.Add(new ParallelCorpusConnectionMenuItemViewModel
            {
                Header = LocalizationService.Get("Pds_ResetVerseVersification"),
                Id = DesignSurfaceMenuIds.ResetVerseVersifications,
                IconKind = PackIconPicolIconsKind.Refresh.ToString(),
                ProjectDesignSurfaceViewModel = projectDesignSurfaceViewModel,
                ConnectionId = parallelCorpusConnection.ParallelCorpusId.Id,
            });
        }



        private void AddInterlinearMenu(ParallelCorpusConnectionViewModel parallelCorpusConnection,
        TopLevelProjectIds topLevelProjectIds, IProjectDesignSurfaceViewModel projectDesignSurfaceViewModel,
        BindableCollection<ParallelCorpusConnectionMenuItemViewModel> connectionMenuItems)
        {

            var alignmentSetCount = topLevelProjectIds.AlignmentSetIds.Count(alignmentSet =>
                alignmentSet.ParallelCorpusId!.IdEquals(parallelCorpusConnection.ParallelCorpusId!));
            connectionMenuItems.Add(new ParallelCorpusConnectionMenuItemViewModel
            {
                Header = LocalizationService.Get("Pds_CreateNewInterlinear"),
                Id = DesignSurfaceMenuIds.CreateNewInterlinear,
                IconKind = PackIconPicolIconsKind.BookTextAdd.ToString(),
                ProjectDesignSurfaceViewModel = projectDesignSurfaceViewModel,
                ConnectionId = parallelCorpusConnection.Id,
                Enabled = (alignmentSetCount > 0),
                ParallelCorpusId = parallelCorpusConnection.Id.ToString(),
            });


            AddMenuSeparator(connectionMenuItems);


            var parallelCorpusIds = topLevelProjectIds.ParallelCorpusIds.Where(x =>
                x.TargetTokenizedCorpusId.IdEquals(parallelCorpusConnection.ParallelCorpusId.TargetTokenizedCorpusId) &&
                x.SourceTokenizedCorpusId.IdEquals(parallelCorpusConnection.ParallelCorpusId.SourceTokenizedCorpusId));

            foreach (var parallelCorpusId in parallelCorpusIds)
            {
                var translationSets = topLevelProjectIds.TranslationSetIds.Where(translationSet =>
                    translationSet.ParallelCorpusId!.IdEquals(parallelCorpusId));
                foreach (var translationSet in translationSets)
                {

                    var alignmentSet = topLevelProjectIds.AlignmentSetIds.FirstOrDefault(alignmentSet =>
                        alignmentSet.Id == translationSet.AlignmentSetGuid);

                    var header = translationSet.DisplayName;
                    if (alignmentSet is not null)
                    {
                        header += $" [{alignmentSet.SmtModel}]";
                    }

                    connectionMenuItems.Add(new ParallelCorpusConnectionMenuItemViewModel
                    {
                        Header = header,
                        Id = translationSet.Id.ToString(),
                        IconKind = PackIconPicolIconsKind.Relevance.ToString(),
                        MenuItems = new BindableCollection<ParallelCorpusConnectionMenuItemViewModel>
                        {
                            new()
                            {
                                Header = LocalizationService.Get("Pds_AddConnectionToEnhancedViewMenu"),
                                Id = DesignSurfaceMenuIds.AddInterlinearToCurrentEnhancedView,
                                ProjectDesignSurfaceViewModel = projectDesignSurfaceViewModel,
                                IconKind = PackIconPicolIconsKind.DocumentTextAdd.ToString(),
                                TranslationSetId = translationSet.Id.ToString(),
                                DisplayName = translationSet.DisplayName,
                                ParallelCorpusId = translationSet.ParallelCorpusId!.Id.ToString(),
                                ParallelCorpusDisplayName = translationSet.ParallelCorpusId!.DisplayName,

                                IsRtl = parallelCorpusConnection.ParallelCorpusId!.SourceTokenizedCorpusId!.CorpusId!
                                    .IsRtl,
                                SourceParatextId = parallelCorpusConnection.SourceConnector!.ParatextId,
                                TargetParatextId = parallelCorpusConnection.DestinationConnector!.ParatextId,
                            },
                            new()
                            {
                                Header = LocalizationService.Get("Pds_AddConnectionToNewEnhancedViewMenu"),
                                Id = DesignSurfaceMenuIds.AddInterlinearToNewEnhancedView,
                                ProjectDesignSurfaceViewModel = projectDesignSurfaceViewModel,
                                IconKind = PackIconPicolIconsKind.DocumentTextAdd.ToString(),
                                TranslationSetId = translationSet.Id.ToString(),
                                DisplayName = translationSet.DisplayName,
                                ParallelCorpusId = translationSet.ParallelCorpusId!.Id.ToString(),
                                ParallelCorpusDisplayName = translationSet.ParallelCorpusId!.DisplayName,

                                IsRtl = parallelCorpusConnection.ParallelCorpusId!.SourceTokenizedCorpusId!.CorpusId!
                                    .IsRtl,
                                SourceParatextId = parallelCorpusConnection.SourceConnector!.ParatextId,
                                TargetParatextId = parallelCorpusConnection.DestinationConnector!.ParatextId,
                            },
                            new()
                            {
                                Header = LocalizationService.Get("Pds_DeleteTranaslationSet"),
                                Id = DesignSurfaceMenuIds.DeleteTranslationSet,
                                ProjectDesignSurfaceViewModel = projectDesignSurfaceViewModel,
                                IconKind = PackIconPicolIconsKind.Cancel.ToString(),
                                TranslationSetId = translationSet.Id.ToString(),
                                DisplayName = translationSet.DisplayName,
                                ParallelCorpusId = translationSet.ParallelCorpusId!.Id.ToString(),
                                ParallelCorpusDisplayName = translationSet.ParallelCorpusId!.DisplayName,

                                IsRtl = parallelCorpusConnection.ParallelCorpusId!.SourceTokenizedCorpusId!.CorpusId!
                                    .IsRtl,
                                SourceParatextId = parallelCorpusConnection.SourceConnector!.ParatextId,
                                TargetParatextId = parallelCorpusConnection.DestinationConnector!.ParatextId,

                            }
                        }
                    });
                }
            }
        }

        private async Task<bool> SmtIsAvailable(ParallelCorpusConnectionMenuItemViewModel connectionMenuItem)
        {
            var SmtList = new List<ClearDashboard.Wpf.Application.Models.SmtAlgorithm>();
            var newParallelCorpusConnection = ParallelCorpusConnections.FirstOrDefault(c => c.Id == connectionMenuItem.ConnectionId);
            var topLevelProjectIds = await TopLevelProjectIds.GetTopLevelProjectIds(Mediator!);
            var sourceCorpusNode = CorpusNodes.FirstOrDefault(b => b.Id == newParallelCorpusConnection.SourceConnector!.ParentNode!.Id);
            var targetCorpusNode = CorpusNodes.FirstOrDefault(b => b.Id == newParallelCorpusConnection.DestinationConnector!.ParentNode!.Id);

            var parallelCorpa = topLevelProjectIds.ParallelCorpusIds.Where(x =>
                x.SourceTokenizedCorpusId!.CorpusId!.Id == sourceCorpusNode.CorpusId
                && x.TargetTokenizedCorpusId!.CorpusId!.Id == targetCorpusNode.CorpusId
            ).ToList();

            List<string> smts = new();
            foreach (var parallelCorpusId in parallelCorpa)
            {
                var alignments =
                    topLevelProjectIds.AlignmentSetIds.Where(x =>
                        x.ParallelCorpusId!.Id == parallelCorpusId.Id);
                foreach (var alignment in alignments)
                {
                    smts.Add(alignment.SmtModel!);
                }
            }

            var list = Enum.GetNames(typeof(SmtModelType)).ToList();
            foreach (var smt in list)
            {
                if (smts.Contains(smt))
                {
                    SmtList.Add(new ClearDashboard.Wpf.Application.Models.SmtAlgorithm
                    {
                        SmtName = smt,
                        IsEnabled = false,
                    });
                }
                else
                {
                    // ReSharper disable once InconsistentNaming
                    var newSMT = new ClearDashboard.Wpf.Application.Models.SmtAlgorithm
                    {
                        SmtName = smt,
                        IsEnabled = true,
                    };
                    
                    SmtList.Add(newSMT);
                }
            }

            // select next available smt that is enabled
            bool found = false;
            foreach (var smt in SmtList)
            {
                if (smt.IsEnabled)
                {
                    found = true;
                    break;
                }
            }

            return found;
        }

        private async void AddAlignmentSetMenu(
            ParallelCorpusConnectionViewModel parallelCorpusConnection,
            TopLevelProjectIds topLevelProjectIds, IProjectDesignSurfaceViewModel projectDesignSurfaceViewModel,
            BindableCollection<ParallelCorpusConnectionMenuItemViewModel> connectionMenuItems)
        {
            var connectionItem = new ParallelCorpusConnectionMenuItemViewModel
            {
                Header = LocalizationService.Get("Pds_CreateNewAlignmentSetMenu"),
                Id = DesignSurfaceMenuIds.CreateNewAlignmentSet,
                IconKind = PackIconPicolIconsKind.BookTextAdd.ToString(),
                ProjectDesignSurfaceViewModel = projectDesignSurfaceViewModel,
                ConnectionId = parallelCorpusConnection.Id,
                ParallelCorpusId = parallelCorpusConnection.ParallelCorpusId?.Id.ToString(),
                ParallelCorpusDisplayName = parallelCorpusConnection.ParallelCorpusDisplayName,
                IsRtl = parallelCorpusConnection.IsRtl,
                SourceParatextId = parallelCorpusConnection.SourceConnector?.ParatextId,
                TargetParatextId = parallelCorpusConnection.DestinationConnector?.ParatextId,
            };

            if (await SmtIsAvailable(connectionItem))
            {
                connectionMenuItems.Add(connectionItem);
                AddMenuSeparator(connectionMenuItems);
            }

            var parallelCorpusIds = topLevelProjectIds.ParallelCorpusIds.Where(x =>
                x.TargetTokenizedCorpusId.IdEquals(parallelCorpusConnection.ParallelCorpusId.TargetTokenizedCorpusId) &&
                x.SourceTokenizedCorpusId.IdEquals(parallelCorpusConnection.ParallelCorpusId.SourceTokenizedCorpusId));

            foreach (var parallelCorpusId in parallelCorpusIds)
            {
                var alignmentSets = topLevelProjectIds.AlignmentSetIds.Where(alignmentSet =>
                    alignmentSet.ParallelCorpusId!.IdEquals(parallelCorpusId));
                // ALIGNMENT SETS
                foreach (var alignmentSetInfo in alignmentSets)
                {
                    connectionMenuItems.Add(new ParallelCorpusConnectionMenuItemViewModel
                    {
                        Header = $"{alignmentSetInfo.DisplayName} [{alignmentSetInfo.SmtModel}]",
                        Id = alignmentSetInfo.Id.ToString(),
                        IconKind = PackIconPicolIconsKind.Sitemap.ToString(),
                        IsEnabled = true,
                        MenuItems = new BindableCollection<ParallelCorpusConnectionMenuItemViewModel>
                        {
                            new()
                            {
                                // Add Verses to focused enhanced view
                                Header = LocalizationService.Get("Pds_AddVerseViewToEnhancedViewMenu"),
                                Id = DesignSurfaceMenuIds.AddAlignmentSetToCurrentEnhancedView,
                                ProjectDesignSurfaceViewModel = projectDesignSurfaceViewModel,
                                IconKind = PackIconPicolIconsKind.DocumentTextAdd.ToString(),
                                AlignmentSetId = alignmentSetInfo.Id.ToString(),
                                DisplayName = $"{alignmentSetInfo.DisplayName} [{alignmentSetInfo.SmtModel}]",
                                ParallelCorpusId = alignmentSetInfo.ParallelCorpusId!.Id.ToString(),
                                ParallelCorpusDisplayName = alignmentSetInfo.ParallelCorpusId.DisplayName,
                                IsEnabled = true,
                                IsRtl = parallelCorpusConnection.SourceConnector!.ParentNode!.IsRtl,
                                IsTargetRtl = parallelCorpusConnection.DestinationConnector!.ParentNode!.IsRtl,
                                SourceParatextId = parallelCorpusConnection.SourceConnector.ParatextId,
                                TargetParatextId = parallelCorpusConnection.DestinationConnector.ParatextId,
                                SmtModel = alignmentSetInfo.SmtModel,
                            },
                            new()
                            {
                                // Add Verses to focused enhanced view
                                Header = LocalizationService.Get("Pds_AddBulkAlignmentApprovalToEnhancedViewMenu"),
                                Id = DesignSurfaceMenuIds.AddAlignmentsBatchReviewViewToCurrentEnhancedView,
                                ProjectDesignSurfaceViewModel = projectDesignSurfaceViewModel,
                                IconKind = PackIconPicolIconsKind.DocumentTextAdd.ToString(),
                                AlignmentSetId = alignmentSetInfo.Id.ToString(),
                                DisplayName = $"{alignmentSetInfo.DisplayName} [{alignmentSetInfo.SmtModel}]",
                                ParallelCorpusId = alignmentSetInfo.ParallelCorpusId!.Id.ToString(),
                                ParallelCorpusDisplayName = alignmentSetInfo.ParallelCorpusId.DisplayName,
                                IsEnabled = true,
                                IsRtl = parallelCorpusConnection.SourceConnector!.ParentNode!.IsRtl,
                                IsTargetRtl = parallelCorpusConnection.DestinationConnector!.ParentNode!.IsRtl,
                                SourceParatextId = parallelCorpusConnection.SourceConnector.ParatextId,
                                TargetParatextId = parallelCorpusConnection.DestinationConnector.ParatextId,
                                SmtModel = alignmentSetInfo.SmtModel,
                            },
                            new()
                            {
                                // Add Verses to new enhanced view
                                Header = LocalizationService.Get("Pds_AddConnectionToNewEnhancedViewMenu"),
                                Id = DesignSurfaceMenuIds.AddAlignmentSetToNewEnhancedView,
                                ProjectDesignSurfaceViewModel = projectDesignSurfaceViewModel,
                                IconKind = PackIconPicolIconsKind.DocumentTextAdd.ToString(),
                                AlignmentSetId = alignmentSetInfo.Id.ToString(),
                                DisplayName = $"{alignmentSetInfo.DisplayName} [{alignmentSetInfo.SmtModel}]",
                                ParallelCorpusId = alignmentSetInfo.ParallelCorpusId!.Id.ToString(),
                                ParallelCorpusDisplayName = alignmentSetInfo.ParallelCorpusId.DisplayName,
                                IsEnabled = true,
                                IsRtl = parallelCorpusConnection.SourceConnector!.ParentNode!.IsRtl,
                                IsTargetRtl = parallelCorpusConnection.DestinationConnector!.ParentNode!.IsRtl,
                                SourceParatextId = parallelCorpusConnection.SourceConnector.ParatextId,
                                TargetParatextId = parallelCorpusConnection.DestinationConnector.ParatextId,
                                SmtModel = alignmentSetInfo.SmtModel,
                            }
                            ,
                            new()
                            {
                                // Add Verses to new enhanced view
                                Header = LocalizationService.Get("Pds_DeleteAlignmentSet"),
                                Id = DesignSurfaceMenuIds.DeleteAlignmentSet,
                                ProjectDesignSurfaceViewModel = projectDesignSurfaceViewModel,
                                IconKind = PackIconPicolIconsKind.Cancel.ToString(),
                                AlignmentSetId = alignmentSetInfo.Id.ToString(),
                                DisplayName = $"{alignmentSetInfo.DisplayName} [{alignmentSetInfo.SmtModel}]",
                                ParallelCorpusId = alignmentSetInfo.ParallelCorpusId!.Id.ToString(),
                                ParallelCorpusDisplayName = alignmentSetInfo.ParallelCorpusId.DisplayName,
                                IsEnabled = true,
                                IsRtl = parallelCorpusConnection.SourceConnector!.ParentNode!.IsRtl,
                                IsTargetRtl = parallelCorpusConnection.DestinationConnector!.ParentNode!.IsRtl,
                                SourceParatextId = parallelCorpusConnection.SourceConnector.ParatextId,
                                TargetParatextId = parallelCorpusConnection.DestinationConnector.ParatextId,
                                SmtModel = alignmentSetInfo.SmtModel,
                            }
                        }
                    });
                }
            }
        }


        public async Task CreateCorpusNodeMenu(CorpusNodeViewModel corpusNodeViewModel, IEnumerable<TokenizedTextCorpusId> tokenizedCorpora)
        {
            corpusNodeViewModel.MenuItems.Clear();
            corpusNodeViewModel.TokenizationCount = 0;

            BindableCollection<CorpusNodeMenuItemViewModel> nodeMenuItems = new();

            bool isResource;

            try
            {
                isResource = await IsRelatedParatextProjectAParatextResource(corpusNodeViewModel);
            }
            catch (Exception e)
            {
                isResource = false;
            }

            var addSeparator = false;
            // restrict the ability of Manuscript to add new tokenizers
            if (corpusNodeViewModel.CorpusType != CorpusType.ManuscriptHebrew && corpusNodeViewModel.CorpusType != CorpusType.ManuscriptGreek)
            {
                // Add new tokenization
                //nodeMenuItems.Add(new CorpusNodeMenuItemViewModel
                //{
                //    Header = LocalizationStrings.Get("Pds_AddNewTokenizationMenu", Logger!),
                //    Id = DesignSurfaceMenuIds.AddParatextCorpus,
                //    IconKind = PackIconPicolIconsKind.BookTextAdd.ToString(),
                //    ProjectDesignSurfaceViewModel = ProjectDesignSurfaceViewModel,
                //    CorpusNodeViewModel = corpusNode,
                //});
                //addSeparator = true;
            }
            
            foreach (var tokenizedCorpus in tokenizedCorpora)
            {
                if (!string.IsNullOrEmpty(tokenizedCorpus.TokenizationFunction))
                {
                    if (addSeparator)
                    {
                        AddSeparatorMenu(nodeMenuItems);
                        addSeparator = false;
                    }
                    var tokenizer = (Tokenizers)Enum.Parse(typeof(Tokenizers), tokenizedCorpus.TokenizationFunction);

                    var corpusNodeMenuViewModel = new CorpusNodeMenuItemViewModel
                    {
                        Header = EnumHelper.GetDescription(tokenizer),
                        Id = tokenizedCorpus.Id.ToString(),
                        IconKind = PackIconPicolIconsKind.Relevance.ToString(),
                        MenuItems = new BindableCollection<CorpusNodeMenuItemViewModel>
                        {
                            new()
                            {
                                // Add Verses to focused enhanced view
                                Header = LocalizationService.Get("Pds_AddToEnhancedViewMenu"),
                                Id = DesignSurfaceMenuIds.AddTokenizedCorpusToCurrentEnhancedView,
                                ProjectDesignSurfaceViewModel = ProjectDesignSurfaceViewModel,
                                IconKind = PackIconPicolIconsKind.DocumentTextAdd.ToString(),
                                CorpusNodeViewModel = corpusNodeViewModel,
                                Tokenizer = tokenizer.ToString(),
                            },
                            new()
                            {
                                // Show Verses in New Windows
                                Header = LocalizationService.Get("Pds_AddToNewEnhancedViewMenu"),
                                Id = DesignSurfaceMenuIds.AddTokenizedCorpusToNewEnhancedView,
                                ProjectDesignSurfaceViewModel = ProjectDesignSurfaceViewModel,
                                IconKind = PackIconPicolIconsKind.DocumentText.ToString(),
                                CorpusNodeViewModel = corpusNodeViewModel,
                                Tokenizer = tokenizer.ToString(),
                            },


                }
                    };

                    var menuBuilders = LifetimeScope.Resolve<IEnumerable<IDesignSurfaceMenuBuilder>>();

                    // do not allow MACULA to be updated
                    if ((corpusNodeViewModel.CorpusType == CorpusType.ManuscriptGreek || corpusNodeViewModel.CorpusType == CorpusType.ManuscriptHebrew) ==false)
                    {
                        AddSeparatorMenu(corpusNodeMenuViewModel.MenuItems);

                        corpusNodeMenuViewModel.MenuItems.Add(new CorpusNodeMenuItemViewModel
                        {
                            // Show Verses in New Windows
                            Header = LocalizationService.Get("Pds_GetLatestFromParatext"),
                            Id = DesignSurfaceMenuIds.UpdateParatextCorpus,
                            ProjectDesignSurfaceViewModel = ProjectDesignSurfaceViewModel,
                            IconKind = PackIconPicolIconsKind.Edit.ToString(),
                            CorpusNodeViewModel = corpusNodeViewModel,
                            Tokenizer = tokenizer.ToString(),
                        });
                    }


                    foreach (var menuBuilder in menuBuilders)
                    {
                        menuBuilder.CreateOnlyForNonResouceCorpusNodeChildMenu(corpusNodeMenuViewModel, tokenizedCorpus);
                    }

                    foreach (var menuBuilder in menuBuilders)
                    {
                        menuBuilder.CreateCorpusNodeChildMenu(corpusNodeMenuViewModel, tokenizedCorpus);
                    }

                    if (tokenizedCorpora.Count() > 1)
                    {
                        corpusNodeViewModel.MenuItems.Add(corpusNodeMenuViewModel);
                    }
                    else
                    {
                        corpusNodeViewModel.MenuItems.AddRange(corpusNodeMenuViewModel.MenuItems);
                    }
                    
                    corpusNodeViewModel.TokenizationCount++;
                }
            }

            if (!isResource)
            {
                try
                {
                    // PLUG-IN REVIEW
                    var menuBuilders = LifetimeScope.Resolve<IEnumerable<IDesignSurfaceMenuBuilder>>();

                    foreach (var menuBuilder in menuBuilders)
                    {
                        menuBuilder.CreateCorpusNodeMenu(corpusNodeViewModel);
                    }
                }
                catch (Exception ex)
                {
                    Logger!.LogError(ex, "An unexpected error occurred while creating plug-in menus.");
                }
            }
        }

        private void AddSeparatorMenu(BindableCollection<CorpusNodeMenuItemViewModel> nodeMenuItems)
        {
            nodeMenuItems.Add(new CorpusNodeMenuItemViewModel
            {
                Header = "",
                Id = "SeparatorId",
                ProjectDesignSurfaceViewModel = ProjectDesignSurfaceViewModel,
                IsSeparator = true
            });
        }

        private async Task<bool> IsRelatedParatextProjectAParatextResource(CorpusNodeViewModel corpusNode)
        {
            bool isResource;
            if (corpusNode.CorpusType != CorpusType.ManuscriptHebrew && corpusNode.CorpusType != CorpusType.ManuscriptGreek)
            {
                var requestResult = await Mediator.Send(new GetAllProjectsQuery());

                if (requestResult.Success)
                {
                    var projects = requestResult.Data
                                   ?? throw new InvalidDataEngineException(name: "return", value: "null",
                                       message: "Could not obtain a list of projects from paratext");
                    var project = projects.Find(paratextProject => paratextProject.Guid!.Equals(corpusNode.ParatextProjectId))
                                  ?? throw new InvalidDataEngineException(name: nameof(corpusNode.ParatextProjectId),
                                      value: corpusNode.ParatextProjectId,
                                      message: "not found in list of projects reported by Paratext");
                    isResource = project.IsResource ?? false;
                }
                else
                {
                    Logger?.LogCritical($"Error checking whether project is resource or not: {requestResult.Message}");
                    throw new MediatorErrorEngineException(requestResult.Message);
                }
            }
            else
            {
                isResource = true;
            }

            return isResource;
        }


        /// <summary>
        /// Delete the currently selected nodes from the view-model.
        /// </summary>
        public void DeleteSelectedNodes()
        {
            // Take a copy of the selected nodes list so we can delete nodes while iterating.
            var nodesCopy = CorpusNodes.ToArray();
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
                ParallelCorpusConnections.RemoveRange(node.AttachedConnections);

                //
                // Remove the node from the design surface.
                //
                CorpusNodes.Remove(node);


                if (node.CorpusType == CorpusType.ManuscriptGreek)
                {
                    AddManuscriptGreekEnabled = true;
                }

                if (node.CorpusType == CorpusType.ManuscriptHebrew)
                {
                    AddManuscriptHebrewEnabled = true;
                }


                EventAggregator.PublishOnUIThreadAsync(new CorpusDeletedMessage(node.ParatextProjectId, node.CorpusId));
            });

        }

        /// <summary>
        /// Utility method to delete a connection from the view-model.
        /// </summary>
        public void DeleteParallelCorpusConnection(ParallelCorpusConnectionViewModel parallelCorpusConnection, bool isCurrentlyParallelizing = false)
        {
            if (!isCurrentlyParallelizing)
            {
                EventAggregator.PublishOnUIThreadAsync(new ParallelCorpusDeletedMessage(
                     SourceParatextId: parallelCorpusConnection.SourceConnector!.ParentNode!.ParatextProjectId,
                     TargetParatextId: parallelCorpusConnection.DestinationConnector!.ParentNode!.ParatextProjectId,
                     ConnectorGuid: parallelCorpusConnection.Id,
                     ParallelCorpusId: parallelCorpusConnection.ParallelCorpusId!.Id));
            }

            if (ParallelCorpusConnections.Contains(parallelCorpusConnection))
            {
                ParallelCorpusConnections.Remove(parallelCorpusConnection);
            }

        }


        /// <summary>
        /// Event raised then Connections have been removed.
        /// </summary>
        public void OnConnectionsItemsRemoved(object? sender, CollectionItemsChangedEventArgs e)
        {
            foreach (ParallelCorpusConnectionViewModel connection in e.Items)
            {
                connection.SourceConnector = null;
                connection.DestinationConnector = null;
            }
        }

        /// <summary>
        /// Event raised then Connections has been selected.
        /// </summary>
        public void OnConnectionsItemsSelected(object? sender, CollectionItemsChangedEventArgs e)
        {
            foreach (ParallelCorpusConnectionViewModel connection in e.Items)
            {
                connection.SourceConnector = null;
                connection.DestinationConnector = null;
            }
        }

        /// <summary>
        /// Event raised while the user is dragging a connection.
        /// NB:  This method cannot be moved to the view model as Mouse.GetPosition always returns a Point - (0,0)
        ///      The event is handled in the code behind for ProjectDesignSurfaceView
        /// </summary>
        public void OnProjectDesignSurfaceConnectionDragging(object sender, ConnectionDraggingEventArgs e)
        {
            //OnUIThread(() =>
            //{
            //    var curDragPoint = Mouse.GetPosition((Wpf.Controls.ProjectDesignSurface)sender);
            //    var connection = (ParallelCorpusConnectionViewModel)e.Connection;
            //    ConnectionDragging(curDragPoint, connection);
            //});
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
            ParallelCorpusConnections.Add(connection);

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
            if (ProjectDesignSurfaceViewModel.IsBusy)
            {
                return;
            }

            var draggedOutConnector = (ParallelCorpusConnectorViewModel)e.ConnectorDraggedOut;
            var curDragPoint = Mouse.GetPosition(ProjectDesignSurface);

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
            var curDragPoint = Mouse.GetPosition(ProjectDesignSurface);
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
            await ProjectDesignSurfaceViewModel.SaveDesignSurfaceData();

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
                ParallelCorpusConnections.Remove(newParallelCorpusConnection);
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
                ParallelCorpusConnections.Remove(newParallelCorpusConnection);
                return;
            }

            //
            // The user has dragged the connection on top of another valid connector.
            //

            //
            // Remove any existing connection between the same two connectors.
            //
            var existingConnection = FindConnection(parallelCorpusConnectorDraggedOut, parallelCorpusConnectorDraggedOver);
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (existingConnection != null)
            {
                ParallelCorpusConnections.Remove(existingConnection);
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
                var sourceParatextProjectId = newParallelCorpusConnection.SourceConnector!.ParentNode!.ParatextProjectId;
                if (string.IsNullOrEmpty(sourceParatextProjectId))
                {
                    ParallelCorpusConnections.Remove(newParallelCorpusConnection);
                    return;
                }

                var destinationParatextProjectId = newParallelCorpusConnection.DestinationConnector.ParentNode!.ParatextProjectId;
                if (string.IsNullOrEmpty(destinationParatextProjectId))
                {
                    ParallelCorpusConnections.Remove(newParallelCorpusConnection);
                    return;
                }

                await EventAggregator.PublishOnUIThreadAsync(new ParallelCorpusAddedMessage(
                    SourceParatextId: newParallelCorpusConnection.SourceConnector.ParentNode.ParatextProjectId,
                    TargetParatextId: newParallelCorpusConnection.DestinationConnector.ParentNode.ParatextProjectId,
                    ConnectorGuid: newParallelCorpusConnection.Id));


                newParallelCorpusConnection.SourceFontFamily = await GetFontFamily(newParallelCorpusConnection.SourceConnector.ParentNode.ParatextProjectId);
                newParallelCorpusConnection.TargetFontFamily = await GetFontFamily(newParallelCorpusConnection.DestinationConnector.ParentNode.ParatextProjectId);

                await ProjectDesignSurfaceViewModel.AddParallelCorpus(newParallelCorpusConnection, Enums.ParallelProjectType.WholeProcess);
            }

            await ProjectDesignSurfaceViewModel.SaveDesignSurfaceData();
        }


        /// <summary>
        /// Gets all the resource project names from the paratext
        /// project's _resources directory
        /// </summary>
        /// <returns></returns>
        public List<string> GetParatextResourceNames()
        {
            var fileList = new List<string>();
            var directory = _paratextProxy.ParatextResourcePath;
            if (Directory.Exists(directory))
            {
                var files = Directory.GetFiles(directory, "*.p8z");
                foreach (var file in files)
                {
                    fileList.Add(Path.GetFileNameWithoutExtension(file));
                }
            }

            return fileList;
        }

        private async Task<string?> GetFontFamily(string paratextProjectId)
        {
            var result = await Mediator.Send(new GetProjectFontFamilyQuery(paratextProjectId));
            if (result is { HasData: true })
            {
                return result.Data;
            }

            return FontNames.DefaultFontFamily;
        }


        /// <summary>
        /// kills off the menu item for an alignment set
        /// </summary>
        /// <param name="alignmentSetId"></param>
        public void DeleteAlignmentFromMenus(AlignmentSetId alignmentSetId)
        {
            foreach (var connection in ParallelCorpusConnections)
            {
                foreach (var parallelCorpusConnectionMenuItemViewModel in connection.MenuItems)
                {
                    if (alignmentSetId.Id.ToString() == parallelCorpusConnectionMenuItemViewModel.Id)
                    {
                        connection.MenuItems.Remove(parallelCorpusConnectionMenuItemViewModel);
                        break;
                    }
                }
            }

        }

        /// <summary>
        /// kills off the menu item for a translation set
        /// </summary>
        /// <param name="translationSetId"></param>
        public void DeleteTranslationFromMenus(TranslationSetId translationSetId)
        {
            foreach (var connection in ParallelCorpusConnections)
            {
                foreach (var parallelCorpusConnectionMenuItemViewModel in connection.MenuItems)
                {
                    if (translationSetId.Id.ToString() == parallelCorpusConnectionMenuItemViewModel.Id)
                    {
                        connection.MenuItems.Remove(parallelCorpusConnectionMenuItemViewModel);
                        break;
                    }
                }
            }
        }
    }
}
