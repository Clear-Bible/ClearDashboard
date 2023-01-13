using System;
using System.Collections.Generic;

namespace ClearDashboard.Wpf.Controls
{
    /// <summary>
    /// Partial definition of the ProjectDesignSurface class.
    /// This file only contains private members related to dragging nodes.
    /// </summary>
    public partial class ProjectDesignSurface
    {
        #region Private Methods

        /// <summary>
        /// Event raised when the user starts to drag a node.
        /// </summary>
        private void NodeItem_DragStarted(object source, NodeDragStartedEventArgs e)
        {
            e.Handled = true;

            IsDragging = true;
            IsNotDragging = false;
            IsDraggingNode = true;
            IsNotDraggingNode = false;

            var eventArgs = new NodeDragStartedEventArgs(NodeDragStartedEvent, this, SelectedNodes);            
            RaiseEvent(eventArgs);

            e.Cancel = eventArgs.Cancel;
        }

        /// <summary>
        /// Event raised while the user is dragging a node.
        /// </summary>
        private void NodeItem_Dragging(object source, NodeDraggingEventArgs e)
        {
            e.Handled = true;

            //
            // Cache the NodeItem for each selected node whilst dragging is in progress.
            //
            if (_cachedSelectedNodeItems == null)
            {
                _cachedSelectedNodeItems = new List<NodeItem>();

                foreach (var selectedNode in SelectedNodes)
                {
                    var nodeItem = FindAssociatedNodeItem(selectedNode);
                    if (nodeItem == null)
                    {
                        throw new ApplicationException("Unexpected code path!");
                    }

                    _cachedSelectedNodeItems.Add(nodeItem);
                }
            }

            // 
            // Update the position of the node within the Canvas.
            //
            foreach (var nodeItem in _cachedSelectedNodeItems)
            {
                if (this.ActualWidth - nodeItem.ActualWidth > nodeItem.X + e.HorizontalChange && nodeItem.X + e.HorizontalChange > 0)
                {
                    nodeItem.X += e.HorizontalChange;
                }
                if (this.ActualHeight - nodeItem.ActualHeight > nodeItem.Y + e.VerticalChange && nodeItem.Y + e.VerticalChange > 0)
                {
                    nodeItem.Y += e.VerticalChange;
                }
            }

            var eventArgs = new NodeDraggingEventArgs(NodeDraggingEvent, this, SelectedNodes, e.HorizontalChange, e.VerticalChange);
            RaiseEvent(eventArgs);
        }

        /// <summary>
        /// Event raised when the user has finished dragging a node.
        /// </summary>
        private void NodeItem_DragCompleted(object source, NodeDragCompletedEventArgs e)
        {
            e.Handled = true;

            var eventArgs = new NodeDragCompletedEventArgs(NodeDragCompletedEvent, this, SelectedNodes);
            RaiseEvent(eventArgs);

            if (_cachedSelectedNodeItems != null)
            {
                _cachedSelectedNodeItems = null;
            }

            IsDragging = false;
            IsNotDragging = true;
            IsDraggingNode = false;
            IsNotDraggingNode = true;
        }

        #endregion Private Methods
    }
}
