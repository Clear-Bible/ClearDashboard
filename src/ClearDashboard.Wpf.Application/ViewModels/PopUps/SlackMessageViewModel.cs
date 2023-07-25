using Autofac;
using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using ClearApplicationFoundation.Services;
using ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Wpf.Application.ViewModels.PopUps
{
    public class SlackMessageViewModel : DashboardApplicationScreen
    {
        #region Member Variables   
        private readonly ILogger<SlackMessageViewModel> _logger;

        private string _zipPathAttachment { get; set; } = string.Empty;

        #endregion //Member Variables



        #region Public Properties

        public string ParatextUser { get; set; } = string.Empty;
        public User DashboardUser { get; set; }
        public CollaborationConfiguration GitLabUser { get; set; }
        public List<string> Files { get; set; }

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

        private Visibility _showOkButton = Visibility.Collapsed;

        public Visibility ShowOkButton
        {
            get => _showOkButton;
            set
            {
                _showOkButton = value;
                NotifyOfPropertyChange(() => ShowOkButton);
            }
        }

        private string _workingMessage = "";
        public string WorkingMessage
        {
            get => _workingMessage;
            set
            {
                _workingMessage = value;
                NotifyOfPropertyChange(() => WorkingMessage);
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
            ILifetimeScope? lifetimeScope, ILocalizationService localizationService)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope,localizationService)
        {
            _logger = logger;

            WorkingMessage = "Gathering Files for Transmission";
        }

        protected override async void OnViewLoaded(object view)
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

            ComputerInfo computerInfo = new();
            var destinationComputerInfoPath = Path.Combine(Path.GetTempPath(), "computerInfo.log");

            _ = await computerInfo.GetComputerInfo(destinationComputerInfoPath);
            Files.Add(destinationComputerInfoPath);

            var guid = Guid.NewGuid().ToString();
            _zipPathAttachment = Path.Combine(Path.GetTempPath(), $"{guid}.zip");
            // zip up everything
            if (Files.Count > 0)
            {
                if (File.Exists(_zipPathAttachment))
                {
                    File.Delete(_zipPathAttachment);
                }

                ZipFiles zipFiles = new(Files, _zipPathAttachment);
                var succcess = zipFiles.Zip();

                if (succcess == false)
                {
                    _logger.LogError("Error zipping files");
                }
            }

            ShowOkButton = Visibility.Visible;

            WorkingMessage = "";

            base.OnViewLoaded(view);
        }

        #endregion //Constructor



        #region Methods

        public async void Close()
        {
            await this.TryCloseAsync();
        }


        public async Task SendMessage()
        {
            var bRet = await NetworkHelper.IsConnectedToInternet();
            // check internet connection
            if (bRet == false)
            {
                _logger.LogWarning("No internet connection available");
                return;
            }

            WorkingMessage = "Sending Message...";
            await Task.Delay(200);


            var thisVersion = Assembly.GetEntryAssembly().GetName().Version;
            var versionNumber = $"{thisVersion.Major}.{thisVersion.Minor}.{thisVersion.Build}.{thisVersion.Revision}";

            string msg = $"*Dashboard User:* {DashboardUser.FullName} \n*Paratext User:* {ParatextUser} \n*Github User:* {GitLabUser.RemoteUserName} \n*Version*: {versionNumber} \n*Message:* \n{UserMessage}";

            var logger = LifetimeScope.Resolve<ILogger<SlackMessage>>();
            SlackMessage slackMessage = new SlackMessage(msg, this._zipPathAttachment, logger);
            var bSuccess = await slackMessage.SendFileToSlackAsync();

            if (bSuccess == true)
            {
                SendSuccessfulVisibility = Visibility.Visible;
                SendErrorVisibility = Visibility.Collapsed;
                WorkingMessage = "Message Sent Successfully";
            }
            else
            {
                SendSuccessfulVisibility = Visibility.Collapsed;
                SendErrorVisibility = Visibility.Visible;
                WorkingMessage = "Message Sending Problem";
            }
            
        }

        #endregion // Methods
    }
}
