﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ClearDashboard.Wpf.Controls
{
    /// <summary>
    /// This is the UI element for a connector.
    /// Each nodes has multiple connectors that are used to connect it to other nodes.
    /// </summary>
    public class ConnectorItem : ContentControl
    {
        #region Dependency Property/Event Definitions

        public static readonly DependencyProperty HotspotProperty =
            DependencyProperty.Register("Hotspot", typeof(Point), typeof(ConnectorItem));

        internal static readonly DependencyProperty ParentProjectDesignSurfaceProperty =
            DependencyProperty.Register("ParentProjectDesignSurface", typeof(ProjectDesignSurface), typeof(ConnectorItem),
                new FrameworkPropertyMetadata(ParentProjectDesignSurface_PropertyChanged));

        internal static readonly DependencyProperty ParentNodeItemProperty =
            DependencyProperty.Register("ParentNodeItem", typeof(NodeItem), typeof(ConnectorItem));

        internal static readonly RoutedEvent ConnectorDragStartedEvent =
            EventManager.RegisterRoutedEvent("ConnectorDragStarted", RoutingStrategy.Bubble, typeof(ConnectorItemDragStartedEventHandler), typeof(ConnectorItem));

        internal static readonly RoutedEvent ConnectorDraggingEvent =
            EventManager.RegisterRoutedEvent("ConnectorDragging", RoutingStrategy.Bubble, typeof(ConnectorItemDraggingEventHandler), typeof(ConnectorItem));

        internal static readonly RoutedEvent ConnectorDragCompletedEvent =
            EventManager.RegisterRoutedEvent("ConnectorDragCompleted", RoutingStrategy.Bubble, typeof(ConnectorItemDragCompletedEventHandler), typeof(ConnectorItem));

        #endregion Dependency Property/Event Definitions

        #region Private Data Members

        /// <summary>
        /// The point the mouse was last at when dragging.
        /// </summary>
        private Point _lastMousePoint;

        /// <summary>
        /// Set to 'true' when left mouse button is held down.
        /// </summary>
        private bool _isLeftMouseDown;

        /// <summary>
        /// Set to 'true' when the user is dragging the connector.
        /// </summary>
        private bool _isDragging;

        /// <summary>
        /// The threshold distance the mouse-cursor must move before dragging begins.
        /// </summary>
        private const double DragThreshold = 2;

        #endregion Private Data Members

        public ConnectorItem()
        {
            //
            // By default, we don't want a connector to be focusable.
            //
            Focusable = false;

            //
            // Hook layout update to recompute 'Hotspot' when the layout changes.
            //
            LayoutUpdated += ConnectorItem_LayoutUpdated;
        }

        /// <summary>
        /// Automatically updated dependency property that specifies the hotspot (or center point) of the connector.
        /// Specified in content coordinate.
        /// </summary>
        public Point Hotspot
        {
            get => (Point)GetValue(HotspotProperty);
            set => SetValue(HotspotProperty, value);
        }

        #region Private Data Members\Properties

        /// <summary>
        /// Reference to the data-bound parent ProjectDesignSurface.
        /// </summary>
        internal ProjectDesignSurface ParentProjectDesignSurface
        {
            get => (ProjectDesignSurface)GetValue(ParentProjectDesignSurfaceProperty);
            set => SetValue(ParentProjectDesignSurfaceProperty, value);
        }

       
        /// <summary>
        /// Reference to the data-bound parent NodeItem.
        /// </summary>
        internal NodeItem ParentNodeItem
        {
            get => (NodeItem)GetValue(ParentNodeItemProperty);
            set => SetValue(ParentNodeItemProperty, value);
        }

        #endregion Private Data Members\Properties

        #region Private Methods

        /// <summary>
        /// Static constructor.
        /// </summary>
        static ConnectorItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ConnectorItem), new FrameworkPropertyMetadata(typeof(ConnectorItem)));
        }

        /// <summary>
        /// A mouse button has been held down.
        /// </summary>
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);


            if (ParentNodeItem != null)
            {
                ParentNodeItem.BringToFront();
            }

            if (ParentProjectDesignSurface != null)
            {
                ParentProjectDesignSurface.Focus();
            }

            if (e.ChangedButton == MouseButton.Left)
            {
                if (ParentNodeItem != null)
                {
                    //
                    // Delegate to parent node to execute selection logic.
                    //
                    ParentNodeItem.LeftMouseDownSelectionLogic();
                }

                _lastMousePoint = e.GetPosition(ParentProjectDesignSurface);
                _isLeftMouseDown = true;
                e.Handled = true;
            }
            else if (e.ChangedButton == MouseButton.Right)
            {
                if (ParentNodeItem != null)
                {
                    //
                    // Delegate to parent node to execute selection logic.
                    //
                    ParentNodeItem.RightMouseDownSelectionLogic();
                }
            }
        }

        /// <summary>
        /// The mouse cursor has been moved.
        /// </summary>        
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_isDragging)
            {
                //
                // Raise the event to notify that dragging is in progress.
                //

                var curMousePoint = e.GetPosition(ParentProjectDesignSurface);
                var offset = curMousePoint - _lastMousePoint;
                if (offset.X != 0.0 &&
                    offset.Y != 0.0)
                {
                    _lastMousePoint = curMousePoint;

                    RaiseEvent(new ConnectorItemDraggingEventArgs(ConnectorDraggingEvent, this, offset.X, offset.Y));
                }

                e.Handled = true;
            }
            else if (_isLeftMouseDown)
            {
                if (ParentProjectDesignSurface != null ||
                    ParentProjectDesignSurface.EnableConnectionDragging)
                {
                    //
                    // The user is left-dragging the connector and connection dragging is enabled,
                    // but don't initiate the drag operation until 
                    // the mouse cursor has moved more than the threshold distance.
                    //
                    Point curMousePoint = e.GetPosition(ParentProjectDesignSurface);
                    var dragDelta = curMousePoint - _lastMousePoint;
                    var dragDistance = Math.Abs(dragDelta.Length);
                    if (dragDistance > DragThreshold)
                    {
                        //
                        // When the mouse has been dragged more than the threshold value commence dragging the node.
                        //

                        //
                        // Raise an event to notify that that dragging has commenced.
                        //
                        var eventArgs = new ConnectorItemDragStartedEventArgs(ConnectorDragStartedEvent, this);
                        RaiseEvent(eventArgs);

                        if (eventArgs.Cancel)
                        {
                            //
                            // Handler of the event disallowed dragging of the node.
                            //
                            _isLeftMouseDown = false;
                            return;
                        }

                        _isDragging = true;
                        CaptureMouse();
                        e.Handled = true;
                    }
                }
            }
        }

        /// <summary>
        /// A mouse button has been released.
        /// </summary>
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.ChangedButton == MouseButton.Left)
            {
                if (_isLeftMouseDown)
                {
                    if (_isDragging)
                    {
                        RaiseEvent(new ConnectorItemDragCompletedEventArgs(ConnectorDragCompletedEvent, this));
                        
                        ReleaseMouseCapture();

                        _isDragging = false;
                    }
                    else
                    {
                        //
                        // Execute mouse up selection logic only if there was no drag operation.
                        //
                        if (ParentNodeItem != null)
                        {
                            //
                            // Delegate to parent node to execute selection logic.
                            //
                            ParentNodeItem.LeftMouseUpSelectionLogic();
                        }
                    }

                    _isLeftMouseDown = false;

                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Cancel connection dragging for the connector that was dragged out.
        /// </summary>
        internal void CancelConnectionDragging()
        {
            if (_isLeftMouseDown)
            {
                //
                // Raise ConnectorDragCompleted, with a null connector.
                //
                RaiseEvent(new ConnectorItemDragCompletedEventArgs(ConnectorDragCompletedEvent, null));

                _isLeftMouseDown = false;
                ReleaseMouseCapture();
            }
        }

        /// <summary>
        /// Event raised when 'ParentProjectDesignSurface' property has changed.
        /// </summary>
        private static void ParentProjectDesignSurface_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var connectorItem = (ConnectorItem)d;
            connectorItem.UpdateHotspot();
        }

        /// <summary>
        /// Event raised when the layout of the connector has been updated.
        /// </summary>
        private void ConnectorItem_LayoutUpdated(object sender, EventArgs e)
        {
            UpdateHotspot();
        }

        /// <summary>
        /// Update the connector hotspot.
        /// </summary>
        private void UpdateHotspot()
        {
            if (ParentProjectDesignSurface == null)
            {
                // No parent ProjectDesignSurface is set.
                return;
            }

            if (!ParentProjectDesignSurface.IsAncestorOf(this))
            {
                //
                // The parent ProjectDesignSurface is no longer an ancestor of the connector.
                // This happens when the connector (and its parent node) has been removed from the network.
                // Reset the property null so we don't attempt to check again.
                //
                ParentProjectDesignSurface = null;
                return;
            }

            //
            // The parent ProjectDesignSurface is still valid.
            // Compute the center point of the connector.
            //
            var centerPoint = new Point(ActualWidth / 2, ActualHeight / 2);

            //
            // Transform the center point so that it is relative to the parent ProjectDesignSurface.
            // Then assign it to Hotspot.  Usually Hotspot will be data-bound to the application
            // view-model using OneWayToSource so that the value of the hotspot is then pushed through
            // to the view-model.
            //
            Hotspot = TransformToAncestor(ParentProjectDesignSurface).Transform(centerPoint);
       }

        #endregion Private Methods
    }
}
