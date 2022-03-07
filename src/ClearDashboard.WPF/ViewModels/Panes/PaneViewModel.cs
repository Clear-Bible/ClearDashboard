using Caliburn.Micro;
using System.Windows.Media;

namespace ClearDashboard.Wpf.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class PaneViewModel : PropertyChangedBase
    {
        public enum EDockSide
        {
            Left,
            Bottom,
        }


        #region Member Variables
        private string _title = null;
        private string _contentId = null;
        private bool _isSelected = false;
        private bool _isActive = false;
        #endregion //Member Variables

        #region Public Properties

        public EDockSide DockSide = EDockSide.Bottom;

        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    NotifyOfPropertyChange(() => Title);
                }
            }
        }

        public ImageSource IconSource { get; protected set; }

        public string ContentId
        {
            get => _contentId;
            set
            {
                if (_contentId != value)
                {
                    _contentId = value;
                    NotifyOfPropertyChange(() => ContentId);
                }
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    NotifyOfPropertyChange(() => IsSelected);
                }
            }
        }

        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    NotifyOfPropertyChange(() => IsActive);
                }
            }
        }
		#endregion //Public Properties

		#region Observable Properties

		#endregion //Observable Properties

		#region Constructor
		public PaneViewModel()
        {
        }

        #endregion //Constructor

        #region Methods

        #endregion // Methods

    }
}
