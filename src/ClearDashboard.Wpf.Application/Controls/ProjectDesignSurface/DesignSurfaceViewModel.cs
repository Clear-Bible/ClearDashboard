using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.Controls.Utils;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface
{
    /// <summary>
    /// Defines a network of nodes and connections between the nodes.
    /// </summary>
    public sealed class DesignSurfaceViewModel
    {

        #region Internal Data Members

        /// <summary>
        /// The collection of nodes in the network.
        /// </summary>
        private ImpObservableCollection<CorpusNodeViewModel>? _nodes;

        /// <summary>
        /// The collection of connections in the network.
        /// </summary>
        private ImpObservableCollection<ConnectionViewModel>? _connections;

        #endregion Internal Data Members

        private readonly INavigationService? _navigationService;
        private readonly ILogger<DesignSurfaceViewModel>? _logger;
        private readonly DashboardProjectManager? _projectManager;

        public IEventAggregator? EventAggregator { get; set; }


        /// <summary>
        /// The collection of nodes in the network.
        /// </summary>
        public ImpObservableCollection<CorpusNodeViewModel> CorpusNodes => _nodes ??= new ImpObservableCollection<CorpusNodeViewModel>();

        /// <summary>
        /// The collection of connections in the network.
        /// </summary>
        public ImpObservableCollection<ConnectionViewModel> Connections
        {
            get
            {
                if (_connections == null)
                {
                    _connections = new ImpObservableCollection<ConnectionViewModel>();
                    _connections.ItemsRemoved += OnConnectionsItemsRemoved;
                    _connections.ItemsSelected += OnConnectionsItemsSelected;
                }

                return _connections;
            }
        }

        #region Private Methods

        /// <summary>
        /// Event raised then Connections have been removed.
        /// </summary>
        private void OnConnectionsItemsRemoved(object? sender, CollectionItemsChangedEventArgs e)
        {
            foreach (ConnectionViewModel connection in e.Items)
            {
                connection.SourceConnector = null;
                connection.DestinationConnector = null;
            }
        }

        /// <summary>
        /// Event raised then Connections has been selected.
        /// </summary>
        private void OnConnectionsItemsSelected(object? sender, CollectionItemsChangedEventArgs e)
        {
            foreach (ConnectionViewModel connection in e.Items)
            {
                connection.SourceConnector = null;
                connection.DestinationConnector = null;
            }
        }

        #endregion Private Methods

        #region ctor

        public DesignSurfaceViewModel(INavigationService? navigationService,
            ILogger<DesignSurfaceViewModel>? logger,
            DashboardProjectManager? projectManager, IEventAggregator? eventAggregator)
        {
            _navigationService = navigationService;
            _logger = logger;
            _projectManager = projectManager;
            EventAggregator = eventAggregator;
        }
        #endregion
    }
}
