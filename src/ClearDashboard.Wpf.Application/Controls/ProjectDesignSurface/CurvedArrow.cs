using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Wpf;

namespace ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface
{
    /// <summary>
    /// Defines an arrow that has multiple points.
    /// </summary>
    public class CurvedArrow : Shape
    {
        private readonly DashboardProjectManager _projectManager;
        private readonly IEventAggregator _eventAggregator;

        #region Dependency Property/Event Definitions

        public static readonly DependencyProperty ArrowHeadLengthProperty =
            DependencyProperty.Register("ArrowHeadLength", typeof(double), typeof(CurvedArrow),
                new FrameworkPropertyMetadata(20.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ArrowHeadWidthProperty =
            DependencyProperty.Register("ArrowHeadWidth", typeof(double), typeof(CurvedArrow),
                new FrameworkPropertyMetadata(12.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty PointsProperty =
            DependencyProperty.Register("Points", typeof(PointCollection), typeof(CurvedArrow),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty NodeSourceProperty = DependencyProperty.Register(nameof(NodeSource), typeof(ParallelCorpusConnectorViewModel), typeof(CurvedArrow), new PropertyMetadata(default(ParallelCorpusConnectorViewModel)));
        public static readonly DependencyProperty NodeTargetProperty = DependencyProperty.Register(nameof(NodeTarget), typeof(ParallelCorpusConnectorViewModel), typeof(CurvedArrow), new PropertyMetadata(default(ParallelCorpusConnectorViewModel)));
        public static readonly DependencyProperty ConnectionIdProperty = DependencyProperty.Register(nameof(ConnectionId), typeof(Guid), typeof(CurvedArrow), new PropertyMetadata(default(Guid)));

        #endregion Dependency Property/Event Definitions

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.ChangedButton == MouseButton.Left)
            {
                if (NodeSource is not null)
                {
                    NodeSource.SelectedConnection = ConnectionId;
                }
            }
        }



        /// <summary>
        /// The angle (in degrees) of the arrow head.
        /// </summary>
        public double ArrowHeadLength
        {
            get => (double)GetValue(ArrowHeadLengthProperty);
            set => SetValue(ArrowHeadLengthProperty, value);
        }

        /// <summary>
        /// The width of the arrow head.
        /// </summary>
        public double ArrowHeadWidth
        {
            get => (double)GetValue(ArrowHeadWidthProperty);
            set => SetValue(ArrowHeadWidthProperty, value);
        }

        /// <summary>
        /// The intermediate points that make up the line between the start and the end.
        /// </summary>
        public PointCollection Points
        {
            get => (PointCollection)GetValue(PointsProperty);
            set => SetValue(PointsProperty, value);
        }

        public CurvedArrow()
        {

        }

        public CurvedArrow(DashboardProjectManager projectManager, IEventAggregator eventAggregator)
        {
            _projectManager = projectManager;
            _eventAggregator = eventAggregator;
        }

        #region Private Methods

        /// <summary>
        /// Return the shape's geometry.
        /// </summary>
        protected override Geometry DefiningGeometry
        {
            get
            {
                if (Points == null || Points.Count < 2)
                {
                    return new GeometryGroup();
                }

                //
                // Geometry has not yet been generated.
                // Generate geometry and cache it.
                //
                var geometry = GenerateGeometry();

                var group = new GeometryGroup();
                group.Children.Add(geometry);

                GenerateArrowHeadGeometry(group);

                //
                // Return cached geometry.
                //
                return group;
            }
        }

        public ParallelCorpusConnectorViewModel NodeSource
        {
            get => (ParallelCorpusConnectorViewModel)GetValue(NodeSourceProperty);
            set => SetValue(NodeSourceProperty, value);
        }

        public ParallelCorpusConnectorViewModel NodeTarget
        {
            get => (ParallelCorpusConnectorViewModel)GetValue(NodeTargetProperty);
            set => SetValue(NodeTargetProperty, value);
        }

        public Guid ConnectionId
        {
            get => (Guid)GetValue(ConnectionIdProperty);
            set => SetValue(ConnectionIdProperty, value);
        }


        /// <summary>
        /// Generate the geometry for the three optional arrow symbols at the start, middle and end of the arrow.
        /// </summary>
        private void GenerateArrowHeadGeometry(GeometryGroup geometryGroup)
        {
            //var startPoint = Points[0];

            var penultimatePoint = Points[Points.Count - 2];
            var arrowHeadTip = Points[Points.Count - 1];
            var startDir = arrowHeadTip - penultimatePoint;
            startDir.Normalize();
            var basePoint = arrowHeadTip - (startDir * ArrowHeadLength);
            var crossDir = new Vector(-startDir.Y, startDir.X);

            var arrowHeadPoints = new Point[3];
            arrowHeadPoints[0] = arrowHeadTip;
            arrowHeadPoints[1] = basePoint - (crossDir * (ArrowHeadWidth / 2));
            arrowHeadPoints[2] = basePoint + (crossDir * (ArrowHeadWidth / 2));

            //
            // Build geometry for the arrow head.
            //
            var arrowHeadFig = new PathFigure
            {
                IsClosed = true,
                IsFilled = true,
                StartPoint = arrowHeadPoints[0]
            };
            arrowHeadFig.Segments.Add(new LineSegment(arrowHeadPoints[1], true));
            arrowHeadFig.Segments.Add(new LineSegment(arrowHeadPoints[2], true));

            var pathGeometry = new PathGeometry();
            pathGeometry.Figures.Add(arrowHeadFig);

            geometryGroup.Children.Add(pathGeometry);
        }

        /// <summary>
        /// Generate the shapes geometry.
        /// </summary>
        protected Geometry GenerateGeometry()
        {
            var pathGeometry = new PathGeometry();

            if (Points.Count == 2 || Points.Count == 3)
            {
                // Make a straight line.
                var pathFigure = new PathFigure
                {
                    IsClosed = false,
                    IsFilled = false,
                    StartPoint = Points[0]
                };

                for (var i = 1; i < Points.Count; ++i)
                {
                    pathFigure.Segments.Add(new LineSegment(Points[i], true));
                }

                pathGeometry.Figures.Add(pathFigure);
            }
            else
            {
                var adjustedPoints = new PointCollection { Points[0] };
                for (var i = 1; i < Points.Count; ++i)
                {
                    adjustedPoints.Add(Points[i]);
                }

                if (adjustedPoints.Count == 4)
                {
                    // Make a curved line.
                    var pathFigure = new PathFigure
                    {
                        IsClosed = false,
                        IsFilled = false,
                        StartPoint = adjustedPoints[0]
                    };
                    pathFigure.Segments.Add(new BezierSegment(adjustedPoints[1], adjustedPoints[2], adjustedPoints[3], true));

                    pathGeometry.Figures.Add(pathFigure);
                }
                else if (adjustedPoints.Count >= 5)
                {
                    // Make a curved line.
                    var pathFigure = new PathFigure
                    {
                        IsClosed = false,
                        IsFilled = false,
                        StartPoint = adjustedPoints[0]
                    };

                    adjustedPoints.RemoveAt(0);

                    while (adjustedPoints.Count > 3)
                    {
                        var generatedPoint = adjustedPoints[1] + ((adjustedPoints[2] - adjustedPoints[1]) / 2);

                        pathFigure.Segments.Add(new BezierSegment(adjustedPoints[0], adjustedPoints[1], generatedPoint, true));

                        adjustedPoints.RemoveAt(0);
                        adjustedPoints.RemoveAt(0);
                    }

                    if (adjustedPoints.Count == 2)
                    {
                        pathFigure.Segments.Add(new BezierSegment(adjustedPoints[0], adjustedPoints[0], adjustedPoints[1], true));
                    }
                    else
                    {
                        Trace.Assert(adjustedPoints.Count == 2);

                        pathFigure.Segments.Add(new BezierSegment(adjustedPoints[0], adjustedPoints[1], adjustedPoints[2], true));
                    }

                    pathGeometry.Figures.Add(pathFigure);
                }
            }

            return pathGeometry;
        }

        #endregion Private Methods
    }
}
