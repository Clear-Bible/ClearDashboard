using Caliburn.Micro;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.Controls.Utils;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.DataAccessLayer.Wpf.Messages;
using Size = System.Windows.Size;

namespace ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface
{
    /// <summary>
    /// Defines a node in the view-model.
    /// CorpusNodes are connected to other nodes through attached connectors (aka anchor/connection points).
    /// </summary>
    public class CorpusNodeViewModel : Screen, IHandle<ConnectionSelectedChangedMessage>
    {

        #region events

        /// <summary>
        /// Event raised when the size of the node is changed.
        /// The size will change when the UI has determined its size based on the contents
        /// of the nodes data-template.  It then pushes the size through to the view-model
        /// and this 'SizeChanged' event occurs.
        /// </summary>
        public event EventHandler<EventArgs>? SizeChanged;

        #endregion

        #region Private Data Members

        /// <summary>
        /// The name of the node.
        /// </summary>
        private string _name = string.Empty;

        private readonly IEventAggregator? _eventAggregator;


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
        private ImpObservableCollection<ParallelCorpusConnectorViewModel> _inputConnectors = null;

        /// <summary>
        /// List of output connectors (connections points) attached to the node.
        /// </summary>
        private ImpObservableCollection<ParallelCorpusConnectorViewModel> _outputConnectors = null;

        /// <summary>
        /// Set to 'true' when the node is selected.
        /// </summary>
        private bool _isSelected = false;

        #endregion Private Data Members

        #region Public Properties

        /// <summary>
        /// The name of the node.
        /// </summary>
        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }

        [JsonIgnore]
        public string TranslationFontFamily { get; set; } = FontNames.DefaultFontFamily;

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
        [JsonIgnore]
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
        [JsonIgnore]
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

        #endregion Public Properties


        #region Observable Properties

        /// <summary>
        /// List of input connectors (connections points) attached to the node.
        /// </summary>
        [JsonIgnore]
        public ImpObservableCollection<ParallelCorpusConnectorViewModel> InputConnectors
        {
            get
            {
                if (_inputConnectors == null)
                {
                    _inputConnectors = new ImpObservableCollection<ParallelCorpusConnectorViewModel>();
                    _inputConnectors.ItemsAdded += OnInputConnectorsItemsAdded;
                    _inputConnectors.ItemsRemoved += OnInputConnectorsItemsRemoved;
                }

                return _inputConnectors;
            }
        }

        /// <summary>
        /// List of output connectors (connections points) attached to the node.
        /// </summary>
        [JsonIgnore]
        public ImpObservableCollection<ParallelCorpusConnectorViewModel> OutputConnectors
        {
            get
            {
                if (_outputConnectors == null)
                {
                    _outputConnectors = new ImpObservableCollection<ParallelCorpusConnectorViewModel>();
                    _outputConnectors.ItemsAdded += OnOutputConnectorsItemsAdded;
                    _outputConnectors.ItemsRemoved += OnOutputConnectorsItemsRemoved;
                }

                return _outputConnectors;
            }
        }

        /// <summary>
        /// A helper property that retrieves a list (a new list each time) of all connections attached to the node. 
        /// </summary>
        [JsonIgnore]
        public ICollection<ParallelCorpusConnectionViewModel> AttachedConnections
        {
            get
            {
                var attachedConnections = new List<ParallelCorpusConnectionViewModel>();

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
        [JsonIgnore]
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                Set(ref _isSelected, value);

                _eventAggregator.PublishOnUIThreadAsync(_isSelected
                    ? new NodeSelectedChangedMessage(this)
                    : new NodeSelectedChangedMessage(null));
            }
        }

        //private BindableCollection<SerializedTokenization> _tokenizations = new();
        //public BindableCollection<SerializedTokenization> Tokenizations
        //{
        //    get => _tokenizations;
        //    set =>Set(ref _tokenizations, value);
        //}

        [JsonIgnore]
        public int TokenizationCount
        {
            get => _tokenizationCount;
            set => Set(ref _tokenizationCount, value);
        }

        //public void NotifyOfTokenizationCount()
        //{
        //    NotifyOfPropertyChange(()=>TokenizationCount);
        //}

        private bool _isRtl;
        [JsonIgnore]
        public bool IsRtl
        {
            get => _isRtl;
            set => Set(ref _isRtl, value);
        }

        [JsonIgnore]
        private Guid _id = Guid.NewGuid();
        public Guid Id
        {
            get => _id;
            set => Set(ref _id, value);
        }

        private Guid _corpusId;
        public Guid CorpusId
        {
            get => _corpusId;
            set => Set(ref _corpusId, value);
        }

        [JsonIgnore]
        private BindableCollection<CorpusNodeMenuItemViewModel> _menuItems = new();
        public BindableCollection<CorpusNodeMenuItemViewModel> MenuItems
        {
            get => _menuItems;
            set => Set(ref _menuItems, value);
        }

        private string _paratextProjectId = string.Empty;
        /// <summary>
        /// The paratext guid
        /// </summary>
        [JsonIgnore]
        public string ParatextProjectId
        {
            get => _paratextProjectId;
            set => Set(ref _paratextProjectId, value);
        }

        
        private CorpusType _corpusType = CorpusType.Standard;
        private int _tokenizationCount;

        /// <summary>
        /// The paratext project type
        /// </summary>
        [JsonIgnore]
        public CorpusType CorpusType
        {
            get => _corpusType;
            set => Set(ref _corpusType, value);
        }

        #endregion //Observable Properties



        #region Constructor

        public CorpusNodeViewModel()
        {
        }

        public CorpusNodeViewModel(string name, IEventAggregator? eventAggregator)
        {
            _name = name;
            _eventAggregator = eventAggregator;
        }

        #endregion //Constructor


        #region Private Methods

        /// <summary>
        /// Event raised when connectors are added to the node.
        /// </summary>
        private void OnInputConnectorsItemsAdded(object? sender, CollectionItemsChangedEventArgs e)
        {
            foreach (ParallelCorpusConnectorViewModel connector in e.Items)
            {
                connector.ParentNode = this;
                connector.ConnectorType = ConnectorType.Input;
            }
        }

        /// <summary>
        /// Event raised when connectors are removed from the node.
        /// </summary>
        private void OnInputConnectorsItemsRemoved(object? sender, CollectionItemsChangedEventArgs e)
        {
            foreach (ParallelCorpusConnectorViewModel connector in e.Items)
            {
                connector.ParentNode = null;
                connector.ConnectorType = ConnectorType.Undefined;
            }
        }

        /// <summary>
        /// Event raised when connectors are added to the node.
        /// </summary>
        private void OnOutputConnectorsItemsAdded(object sender, CollectionItemsChangedEventArgs e)
        {
            foreach (ParallelCorpusConnectorViewModel connector in e.Items)
            {
                connector.ParentNode = this;
                connector.ConnectorType = ConnectorType.Output;
            }
        }

        /// <summary>
        /// Event raised when connectors are removed from the node.
        /// </summary>
        private void OnOutputConnectorsItemsRemoved(object sender, CollectionItemsChangedEventArgs e)
        {
            foreach (ParallelCorpusConnectorViewModel connector in e.Items)
            {
                connector.ParentNode = null;
                connector.ConnectorType = ConnectorType.Undefined;
            }
        }


        #endregion Private Methods

        public Task HandleAsync(ConnectionSelectedChangedMessage message, CancellationToken cancellationToken)
        {
            var connection = message.ConnectorId;

            return Task.CompletedTask;
        }


    }
}
