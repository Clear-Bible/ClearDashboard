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
        private readonly IEventAggregator _eventAggregator;

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
            ILogger<LandingViewModel> logger, IEventAggregator eventAggregator) : base(navigationService, logger)
        {
            _logger = logger;
            _navigationService = navigationService;
            _projectManager = projectManager;
            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe(this);

            flowDirection = _projectManager.CurrentLanguageFlowDirection;
        }

        protected override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            return base.OnActivateAsync(cancellationToken);
        }

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            return base.OnDeactivateAsync(close, cancellationToken);
        }


        public void Handle(object message)
        {
            Console.WriteLine();
        }
        #endregion



        #region Methods


        #endregion
    }
}
