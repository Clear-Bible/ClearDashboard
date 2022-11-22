using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Wpf.Application.ViewModels;
using ClearDashboard.Wpf.Controls;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface;
using ClearDashboard.Wpf.Application.ViewModels.Project;


namespace ClearDashboard.Wpf.Application.Views.Project
{
    /// <summary>
    /// Interaction logic for ProjectDesignSurfaceView.xaml
    /// </summary>
    public partial class ProjectDesignSurfaceView : UserControl
    {
        public ProjectDesignSurfaceView()
        {
            InitializeComponent();
        }


        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

        }

        //public void AddControl(FrameworkElement control)
        //{
        //    DesignSurfaceCanvas.Dispatcher.Invoke(() => { DesignSurfaceCanvas.Children.Add(control); });


        //}

        /// <summary>
        /// Convenient accessor for the view-model.
        /// </summary>
        public ProjectDesignSurfaceViewModel ViewModel => (ProjectDesignSurfaceViewModel)DataContext;





        ///// <summary>
        ///// Event raised, to query for feedback, while the user is dragging a connection.
        ///// </summary>
        //private void OnProjectDesignSurfaceQueryConnectionFeedback(object sender, QueryConnectionFeedbackEventArgs e)
        //{
        //    var draggedOutConnector = (ParallelCorpusConnectorViewModel)e.ConnectorDraggedOut;
        //    var draggedOverConnector = (ParallelCorpusConnectorViewModel)e.DraggedOverConnector;

        //    ViewModel.QueryConnectionFeedback(draggedOutConnector, draggedOverConnector, out var feedbackIndicator, out var connectionOk);

        //    //
        //    // Return the feedback object to ProjectDesignSurfaceView.
        //    // The object combined with the data-template for it will be used to create a 'feedback icon' to
        //    // display (in an adorner) to the user.
        //    //
        //    e.FeedbackIndicator = feedbackIndicator;

        //    //
        //    // Let ProjectDesignSurfaceView know if the connection is ok or not ok.
        //    //
        //    e.ConnectionOk = connectionOk;
        //}


        ///// <summary>
        ///// Event raised when the user has started to drag out a connection.
        ///// </summary>
        //private void OnProjectDesignSurfaceConnectionDragStarted(object sender, ConnectionDragStartedEventArgs e)
        //{
        //    if (ViewModel.IsBusy)
        //    {
        //        return;
        //    }

        //    var draggedOutConnector = (ParallelCorpusConnectorViewModel)e.ConnectorDraggedOut;
        //    var curDragPoint = Mouse.GetPosition(ProjectDesignSurface);

        //    //
        //    // Delegate the real work to the view model.
        //    //
        //    var connection = this.ViewModel.ConnectionDragStarted(draggedOutConnector, curDragPoint);

        //    //
        //    // Must return the view-model object that represents the connection via the event args.
        //    // This is so that ProjectDesignSurfaceView can keep track of the object while it is being dragged.
        //    //
        //    e.Connection = connection;
        //}

        /// <summary>
        /// Event raised while the user is dragging a connection.
        /// NB:  This method cannot be moved to the view model as Mouse.GetPosition always returns a Point - (0,0)
        /// </summary>
        private void OnProjectDesignSurfaceConnectionDragging(object sender, ConnectionDraggingEventArgs e)
        {
            var curDragPoint = Mouse.GetPosition(ProjectDesignSurface);
            var connection = (ParallelCorpusConnectionViewModel)e.Connection;
            this.ViewModel.ConnectionDragging(curDragPoint, connection);
        }

        ///// <summary>
        ///// Event raised when the user has finished dragging out a connection.
        ///// </summary>
        //private void OnProjectDesignSurfaceConnectionDragCompleted(object sender, ConnectionDragCompletedEventArgs e)
        //{
        //    var connectorDraggedOut = (ParallelCorpusConnectorViewModel)e.ConnectorDraggedOut;
        //    var connectorDraggedOver = (ParallelCorpusConnectorViewModel)e.ConnectorDraggedOver;
        //    var newConnection = (ParallelCorpusConnectionViewModel)e.Connection;
        //    this.ViewModel.ConnectionDragCompleted(newConnection, connectorDraggedOut, connectorDraggedOver);
        //}

        /// <summary>
        /// Event raised to delete the selected node.
        /// </summary>
        private void OnDeleteSelectedNodesExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            //this.ViewModel.DeleteSelectedNodes();
        }

        /// <summary>
        /// Event raised to create a new node.
        /// </summary>
        private void OnCreateCorpusNodeExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            CreateNode();
        }

        /// <summary>
        /// Event raised to delete a node.
        /// </summary>
        private void OnDeleteCorpusNodeExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var node = (CorpusNodeViewModel)e.Parameter;
            //this.ViewModel.DeleteNode(node);
        }

        /// <summary>
        /// Event raised to delete a connection.
        /// </summary>
        private void OnDeleteConnectionExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var connection = (ParallelCorpusConnectionViewModel)e.Parameter;
            //this.ViewModel.DeleteConnection(connection);
        }

        /// <summary>
        /// Creates a new node in the network at the current mouse location.
        /// </summary>
        private void CreateNode()
        {
            var newNodePosition = Mouse.GetPosition(ProjectDesignSurface);
            //this.ViewModel.CreateNode("New Corpus!", newNodePosition, true, CorpusType.Standard,
            //    Guid.NewGuid().ToString());
        }

        /// <summary>
        /// Event raised when the size of a node has changed.
        /// </summary>
        private void OnCorpusNodeSizeChanged(object sender, SizeChangedEventArgs e)
        {
            //
            // The size of a node, as determined in the UI by the node's data-template,
            // has changed.  Push the size of the node through to the view-model.
            //
            var element = (FrameworkElement)sender;
            var node = (CorpusNodeViewModel)element.DataContext;
            node.Size = new Size(element.ActualWidth, element.ActualHeight);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var node = e.Source as CorpusNodeViewModel;
        }

        private void OnCorpusNodeProperties(object sender, ExecutedRoutedEventArgs e)
        {
            var corpus = (CorpusNodeViewModel)e.Parameter;
            this.ViewModel.ShowCorpusProperties(corpus);

        }

        private void OnConnectionProperties(object sender, ExecutedRoutedEventArgs e)
        {
            var connection = (ParallelCorpusConnectionViewModel)e.Parameter;
            this.ViewModel.ShowConnectionProperties(connection);
        }
    }
}
