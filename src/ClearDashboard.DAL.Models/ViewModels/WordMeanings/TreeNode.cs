using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ClearDashboard.DataAccessLayer.Models.ViewModels.WordMeanings
{
    public delegate void TreeEventHandler(object sender, TreeEventArgs e);
    public class TreeEventArgs : EventArgs
    {
        private string _letter;
        private bool _isExpanded;
        private int _nodeId;
        public TreeEventArgs(string letter, bool isExpanded, int nodeId)
        {
            _letter = letter;
            _isExpanded = isExpanded;
            _nodeId = nodeId;
        }
        public string Letter
        {
            get => _letter;
        }

        public bool IsExpanded
        {
            get => _isExpanded;
        }

        public int NodeId
        {
            get => _nodeId;
        }
    }

    public class TreeNode : INotifyPropertyChanged
    {
        public event TreeEventHandler NodeExpanded;
        public event PropertyChangedEventHandler PropertyChanged;

        public int NodeId { get; set; }
        public string NodeName { get; set; }
        public string Letter { get; set; }


        private ObservableCollection<TreeNode> _childNodes = new();
        public ObservableCollection<TreeNode> ChildNodes
        {
            get => _childNodes;
            set
            {
                _childNodes = value;
                OnPropertyChanged();
            }
        }


        public bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                _isExpanded = value;
                OnPropertyChanged();
                OnExpandedChange(this, new TreeEventArgs(Letter, value, NodeId));
            }
        }

        public void OnExpandedChange(object sender, TreeEventArgs data)
        {
            // Check if there are any Subscribers
            if (NodeExpanded != null)
            {
                if (IsExpanded)
                {
                    // Call the Event
                    NodeExpanded(this, data);
                }

            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }
}

