using ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface;
using ClearDashboard.Wpf.Application.ViewModels.Project;
using ClearDashboard.Wpf.Controls;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace ClearDashboard.Wpf.Application.Views.Project
{
    /// <summary>
    /// Interaction logic for ReadonlyProjectDesignSurfaceView.xaml
    /// </summary>
    public partial class ReadonlyProjectDesignSurfaceView : UserControl
    {
        public ReadonlyProjectDesignSurfaceView()
        {
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

        }

        /// <summary>
        /// Convenient accessor for the view-model.
        /// </summary>
        public ReadonlyProjectDesignSurfaceViewModel ProjectDesignSurfaceViewModel => (ReadonlyProjectDesignSurfaceViewModel)DataContext;

        /// <summary>
        /// Event raised while the user is dragging a connection.
        /// NB:  This method cannot be moved to the view model as Mouse.GetPosition always returns a Point - (0,0)
        /// </summary>
        private void OnProjectDesignSurfaceConnectionDragging(object sender, ConnectionDraggingEventArgs e)
        {
            var curDragPoint = Mouse.GetPosition(ProjectDesignSurface);
            var connection = (ParallelCorpusConnectionViewModel)e.Connection;
            this.ProjectDesignSurfaceViewModel.DesignSurfaceViewModel!.ConnectionDragging(curDragPoint, connection);
        }

        ///// <summary>
        ///// Event raised to delete the selected node.
        ///// </summary>
        //private void OnDeleteSelectedNodesExecuted(object sender, ExecutedRoutedEventArgs e)
        //{
        //    //this.ViewModel.DeleteSelectedNodes();
        //}

        ///// <summary>
        ///// Event raised to create a new node.
        ///// </summary>
        //private void OnCreateCorpusNodeExecuted(object sender, ExecutedRoutedEventArgs e)
        //{
        //    //CreateNode();
        //}

        /// <summary>
        /// Event raised to delete a CorpusNode.
        /// </summary>
        private void OnDeleteCorpusNodeExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var node = (CorpusNodeViewModel)e.Parameter;
            ProjectDesignSurfaceViewModel!.DeleteCorpusNode(node);
        }

        /// <summary>
        /// Event raised to delete a ParallelCorpusConnection.
        /// </summary>
        private void OnDeleteConnectionExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var connection = (ParallelCorpusConnectionViewModel)e.Parameter;
            ProjectDesignSurfaceViewModel!.DeleteParallelCorpusConnection(connection);
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

       
        private void OnCorpusNodeProperties(object sender, ExecutedRoutedEventArgs e)
        {
            var corpus = (CorpusNodeViewModel)e.Parameter;
            this.ProjectDesignSurfaceViewModel.ShowCorpusProperties(corpus);

        }

        private void OnConnectionProperties(object sender, ExecutedRoutedEventArgs e)
        {
            var connection = (ParallelCorpusConnectionViewModel)e.Parameter;
            this.ProjectDesignSurfaceViewModel.ShowConnectionProperties(connection);
        }
    }
}
