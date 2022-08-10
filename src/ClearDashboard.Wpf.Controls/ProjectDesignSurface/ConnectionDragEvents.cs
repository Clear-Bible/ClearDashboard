using System.Windows;

namespace ClearDashboard.Wpf.Controls
{
    /// <summary>
    /// Base class for connection dragging event args.
    /// </summary>
    public class ConnectionDragEventArgs : RoutedEventArgs
    {
        #region Private Data Members

        /// <summary>
        /// The connector that will be dragged out.
        /// </summary>
        protected object Connection;

        #endregion Private Data Members

        /// <summary>
        /// The NodeItem or it's DataContext (when non-NULL).
        /// </summary>
        public object Node { get; }

        /// <summary>
        /// The ConnectorItem or it's DataContext (when non-NULL).
        /// </summary>
        public object ConnectorDraggedOut { get; }

        #region Private Methods

        protected ConnectionDragEventArgs(RoutedEvent routedEvent, object source, object node, object connection, object connector) :
            base(routedEvent, source)
        {
            Node = node;
            ConnectorDraggedOut = connector;
            Connection = connection;
        }

        #endregion Private Methods
    }

    /// <summary>
    /// Arguments for event raised when the user starts to drag a connection out from a node.
    /// </summary>
    public class ConnectionDragStartedEventArgs : ConnectionDragEventArgs
    {
        /// <summary>
        /// The connection that will be dragged out.
        /// </summary>
        public new object Connection
        {
            get => base.Connection;
            set => base.Connection = value;
        }

        #region Private Methods

        internal ConnectionDragStartedEventArgs(RoutedEvent routedEvent, object source, object node, object connector) :
            base(routedEvent, source, node, null, connector)
        {
        }

        #endregion Private Methods
    }

    /// <summary>
    /// Defines the event handler for the ConnectionDragStarted event.
    /// </summary>
    public delegate void ConnectionDragStartedEventHandler(object sender, ConnectionDragStartedEventArgs e);

    /// <summary>
    /// Arguments for event raised while user is dragging a node in the network.
    /// </summary>
    public class QueryConnectionFeedbackEventArgs : ConnectionDragEventArgs
    {
        #region Private Data Members

        #endregion Private Data Members

        /// <summary>
        /// The ConnectorItem or it's DataContext (when non-NULL).
        /// </summary>
        public object DraggedOverConnector { get; }

        /// <summary>
        /// The connection that will be dragged out.
        /// </summary>
        public new object Connection => base.Connection;

        /// <summary>
        /// Set to 'true' / 'false' to indicate that the connection from the dragged out connection to the dragged over connector is valid.
        /// </summary>
        public bool ConnectionOk { get; set; } = true;

        /// <summary>
        /// The indicator to display.
        /// </summary>
        public object FeedbackIndicator { get; set; }

        #region Private Methods

        internal QueryConnectionFeedbackEventArgs(RoutedEvent routedEvent, object source, 
            object node, object connection, object connector, object draggedOverConnector) :
            base(routedEvent, source, node, connection, connector)
        {
            DraggedOverConnector = draggedOverConnector;
        }

        #endregion Private Methods
    }

    /// <summary>
    /// Defines the event handler for the QueryConnectionFeedback event.
    /// </summary>
    public delegate void QueryConnectionFeedbackEventHandler(object sender, QueryConnectionFeedbackEventArgs e);

    /// <summary>
    /// Arguments for event raised while user is dragging a node in the network.
    /// </summary>
    public class ConnectionDraggingEventArgs : ConnectionDragEventArgs
    {
        /// <summary>
        /// The connection being dragged out.
        /// </summary>
        public new object Connection => base.Connection;

        #region Private Methods

        internal ConnectionDraggingEventArgs(RoutedEvent routedEvent, object source,
                object node, object connection, object connector) :
            base(routedEvent, source, node, connection, connector)
        {
        }

        #endregion Private Methods
    }

    /// <summary>
    /// Defines the event handler for the ConnectionDragging event.
    /// </summary>
    public delegate void ConnectionDraggingEventHandler(object sender, ConnectionDraggingEventArgs e);

    /// <summary>
    /// Arguments for event raised when the user has completed dragging a connector.
    /// </summary>
    public class ConnectionDragCompletedEventArgs : ConnectionDragEventArgs
    {
        #region Private Data Members

        #endregion Private Data Members

        /// <summary>
        /// The ConnectorItem or it's DataContext (when non-NULL).
        /// </summary>
        public object ConnectorDraggedOver { get; }

        /// <summary>
        /// The connection that will be dragged out.
        /// </summary>
        public new object Connection => base.Connection;

        #region Private Methods

        internal ConnectionDragCompletedEventArgs(RoutedEvent routedEvent, object source, object node, object connection, object connector, object connectorDraggedOver) :
            base(routedEvent, source, node, connection, connector)
        {
            ConnectorDraggedOver = connectorDraggedOver;
        }

        #endregion Private Methods
    }

    /// <summary>
    /// Defines the event handler for the ConnectionDragCompleted event.
    /// </summary>
    public delegate void ConnectionDragCompletedEventHandler(object sender, ConnectionDragCompletedEventArgs e);
}
