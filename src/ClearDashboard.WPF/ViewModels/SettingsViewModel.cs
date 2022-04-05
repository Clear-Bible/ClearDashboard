using System;
using Caliburn.Micro;
using ClearDashboard.Wpf.Views;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using ClearDashboard.DataAccessLayer;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.ViewModels
{
    public class SettingsViewModel : ApplicationScreen
    {
        #region Member Variables
        private ILogger _logger { get; set; }
        private INavigationService _navigationService { get; set; }
        private ProjectManager _projectManager;


        #endregion //Member Variables

        #region Public Properties

        #endregion //Public Properties

        #region Observable Properties

        public ObservableCollection<ItemInfo> observableCollection { get; set; } = new ObservableCollection<ItemInfo>();

        private FlowDirection _flowDirection = FlowDirection.LeftToRight;
        public FlowDirection flowDirection
        {
            get => _flowDirection;
            set
            {
                _flowDirection = value;
                NotifyOfPropertyChange(() => flowDirection);
            }
        }

        #endregion //Observable Properties

        #region Constructor

        /// <summary>
        /// Required for design-time support
        /// </summary>
        public SettingsViewModel()
        {

        }

        public SettingsViewModel(INavigationService navigationService, ILogger<SettingsViewModel> logger, ProjectManager projectManager) : base(navigationService, logger)
        {
            _logger = logger;
            _navigationService = navigationService;
            _projectManager = projectManager;
            flowDirection = _projectManager.CurrentLanguageFlowDirection;

            observableCollection.Add(new ItemInfo() { ImagePath = @"/Resources/NewProject_Icon_96.png", ImageName = "NEW" });
            observableCollection.Add(new ItemInfo() { ImagePath = @"/Resources/settings_logo_96.png", ImageName = "SETTINGS" });
        }

        #endregion //Constructor

        #region Methods

        #endregion // Methods
    }
}
