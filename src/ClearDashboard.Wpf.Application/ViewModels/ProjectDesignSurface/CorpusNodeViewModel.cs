using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.Application.Models;
using ClearDashboard.Wpf.Application.ViewModels.ProjectDesignSurface;
using ClearDashboard.Wpf.Controls.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ClearDashboard.Wpf.Application.ViewModels
{
    /// <summary>
    /// Defines a node in the view-model.
    /// CorpusNodes are connected to other nodes through attached connectors (aka anchor/connection points).
    /// </summary>
    public class CorpusNodeViewModel : AbstractModelBase, IHandle<ConnectionSelectedChangedMessage>
    {

        #region events

        #endregion


        #region Private Data Members

        /// <summary>
        /// The name of the node.
        /// </summary>
        private string _name = string.Empty;

        private readonly IEventAggregator? _eventAggregator;
        private readonly DashboardProjectManager? _projectManager;

        /// <summary>
        /// The X coordinate for the position of the node.
        /// </summary>
        private double _x = 0;

        /// <summary>
        /// The Y coordinate for the position of the node.
        /// </summary>
        private double _y = 0;

        /// <summary>
        /// The Z index of the node.
        /// </summary>
        private int _zIndex = 0;

        /// <summary>
        /// The size of the node.
        /// 
        /// Important Note: 
        ///     The size of a node in the UI is not determined by this property!!
        ///     Instead the size of a node in the UI is determined by the data-template for the Node class.
        ///     When the size is computed via the UI it is then pushed into the view-model
        ///     so that our application code has access to the size of a node.
        /// </summary>
        private Size _size = Size.Empty;

        /// <summary>
        /// List of input connectors (connections points) attached to the node.
        /// </summary>
        private ImpObservableCollection<ConnectorViewModel> _inputConnectors = null;

        /// <summary>
        /// List of output connectors (connections points) attached to the node.
        /// </summary>
        private ImpObservableCollection<ConnectorViewModel> _outputConnectors = null;

        /// <summary>
        /// Set to 'true' when the node is selected.
        /// </summary>
        private bool _isSelected = false;

        #endregion Private Data Members

        public CorpusNodeViewModel()
        {
        }

        public CorpusNodeViewModel(string name, IEventAggregator? eventAggregator, DashboardProjectManager? projectManager)
        {
            _name = name;
            _eventAggregator = eventAggregator;
            _projectManager = projectManager;
        }


        private List<NodeTokenization> _nodeTokenizations = new();
        public List<NodeTokenization> NodeTokenizations
        {
            get => _nodeTokenizations;
            set
            {
                _nodeTokenizations = value;
                NotifyOfPropertyChange(() => NodeTokenizations);
            }
        }


        private Guid _id = Guid.NewGuid();
        public Guid Id
        {
            get => _id;
            set
            {
                _id = value;
                NotifyOfPropertyChange(() => Id);
            }
        }

        private ObservableCollection<CorpusNodeMenuItemViewModel> _menuItems = new();
        public ObservableCollection<CorpusNodeMenuItemViewModel> MenuItems
        {
            get => _menuItems;
            set
            {
                _menuItems = value;
                NotifyOfPropertyChange(() => MenuItems);
            }
        }

        /// <summary>
        /// The name of the node.
        /// </summary>
        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }


        private string _paratextProjectId = string.Empty;
        /// <summary>
        /// The paratext guid
        /// </summary>
        public string ParatextProjectId
        {
            get => _paratextProjectId;
            set => Set(ref _paratextProjectId, value);
        }


        private CorpusType _corpusType = CorpusType.Standard;
        /// <summary>
        /// The paratext project type
        /// </summary>
        public CorpusType CorpusType
        {
            get => _corpusType;
            set => Set(ref _corpusType, value);
        }


        /// <summary>
        /// The X coordinate for the position of the node.
        /// </summary>
        public double X
        {
            get => _x;
            set => Set(ref _x, value);
        }

        /// <summary>
        /// The Y coordinate for the position of the node.
        /// </summary>
        public double Y
        {
            get => _y;
            set => Set(ref _y, value);
        }

        /// <summary>
        /// The Z index of the node.
        /// </summary>
        public int ZIndex
        {
            get => _zIndex;
            set => Set(ref _zIndex, value);
        }

        /// <summary>
        /// The size of the node.
        /// 
        /// Important Note: 
        ///     The size of a node in the UI is not determined by this property!!
        ///     Instead the size of a node in the UI is determined by the data-template for the Node class.
        ///     When the size is computed via the UI it is then pushed into the view-model
        ///     so that our application code has access to the size of a node.
        /// </summary>
        public Size Size
        {
            get => _size;
            set
            {
                Set(ref _size, value);

                if (SizeChanged != null)
                {
                    SizeChanged(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Event raised when the size of the node is changed.
        /// The size will change when the UI has determined its size based on the contents
        /// of the nodes data-template.  It then pushes the size through to the view-model
        /// and this 'SizeChanged' event occurs.
        /// </summary>
        public event EventHandler<EventArgs>? SizeChanged;

        /// <summary>
        /// List of input connectors (connections points) attached to the node.
        /// </summary>
        public ImpObservableCollection<ConnectorViewModel> InputConnectors
        {
            get
            {
                if (_inputConnectors == null)
                {
                    _inputConnectors = new ImpObservableCollection<ConnectorViewModel>();
                    _inputConnectors.ItemsAdded += OnInputConnectorsItemsAdded;
                    _inputConnectors.ItemsRemoved += OnInputConnectorsItemsRemoved;
                }

                return _inputConnectors;
            }
        }

        /// <summary>
        /// List of output connectors (connections points) attached to the node.
        /// </summary>
        public ImpObservableCollection<ConnectorViewModel> OutputConnectors
        {
            get
            {
                if (_outputConnectors == null)
                {
                    _outputConnectors = new ImpObservableCollection<ConnectorViewModel>();
                    _outputConnectors.ItemsAdded += OnOutputConnectorsItemsAdded;
                    _outputConnectors.ItemsRemoved += OnOutputConnectorsItemsRemoved;
                }

                return _outputConnectors;
            }
        }

        /// <summary>
        /// A helper property that retrieves a list (a new list each time) of all connections attached to the node. 
        /// </summary>
        public ICollection<ConnectionViewModel> AttachedConnections
        {
            get
            {
                var attachedConnections = new List<ConnectionViewModel>();

                foreach (var connector in InputConnectors)
                {
                    attachedConnections.AddRange(connector.AttachedConnections);
                }

                foreach (var connector in OutputConnectors)
                {
                    attachedConnections.AddRange(connector.AttachedConnections);
                }

                return attachedConnections;
            }
        }

        /// <summary>
        /// Set to 'true' when the node is selected.
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                Set(ref _isSelected, value);

                if (_isSelected)
                {
                    _eventAggregator.PublishOnUIThreadAsync(new NodeSelectedChangedMessage(this));
                }
                else
                {
                    _eventAggregator.PublishOnUIThreadAsync(new NodeSelectedChangedMessage(null));
                }
            }
        }

        #region Private Methods

        /// <summary>
        /// Event raised when connectors are added to the node.
        /// </summary>
        private void OnInputConnectorsItemsAdded(object sender, CollectionItemsChangedEventArgs e)
        {
            foreach (ConnectorViewModel connector in e.Items)
            {
                connector.ParentNode = this;
                connector.Type = ConnectorType.Input;
            }
        }

        /// <summary>
        /// Event raised when connectors are removed from the node.
        /// </summary>
        private void OnInputConnectorsItemsRemoved(object sender, CollectionItemsChangedEventArgs e)
        {
            foreach (ConnectorViewModel connector in e.Items)
            {
                connector.ParentNode = null;
                connector.Type = ConnectorType.Undefined;
            }
        }

        /// <summary>
        /// Event raised when connectors are added to the node.
        /// </summary>
        private void OnOutputConnectorsItemsAdded(object sender, CollectionItemsChangedEventArgs e)
        {
            foreach (ConnectorViewModel connector in e.Items)
            {
                connector.ParentNode = this;
                connector.Type = ConnectorType.Output;
            }
        }

        /// <summary>
        /// Event raised when connectors are removed from the node.
        /// </summary>
        private void OnOutputConnectorsItemsRemoved(object sender, CollectionItemsChangedEventArgs e)
        {
            foreach (ConnectorViewModel connector in e.Items)
            {
                connector.ParentNode = null;
                connector.Type = ConnectorType.Undefined;
            }
        }

        public Task HandleAsync(ConnectionSelectedChangedMessage message, CancellationToken cancellationToken)
        {
            var connection = message.ConnectorId;

            return Task.CompletedTask;
        }



        #endregion Private Methods
    }
}
