using System;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.Controls.Utils;

namespace ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface
{
    /// <summary>
    /// Defines a connector (aka connection point) that can be attached to a node and is used to connect the node to another node.
    /// </summary>
    public sealed class ConnectorViewModel : AbstractModelBase
    {
        private readonly IEventAggregator? _eventAggregator;
    

        #region Internal Data Members

        /// <summary>
        /// The connections that are attached to this connector, or null if no connections are attached.
        /// </summary>
        private ImpObservableCollection<ConnectionViewModel>? _attachedConnections;

        /// <summary>
        /// The hotspot (or center) of the connector.
        /// This is pushed through from ConnectorItem in the UI.
        /// </summary>
        private Point _hotspot;

        #endregion Internal Data Members

        public ConnectorViewModel(string name, IEventAggregator? eventAggregator, string paratextProjectId)
        {
            _eventAggregator = eventAggregator;
            Name = name;
            Type = ConnectorType.Undefined;
            ParatextId = paratextProjectId;
        }

        public string ParatextId { get; init; }

        /// <summary>
        /// The name of the connector.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Defines the type of the connector.
        /// </summary>
        public ConnectorType Type { get; internal set; }

        /// <summary>
        /// Returns 'true' if the connector connected to another node.
        /// </summary>
        public bool IsConnected
        {
            get
            {
                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                return AttachedConnections.Any(connection => connection.SourceConnector != null && connection.DestinationConnector != null);
            }
        }

        /// <summary>
        /// Returns 'true' if a connection is attached to the connector.
        /// The other end of the connection may or may not be attached to a node.
        /// </summary>
        public bool IsConnectionAttached => AttachedConnections.Count > 0;

        /// <summary>
        /// The connections that are attached to this connector, or null if no connections are attached.
        /// </summary>
        public ImpObservableCollection<ConnectionViewModel> AttachedConnections
        {
            get
            {
                if (_attachedConnections == null)
                {
                    _attachedConnections = new ImpObservableCollection<ConnectionViewModel>();
                    _attachedConnections.ItemsAdded += OnAttachedConnectionsItemsAdded;
                    _attachedConnections.ItemsRemoved += OnAttachedConnectionsItemsRemoved;
                }

                return _attachedConnections;
            }
        }

        /// <summary>
        /// The parent node that the connector is attached to, or null if the connector is not attached to any node.
        /// </summary>
        public CorpusNodeViewModel? ParentNode
        {
            get;
            internal set;
        }

        /// <summary>
        /// The hotspot (or center) of the connector.
        /// This is pushed through from ConnectorItem in the UI.
        /// </summary>
        public Point Hotspot
        {
            get => _hotspot;
            set
            {
                if (_hotspot == value)
                {
                    return;
                }

                _hotspot = value;

                OnHotspotUpdated();
            }
        }


        private Guid _selectedConnection;

        public Guid SelectedConnection
        {
            get => _selectedConnection;
            set
            {
                Set(ref _selectedConnection, value);
                _eventAggregator.PublishOnUIThreadAsync(new ConnectionSelectedChangedMessage(value));
            }
        }



        /// <summary>
        /// Event raised when the connector hotspot has been updated.
        /// </summary>
        public event EventHandler<EventArgs>? HotspotUpdated;

        #region Private Methods

        /// <summary>
        /// Debug checking to ensure that no connection is added to the list twice.
        /// </summary>
        private void OnAttachedConnectionsItemsAdded(object? sender, CollectionItemsChangedEventArgs e)
        {
            foreach (ConnectionViewModel connection in e.Items)
            {
                connection.ConnectionChanged += OnConnectionChanged;
            }

            if ((AttachedConnections.Count - e.Items.Count) == 0)
            {
                // 
                // The first connection has been added, notify the data-binding system that
                // 'IsConnected' should be re-evaluated.
                //
                NotifyOfPropertyChange(() => IsConnectionAttached);
                NotifyOfPropertyChange(() => IsConnected);
            }
        }

        /// <summary>
        /// Event raised when connections have been removed from the connector.
        /// </summary>
        private void OnAttachedConnectionsItemsRemoved(object? sender, CollectionItemsChangedEventArgs e)
        {
            foreach (ConnectionViewModel connection in e.Items)
            {
                connection.ConnectionChanged -= OnConnectionChanged;
            }

            if (AttachedConnections.Count == 0)
            {
                // 
                // No longer connected to anything, notify the data-binding system that
                // 'IsConnected' should be re-evaluated.
                //
                NotifyOfPropertyChange(() => IsConnectionAttached);
                NotifyOfPropertyChange(() => IsConnected);
            }
        }

        /// <summary>
        /// Event raised when a connection attached to the connector has changed.
        /// </summary>
        private void OnConnectionChanged(object? sender, EventArgs e)
        {
            NotifyOfPropertyChange(() => IsConnectionAttached);
            NotifyOfPropertyChange(() => IsConnected);
        }

        /// <summary>
        /// Called when the connector hotspot has been updated.
        /// </summary>
        private void OnHotspotUpdated()
        {
            NotifyOfPropertyChange(() => Hotspot);

            if (HotspotUpdated != null)
            {
                HotspotUpdated(this, EventArgs.Empty);
            }
        }

        #endregion Private Methods
    }
}
