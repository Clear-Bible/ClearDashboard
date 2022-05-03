using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.Views;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Diagnostics;
using ClearDashboard.Wpf.Models;

namespace ClearDashboard.Wpf.ViewModels
{
    public class SettingsViewModel : ApplicationScreen
    {
        #region Member Variables
      

        #endregion //Member Variables

        #region Public Properties

        #endregion //Public Properties

        #region Observable Properties

        public ObservableCollection<ItemInfo> observableCollection { get; set; } = new ObservableCollection<ItemInfo>();

        #endregion //Observable Properties

        #region Constructor

        /// <summary>
        /// Required for design-time support
        /// </summary>
        public SettingsViewModel()
        {

        }

        public SettingsViewModel(INavigationService navigationService, 
            ILogger<SettingsViewModel> logger, DashboardProjectManager projectManager) 
            : base(navigationService, logger, projectManager)
        {
           
         
            observableCollection.Add(new ItemInfo() { ImagePath = @"/Resources/NewProject_Icon_96.png", ImageName = "NEW" });
            observableCollection.Add(new ItemInfo() { ImagePath = @"/Resources/settings_logo_96.png", ImageName = "SETTINGS" });





            FileLayouts.Add(new LayoutFile { LayoutID="1", LayoutName="NAME:1", LayoutPath="PATH:1" });
            FileLayouts.Add(new LayoutFile { LayoutID = "2", LayoutName = "NAME:2", LayoutPath = "PATH:2" });
            FileLayouts.Add(new LayoutFile { LayoutID = "3", LayoutName = "NAME:3", LayoutPath = "PATH:3" });
        }

        #endregion //Constructor

        #region Methods

        private bool _gridIsVisible = true;
        public bool GridIsVisible
        {
            get => _gridIsVisible;
            set
            {
                _gridIsVisible = value;
                NotifyOfPropertyChange(() => GridIsVisible);
            }
        }


        private ObservableCollection<LayoutFile> _fileLayouts = new();
        public ObservableCollection<LayoutFile> FileLayouts
        {
            get => _fileLayouts;
            set
            {
                _fileLayouts = value;
                NotifyOfPropertyChange(() => FileLayouts);
            }
        }

        private LayoutFile _SelectedLayout;
        public LayoutFile SelectedLayout
        {
            get => _SelectedLayout;
            set
            {
                _SelectedLayout = value;
                NotifyOfPropertyChange(nameof(SelectedLayout));
            }
        }

        private string _selectedLayoutText;
        public string SelectedLayoutText
        {
            get => _selectedLayoutText;
            set
            {
                _selectedLayoutText = value;
                NotifyOfPropertyChange(() => SelectedLayoutText);
            }
        }


        public void OkSave()
        {
            // todo
            if (SelectedLayout is not null)
            {
                Debug.WriteLine(SelectedLayout.LayoutPath);
            }
            else
            {
                Debug.WriteLine(SelectedLayoutText);
            }
            GridIsVisible = false;
        }

        public void CancelSave()
        {
            GridIsVisible = false;
        }

        public void FlipVisibility()
        {
            GridIsVisible = !GridIsVisible;
        }

        #endregion // Methods
    }
}
