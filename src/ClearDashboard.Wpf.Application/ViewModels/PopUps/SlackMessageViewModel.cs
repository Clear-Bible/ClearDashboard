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
using ClearDashboard.DataAccessLayer.Models;
using static ClearDashboard.Wpf.Application.Helpers.SlackMessage;
using System.Dynamic;
using ClearDashboard.Wpf.Application.Models;
using ClearDashboard.DataAccessLayer.Models.LicenseGenerator;
using MdXaml;
using System.Windows.Documents;
using Markdig;
using Markdown = Markdig.Markdown;

namespace ClearDashboard.Wpf.Application.ViewModels.PopUps
{
    public class SlackMessageViewModel : DashboardApplicationScreen
    {
        #region Member Variables   
        private readonly ILogger<SlackMessageViewModel> _logger;
        private readonly CollaborationServerHttpClientServices _collaborationHttpClientServices;


        private User _currentDashboardUser;
        private DashboardUser _dashboardUser;
        private string _zipPathAttachment { get; set; } = string.Empty;
        private List<JiraUser> _jiraUsersList = new();

        #endregion //Member Variables



        #region Public Properties

        public string ParatextUser { get; set; } = string.Empty;
        public User DashboardUser { get; set; }
        public CollaborationConfiguration GitLabUser { get; set; }
        public List<string> Files { get; set; }

        #endregion //Public Properties


        #region Observable Properties

        private bool _bugReport = true;
        public bool BugReport
        {
            get => _bugReport;
            set
            {
                _bugReport = value;
                NotifyOfPropertyChange(() => BugReport);
            }
        }


        private bool _suggestion;
        public bool Suggestion
        {
            get => _suggestion;
            set
            {
                _suggestion = value;
                NotifyOfPropertyChange(() => Suggestion);
            }
        }


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


        private string _jiraTitle;
        public string JiraTitle
        {
            get => _jiraTitle;
            set
            {
                _jiraTitle = value;
                NotifyOfPropertyChange(() => JiraTitle);
                CheckJiraButtonEnabled();
            }
        }

        private string _jiraSeverity;
        public string JiraSeverity
        {
            get => _jiraSeverity;
            set
            {
                _jiraSeverity = value;
                NotifyOfPropertyChange(() => JiraSeverity);
                CheckJiraButtonEnabled();
            }
        }


        private string _jiraDescription;
        public string JiraDescription
        {
            get => _jiraDescription;
            set
            {
                _jiraDescription = value;
                NotifyOfPropertyChange(() => JiraDescription);
                CheckJiraButtonEnabled();
            }
        }

        private bool _jiraButtonEnabled = false;
        public bool JiraButtonEnabled
        {
            get => _jiraButtonEnabled;
            set
            {
                _jiraButtonEnabled = value;
                NotifyOfPropertyChange(() => JiraButtonEnabled);
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
            CollaborationServerHttpClientServices collaborationHttpClientServices,
            ILifetimeScope? lifetimeScope, ILocalizationService localizationService)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
        {
            _logger = logger;
            _collaborationHttpClientServices = collaborationHttpClientServices;

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


            _jiraUsersList = await JiraClient.GetAllUsers();

            
            _currentDashboardUser = ProjectManager.CurrentUser;
            _dashboardUser = await _collaborationHttpClientServices.GetDashboardUserExistsById(_currentDashboardUser.Id);

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

            SlackMessageType slackMessageType = SlackMessageType.BugReport;


            if (BugReport)
            {
                slackMessageType = SlackMessageType.BugReport;


                SlackMessage slackMessage = new SlackMessage(msg, this._zipPathAttachment, logger, slackMessageType);
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
            else
            {
                slackMessageType = SlackMessageType.Suggestion;
            }



        }


        private void CheckJiraButtonEnabled()
        {
            if (string.IsNullOrEmpty(JiraTitle) == false && string.IsNullOrEmpty(JiraDescription) == false && string.IsNullOrEmpty(JiraSeverity) == false)
            {
                JiraButtonEnabled = true;
            }
            else
            {
                JiraButtonEnabled = false;
            }
        }   


        public async Task SendJiraMessage()
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

            // does the user have a jira account?
            var jiraUser = await JiraClient.GetUserByEmail(_jiraUsersList, _dashboardUser);



            // convert the markdown to html
            string markdownTxt = File.ReadAllText(@"d:\Downloads\markdown-cheat-sheet.md");

            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            var html = Markdown.ToHtml(JiraDescription, pipeline);

            // convert html to ADF format
            var adf = await Html2Adf.Convert(html);


            var result = await JiraClient.CreateTaskTicket(JiraTitle, adf, JiraSeverity, jiraUser);
              





            //var thisVersion = Assembly.GetEntryAssembly().GetName().Version;
            //var versionNumber = $"{thisVersion.Major}.{thisVersion.Minor}.{thisVersion.Build}.{thisVersion.Revision}";

            //string msg = $"*Dashboard User:* {DashboardUser.FullName} \n*Paratext User:* {ParatextUser} \n*Github User:* {GitLabUser.RemoteUserName} \n*Version*: {versionNumber} \n*Message:* \n{UserMessage}";

            //var logger = LifetimeScope.Resolve<ILogger<SlackMessage>>();

            //SlackMessageType slackMessageType = SlackMessageType.BugReport;


            //if (BugReport)
            //{
            //    slackMessageType = SlackMessageType.BugReport;


            //    SlackMessage slackMessage = new SlackMessage(msg, this._zipPathAttachment, logger, slackMessageType);
            //    var bSuccess = await slackMessage.SendFileToSlackAsync();

            //    if (bSuccess == true)
            //    {
            //        SendSuccessfulVisibility = Visibility.Visible;
            //        SendErrorVisibility = Visibility.Collapsed;
            //        WorkingMessage = "Message Sent Successfully";
            //    }
            //    else
            //    {
            //        SendSuccessfulVisibility = Visibility.Collapsed;
            //        SendErrorVisibility = Visibility.Visible;
            //        WorkingMessage = "Message Sending Problem";
            //    }

            //}
            //else
            //{
            //    slackMessageType = SlackMessageType.Suggestion;
            //}



        }


        public void ClickMarkDown()
        {
            dynamic settings = new ExpandoObject();
            settings.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            settings.ResizeMode = ResizeMode.CanResize;
            settings.MinWidth = 500;
            settings.MinHeight = 500;
            settings.Title = "Markdown Format";

            var viewModel = IoC.Get<MarkDownViewModel>();

            IWindowManager manager = new WindowManager();
            manager.ShowWindowAsync(viewModel, null, settings);

        }

        #endregion // Methods
    }
}
