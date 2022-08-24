using ClearDashboard.Wpf.Controls.Utils;

namespace ViewModels.ProjectDesignSurface
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
        private ImpObservableCollection<CorpusNodeViewModel> _nodes;

        /// <summary>
        /// The collection of connections in the network.
        /// </summary>
        private ImpObservableCollection<ConnectionViewModel> _connections;

        #endregion Internal Data Members

        /// <summary>
        /// The collection of nodes in the network.
        /// </summary>
        public ImpObservableCollection<CorpusNodeViewModel> CorpusNodes
        {
            get
            {
                if (_nodes == null)
                {
                    _nodes = new ImpObservableCollection<CorpusNodeViewModel>();
                }

                return _nodes;
            }
        }

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
                }

                return _connections;
            }
        }

        #region Private Methods

        /// <summary>
        /// Event raised then Connections have been removed.
        /// </summary>
        private void OnConnectionsItemsRemoved(object sender, CollectionItemsChangedEventArgs e)
        {
            foreach (ConnectionViewModel connection in e.Items)
            {
                connection.SourceConnector = null;
                connection.DestinationConnector = null;
            }
        }

        #endregion Private Methods

        #region ctor

        public DesignSurfaceViewModel()
        {
            //CorpusNodes.CorpusNodeViewModel.SomethingHappened += HandleEvent;
        }
        #endregion
    }
}
