using ClearDashboard.Common.Models;
using MvvmHelpers;
using Newtonsoft.Json;
using System.Windows.Media;

namespace ClearDashboard.Wpf.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class PaneViewModel : ObservableObject
    {
        #region Member Variables
        private string _title = null;
        private string _contentId = null;
        private bool _isSelected = false;
        private bool _isActive = false;
        #endregion //Member Variables

        #region Public Properties


        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    SetProperty(ref _title, value, nameof(Title));
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
                    SetProperty(ref _contentId, value, nameof(ContentId));
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
                    SetProperty(ref _isSelected, value, nameof(IsSelected));
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
                    SetProperty(ref _isActive, value, nameof(IsActive));
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
