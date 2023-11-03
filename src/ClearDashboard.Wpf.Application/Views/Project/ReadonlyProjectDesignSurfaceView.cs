using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface;
using ClearDashboard.Wpf.Controls;

namespace ClearDashboard.Wpf.Application.Views.Project;

/// <summary>
/// This is a partial implementation of ProjectDesignSurfaceView that just contains most of the zooming and panning functionality.
/// </summary>
public partial class ReadonlyProjectDesignSurfaceView
{
    /// <summary>
    /// Specifies the current state of the mouse handling logic.
    /// </summary>
    private MouseHandlingMode _mouseHandlingMode = MouseHandlingMode.None;

    /// <summary>
    /// The point that was clicked relative to the ZoomAndPanControl.
    /// </summary>
    private Point _origZoomAndPanControlMouseDownPoint;

    /// <summary>
    /// The point that was clicked relative to the content that is contained within the ZoomAndPanControl.
    /// </summary>
    private Point _origContentMouseDownPoint;

    /// <summary>
    /// Records which mouse button clicked during mouse dragging.
    /// </summary>
    private MouseButton _mouseButtonDown;

    /// <summary>
    /// Saves the previous zoom rectangle, pressing the backspace key jumps back to this zoom rectangle.
    /// </summary>
    private Rect _prevZoomRect;

    /// <summary>
    /// Save the previous content scale, pressing the backspace key jumps back to this scale.
    /// </summary>
    private double _prevZoomScale;

    /// <summary>
    /// Set to 'true' when the previous zoom rect is saved.
    /// </summary>
    private bool _prevZoomRectSet;

    /// <summary>
    /// Event raised on mouse down in the ProjectDesignSurfaceView.
    /// </summary> 
    //private void OnDesignSurfaceMouseDown(object sender, MouseButtonEventArgs e)
    //{
    //    ProjectDesignSurface.Focus();
    //    Keyboard.Focus(ProjectDesignSurface);

    //    _mouseButtonDown = e.ChangedButton;
    //    _origZoomAndPanControlMouseDownPoint = e.GetPosition(zoomAndPanControl);
    //    _origContentMouseDownPoint = e.GetPosition(ProjectDesignSurface);

    //    if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0 &&
    //        (e.ChangedButton == MouseButton.Left ||
    //         e.ChangedButton == MouseButton.Right))
    //    {
    //        // Shift + left- or right-down initiates zooming mode.
    //        _mouseHandlingMode = MouseHandlingMode.Zooming;
    //    }
    //    else if (_mouseButtonDown == MouseButton.Left &&
    //             (Keyboard.Modifiers & ModifierKeys.Control) == 0)
    //    {
    //        //
    //        // Initiate panning, when control is not held down.
    //        // When control is held down left dragging is used for drag selection.
    //        // After panning has been initiated the user must drag further than the threshold value to actually start drag panning.
    //        //
    //        _mouseHandlingMode = MouseHandlingMode.Panning;
    //    }

    //    if (_mouseHandlingMode != MouseHandlingMode.None)
    //    {
    //        // Capture the mouse so that we eventually receive the mouse up event.
    //        ProjectDesignSurface.CaptureMouse();
    //        e.Handled = true;
    //    }
    //}

    /// <summary>
    /// Event raised on mouse up in the ProjectDesignSurfaceView.
    /// </summary>
    //private void OnDesignSurfaceMouseUp(object sender, MouseButtonEventArgs e)
    //{
    //    if (_mouseHandlingMode != MouseHandlingMode.None)
    //    {
    //        if (_mouseHandlingMode == MouseHandlingMode.Panning)
    //        {
    //            //
    //            // Panning was initiated but dragging was abandoned before the mouse
    //            // cursor was dragged further than the threshold distance.
    //            // This means that this basically just a regular left mouse click.
    //            // Because it was a mouse click in empty space we need to clear the current selection.
    //            //
    //        }
    //        else if (_mouseHandlingMode == MouseHandlingMode.Zooming)
    //        {
    //            if (_mouseButtonDown == MouseButton.Left)
    //            {
    //                // Shift + left-click zooms in on the content.
    //                ZoomIn(_origContentMouseDownPoint);
    //            }
    //            else if (_mouseButtonDown == MouseButton.Right)
    //            {
    //                // Shift + left-click zooms out from the content.
    //                ZoomOut(_origContentMouseDownPoint);
    //            }
    //        }
    //        else if (_mouseHandlingMode == MouseHandlingMode.DragZooming)
    //        {
    //            // When drag-zooming has finished we zoom in on the rectangle that was highlighted by the user.
    //            ApplyDragZoomRect();
    //        }

    //        //
    //        // Reenable clearing of selection when empty space is clicked.
    //        // This is disabled when drag panning is in progress.
    //        //
    //        ProjectDesignSurface.IsClearSelectionOnEmptySpaceClickEnabled = true;

    //        //
    //        // Reset the override cursor.
    //        // This is set to a special cursor while drag panning is in progress.
    //        //
    //        Mouse.OverrideCursor = null;

    //        ProjectDesignSurface.ReleaseMouseCapture();
    //        _mouseHandlingMode = MouseHandlingMode.None;
    //        e.Handled = true;
    //    }
    //}

    /// <summary>
    /// Event raised on mouse move in the ProjectDesignSurfaceView.
    /// </summary>
    //private void OnDesignSurfaceMouseMove(object sender, MouseEventArgs e)
    //{
    //    if (_mouseHandlingMode == MouseHandlingMode.Panning)
    //    {
    //        var currentPoint = e.GetPosition(zoomAndPanControl);
    //        var dragOffset = currentPoint - _origZoomAndPanControlMouseDownPoint;
    //        var dragThreshold = 10;
    //        if (Math.Abs(dragOffset.X) > dragThreshold ||
    //            Math.Abs(dragOffset.Y) > dragThreshold)
    //        {
    //            //
    //            // The user has dragged the cursor further than the threshold distance, initiate
    //            // drag panning.
    //            //
    //            _mouseHandlingMode = MouseHandlingMode.DragPanning;
    //            ProjectDesignSurface.IsClearSelectionOnEmptySpaceClickEnabled = false;
    //            Mouse.OverrideCursor = Cursors.ScrollAll;
    //        }

    //        e.Handled = true;
    //    }
    //    else if (_mouseHandlingMode == MouseHandlingMode.DragPanning)
    //    {
    //        //
    //        // The user is left-dragging the mouse.
    //        // Pan the viewport by the appropriate amount.
    //        //
    //        var curContentMousePoint = e.GetPosition(ProjectDesignSurface);
    //        var dragOffset = curContentMousePoint - _origContentMouseDownPoint;

    //        zoomAndPanControl.ContentOffsetX -= dragOffset.X;
    //        zoomAndPanControl.ContentOffsetY -= dragOffset.Y;

    //        e.Handled = true;
    //    }
    //    else if (_mouseHandlingMode == MouseHandlingMode.Zooming)
    //    {
    //        var curZoomAndPanControlMousePoint = e.GetPosition(zoomAndPanControl);
    //        var dragOffset = curZoomAndPanControlMousePoint - _origZoomAndPanControlMouseDownPoint;
    //        double dragThreshold = 10;
    //        if (_mouseButtonDown == MouseButton.Left &&
    //            (Math.Abs(dragOffset.X) > dragThreshold ||
    //             Math.Abs(dragOffset.Y) > dragThreshold))
    //        {
    //            //
    //            // When Shift + left-down zooming mode and the user drags beyond the drag threshold,
    //            // initiate drag zooming mode where the user can drag out a rectangle to select the area
    //            // to zoom in on.
    //            //
    //            _mouseHandlingMode = MouseHandlingMode.DragZooming;
    //            var curContentMousePoint = e.GetPosition(ProjectDesignSurface);
    //            InitDragZoomRect(_origContentMouseDownPoint, curContentMousePoint);
    //        }

    //        e.Handled = true;
    //    }
    //    else if (_mouseHandlingMode == MouseHandlingMode.DragZooming)
    //    {
    //        //
    //        // When in drag zooming mode continuously update the position of the rectangle
    //        // that the user is dragging out.
    //        //
    //        var curContentMousePoint = e.GetPosition(ProjectDesignSurface);
    //        SetDragZoomRect(_origContentMouseDownPoint, curContentMousePoint);

    //        e.Handled = true;
    //    }
    //}

    /// <summary>
    /// Event raised by rotating the mouse wheel.
    /// </summary>
    //private void OnMouseWheel(object sender, MouseWheelEventArgs e)
    //{
    //    e.Handled = true;

    //    if (e.Delta > 0)
    //    {
    //        var currentPoint = e.GetPosition(ProjectDesignSurface);
    //        ZoomIn(currentPoint);
    //    }
    //    else if (e.Delta < 0)
    //    {
    //        var currentPoint = e.GetPosition(ProjectDesignSurface);
    //        ZoomOut(currentPoint);
    //    }
    //}

    /// <summary>
    /// Event raised when the user has double clicked in the zoom and pan control.
    /// </summary>
    //private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
    //{
    //    if ((Keyboard.Modifiers & ModifierKeys.Shift) == 0)
    //    {
    //        var doubleClickPoint = e.GetPosition(ProjectDesignSurface);
    //        zoomAndPanControl.AnimatedSnapTo(doubleClickPoint);
    //    }
    //}

    /// <summary>
    /// The 'ZoomIn' command (bound to the plus key) was executed.
    /// </summary>
    private void ZoomIn_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        // var o = ProjectDesignSurface.SelectedNode;

        ZoomIn(new Point(zoomAndPanControl.ContentZoomFocusX, zoomAndPanControl.ContentZoomFocusY));
    }

    /// <summary>
    /// The 'ZoomOut' command (bound to the minus key) was executed.
    /// </summary>
    private void ZoomOut_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        ZoomOut(new Point(zoomAndPanControl.ContentZoomFocusX, zoomAndPanControl.ContentZoomFocusY));
    }

    /// <summary>
    /// The 'JumpBackToPrevZoom' command was executed.
    /// </summary>
    private void JumpBackToPrevZoom_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        JumpBackToPrevZoom();
    }

    /// <summary>
    /// Determines whether the 'JumpBackToPrevZoom' command can be executed.
    /// </summary>
    private void JumpBackToPrevZoom_CanExecuted(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = _prevZoomRectSet;
    }

    /// <summary>
    /// The 'Fill' command was executed.
    /// </summary>
    //private void FitContent_Executed(object sender, ExecutedRoutedEventArgs e)
    //{
    //    var nodes = ProjectDesignSurfaceViewModel.DesignSurfaceViewModel!.CorpusNodes;
    //    if (nodes.Count == 0)
    //    {
    //        return;
    //    }

    //    SavePrevZoomRect();

    //    var actualContentRect = DetermineAreaOfNodes(nodes);

    //    //
    //    // Inflate the content rect by a fraction of the actual size of the total content area.
    //    // This puts a nice border around the content we are fitting to the viewport.
    //    //
    //    actualContentRect.Inflate(ProjectDesignSurface.ActualWidth / 40, ProjectDesignSurface.ActualHeight / 40);

    //    zoomAndPanControl.AnimatedZoomTo(actualContentRect);
    //}

    /// <summary>
    /// Determine the area covered by the specified list of nodes.
    /// </summary>
    private Rect DetermineAreaOfNodes(IList nodes)
    {
        var firstNode = (CorpusNodeViewModel)nodes[0];
        var actualContentRect = new Rect(firstNode.X, firstNode.Y, firstNode.Size.Width, firstNode.Size.Height);

        for (var i = 1; i < nodes.Count; ++i)
        {
            var node = (CorpusNodeViewModel)nodes[i];
            var nodeRect = new Rect(node.X, node.Y, node.Size.Width, node.Size.Height);
            actualContentRect = Rect.Union(actualContentRect, nodeRect);
        }
        return actualContentRect;
    }

    /// <summary>
    /// The 'Fill' command was executed.
    /// </summary>
    private void Fill_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        SavePrevZoomRect();
        zoomAndPanControl.AnimatedScaleToFit();
    }

    /// <summary>
    /// The 'OneHundredPercent' command was executed.
    /// </summary>
    private void OneHundredPercent_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        SavePrevZoomRect();
        zoomAndPanControl.AnimatedZoomTo(1.0);
    }


    /// <summary>
    /// Jump back to the previous zoom level.
    /// </summary>
    private void JumpBackToPrevZoom()
    {
        zoomAndPanControl.AnimatedZoomTo(_prevZoomScale, _prevZoomRect);
        ClearPrevZoomRect();
    }

    /// <summary>
    /// Zoom the viewport out, centering on the specified point (in content coordinates).
    /// </summary>
    private void ZoomOut(Point contentZoomCenter)
    {
        if (zoomAndPanControl.ContentScale > 0.02)
        {
            zoomAndPanControl.ZoomAboutPoint(zoomAndPanControl.ContentScale - 0.01, contentZoomCenter);
        }
    }

    /// <summary>
    /// Zoom the viewport in, centering on the specified point (in content coordinates).
    /// </summary>
    private void ZoomIn(Point contentZoomCenter)
    {
        if (zoomAndPanControl.ContentScale <= 2.0)
        {
            zoomAndPanControl.ZoomAboutPoint(zoomAndPanControl.ContentScale + 0.01, contentZoomCenter);
        }
    }

    /// <summary>
    /// Initialize the rectangle that the use is dragging out.
    /// </summary>
    private void InitDragZoomRect(Point pt1, Point pt2)
    {
        SetDragZoomRect(pt1, pt2);

        DragZoomCanvas.Visibility = Visibility.Visible;
        DragZoomBorder.Opacity = 0.5;
    }

    /// <summary>
    /// Update the position and size of the rectangle that user is dragging out.
    /// </summary>
    private void SetDragZoomRect(Point pt1, Point pt2)
    {
        double x, y, width, height;

        //
        // Determine x,y,width and height of the rect inverting the points if necessary.
        // 

        if (pt2.X < pt1.X)
        {
            x = pt2.X;
            width = pt1.X - pt2.X;
        }
        else
        {
            x = pt1.X;
            width = pt2.X - pt1.X;
        }

        if (pt2.Y < pt1.Y)
        {
            y = pt2.Y;
            height = pt1.Y - pt2.Y;
        }
        else
        {
            y = pt1.Y;
            height = pt2.Y - pt1.Y;
        }

        //
        // Update the coordinates of the rectangle that is being dragged out by the user.
        // The we offset and rescale to convert from content coordinates.
        //
        Canvas.SetLeft(DragZoomBorder, x);
        Canvas.SetTop(DragZoomBorder, y);
        DragZoomBorder.Width = width;
        DragZoomBorder.Height = height;
    }

    /// <summary>
    /// When the user has finished dragging out the rectangle the zoom operation is applied.
    /// </summary>
    private void ApplyDragZoomRect()
    {
        //
        // Record the previous zoom level, so that we can jump back to it when the backspace key is pressed.
        //
        SavePrevZoomRect();

        //
        // Retrieve the rectangle that the user dragged out and zoom in on it.
        //
        var contentX = Canvas.GetLeft(DragZoomBorder);
        var contentY = Canvas.GetTop(DragZoomBorder);
        var contentWidth = DragZoomBorder.Width;
        var contentHeight = DragZoomBorder.Height;
        zoomAndPanControl.AnimatedZoomTo(new Rect(contentX, contentY, contentWidth, contentHeight));

        FadeOutDragZoomRect();
    }

    //
    // Fade out the drag zoom rectangle.
    //
    private void FadeOutDragZoomRect()
    {
        //AnimationHelper.StartAnimation(DragZoomBorder, UIElement.OpacityProperty, 0.0, 0.1,
        //    (sender, e) => DragZoomCanvas.Visibility = Visibility.Collapsed);

        AnimationHelper.StartAnimation(DragZoomBorder, UIElement.OpacityProperty, 0.0, 0.1,
            delegate (object sender, EventArgs e)
            {
                DragZoomCanvas.Visibility = Visibility.Collapsed;
            });
    }

    //
    // Record the previous zoom level, so that we can jump back to it when the backspace key is pressed.
    //
    private void SavePrevZoomRect()
    {
        _prevZoomRect = new Rect(zoomAndPanControl.ContentOffsetX, zoomAndPanControl.ContentOffsetY, zoomAndPanControl.ContentViewportWidth, zoomAndPanControl.ContentViewportHeight);
        _prevZoomScale = zoomAndPanControl.ContentScale;
        _prevZoomRectSet = true;
    }

    /// <summary>
    /// Clear the memory of the previous zoom level.
    /// </summary>
    private void ClearPrevZoomRect()
    {
        _prevZoomRectSet = false;
    }



}