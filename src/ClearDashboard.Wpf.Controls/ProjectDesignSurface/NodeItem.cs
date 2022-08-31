using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ClearDashboard.Wpf.Controls
{
    /// <summary>
    /// This is a UI element that represents a network/flow-chart node.
    /// </summary>
    public class NodeItem : ListBoxItem
    {
        #region Dependency Property/Event Definitions

        public static readonly DependencyProperty XProperty =
            DependencyProperty.Register("X", typeof(double), typeof(NodeItem),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty YProperty =
            DependencyProperty.Register("Y", typeof(double), typeof(NodeItem),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty ZIndexProperty =
            DependencyProperty.Register("ZIndex", typeof(int), typeof(NodeItem),
                new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        internal static readonly DependencyProperty ParentProjectDesignSurfaceProperty =
            DependencyProperty.Register("ParentProjectDesignSurface", typeof(ProjectDesignSurface), typeof(NodeItem),
                new FrameworkPropertyMetadata(ParentProjectDesignSurface_PropertyChanged));

        internal static readonly RoutedEvent NodeDragStartedEvent =
            EventManager.RegisterRoutedEvent("NodeDragStarted", RoutingStrategy.Bubble, typeof(NodeDragStartedEventHandler), typeof(NodeItem));

        internal static readonly RoutedEvent NodeDraggingEvent =
            EventManager.RegisterRoutedEvent("NodeDragging", RoutingStrategy.Bubble, typeof(NodeDraggingEventHandler), typeof(NodeItem));

        internal static readonly RoutedEvent NodeDragCompletedEvent =
            EventManager.RegisterRoutedEvent("NodeDragCompleted", RoutingStrategy.Bubble, typeof(NodeDragCompletedEventHandler), typeof(NodeItem));

        #endregion Dependency Property/Event Definitions

        public NodeItem()
        {
            //
            // By default, we don't want this UI element to be focusable.
            //
            Focusable = false;
        }

        /// <summary>
        /// The X coordinate of the node.
        /// </summary>
        public double X
        {
            get => (double)GetValue(XProperty);
            set => SetValue(XProperty, value);
        }

        /// <summary>
        /// The Y coordinate of the node.
        /// </summary>
        public double Y
        {
            get => (double)GetValue(YProperty);
            set => SetValue(YProperty, value);
        }

        /// <summary>
        /// The Z index of the node.
        /// </summary>
        public int ZIndex
        {
            get => (int)GetValue(ZIndexProperty);
            set => SetValue(ZIndexProperty, value);
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
        /// The point the mouse was last at when dragging.
        /// </summary>
        private Point _lastMousePoint;

        /// <summary>
        /// Set to 'true' when left mouse button is held down.
        /// </summary>
        private bool _isLeftMouseDown;

        /// <summary>
        /// Set to 'true' when left mouse button and the control key are held down.
        /// </summary>
        private bool _isLeftMouseAndControlDown;

        /// <summary>
        /// Set to 'true' when dragging has started.
        /// </summary>
        private bool _isDragging;

        /// <summary>
        /// The threshold distance the mouse-cursor must move before dragging begins.
        /// </summary>
        private static readonly double _dragThreshold = 5;

        #endregion Private Data Members\Properties

        #region Private Methods

        /// <summary>
        /// Static constructor.
        /// </summary>
        static NodeItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NodeItem), new FrameworkPropertyMetadata(typeof(NodeItem)));
        }

        /// <summary>
        /// Bring the node to the front of other elements.
        /// </summary>
        internal void BringToFront()
        {
            if (ParentProjectDesignSurface == null)
            {
                return;
            }

            var maxZ = ParentProjectDesignSurface.FindMaxZIndex();
            ZIndex = maxZ + 1;
        }

        /// <summary>
        /// Called when a mouse button is held down.
        /// </summary>
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            BringToFront();

            if (ParentProjectDesignSurface != null)
            {
                ParentProjectDesignSurface.Focus();
            }

            if (e.ChangedButton == MouseButton.Left && ParentProjectDesignSurface != null)
            {
                _lastMousePoint = e.GetPosition(ParentProjectDesignSurface);
                _isLeftMouseDown = true;

                LeftMouseDownSelectionLogic();

                e.Handled = true;
            }
            else if (e.ChangedButton == MouseButton.Right && ParentProjectDesignSurface != null)
            {
                RightMouseDownSelectionLogic();
            }
        }

        /// <summary>
        /// This method contains selection logic that is invoked when the left mouse button is pressed down.
        /// The reason this exists in its own method rather than being included in OnMouseDown is 
        /// so that ConnectorItem can reuse this logic from its OnMouseDown.
        /// </summary>
        internal void LeftMouseDownSelectionLogic()
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                //
                // Control key was held down.
                // This means that the rectangle is being added to or removed from the existing selection.
                // Don't do anything yet, we will act on this later in the MouseUp event handler.
                //
                _isLeftMouseAndControlDown = true;
            }
            else
            {
                //
                // Control key is not held down.
                //
                _isLeftMouseAndControlDown = false;

                if (ParentProjectDesignSurface.SelectedNodes.Count == 0)
                {
                    //
                    // Nothing already selected, select the item.
                    //
                    IsSelected = true;
                }
                else if (ParentProjectDesignSurface.SelectedNodes.Contains(this) ||
                         ParentProjectDesignSurface.SelectedNodes.Contains(DataContext))
                {
                    // 
                    // Item is already selected, do nothing.
                    // We will act on this in the MouseUp if there was no drag operation.
                    //
                }
                else
                {
                    //
                    // Item is not selected.
                    // Deselect all, and select the item.
                    //
                    ParentProjectDesignSurface.SelectedNodes.Clear();
                    IsSelected = true;
                }
            }
        }

        /// <summary>
        /// This method contains selection logic that is invoked when the right mouse button is pressed down.
        /// The reason this exists in its own method rather than being included in OnMouseDown is 
        /// so that ConnectorItem can reuse this logic from its OnMouseDown.
        /// </summary>
        internal void RightMouseDownSelectionLogic()
        {
            if (ParentProjectDesignSurface.SelectedNodes.Count == 0)
            {
                //
                // Nothing already selected, select the item.
                //
                IsSelected = true;
            }
            else if (ParentProjectDesignSurface.SelectedNodes.Contains(this) ||
                     ParentProjectDesignSurface.SelectedNodes.Contains(DataContext))
            {
                // 
                // Item is already selected, do nothing.
                //
            }
            else
            {
                //
                // Item is not selected.
                // Deselect all, and select the item.
                //
                ParentProjectDesignSurface.SelectedNodes.Clear();
                IsSelected = true;
            }
        }

        /// <summary>
        /// Called when the mouse cursor is moved.
        /// </summary>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_isDragging)
            {
                //
                // Raise the event to notify that dragging is in progress.
                //

                Point curMousePoint = e.GetPosition(ParentProjectDesignSurface);

                object item = this;
                if (DataContext != null)
                {
                    item = DataContext;
                }

                Vector offset = curMousePoint - _lastMousePoint;
                if (offset.X != 0.0 ||
                    offset.Y != 0.0)
                {
                    _lastMousePoint = curMousePoint;

                    RaiseEvent(new NodeDraggingEventArgs(NodeDraggingEvent, this, new object[] { item }, offset.X, offset.Y));
                }
            }
            else if (_isLeftMouseDown && ParentProjectDesignSurface.EnableNodeDragging)
            {
                //
                // The user is left-dragging the node,
                // but don't initiate the drag operation until 
                // the mouse cursor has moved more than the threshold distance.
                //
                Point curMousePoint = e.GetPosition(ParentProjectDesignSurface);
                var dragDelta = curMousePoint - _lastMousePoint;
                double dragDistance = Math.Abs(dragDelta.Length);
                if (dragDistance > _dragThreshold)
                {
                    //
                    // When the mouse has been dragged more than the threshold value commence dragging the node.
                    //

                    //
                    // Raise an event to notify that that dragging has commenced.
                    //
                    NodeDragStartedEventArgs eventArgs = new NodeDragStartedEventArgs(NodeDragStartedEvent, this, new NodeItem[] { this });
                    RaiseEvent(eventArgs);

                    if (eventArgs.Cancel)
                    {
                        //
                        // Handler of the event disallowed dragging of the node.
                        //
                        _isLeftMouseDown = false;
                        _isLeftMouseAndControlDown = false;
                        return;
                    }

                    _isDragging = true;
                    CaptureMouse();
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Called when a mouse button is released.
        /// </summary>
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            if (_isLeftMouseDown)
            {
                if (_isDragging)
                {
                    //
                    // Raise an event to notify that node dragging has finished.
                    //

                    RaiseEvent(new NodeDragCompletedEventArgs(NodeDragCompletedEvent, this, new NodeItem[] { this }));

                    ReleaseMouseCapture();

                    _isDragging = false;
                }
                else
                {
                    //
                    // Execute mouse up selection logic only if there was no drag operation.
                    //

                    LeftMouseUpSelectionLogic();
                }

                _isLeftMouseDown = false;
                _isLeftMouseAndControlDown = false;

                e.Handled = true;
            }
        }

        /// <summary>
        /// This method contains selection logic that is invoked when the left mouse button is released.
        /// The reason this exists in its own method rather than being included in OnMouseUp is 
        /// so that ConnectorItem can reuse this logic from its OnMouseUp.
        /// </summary>
        internal void LeftMouseUpSelectionLogic()
        {
            if (_isLeftMouseAndControlDown)
            {
                //
                // Control key was held down.
                // Toggle the selection.
                //
                IsSelected = !IsSelected;
            }
            else
            {
                //
                // Control key was not held down.
                //
                if (ParentProjectDesignSurface.SelectedNodes.Count == 1 &&
                    (ParentProjectDesignSurface.SelectedNode == this ||
                     ParentProjectDesignSurface.SelectedNode == DataContext))
                {
                    //
                    // The item that was clicked is already the only selected item.
                    // Don't need to do anything.
                    //
                }
                else
                {
                    //
                    // Clear the selection and select the clicked item as the only selected item.
                    //
                    ParentProjectDesignSurface.SelectedNodes.Clear();
                    IsSelected = true;
                }
            }

            _isLeftMouseAndControlDown = false;
        }

        /// <summary>
        /// Event raised when the ParentProjectDesignSurface property has changed.
        /// </summary>
        private static void ParentProjectDesignSurface_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            //
            // Bring new nodes to the front of the z-order.
            //
            var nodeItem = (NodeItem)o;
            nodeItem.BringToFront();
        }

        #endregion Private Methods
    }
}
