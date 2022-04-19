using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using ClearDashboard.Common.Models;
using ClearDashboard.DataAccessLayer;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.ViewModels
{
    public class ProcessUSFMViewModel : ApplicationScreen
    {
        #region   Member Variables

        private readonly INavigationService _navigationService;
        private readonly ProjectManager _projectManager;
        private readonly ILogger _logger;

        public DashboardProject DashboardProject { get; set; }

        #endregion


        #region Observable Objects

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

        #endregion


        #region Constructor
        public ProcessUSFMViewModel()
        {
            
        }

        public ProcessUSFMViewModel(ProjectManager projectManager, INavigationService navigationService, 
            ILogger<LandingViewModel> logger) : base(navigationService, logger)
        {
            _logger = logger;
            _navigationService = navigationService;
            _projectManager = projectManager;

            flowDirection = _projectManager.CurrentLanguageFlowDirection;
        }

        protected override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine();
            return base.OnActivateAsync(cancellationToken);
        }

        #endregion



        #region Methods


        #endregion

    }
}
