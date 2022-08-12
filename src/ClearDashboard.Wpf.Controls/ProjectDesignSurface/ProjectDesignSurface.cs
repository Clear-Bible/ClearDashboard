using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using ClearDashboard.Wpf.Controls.Utils;

namespace ClearDashboard.Wpf.Controls
{
    public partial class ProjectDesignSurface : Control
    {
        #region Dependency Property/Event Definitions

        private static readonly DependencyPropertyKey _nodesPropertyKey =
            DependencyProperty.RegisterReadOnly("Nodes", typeof(ImpObservableCollection<object>), typeof(ProjectDesignSurface),
                new FrameworkPropertyMetadata());
        public static readonly DependencyProperty NodesProperty = _nodesPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey _connectionsPropertyKey =
            DependencyProperty.RegisterReadOnly("Connections", typeof(ImpObservableCollection<object>), typeof(ProjectDesignSurface),
                new FrameworkPropertyMetadata());
        public static readonly DependencyProperty ConnectionsProperty = _connectionsPropertyKey.DependencyProperty;

        public static readonly DependencyProperty NodesSourceProperty =
            DependencyProperty.Register("NodesSource", typeof(IEnumerable), typeof(ProjectDesignSurface),
                new FrameworkPropertyMetadata(NodesSource_PropertyChanged));

        public static readonly DependencyProperty ConnectionsSourceProperty =
            DependencyProperty.Register("ConnectionsSource", typeof(IEnumerable), typeof(ProjectDesignSurface),
                new FrameworkPropertyMetadata(ConnectionsSource_PropertyChanged));

        public static readonly DependencyProperty IsClearSelectionOnEmptySpaceClickEnabledProperty =
            DependencyProperty.Register("IsClearSelectionOnEmptySpaceClickEnabled", typeof(bool), typeof(ProjectDesignSurface),
                new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty EnableConnectionDraggingProperty =
            DependencyProperty.Register("EnableConnectionDragging", typeof(bool), typeof(ProjectDesignSurface),
                new FrameworkPropertyMetadata(true));

        private static readonly DependencyPropertyKey _isDraggingConnectionPropertyKey =
            DependencyProperty.RegisterReadOnly("IsDraggingConnection", typeof(bool), typeof(ProjectDesignSurface),
                new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty IsDraggingConnectionProperty = _isDraggingConnectionPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey _isNotDraggingConnectionPropertyKey =
            DependencyProperty.RegisterReadOnly("IsNotDraggingConnection", typeof(bool), typeof(ProjectDesignSurface),
                new FrameworkPropertyMetadata(true));
        public static readonly DependencyProperty IsNotDraggingConnectionProperty = _isNotDraggingConnectionPropertyKey.DependencyProperty;

        public static readonly DependencyProperty EnableNodeDraggingProperty =
            DependencyProperty.Register("EnableNodeDragging", typeof(bool), typeof(ProjectDesignSurface),
                new FrameworkPropertyMetadata(true));

        private static readonly DependencyPropertyKey _isDraggingNodePropertyKey =
            DependencyProperty.RegisterReadOnly("IsDraggingNode", typeof(bool), typeof(ProjectDesignSurface),
                new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty IsDraggingNodeProperty = _isDraggingNodePropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey _isNotDraggingNodePropertyKey =
            DependencyProperty.RegisterReadOnly("IsNotDraggingNode", typeof(bool), typeof(ProjectDesignSurface),
                new FrameworkPropertyMetadata(true));
        public static readonly DependencyProperty IsNotDraggingNodeProperty = _isDraggingNodePropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey _isDraggingPropertyKey =
            DependencyProperty.RegisterReadOnly("IsDragging", typeof(bool), typeof(ProjectDesignSurface),
                new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty IsDraggingProperty = _isDraggingPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey _isNotDraggingPropertyKey =
            DependencyProperty.RegisterReadOnly("IsNotDragging", typeof(bool), typeof(ProjectDesignSurface),
                new FrameworkPropertyMetadata(true));
        public static readonly DependencyProperty IsNotDraggingProperty = _isNotDraggingPropertyKey.DependencyProperty;

        public static readonly DependencyProperty NodeItemTemplateProperty =
            DependencyProperty.Register("NodeItemTemplate", typeof(DataTemplate), typeof(ProjectDesignSurface));

        public static readonly DependencyProperty NodeItemTemplateSelectorProperty =
            DependencyProperty.Register("NodeItemTemplateSelector", typeof(DataTemplateSelector), typeof(ProjectDesignSurface));

        public static readonly DependencyProperty NodeItemContainerStyleProperty =
            DependencyProperty.Register("NodeItemContainerStyle", typeof(Style), typeof(ProjectDesignSurface));

        public static readonly DependencyProperty ConnectionItemTemplateProperty =
            DependencyProperty.Register("ConnectionItemTemplate", typeof(DataTemplate), typeof(ProjectDesignSurface));

        public static readonly DependencyProperty ConnectionItemTemplateSelectorProperty =
            DependencyProperty.Register("ConnectionItemTemplateSelector", typeof(DataTemplateSelector), typeof(ProjectDesignSurface));

        public static readonly DependencyProperty ConnectionItemContainerStyleProperty =
            DependencyProperty.Register("ConnectionItemContainerStyle", typeof(Style), typeof(ProjectDesignSurface));

        public static readonly RoutedEvent NodeDragStartedEvent =
            EventManager.RegisterRoutedEvent("NodeDragStarted", RoutingStrategy.Bubble, typeof(NodeDragStartedEventHandler), typeof(ProjectDesignSurface));

        public static readonly RoutedEvent NodeDraggingEvent =
            EventManager.RegisterRoutedEvent("NodeDragging", RoutingStrategy.Bubble, typeof(NodeDraggingEventHandler), typeof(ProjectDesignSurface));

        public static readonly RoutedEvent NodeDragCompletedEvent =
            EventManager.RegisterRoutedEvent("NodeDragCompleted", RoutingStrategy.Bubble, typeof(NodeDragCompletedEventHandler), typeof(ProjectDesignSurface));

        public static readonly RoutedEvent ConnectionDragStartedEvent =
            EventManager.RegisterRoutedEvent("ConnectionDragStarted", RoutingStrategy.Bubble, typeof(ConnectionDragStartedEventHandler), typeof(ProjectDesignSurface));

        public static readonly RoutedEvent QueryConnectionFeedbackEvent =
            EventManager.RegisterRoutedEvent("QueryConnectionFeedback", RoutingStrategy.Bubble, typeof(QueryConnectionFeedbackEventHandler), typeof(ProjectDesignSurface));

        public static readonly RoutedEvent ConnectionDraggingEvent =
            EventManager.RegisterRoutedEvent("ConnectionDragging", RoutingStrategy.Bubble, typeof(ConnectionDraggingEventHandler), typeof(ProjectDesignSurface));

        public static readonly RoutedEvent ConnectionDragCompletedEvent =
            EventManager.RegisterRoutedEvent("ConnectionDragCompleted", RoutingStrategy.Bubble, typeof(ConnectionDragCompletedEventHandler), typeof(ProjectDesignSurface));

        public static readonly RoutedCommand SelectAllCommand;
        public static readonly RoutedCommand SelectNoneCommand;
        public static readonly RoutedCommand InvertSelectionCommand;
        public static readonly RoutedCommand CancelConnectionDraggingCommand;

        #endregion Dependency Property/Event Definitions

        #region Private Data Members

        /// <summary>
        /// Cached reference to the NodeItemsControl in the visual-tree.
        /// </summary>
        private NodeItemsControl _nodeItemsControl;

        /// <summary>
        /// Cached reference to the ItemsControl for connections in the visual-tree.
        /// </summary>
        private ItemsControl _connectionItemsControl;

        /// <summary>
        /// Cached list of currently selected nodes.
        /// </summary>
        private List<object> _initialSelectedNodes;

        #endregion Private Data Members

        public ProjectDesignSurface()
        {
            //
            // Create a collection to contain nodes.
            //
            Nodes = new ImpObservableCollection<object>();

            //
            // Create a collection to contain connections.
            //
            Connections = new ImpObservableCollection<object>();

            //
            // Default background is white.
            //
            Background = Brushes.White;

            //
            // Add handlers for node and connector drag events.
            //
            AddHandler(NodeItem.NodeDragStartedEvent, new NodeDragStartedEventHandler(NodeItem_DragStarted));
            AddHandler(NodeItem.NodeDraggingEvent, new NodeDraggingEventHandler(NodeItem_Dragging));
            AddHandler(NodeItem.NodeDragCompletedEvent, new NodeDragCompletedEventHandler(NodeItem_DragCompleted));
            AddHandler(ConnectorItem.ConnectorDragStartedEvent, new ConnectorItemDragStartedEventHandler(ConnectorItem_DragStarted));
            AddHandler(ConnectorItem.ConnectorDraggingEvent, new ConnectorItemDraggingEventHandler(ConnectorItem_Dragging));
            AddHandler(ConnectorItem.ConnectorDragCompletedEvent, new ConnectorItemDragCompletedEventHandler(ConnectorItem_DragCompleted));
        }

        /// <summary>
        /// Event raised when the user starts dragging a node in the network.
        /// </summary>
        public event NodeDragStartedEventHandler NodeDragStarted
        {
            add => AddHandler(NodeDragStartedEvent, value);
            remove => RemoveHandler(NodeDragStartedEvent, value);
        }

        /// <summary>
        /// Event raised while user is dragging a node in the network.
        /// </summary>
        public event NodeDraggingEventHandler NodeDragging
        {
            add => AddHandler(NodeDraggingEvent, value);
            remove => RemoveHandler(NodeDraggingEvent, value);
        }

        /// <summary>
        /// Event raised when the user has completed dragging a node in the network.
        /// </summary>
        public event NodeDragCompletedEventHandler NodeDragCompleted
        {
            add => AddHandler(NodeDragCompletedEvent, value);
            remove => RemoveHandler(NodeDragCompletedEvent, value);
        }

        /// <summary>
        /// Event raised when the user starts dragging a connector in the network.
        /// </summary>
        public event ConnectionDragStartedEventHandler ConnectionDragStarted
        {
            add => AddHandler(ConnectionDragStartedEvent, value);
            remove => RemoveHandler(ConnectionDragStartedEvent, value);
        }

        /// <summary>
        /// Event raised while user drags a connection over the connector of a node in the network.
        /// The event handlers should supply a feedback objects and data-template that displays the 
        /// object as an appropriate graphic.
        /// </summary>
        public event QueryConnectionFeedbackEventHandler QueryConnectionFeedback
        {
            add => AddHandler(QueryConnectionFeedbackEvent, value);
            remove => RemoveHandler(QueryConnectionFeedbackEvent, value);
        }

        /// <summary>
        /// Event raised when a connection is being dragged.
        /// </summary>
        public event ConnectionDraggingEventHandler ConnectionDragging
        {
            add => AddHandler(ConnectionDraggingEvent, value);
            remove => RemoveHandler(ConnectionDraggingEvent, value);
        }

        /// <summary>
        /// Event raised when the user has completed dragging a connection in the network.
        /// </summary>
        public event ConnectionDragCompletedEventHandler ConnectionDragCompleted
        {
            add => AddHandler(ConnectionDragCompletedEvent, value);
            remove => RemoveHandler(ConnectionDragCompletedEvent, value);
        }

        /// <summary>
        /// Collection of nodes in the network.
        /// </summary>
        public ImpObservableCollection<object> Nodes
        {
            get => (ImpObservableCollection<object>)GetValue(NodesProperty);
            private set => SetValue(_nodesPropertyKey, value);
        }

        /// <summary>
        /// Collection of connections in the network.
        /// </summary>
        public ImpObservableCollection<object> Connections
        {
            get => (ImpObservableCollection<object>)GetValue(ConnectionsProperty);
            private set => SetValue(_connectionsPropertyKey, value);
        }

        /// <summary>
        /// A reference to the collection that is the source used to populate 'Nodes'.
        /// Used in the same way as 'ItemsSource' in 'ItemsControl'.
        /// </summary>
        public IEnumerable NodesSource
        {
            get => (IEnumerable)GetValue(NodesSourceProperty);
            set => SetValue(NodesSourceProperty, value);
        }

        /// <summary>
        /// A reference to the collection that is the source used to populate 'Connections'.
        /// Used in the same way as 'ItemsSource' in 'ItemsControl'.
        /// </summary>
        public IEnumerable ConnectionsSource
        {
            get => (IEnumerable)GetValue(ConnectionsSourceProperty);
            set => SetValue(ConnectionsSourceProperty, value);
        }

        /// <summary>
        /// Set to 'true' to enable the clearing of selection when empty space is clicked.
        /// This is set to 'true' by default.
        /// </summary>
        public bool IsClearSelectionOnEmptySpaceClickEnabled
        {
            get => (bool)GetValue(IsClearSelectionOnEmptySpaceClickEnabledProperty);
            set => SetValue(IsClearSelectionOnEmptySpaceClickEnabledProperty, value);
        }

        /// <summary>
        /// Set to 'true' to enable drag out of connectors to create new connections.
        /// </summary>
        public bool EnableConnectionDragging
        {
            get => (bool)GetValue(EnableConnectionDraggingProperty);
            set => SetValue(EnableConnectionDraggingProperty, value);
        }

        /// <summary>
        /// Dependency property that is set to 'true' when the user is 
        /// dragging out a connection.
        /// </summary>
        public bool IsDraggingConnection
        {
            get => (bool)GetValue(IsDraggingConnectionProperty);
            private set => SetValue(_isDraggingConnectionPropertyKey, value);
        }

        /// <summary>
        /// Dependency property that is set to 'false' when the user is 
        /// dragging out a connection.
        /// </summary>
        public bool IsNotDraggingConnection
        {
            get => (bool)GetValue(IsNotDraggingConnectionProperty);
            private set => SetValue(_isNotDraggingConnectionPropertyKey, value);
        }

        /// <summary>
        /// Set to 'true' to enable dragging of nodes.
        /// </summary>
        public bool EnableNodeDragging
        {
            get => (bool)GetValue(EnableNodeDraggingProperty);
            set => SetValue(EnableNodeDraggingProperty, value);
        }

        /// <summary>
        /// Dependency property that is set to 'true' when the user is 
        /// dragging out a connection.
        /// </summary>
        public bool IsDraggingNode
        {
            get => (bool)GetValue(IsDraggingNodeProperty);
            private set => SetValue(_isDraggingNodePropertyKey, value);
        }

        /// <summary>
        /// Dependency property that is set to 'false' when the user is 
        /// dragging out a connection.
        /// </summary>
        public bool IsNotDraggingNode
        {
            get => (bool)GetValue(IsNotDraggingNodeProperty);
            private set => SetValue(_isNotDraggingNodePropertyKey, value);
        }

        /// <summary>
        /// Set to 'true' when the user is dragging either a node or a connection.
        /// </summary>
        public bool IsDragging
        {
            get => (bool)GetValue(IsDraggingProperty);
            private set => SetValue(_isDraggingPropertyKey, value);
        }

        /// <summary>
        /// Set to 'true' when the user is not dragging anything.
        /// </summary>
        public bool IsNotDragging
        {
            get => (bool)GetValue(IsNotDraggingProperty);
            private set => SetValue(_isNotDraggingPropertyKey, value);
        }

        /// <summary>
        /// Gets or sets the DataTemplate used to display each node item.
        /// This is the equivalent to 'ItemTemplate' for ItemsControl.
        /// </summary>
        public DataTemplate NodeItemTemplate
        {
            get => (DataTemplate)GetValue(NodeItemTemplateProperty);
            set => SetValue(NodeItemTemplateProperty, value);
        }

        /// <summary>
        /// Gets or sets custom style-selection logic for a style that can be applied to each generated container element. 
        /// This is the equivalent to 'ItemTemplateSelector' for ItemsControl.
        /// </summary>
        public DataTemplateSelector NodeItemTemplateSelector
        {
            get => (DataTemplateSelector)GetValue(NodeItemTemplateSelectorProperty);
            set => SetValue(NodeItemTemplateSelectorProperty, value);
        }

        /// <summary>
        /// Gets or sets the Style that is applied to the item container for each node item.
        /// This is the equivalent to 'ItemContainerStyle' for ItemsControl.
        /// </summary>
        public Style NodeItemContainerStyle
        {
            get => (Style)GetValue(NodeItemContainerStyleProperty);
            set => SetValue(NodeItemContainerStyleProperty, value);
        }

        /// <summary>
        /// Gets or sets the DataTemplate used to display each connection item.
        /// This is the equivalent to 'ItemTemplate' for ItemsControl.
        /// </summary>
        public DataTemplate ConnectionItemTemplate
        {
            get => (DataTemplate)GetValue(ConnectionItemTemplateProperty);
            set => SetValue(ConnectionItemTemplateProperty, value);
        }

        /// <summary>
        /// Gets or sets custom style-selection logic for a style that can be applied to each generated container element. 
        /// This is the equivalent to 'ItemTemplateSelector' for ItemsControl.
        /// </summary>
        public DataTemplateSelector ConnectionItemTemplateSelector
        {
            get => (DataTemplateSelector)GetValue(ConnectionItemTemplateSelectorProperty);
            set => SetValue(ConnectionItemTemplateSelectorProperty, value);
        }

        /// <summary>
        /// Gets or sets the Style that is applied to the item container for each connection item.
        /// This is the equivalent to 'ItemContainerStyle' for ItemsControl.
        /// </summary>
        public Style ConnectionItemContainerStyle
        {
            get => (Style)GetValue(ConnectionItemContainerStyleProperty);
            set => SetValue(ConnectionItemContainerStyleProperty, value);
        }

        /// <summary>
        /// A reference to currently selected node.
        /// </summary>
        public object SelectedNode
        {
            get
            {
                if (_nodeItemsControl != null)
                {
                    return _nodeItemsControl.SelectedItem;
                }
                else
                {
                    if (_initialSelectedNodes == null)
                    {
                        return null;
                    }

                    if (_initialSelectedNodes.Count != 1)
                    {
                        return null;
                    }

                    return _initialSelectedNodes[0];
                }
            }
            set
            {
                if (_nodeItemsControl != null)
                {
                    _nodeItemsControl.SelectedItem = value;
                }
                else
                {
                    if (_initialSelectedNodes == null)
                    {
                        _initialSelectedNodes = new List<object>();
                    }

                    _initialSelectedNodes.Clear();
                    _initialSelectedNodes.Add(value);
                }
            }
        }

        /// <summary>
        /// A list of selected nodes.
        /// </summary>
        public IList SelectedNodes
        {
            get
            {
                if (_nodeItemsControl != null)
                {
                    return _nodeItemsControl.SelectedItems;
                }
                else
                {
                    if (_initialSelectedNodes == null)
                    {
                        _initialSelectedNodes = new List<object>();
                    }

                    return _initialSelectedNodes;
                }
            }
        }

        /// <summary>
        /// An event raised when the nodes selected in the ProjectDesignSurface has changed.
        /// </summary>
        public event SelectionChangedEventHandler SelectionChanged;

        /// <summary>
        /// Bring the currently selected nodes into view.
        /// This affects ContentViewportOffsetX/ContentViewportOffsetY, but doesn't affect 'ContentScale'.
        /// </summary>
        public void BringSelectedNodesIntoView()
        {
            BringNodesIntoView(SelectedNodes);
        }

        /// <summary>
        /// Bring the collection of nodes into view.
        /// This affects ContentViewportOffsetX/ContentViewportOffsetY, but doesn't affect 'ContentScale'.
        /// </summary>
        public void BringNodesIntoView(ICollection nodes)
        {
            if (nodes == null)
            {
                throw new ArgumentNullException("'nodes' argument shouldn't be null.");
            }

            if (nodes.Count == 0)
            {
                return;
            }

            var rect = Rect.Empty;

            foreach (var node in nodes)
            {
                var nodeItem = FindAssociatedNodeItem(node);
                var nodeRect = new Rect(nodeItem.X, nodeItem.Y, nodeItem.ActualWidth, nodeItem.ActualHeight);

                if (rect == Rect.Empty)
                {
                    rect = nodeRect;
                }
                else
                {
                    rect.Intersect(nodeRect);
                }
            }

            BringIntoView(rect);
        }

        /// <summary>
        /// Clear the selection.
        /// </summary>
        public void SelectNone()
        {
            SelectedNodes.Clear();
        }

        /// <summary>
        /// Selects all of the nodes.
        /// </summary>
        public void SelectAll()
        {
            if (SelectedNodes.Count != Nodes.Count)
            {
                SelectedNodes.Clear();
                foreach (var node in Nodes)
                {
                    SelectedNodes.Add(node);
                }
            }
        }

        /// <summary>
        /// Inverts the current selection.
        /// </summary>
        public void InvertSelection()
        {
            var selectedNodesCopy = new ArrayList(SelectedNodes);
            SelectedNodes.Clear();

            foreach (var node in Nodes)
            {
                if (!selectedNodesCopy.Contains(node))
                {
                    SelectedNodes.Add(node);
                }
            }
        }

        /// <summary>
        /// When connection dragging is progress this function cancels it.
        /// </summary>
        public void CancelConnectionDragging()
        {
            if (!IsDraggingConnection)
            {
                return;
            }

            //
            // Now that connection dragging has completed, don't any feedback adorner.
            //
            ClearFeedbackAdorner();

            _draggedOutConnectorItem.CancelConnectionDragging();

            IsDragging = false;
            IsNotDragging = true;
            IsDraggingConnection = false;
            IsNotDraggingConnection = true;
            _draggedOutConnectorItem = null;
            _draggedOutNodeDataContext = null;
            _draggedOutConnectorDataContext = null;
            _draggingConnectionDataContext = null;
        }

        #region Private Methods

        /// <summary>
        /// Static constructor.
        /// </summary>
        static ProjectDesignSurface()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ProjectDesignSurface), new FrameworkPropertyMetadata(typeof(ProjectDesignSurface)));

            var inputs = new InputGestureCollection { new KeyGesture(Key.A, ModifierKeys.Control) };
            SelectAllCommand = new RoutedCommand("SelectAll", typeof(ProjectDesignSurface), inputs);

            inputs = new InputGestureCollection { new KeyGesture(Key.Escape) };
            SelectNoneCommand = new RoutedCommand("SelectNone", typeof(ProjectDesignSurface), inputs);

            inputs = new InputGestureCollection { new KeyGesture(Key.I, ModifierKeys.Control) };
            InvertSelectionCommand = new RoutedCommand("InvertSelection", typeof(ProjectDesignSurface), inputs);

            CancelConnectionDraggingCommand = new RoutedCommand("CancelConnectionDragging", typeof(ProjectDesignSurface));

            var binding = new CommandBinding
            {
                Command = SelectAllCommand
            };
            binding.Executed += SelectAll_Executed;
            CommandManager.RegisterClassCommandBinding(typeof(ProjectDesignSurface), binding);

            binding = new CommandBinding
            {
                Command = SelectNoneCommand
            };
            binding.Executed += SelectNone_Executed;
            CommandManager.RegisterClassCommandBinding(typeof(ProjectDesignSurface), binding);

            binding = new CommandBinding
            {
                Command = InvertSelectionCommand
            };
            binding.Executed += InvertSelection_Executed;
            CommandManager.RegisterClassCommandBinding(typeof(ProjectDesignSurface), binding);

            binding = new CommandBinding
            {
                Command = CancelConnectionDraggingCommand
            };
            binding.Executed += CancelConnectionDragging_Executed;
            CommandManager.RegisterClassCommandBinding(typeof(ProjectDesignSurface), binding);
        }

        /// <summary>
        /// Executes the 'SelectAll' command.
        /// </summary>
        private static void SelectAll_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var c = (ProjectDesignSurface)sender;
            c.SelectAll();
        }

        /// <summary>
        /// Executes the 'SelectNone' command.
        /// </summary>
        private static void SelectNone_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var c = (ProjectDesignSurface)sender;
            c.SelectNone();
        }

        /// <summary>
        /// Executes the 'InvertSelection' command.
        /// </summary>
        private static void InvertSelection_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var c = (ProjectDesignSurface)sender;
            c.InvertSelection();
        }

        /// <summary>
        /// Executes the 'CancelConnectionDragging' command.
        /// </summary>
        private static void CancelConnectionDragging_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var c = (ProjectDesignSurface)sender;
            c.CancelConnectionDragging();
        }

        /// <summary>
        /// Event raised when a new collection has been assigned to the 'NodesSource' property.
        /// </summary>
        private static void NodesSource_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var projectDesignSurface = (ProjectDesignSurface)d;

            //
            // Clear 'Nodes'.
            //
            projectDesignSurface.Nodes.Clear();

            if (e.OldValue != null)
            {
                if (e.OldValue is INotifyCollectionChanged notifyCollectionChanged)
                {
                    //
                    // Unhook events from previous collection.
                    //
                    notifyCollectionChanged.CollectionChanged -= projectDesignSurface.NodesSource_CollectionChanged;
                }
            }

            if (e.NewValue != null)
            {
                if (e.NewValue is IEnumerable enumerable)
                {
                    //
                    // Populate 'Nodes' from 'NodesSource'.
                    //
                    foreach (var obj in enumerable)
                    {
                        projectDesignSurface.Nodes.Add(obj);
                    }
                }

                if (e.NewValue is INotifyCollectionChanged notifyCollectionChanged)
                {
                    //
                    // Hook events in new collection.
                    //
                    notifyCollectionChanged.CollectionChanged += projectDesignSurface.NodesSource_CollectionChanged;
                }
            }
        }

        /// <summary>
        /// Event raised when a node has been added to or removed from the collection assigned to 'NodesSource'.
        /// </summary>
        private void NodesSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                //
                // 'NodesSource' has been cleared, also clear 'Nodes'.
                //
                Nodes.Clear();
            }
            else
            {
                if (e.OldItems != null)
                {
                    //
                    // For each item that has been removed from 'NodesSource' also remove it from 'Nodes'.
                    //
                    foreach (var obj in e.OldItems)
                    {
                        Nodes.Remove(obj);
                    }
                }

                if (e.NewItems != null)
                {
                    //
                    // For each item that has been added to 'NodesSource' also add it to 'Nodes'.
                    //
                    foreach (var obj in e.NewItems)
                    {
                        Nodes.Add(obj);
                    }
                }
            }
        }

        /// <summary>
        /// Event raised when a new collection has been assigned to the 'ConnectionsSource' property.
        /// </summary>
        private static void ConnectionsSource_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var c = (ProjectDesignSurface)d;

            //
            // Clear 'Connections'.
            //
            c.Connections.Clear();

            if (e.OldValue != null)
            {
                if (e.NewValue is INotifyCollectionChanged notifyCollectionChanged)
                {
                    //
                    // Unhook events from previous collection.
                    //
                    notifyCollectionChanged.CollectionChanged -= c.ConnectionsSource_CollectionChanged;
                }
            }

            if (e.NewValue != null)
            {
                if (e.NewValue is IEnumerable enumerable)
                {
                    //
                    // Populate 'Connections' from 'ConnectionsSource'.
                    //
                    foreach (var obj in enumerable)
                    {
                        c.Connections.Add(obj);
                    }
                }

                if (e.NewValue is INotifyCollectionChanged notifyCollectionChanged)
                {
                    //
                    // Hook events in new collection.
                    //
                    notifyCollectionChanged.CollectionChanged += c.ConnectionsSource_CollectionChanged;
                }
            }
        }

        /// <summary>
        /// Event raised when a connection has been added to or removed from the collection assigned to 'ConnectionsSource'.
        /// </summary>
        private void ConnectionsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                //
                // 'ConnectionsSource' has been cleared, also clear 'Connections'.
                //
                Connections.Clear();
            }
            else
            {
                if (e.OldItems != null)
                {
                    //
                    // For each item that has been removed from 'ConnectionsSource' also remove it from 'Connections'.
                    //
                    foreach (var obj in e.OldItems)
                    {
                        Connections.Remove(obj);
                    }
                }

                if (e.NewItems != null)
                {
                    //
                    // For each item that has been added to 'ConnectionsSource' also add it to 'Connections'.
                    //
                    foreach (var obj in e.NewItems)
                    {
                        Connections.Add(obj);
                    }
                }
            }
        }

        /// <summary>
        /// Called after the visual tree of the control has been built.
        /// Search for and cache references to named parts defined in the XAML control template for ProjectDesignSurface.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            //
            // Cache the parts of the visual tree that we need access to later.
            //

            _nodeItemsControl = (NodeItemsControl)Template.FindName("PART_NodeItemsControl", this);
            if (_nodeItemsControl == null)
            {
                throw new ApplicationException("Failed to find 'PART_NodeItemsControl' in the visual tree for 'ProjectDesignSurface'.");
            }

            //
            // Synchronize initial selected nodes to the NodeItemsControl.
            //
            if (_initialSelectedNodes != null && _initialSelectedNodes.Count > 0)
            {
                foreach (var node in _initialSelectedNodes)
                {
                    _nodeItemsControl.SelectedItems.Add(node);
                }
            }

            _initialSelectedNodes = null; // Don't need this any more.

            _nodeItemsControl.SelectionChanged += nodeItemsControl_SelectionChanged;

            _connectionItemsControl = (ItemsControl)Template.FindName("PART_ConnectionItemsControl", this);
            if (_connectionItemsControl == null)
            {
                throw new ApplicationException("Failed to find 'PART_ConnectionItemsControl' in the visual tree for 'ProjectDesignSurface'.");
            }

            _dragSelectionCanvas = (FrameworkElement)Template.FindName("PART_DragSelectionCanvas", this);
            if (_dragSelectionCanvas == null)
            {
                throw new ApplicationException("Failed to find 'PART_DragSelectionCanvas' in the visual tree for 'ProjectDesignSurface'.");
            }

            _dragSelectionBorder = (FrameworkElement)Template.FindName("PART_DragSelectionBorder", this);
            if (_dragSelectionBorder == null)
            {
                throw new ApplicationException("Failed to find 'PART_dragSelectionBorder' in the visual tree for 'ProjectDesignSurface'.");
            }
        }

        /// <summary>
        /// Event raised when the selection in 'nodeItemsControl' changes.
        /// </summary>
        private void nodeItemsControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectionChanged != null)
            {
                SelectionChanged(this, new SelectionChangedEventArgs(Selector.SelectionChangedEvent, e.RemovedItems, e.AddedItems));
            }
        }

        /// <summary>
        /// Find the max ZIndex of all the nodes.
        /// </summary>
        internal int FindMaxZIndex()
        {
            if (_nodeItemsControl == null)
            {
                return 0;
            }

            var maxZ = 0;

            for (var nodeIndex = 0; ; ++nodeIndex)
            {
                var nodeItem = (NodeItem)_nodeItemsControl.ItemContainerGenerator.ContainerFromIndex(nodeIndex);
                if (nodeItem == null)
                {
                    break;
                }

                if (nodeItem.ZIndex > maxZ)
                {
                    maxZ = nodeItem.ZIndex;
                }
            }

            return maxZ;
        }

        /// <summary>
        /// Find the NodeItem UI element that is associated with 'node'.
        /// 'node' can be a view-model object, in which case the visual-tree
        /// is searched for the associated NodeItem.
        /// Otherwise 'node' can actually be a 'NodeItem' in which case it is 
        /// simply returned.
        /// </summary>
        internal NodeItem FindAssociatedNodeItem(object node)
        {
            var nodeItem = node as NodeItem;
            if (nodeItem == null)
            {
                nodeItem = _nodeItemsControl.FindAssociatedNodeItem(node);
            }
            return nodeItem;
        }

        #endregion Private Methods
    }

}
