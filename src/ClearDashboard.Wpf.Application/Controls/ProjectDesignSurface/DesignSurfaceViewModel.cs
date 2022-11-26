using Autofac;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.ViewModels.Project;
using ClearDashboard.Wpf.Controls.Utils;
using MahApps.Metro.IconPacks;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Corpus = ClearDashboard.DAL.Alignment.Corpora.Corpus;
using TopLevelProjectIds = ClearDashboard.DAL.Alignment.TopLevelProjectIds;


namespace ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface
{
    /// <summary>
    /// Defines a design surface with nodes and connections between the nodes.
    /// </summary>
    public sealed class DesignSurfaceViewModel : Screen
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

        //private readonly INavigationService? _navigationService;
        private readonly ILogger<DesignSurfaceViewModel>? _logger;
        //private readonly DashboardProjectManager? _projectManager;
        private readonly ILifetimeScope _lifecycleScope;
        private readonly IEventAggregator? _eventAggregator;
        private readonly IMediator _mediator;


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
        public Point GetFreeSpot()
        {
            double x = 25;
            double y = 25;
            double yOffset = 0;

            foreach (var corpusNode in CorpusNodes)
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
        /// Create a node and add it to the view-model.
        /// </summary>
        public CorpusNodeViewModel CreateCorpusNode(Corpus corpus, Point nodeLocation)
        {
            if (nodeLocation.X == 0 && nodeLocation.Y == 0)
            {
                // figure out some offset based on the number of nodes already on the design surface
                // so we don't overlap
                nodeLocation = GetFreeSpot();
            }


            var parameters = new List<Autofac.Core.Parameter>
            {
                new NamedParameter("name", corpus.CorpusId.Name ?? string.Empty)
            };

            var node = _lifecycleScope.Resolve<CorpusNodeViewModel>(parameters);

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
            node.ParatextProjectId = corpus.CorpusId.ParatextGuid ?? string.Empty;
            node.CorpusId = corpus.CorpusId.Id;
            node.IsRtl = corpus.CorpusId.IsRtl;
            node.TranslationFontFamily = corpus.CorpusId.FontFamily ?? Corpus.DefaultFontFamily;

            var targetConnector = _lifecycleScope.Resolve<ParallelCorpusConnectorViewModel>(new List<Autofac.Core.Parameter>
            {
                new NamedParameter("name", "Target"),
                new NamedParameter("paratextProjectId", node.ParatextProjectId),
                new NamedParameter("connectorType", ConnectorType.Input)
            });
            node.InputConnectors.Add(targetConnector);


            var outputConnector = _lifecycleScope.Resolve<ParallelCorpusConnectorViewModel>(new List<Autofac.Core.Parameter>
            {
                new NamedParameter("name", "Source"),
                new NamedParameter("paratextProjectId", node.ParatextProjectId),
                new NamedParameter("connectorType", ConnectorType.Output)
            });
            node.OutputConnectors.Add(outputConnector);

            //
            // Add the node to the view-model.
            //
            OnUIThread(() =>
            {
                CorpusNodes.Add(node);
                //// NB: Allow the newly added node to be drawn on the design surface - even if there are other long running background processes running
                ////     This is the equivalent of calling Application.DoEvents() in a WinForms app and should only be used as a last resort.
                //_ = App.Current.Dispatcher.Invoke(
                //    DispatcherPriority.Background,
                //    new ThreadStart(delegate { }));
            });

            _eventAggregator.PublishOnUIThreadAsync(new CorpusAddedMessage(node.ParatextProjectId));

            return node;
        }


        public async Task UpdateNodeTokenization(CorpusNodeViewModel node, ProjectDesignSurfaceViewModel projectDesignSurfaceViewModel)
        {
            var corpusNode = CorpusNodes.FirstOrDefault(b => b.Id == node.Id);
            if (corpusNode is not null)
            {
                var topLevelProjectIds = await TopLevelProjectIds.GetTopLevelProjectIds(_mediator);
                var tokenizedCorpora = topLevelProjectIds.TokenizedTextCorpusIds.Where(ttc => ttc.CorpusId!.Id == corpusNode.CorpusId);
                CreateCorpusNodeMenu(corpusNode, tokenizedCorpora, projectDesignSurfaceViewModel);
            }
        }

        /// <summary>
        /// creates the data bound menu for the node
        /// </summary>
        /// <param name="parallelCorpusConnection"></param>
        /// <param name="topLevelProjectIds"></param>
        /// <param name="projectDesignSurfaceViewModel"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void CreateConnectionMenu(ParallelCorpusConnectionViewModel parallelCorpusConnection, TopLevelProjectIds topLevelProjectIds, ProjectDesignSurfaceViewModel projectDesignSurfaceViewModel)
        {
            // initiate the menu system
            parallelCorpusConnection.MenuItems.Clear();

            var connectionMenuItems = new BindableCollection<ParallelCorpusConnectionMenuItemViewModel>();

            AddAlignmentSetMenu(parallelCorpusConnection, topLevelProjectIds, projectDesignSurfaceViewModel, connectionMenuItems);
            AddMenuSeparator(projectDesignSurfaceViewModel, connectionMenuItems);
            AddInterlinearMenu(parallelCorpusConnection, topLevelProjectIds, projectDesignSurfaceViewModel, connectionMenuItems);
            AddMenuSeparator(projectDesignSurfaceViewModel, connectionMenuItems);
            AddPropertiesMenu(parallelCorpusConnection, projectDesignSurfaceViewModel, connectionMenuItems);

            parallelCorpusConnection.MenuItems = connectionMenuItems;
        }

        private static void AddMenuSeparator(ProjectDesignSurfaceViewModel projectDesignSurfaceViewModel,
            BindableCollection<ParallelCorpusConnectionMenuItemViewModel> connectionMenuItems)
        {
            connectionMenuItems.Add(new ParallelCorpusConnectionMenuItemViewModel
            {
                Header = "",
                Id = "SeparatorId",
                ProjectDesignSurfaceViewModel = projectDesignSurfaceViewModel,
                IsSeparator = true
            });
        }

        private void AddPropertiesMenu(ParallelCorpusConnectionViewModel parallelCorpusConnection,
            ProjectDesignSurfaceViewModel projectDesignSurfaceViewModel, BindableCollection<ParallelCorpusConnectionMenuItemViewModel> connectionMenuItems)
        {
            connectionMenuItems.Add(new ParallelCorpusConnectionMenuItemViewModel
            {
                // Properties
                Header = LocalizationStrings.Get("Pds_PropertiesMenu", _logger!),
                Id = "PropertiesId",
                IconKind = "Settings",
                ConnectionViewModel = parallelCorpusConnection,
                ProjectDesignSurfaceViewModel = projectDesignSurfaceViewModel
            });
        }

        private void AddInterlinearMenu(ParallelCorpusConnectionViewModel parallelCorpusConnection,
            TopLevelProjectIds topLevelProjectIds, ProjectDesignSurfaceViewModel projectDesignSurfaceViewModel,
            BindableCollection<ParallelCorpusConnectionMenuItemViewModel> connectionMenuItems)
        {
            // Add new tokenization
            connectionMenuItems.Add(new ParallelCorpusConnectionMenuItemViewModel
            {
                Header = LocalizationStrings.Get("Pds_CreateNewInterlinear", _logger!),
                Id = "CreateNewInterlinearId",
                IconKind = PackIconPicolIconsKind.BookTextAdd.ToString(),
                ProjectDesignSurfaceViewModel = projectDesignSurfaceViewModel,
                ConnectionId = parallelCorpusConnection.Id,
                Enabled = (parallelCorpusConnection.AlignmentSetInfo.Count > 0)
            });


            AddMenuSeparator(projectDesignSurfaceViewModel, connectionMenuItems);
            //connectionMenuItems.Add(new ParallelCorpusConnectionMenuItemViewModel
            //{
            //    Header = "", Id = "SeparatorId", ProjectDesignSurfaceViewModel = projectDesignSurfaceViewModel,
            //    IsSeparator = true
            //});

            var translationSets = topLevelProjectIds.TranslationSetIds.Where(translationSet =>
                translationSet.ParallelCorpusId == parallelCorpusConnection.ParallelCorpusId);
            foreach (var translationSet in translationSets)
            {
                connectionMenuItems.Add(new ParallelCorpusConnectionMenuItemViewModel
                {
                    Header = translationSet.DisplayName,
                    Id = translationSet.Id.ToString(),
                    IconKind = PackIconPicolIconsKind.Relevance.ToString(),
                    MenuItems = new BindableCollection<ParallelCorpusConnectionMenuItemViewModel>
                    {
                        new ParallelCorpusConnectionMenuItemViewModel
                        {
                            // Add Verses to focused enhanced view
                            Header = LocalizationStrings.Get("Pds_AddConnectionToEnhancedViewMenu", _logger!),
                            Id = "AddTranslationToEnhancedViewId",
                            ProjectDesignSurfaceViewModel = projectDesignSurfaceViewModel,
                            IconKind = PackIconPicolIconsKind.DocumentTextAdd.ToString(),
                            TranslationSetId = translationSet.Id.ToString(),
                            DisplayName = translationSet.DisplayName,
                            ParallelCorpusId = translationSet.ParallelCorpusId!.Id.ToString(),
                            ParallelCorpusDisplayName = translationSet.ParallelCorpusId!.DisplayName,

                            // TODO:  Where does IsRtl come from?
                            //IsRtl = info.ParallelCorpusId.SourceTokenizedCorpusId.,
                            SourceParatextId = parallelCorpusConnection.SourceConnector!.ParatextId,
                            TargetParatextId = parallelCorpusConnection.DestinationConnector!.ParatextId,
                        }
                    }
                });
            }
        }

        private void AddAlignmentSetMenu(
            ParallelCorpusConnectionViewModel parallelCorpusConnection,
            TopLevelProjectIds topLevelProjectIds, ProjectDesignSurfaceViewModel projectDesignSurfaceViewModel,
            BindableCollection<ParallelCorpusConnectionMenuItemViewModel> connectionMenuItems)
        {

            // Add new alignment set
            connectionMenuItems.Add(new ParallelCorpusConnectionMenuItemViewModel
            {
                Header = LocalizationStrings.Get("Pds_CreateNewAlignmentSetMenu", _logger!),
                Id = "CreateAlignmentSetId",
                IconKind = PackIconPicolIconsKind.BookTextAdd.ToString(),
                ProjectDesignSurfaceViewModel = projectDesignSurfaceViewModel,
                ConnectionId = parallelCorpusConnection.Id,
                ParallelCorpusId = parallelCorpusConnection.ParallelCorpusId?.Id.ToString(),
                ParallelCorpusDisplayName = parallelCorpusConnection.ParallelCorpusDisplayName,
                IsRtl = parallelCorpusConnection.IsRtl,
                SourceParatextId = parallelCorpusConnection.SourceConnector?.ParatextId,
                TargetParatextId = parallelCorpusConnection.DestinationConnector?.ParatextId,
            });

            AddMenuSeparator(projectDesignSurfaceViewModel, connectionMenuItems);
            //new ParallelCorpusConnectionMenuItemViewModel
            //{
            //    Header = "",
            //    Id = "SeparatorId",
            //    ProjectDesignSurfaceViewModel = projectDesignSurfaceViewModel,
            //    IsSeparator = true
            //}



            var alignmentSets = topLevelProjectIds.AlignmentSetIds.Where(alignmentSet =>
                alignmentSet.ParallelCorpusId == parallelCorpusConnection.ParallelCorpusId);
            // ALIGNMENT SETS
            foreach (var alignmentSetInfo in alignmentSets)
            {
                connectionMenuItems.Add(new ParallelCorpusConnectionMenuItemViewModel
                {
                    Header = alignmentSetInfo.DisplayName,
                    Id = alignmentSetInfo.Id.ToString(),
                    IconKind = PackIconPicolIconsKind.Sitemap.ToString(),
                    IsEnabled = true,
                    MenuItems = new BindableCollection<ParallelCorpusConnectionMenuItemViewModel>
                    {
                        new ParallelCorpusConnectionMenuItemViewModel
                        {
                            // Add Verses to focused enhanced view
                            Header = LocalizationStrings.Get("Pds_AddConnectionToEnhancedViewMenu", _logger!),
                            Id = "AddAlignmentToEnhancedViewId",
                            ProjectDesignSurfaceViewModel = projectDesignSurfaceViewModel,
                            IconKind = PackIconPicolIconsKind.DocumentTextAdd.ToString(),
                            AlignmentSetId = alignmentSetInfo.Id.ToString(),
                            DisplayName = alignmentSetInfo.DisplayName,
                            ParallelCorpusId = alignmentSetInfo.ParallelCorpusId!.Id.ToString(),
                            ParallelCorpusDisplayName = alignmentSetInfo.ParallelCorpusId.DisplayName,
                            IsEnabled = true,
                            IsRtl = parallelCorpusConnection.SourceConnector!.ParentNode!.IsRtl,
                            IsTargetRTL = parallelCorpusConnection.DestinationConnector!.ParentNode!.IsRtl,
                            SourceParatextId = parallelCorpusConnection.SourceConnector.ParatextId,
                            TargetParatextId = parallelCorpusConnection.DestinationConnector.ParatextId,
                        },
                    }
                });
            }
        }

        /// <summary>
        /// creates the menu for the CorpusNode
        /// </summary>
        /// <param name="corpusNode"></param>
        /// <param name="tokenizedCorpora"></param>
        /// <param name="projectDesignSurfaceViewModel"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void CreateCorpusNodeMenu(CorpusNodeViewModel corpusNode, IEnumerable<TokenizedTextCorpusId> tokenizedCorpora, ProjectDesignSurfaceViewModel projectDesignSurfaceViewModel)
        {
            // initiate the menu system
            corpusNode.MenuItems.Clear();
            corpusNode.TokenizationCount = 0;

            BindableCollection<CorpusNodeMenuItemViewModel> nodeMenuItems = new();

            // restrict the ability of Manuscript to add new tokenizers
            if (corpusNode.CorpusType != CorpusType.ManuscriptHebrew || corpusNode.CorpusType != CorpusType.ManuscriptGreek)
            {
                // Add new tokenization
                nodeMenuItems.Add(new CorpusNodeMenuItemViewModel
                {
                    Header = LocalizationStrings.Get("Pds_AddNewTokenizationMenu", _logger!),
                    Id = "AddTokenizationId",
                    IconKind = PackIconPicolIconsKind.BookTextAdd.ToString(),
                    ProjectDesignSurfaceViewModel = projectDesignSurfaceViewModel,
                    CorpusNodeViewModel = corpusNode,
                });
                nodeMenuItems.Add(new CorpusNodeMenuItemViewModel { Header = "", Id = "SeparatorId", ProjectDesignSurfaceViewModel = projectDesignSurfaceViewModel, IsSeparator = true });
            }

            foreach (var tokenizedCorpus in tokenizedCorpora)
            {
                if (!string.IsNullOrEmpty(tokenizedCorpus.TokenizationFunction))
                {
                    var tokenizer = (Tokenizers)Enum.Parse(typeof(Tokenizers),
                        tokenizedCorpus.TokenizationFunction);
                    nodeMenuItems.Add(new CorpusNodeMenuItemViewModel
                    {
                        Header = EnumHelper.GetDescription(tokenizer),
                        Id = tokenizedCorpus.Id.ToString(),
                        IconKind = PackIconPicolIconsKind.Relevance.ToString(),
                        MenuItems = new BindableCollection<CorpusNodeMenuItemViewModel>
                        {
                            new CorpusNodeMenuItemViewModel
                            {
                                // Add Verses to focused enhanced view
                                Header = LocalizationStrings.Get("Pds_AddToEnhancedViewMenu", _logger!),
                                Id = "AddToEnhancedViewId",
                                ProjectDesignSurfaceViewModel = projectDesignSurfaceViewModel,
                                IconKind = PackIconPicolIconsKind.DocumentTextAdd.ToString(),
                                CorpusNodeViewModel = corpusNode,
                                Tokenizer = tokenizer.ToString(),
                            },
                            new CorpusNodeMenuItemViewModel
                            {
                                // Show Verses in New Windows
                                Header = LocalizationStrings.Get("Pds_ShowVersesMenu", _logger!),
                                Id = "ShowVerseId", ProjectDesignSurfaceViewModel = projectDesignSurfaceViewModel,
                                IconKind = PackIconPicolIconsKind.DocumentText.ToString(),
                                CorpusNodeViewModel = corpusNode,
                                Tokenizer = tokenizer.ToString(),
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
                    corpusNode.TokenizationCount++;
                }
            }

            nodeMenuItems.Add(new CorpusNodeMenuItemViewModel
            {
                Header = "",
                Id = "SeparatorId",
                ProjectDesignSurfaceViewModel = projectDesignSurfaceViewModel,
                IsSeparator = true
            });

            nodeMenuItems.Add(new CorpusNodeMenuItemViewModel
            {
                // Properties
                Header = LocalizationStrings.Get("Pds_PropertiesMenu", _logger!),
                Id = "PropertiesId",
                IconKind = PackIconPicolIconsKind.Settings.ToString(),
                CorpusNodeViewModel = corpusNode,
                ProjectDesignSurfaceViewModel = projectDesignSurfaceViewModel
            });

            corpusNode.MenuItems = nodeMenuItems;
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

                _eventAggregator.PublishOnUIThreadAsync(new CorpusDeletedMessage(node.ParatextProjectId));
            });

        }

        /// <summary>
        /// Utility method to delete a connection from the view-model.
        /// </summary>
        public void DeleteConnection(ParallelCorpusConnectionViewModel parallelCorpusConnection)
        {
            _eventAggregator.PublishOnUIThreadAsync(new ParallelCorpusDeletedMessage(
                 SourceParatextId: parallelCorpusConnection.SourceConnector.ParentNode.ParatextProjectId,
                 TargetParatextId: parallelCorpusConnection.DestinationConnector.ParentNode.ParatextProjectId,
                 ConnectorGuid: parallelCorpusConnection.Id));

            if (ParallelCorpusConnections.Contains(parallelCorpusConnection))
            {
                ParallelCorpusConnections.Remove(parallelCorpusConnection);
            }

        }

        #region Private Methods

        /// <summary>
        /// Event raised then Connections have been removed.
        /// </summary>
        private void OnConnectionsItemsRemoved(object? sender, CollectionItemsChangedEventArgs e)
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
        private void OnConnectionsItemsSelected(object? sender, CollectionItemsChangedEventArgs e)
        {
            foreach (ParallelCorpusConnectionViewModel connection in e.Items)
            {
                connection.SourceConnector = null;
                connection.DestinationConnector = null;
            }
        }

        #endregion Private Methods

        #region ctor

        public DesignSurfaceViewModel(INavigationService? navigationService,
            ILogger<DesignSurfaceViewModel>? logger,
            DashboardProjectManager? projectManager, IEventAggregator? eventAggregator, ILifetimeScope lifecycleScope, IMediator mediator)
        {
            //_navigationService = navigationService;
            //_projectManager = projectManager;
            _logger = logger;
            _eventAggregator = eventAggregator;
            _lifecycleScope = lifecycleScope;
            _mediator = mediator;
        }
        #endregion
    }
}
