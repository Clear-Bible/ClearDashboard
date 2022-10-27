using Autofac;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.DataAccessLayer.Wpf.Infrastructure;
using ClearDashboard.Wpf.Application.Helpers;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ClearDashboard.Wpf.Application.ViewModels.PopUps
{
    public class SlackMessageViewModel : DashboardApplicationScreen
    {
        #region Member Variables   
        private readonly ILogger<SlackMessageViewModel> _logger;

        #endregion //Member Variables

        
        
        #region Public Properties
        public string FilePathAttachment { get; set; } = string.Empty;
        public string ParatextUser { get; set; } = string.Empty;
        
        #endregion //Public Properties


        #region Observable Properties

        private string _userMessage;
        public string UserMessage
        {
            get => _userMessage;
            set
            {
                _userMessage = value;
                NotifyOfPropertyChange(() => UserMessage);
            }
        }


        private Visibility _noInternetVisibility = Visibility.Collapsed;
        public Visibility NoInternetVisibility
        {
            get => _noInternetVisibility;
            set
            {
                _noInternetVisibility = value;
                NotifyOfPropertyChange(() => NoInternetVisibility);
            }
        }
        
        private Visibility _sendErrorVisibility = Visibility.Collapsed;
        public Visibility SendErrorVisibility
        {
            get => _sendErrorVisibility;
            set
            {
                _sendErrorVisibility = value;
                NotifyOfPropertyChange(() => SendErrorVisibility);
            }
        }

        private Visibility _sendSuccessfulVisibility = Visibility.Collapsed;
        public Visibility SendSuccessfulVisibility
        {
            get => _sendSuccessfulVisibility;
            set
            {
                _sendSuccessfulVisibility = value;
                NotifyOfPropertyChange(() => SendSuccessfulVisibility);
            }
        }

        #endregion //Observable Properties


        #region Constructor

        public SlackMessageViewModel()
        {
            // no-op
        }

        public SlackMessageViewModel(INavigationService navigationService, ILogger<SlackMessageViewModel> logger,
            DashboardProjectManager? projectManager, IEventAggregator eventAggregator, IMediator mediator,
            ILifetimeScope? lifetimeScope)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope)
        {
            _logger = logger;
        }

        protected async override void OnViewLoaded(object view)
        {
            var bRet = await NetworkHelper.IsConnectedToInternet();
            if (bRet == false)
            {
                NoInternetVisibility = Visibility.Visible;
            }
            else
            {
                NoInternetVisibility = Visibility.Collapsed;
            }
            
            base.OnViewLoaded(view);
        }

        #endregion //Constructor



        #region Methods

        public async Task SendMessage()
        {
            var bRet = await NetworkHelper.IsConnectedToInternet();
            // check internet connection
            if (bRet == false)
            {
                _logger.LogWarning("No internet connection available");
                return;
            }

            SlackMessage slackMessage = new SlackMessage("", this.FilePathAttachment, _logger as ILogger<SlackMessage>);
            var bSuccess = await slackMessage.SendFileToSlackAsync();

            if (bSuccess == true)
            {
                SendSuccessfulVisibility = Visibility.Visible;
                SendErrorVisibility = Visibility.Collapsed;
            }
            else
            {
                SendSuccessfulVisibility = Visibility.Collapsed;
                SendErrorVisibility = Visibility.Visible;
            }
        }

        #endregion // Methods
    }
}
