using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.Wpf.Controls.Utils;
using System;
using System.Windows;
using System.Windows.Media;

namespace ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface
{
    /// <summary>
    /// Defines a connection between two connectors (aka connection points) of two nodes.
    /// </summary>
    public sealed class ParallelCorpusConnectionViewModel : AbstractModelBase
    {
        #region Internal Data Members

        /// <summary>
        /// The source connector the connection is attached to.
        /// </summary>
        private ParallelCorpusConnectorViewModel? _sourceConnector;

        /// <summary>
        /// The destination connector the connection is attached to.
        /// </summary>
        private ParallelCorpusConnectorViewModel? _destinationConnector;

        /// <summary>
        /// The source and dest hotspots used for generating connection points.
        /// </summary>
        private Point _sourceConnectorHotspot;
        private Point _destConnectorHotspot;

        /// <summary>
        /// Points that make up the connection.
        /// </summary>
        private PointCollection? _points;

        /// <summary>
        /// Set to 'true' when the node is selected.
        /// </summary>
        private bool _isSelected;

        #endregion Internal Data Members

        /// <summary>
        /// Set to 'true' when the node is selected.
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set => Set(ref _isSelected, value);
        }

        public bool IsRtl { get; set; }

        public Guid Id { get; set; } = Guid.NewGuid();
        
        private BindableCollection<ParallelCorpusConnectionMenuItemViewModel> _menuItems = new();
        public BindableCollection<ParallelCorpusConnectionMenuItemViewModel> MenuItems
        {
            get => _menuItems;
            set
            {
                _menuItems = value;
                NotifyOfPropertyChange(() => MenuItems);
            }
        }

        /// <summary>
        /// The source connector the connection is attached to.
        /// </summary>
        public ParallelCorpusConnectorViewModel? SourceConnector
        {
            get => _sourceConnector;
            set
            {
                if (_sourceConnector == value)
                {
                    return;
                }

                if (_sourceConnector != null)
                {
                    _sourceConnector.AttachedConnections.Remove(this);
                    _sourceConnector.HotspotUpdated -= OnSourceConnectorHotspotUpdated;
                }

                _sourceConnector = value;

                if (_sourceConnector != null)
                {
                    _sourceConnector.AttachedConnections.Add(this);
                    _sourceConnector.HotspotUpdated += OnSourceConnectorHotspotUpdated;
                    SourceConnectorHotspot = _sourceConnector.Hotspot;
                }

                NotifyOfPropertyChange(() => SourceConnector);
                OnConnectionChanged();
            }
        }

        /// <summary>
        /// The destination connector the connection is attached to.
        /// </summary>
        public ParallelCorpusConnectorViewModel? DestinationConnector
        {
            get => _destinationConnector;
            set
            {
                if (_destinationConnector == value)
                {
                    return;
                }

                if (_destinationConnector != null)
                {
                    _destinationConnector.AttachedConnections.Remove(this);
                    _destinationConnector.HotspotUpdated -= OnDestinationConnectorHotspotUpdated;
                }

                _destinationConnector = value;

                if (_destinationConnector != null)
                {
                    _destinationConnector.AttachedConnections.Add(this);
                    _destinationConnector.HotspotUpdated += OnDestinationConnectorHotspotUpdated;
                    DestConnectorHotspot = _destinationConnector.Hotspot;
                }

                NotifyOfPropertyChange(() => DestinationConnector);
                OnConnectionChanged();
            }
        }

        /// <summary>
        /// The source and dest hotspots used for generating connection points.
        /// </summary>
        public Point SourceConnectorHotspot
        {
            get => _sourceConnectorHotspot;
            set
            {
                _sourceConnectorHotspot = value;

                ComputeConnectionPoints();

                NotifyOfPropertyChange(() => SourceConnectorHotspot);
            }
        }

        public Point DestConnectorHotspot
        {
            get => _destConnectorHotspot;
            set
            {
                _destConnectorHotspot = value;

                ComputeConnectionPoints();

                NotifyOfPropertyChange(() => DestConnectorHotspot);
            }
        }

        /// <summary>
        /// Points that make up the connection.
        /// </summary>
        public PointCollection? Points
        {
            get => _points;
            set
            {
                _points = value;
                NotifyOfPropertyChange(() => Points);
            }
        }

        public ParallelCorpusId? ParallelCorpusId { get; set; }
        public string? ParallelCorpusDisplayName { get; set; }

        public string? SourceFontFamily { get; set; } = FontNames.DefaultFontFamily;
        public string? TargetFontFamily { get; set; } = FontNames.DefaultFontFamily;


        /// <summary>
        /// Event fired when the connection has changed.
        /// </summary>
        public event EventHandler<EventArgs>? ConnectionChanged;

        #region Private Methods

        /// <summary>
        /// Raises the 'ConnectionChanged' event.
        /// </summary>
        private void OnConnectionChanged()
        {
            if (ConnectionChanged != null)
            {
                ConnectionChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Event raised when the hotspot of the source connector has been updated.
        /// </summary>
        private void OnSourceConnectorHotspotUpdated(object? sender, EventArgs e)
        {
            SourceConnectorHotspot = SourceConnector!.Hotspot;
        }

        /// <summary>
        /// Event raised when the hotspot of the dest connector has been updated.
        /// </summary>
        private void OnDestinationConnectorHotspotUpdated(object? sender, EventArgs e)
        {
            DestConnectorHotspot = DestinationConnector!.Hotspot;
        }

        /// <summary>
        /// Rebuild connection points.
        /// </summary>
        private void ComputeConnectionPoints()
        {
            var computedPoints = new PointCollection { SourceConnectorHotspot };

            var deltaX = Math.Abs(DestConnectorHotspot.X - SourceConnectorHotspot.X);
            var deltaY = Math.Abs(DestConnectorHotspot.Y - SourceConnectorHotspot.Y);
            if (deltaX > deltaY)
            {
                var midPointX = SourceConnectorHotspot.X + ((DestConnectorHotspot.X - SourceConnectorHotspot.X) / 2);
                computedPoints.Add(new Point(midPointX, SourceConnectorHotspot.Y));
                computedPoints.Add(new Point(midPointX, DestConnectorHotspot.Y));
            }
            else
            {
                var midPointY = SourceConnectorHotspot.Y + ((DestConnectorHotspot.Y - SourceConnectorHotspot.Y) / 2);
                computedPoints.Add(new Point(SourceConnectorHotspot.X, midPointY));
                computedPoints.Add(new Point(DestConnectorHotspot.X, midPointY));
            }

            computedPoints.Add(DestConnectorHotspot);
            computedPoints.Freeze();

            Points = computedPoints;
        }

        #endregion Private Methods
    }
}
