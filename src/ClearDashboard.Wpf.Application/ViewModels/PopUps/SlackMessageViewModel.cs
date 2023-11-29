using Autofac;
using Caliburn.Micro;
using CefSharp.DevTools.Network;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Models.Common;
using ClearDashboard.DataAccessLayer.Models.LicenseGenerator;
using ClearDashboard.ParatextPlugin.CQRS.Features.BiblicalTerms;
using ClearDashboard.ParatextPlugin.CQRS.Features.Projects;
using ClearDashboard.ParatextPlugin.CQRS.Features.UsfmFilePath;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Messages;
using ClearDashboard.Wpf.Application.Models;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.Project;
using ClearDashboard.Wpf.Application.ViewModels.Project.AddParatextCorpusDialog;
using Markdig;
using MediatR;
using Microsoft.Extensions.Logging;
using Paratext.PluginInterfaces;
using SIL.Scripture;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;
using System.Xml.Linq;
using static ClearDashboard.Wpf.Application.Helpers.SlackMessage;
using Markdown = Markdig.Markdown;
using Path = System.IO.Path;

namespace ClearDashboard.Wpf.Application.ViewModels.PopUps
{
    public class SlackMessageViewModel : DashboardApplicationScreen
    {
        #region Member Variables   
        private readonly ILogger<SlackMessageViewModel> _logger;
        private readonly CollaborationServerHttpClientServices _collaborationHttpClientServices;
        public SelectedBookManager SelectedBookManager { get; private set; }


        private User _currentDashboardUser;
        private DashboardUser _dashboardUser;
        private string ZipPathAttachment { get; set; } = string.Empty;
        private List<JiraUser> _jiraUsersList = new();

        private JiraUser? _jiraUser;


        #endregion //Member Variables



        #region Public Properties

        public string ParatextUser { get; set; } = string.Empty;
        public User DashboardUser { get; set; }
        public CollaborationConfiguration GitLabUser { get; set; }
        public List<string> Files { get; set; }


        public ObservableCollection<UsfmError> UsfmErrors { get; set; } = new ObservableCollection<UsfmError>();


        #endregion //Public Properties


        #region Observable Properties

        private string _errorTitle;
        public string ErrorTitle
        {
            get => _errorTitle;
            set
            {
                _errorTitle = value;
                NotifyOfPropertyChange(() => ErrorTitle);
            }
        }


        private List<FileItem> _attachedFiles = new();
        public List<FileItem> AttachedFiles
        {
            get => _attachedFiles;
            set
            {
                _attachedFiles = value;
                NotifyOfPropertyChange(() => AttachedFiles);
            }
        }

        private List<ParatextProjectMetadata>? _projects;
        public List<ParatextProjectMetadata>? Projects
        {
            get => _projects;
            set => Set(ref _projects, value);
        }


        private ParatextProjectMetadata? _selectedProject;
        public ParatextProjectMetadata? SelectedProject
        {
            get => _selectedProject;
            set
            {
                Set(ref _selectedProject, value);
            }
        }


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

                if (_userMessage != string.Empty)
                {
                    ShowSlackSendButton = true;
                }

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


        private bool _showSlackSendButton = true;
        public bool ShowSlackSendButton
        {
            get => _showSlackSendButton;
            set
            {
                _showSlackSendButton = value;
                NotifyOfPropertyChange(() => ShowSlackSendButton);
            }
        }

        private bool _showEmailIcon;
        public bool ShowEmailIcon
        {
            get => _showEmailIcon;
            set
            {
                _showEmailIcon = value;
                NotifyOfPropertyChange(() => ShowEmailIcon);
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

        private bool _jiraButtonEnabled;
        public bool JiraButtonEnabled
        {
            get => _jiraButtonEnabled;
            set
            {
                _jiraButtonEnabled = value;
                NotifyOfPropertyChange(() => JiraButtonEnabled);
            }
        }

        private ObservableCollection<string> _severityItems = new();
        public ObservableCollection<string> SeverityItems
        {
            get { return _severityItems; }
            set
            {
                _severityItems = value;
                NotifyOfPropertyChange(() => SeverityItems);
            }
        }


        private Visibility _titleVisibility = Visibility.Visible;
        public Visibility TitleVisibility
        {
            get => _titleVisibility;
            set
            {
                _titleVisibility = value;
                NotifyOfPropertyChange(() => TitleVisibility);
            }
        }


        private Visibility _severityVisibility = Visibility.Visible;
        public Visibility SeverityVisibility
        {
            get => _severityVisibility;
            set
            {
                _severityVisibility = value;
                NotifyOfPropertyChange(() => SeverityVisibility);
            }
        }


        private Visibility _jiraDescriptionVisibility = Visibility.Visible;
        public Visibility JiraDescriptionVisibility
        {
            get => _jiraDescriptionVisibility;
            set
            {
                _jiraDescriptionVisibility = value;
                NotifyOfPropertyChange(() => JiraDescriptionVisibility);
            }
        }


        private Visibility _showSpinner = Visibility.Collapsed;
        public Visibility ShowSpinner
        {
            get => _showSpinner;
            set
            {
                _showSpinner = value;
                NotifyOfPropertyChange(() => ShowSpinner);
            }
        }

        private List<UsfmErrorsWrapper> _usfmErrorsByProject;
        public List<UsfmErrorsWrapper> UsfmErrorsByProject
        {
            get => _usfmErrorsByProject;
            set => Set(ref _usfmErrorsByProject, value);
        }

        #endregion //Observable Properties


        #region Constructor

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public SlackMessageViewModel()
        {
            // no-op
        }

        public SlackMessageViewModel(INavigationService navigationService, ILogger<SlackMessageViewModel> logger,
            DashboardProjectManager? projectManager, IEventAggregator eventAggregator, IMediator mediator,
            CollaborationServerHttpClientServices collaborationHttpClientServices,
            SelectedBookManager selectedBookManager,
            ILifetimeScope? lifetimeScope, ILocalizationService localizationService)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
        {
            _logger = logger;
            _collaborationHttpClientServices = collaborationHttpClientServices;
            SelectedBookManager = selectedBookManager;
            var localizationService1 = localizationService;

            WorkingMessage = "Gathering Files for Transmission";


            var combo1 = "[1] " + localizationService1["SlackMessageView_Combo1"];
            var combo2 = "[2] " + localizationService1["SlackMessageView_Combo2"];
            var combo3 = "[3] " + localizationService1["SlackMessageView_Combo3"];
            var combo4 = "[4] " + localizationService1["SlackMessageView_Combo4"];

            SeverityItems.Add(combo1);
            SeverityItems.Add(combo2);
            SeverityItems.Add(combo3);
            SeverityItems.Add(combo4);
        }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.


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

            ZipFiles();

            ShowOkButton = Visibility.Visible;

            WorkingMessage = "";

            var jiraClient = IoC.Get<JiraClientServices>();
            _jiraUsersList = await jiraClient.GetAllUsers();


            _currentDashboardUser = ProjectManager!.CurrentUser!;
            _dashboardUser = await _collaborationHttpClientServices.GetDashboardUserExistsById(_currentDashboardUser.Id);

            _jiraUser = await jiraClient.LoadJiraUser();


            base.OnViewLoaded(view);
        }

        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            SelectedBookManager.PropertyChanged += OnSelectedBookManagerPropertyChanged;

            var result = await ProjectManager.ExecuteRequest(new GetProjectMetadataQuery(), cancellationToken);
            if (result.Success)
            {
                var projectDesignSurface = LifetimeScope.Resolve<ProjectDesignSurfaceViewModel>();

                var corpusIds = await DAL.Alignment.Corpora.Corpus.GetAllCorpusIds(Mediator);

                List<string> currentNodeIds = new();

                foreach (var corpusId in corpusIds)
                {
                    if (corpusId.CorpusType != "Resource")
                    {
                        currentNodeIds.Add(corpusId.ParatextGuid);
                    }
                }

                var currentNodes = projectDesignSurface.DesignSurfaceViewModel.CorpusNodes;
                currentNodes.ToList().ForEach(n => currentNodeIds.Add(n.ParatextProjectId));

                if (Projects == null)
                {
                    Projects = result.Data.Where(c => currentNodeIds.Contains(c.Id)).OrderBy(p => p.Name).ToList();
                }
            }
        }

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            SelectedBookManager.PropertyChanged -= OnSelectedBookManagerPropertyChanged;
            return base.OnDeactivateAsync(close, cancellationToken);
        }

        #endregion //Constructor



        #region Methods

        private void ZipFiles()
        {
            var guid = Guid.NewGuid().ToString();
            ZipPathAttachment = Path.Combine(Path.GetTempPath(), $"{guid}.zip");
            // zip up everything
            if (Files.Count > 0)
            {
                if (File.Exists(ZipPathAttachment))
                {
                    File.Delete(ZipPathAttachment);
                }

                ZipFiles zipFiles = new(Files, ZipPathAttachment);
                var succcess = zipFiles.Zip();

                if (succcess == false)
                {
                    _logger.LogError("Error zipping files");
                }
            }
        }

        public void ResetSlackMessage()
        {
            TabControl_SelectionChanged();
            UserMessage = string.Empty;
            ShowSlackSendButton = false;
        }

        public void ResetJiraMessage()
        {
            TabControl_SelectionChanged();

            JiraTitle = string.Empty;
            JiraDescription = string.Empty;
            JiraSeverity = string.Empty;
        }


        public void TabControl_SelectionChanged()
        {
            WorkingMessage = "";
            ShowEmailIcon = false;
        }

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


            ShowSlackSendButton = false;
            WorkingMessage = "Sending Message...";
            await Task.Delay(200);


            // add in attached files
            if (AttachedFiles.Count > 0)
            {
                foreach (var file in AttachedFiles)
                {
                    Files.Add(file.FilePath);
                }

                ZipFiles();
            }

            
            var thisVersion = Assembly.GetEntryAssembly()!.GetName().Version;
            var versionNumber = $"{thisVersion!.Major}.{thisVersion.Minor}.{thisVersion.Build}.{thisVersion.Revision}";

            string msg = $"*Dashboard User:* {DashboardUser.FullName} \n*Paratext User:* {ParatextUser} \n*Github User:* {GitLabUser.RemoteUserName} \n*Version*: {versionNumber} \n*Message:* \n{UserMessage}";

            var logger = LifetimeScope!.Resolve<ILogger<SlackMessage>>();

            if (BugReport)
            {
                SlackMessage slackMessage = new SlackMessage(msg, this.ZipPathAttachment, logger, SlackMessageType.BugReport);
                var bSuccess = await slackMessage.SendFileToSlackAsync();

                if (bSuccess)
                {
                    ShowSlackSendButton = false;
                    SendErrorVisibility = Visibility.Collapsed;
                    WorkingMessage = "Message Sent Successfully";
                    ShowEmailIcon= true;
                }
                else
                {
                    ShowSlackSendButton = true;
                    SendErrorVisibility = Visibility.Visible;
                    WorkingMessage = "Message Sending Problem";
                    ShowEmailIcon = false;
                }

            }
        }


        private void AddAttachedFilesToZip()
        {
            
        }

        private void CheckJiraButtonEnabled()
        {
            if (string.IsNullOrEmpty(JiraTitle) == false)
            {
                TitleVisibility = Visibility.Hidden;
            }
            else
            {
                TitleVisibility = Visibility.Visible;
            }

            if (string.IsNullOrEmpty(JiraDescription) == false)
            {
                JiraDescriptionVisibility = Visibility.Hidden;
            }
            else
            {
                JiraDescriptionVisibility = Visibility.Visible;
            }


            if (string.IsNullOrEmpty(JiraSeverity) == false)
            {
                SeverityVisibility = Visibility.Hidden;
            }
            else
            {
                SeverityVisibility = Visibility.Visible;
            }


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

            JiraButtonEnabled = false;
            WorkingMessage = "Sending Message...";
            await Task.Delay(200);

            var jiraClient = IoC.Get<JiraClientServices>();
            //if (_jiraUser!.EmailAddress == string.Empty)
            //{
            //    // does the user have a jira account?
            //    _jiraUser = await jiraClient.GetUserByEmail(_jiraUsersList, _dashboardUser);
            //}

            _jiraUser = new JiraUser { AccountId = "5fff143cf7ea2a0107ff9f87", DisplayName = "dirk.kaiser@clear.bible", EmailAddress = "dirk.kaiser@clear.bible" };

            // pre-append the user name to the markdown text
            var markdown = $"REPORTED BY: **Dashboard User:** {_dashboardUser.FullName} \n**Paratext User:** {ParatextUser} \n**Email:** {_dashboardUser.Email} \n**Message:** \n{JiraDescription}";
            
            // convert the markdown to html
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            var html = Markdown.ToHtml(markdown, pipeline);

            // convert html to ADF format
            var adf = await Html2Adf.Convert(html);


            // get the ticket label enum
            var jiraLabel = JiraClientServices.JiraTicketLabel.WantToDo;
            var index = SeverityItems.IndexOf(JiraSeverity) + 1;

            switch (index)
            {
                case 1:
                    jiraLabel = JiraClientServices.JiraTicketLabel.LostData;
                    break;
                case 2:
                    jiraLabel = JiraClientServices.JiraTicketLabel.CannotCompleteTask;
                    break;
                case 3:
                    jiraLabel = JiraClientServices.JiraTicketLabel.DifficultToCompleteTask;
                    break;
                case 4:
                    jiraLabel = JiraClientServices.JiraTicketLabel.WantToDo;
                    break;
            }

            JiraTicketResponse? result = await jiraClient.CreateTaskTicket(JiraTitle, adf, _jiraUser, jiraLabel, _dashboardUser);

            // show the icons
            if (result != null)
            {
                SendErrorVisibility = Visibility.Collapsed;
                JiraButtonEnabled = false;
               

                // clear the fields
                JiraTitle = string.Empty;
                JiraDescription = string.Empty;
                JiraSeverity = string.Empty;
                ShowEmailIcon = true;

                WorkingMessage = "Message Sent Successfully";
            }
            else
            {
                SendErrorVisibility = Visibility.Visible;
                JiraButtonEnabled = true;
                WorkingMessage = "Problem Sending Message";
                ShowEmailIcon = false;  
            }
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


        #region SelectBibleBooks

        public void UnselectAllBooks()
        {
            SelectedBookManager.UnselectAllBooks();
        }

        // ReSharper disable once UnusedMember.Global
        public void SelectAllBooks()
        {
            SelectedBookManager.SelectAllBooks();
        }

        // ReSharper disable once UnusedMember.Global
        public void SelectNewTestamentBooks()
        {
            SelectedBookManager.SelectNewTestamentBooks();
        }

        // ReSharper disable once UnusedMember.Global
        public void SelectOldTestamentBooks()
        {
            SelectedBookManager.SelectOldTestamentBooks();
        }

        public async void ProjectSelected()
        {
            ShowSpinner = Visibility.Visible;
            UsfmErrorsByProject = await UsfmChecker.CheckUsfm(SelectedProject, ProjectManager, LocalizationService);
            var firstProject = UsfmErrorsByProject.FirstOrDefault();
            if (firstProject != null)
            {
                ErrorTitle = firstProject.ErrorTitle;
                UsfmErrors = firstProject.UsfmErrors;
            }
            var usfmErrors = new Dictionary<string, IEnumerable<UsfmError>>();
            usfmErrors.Add(SelectedProject.Id!, new List<UsfmError>());
            await SelectedBookManager.InitializeBooks(usfmErrors, true, true, new CancellationToken());

            ShowSpinner = Visibility.Collapsed;
        }

        private void OnSelectedBookManagerPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            var somethingSelected = false;
            var newBooksSelected = false;
            var oldBooksSelected = false;

            foreach (var book in SelectedBookManager.SelectedBooks)
            {
                if (book.IsEnabled && book.IsSelected && !somethingSelected)
                {
                    somethingSelected = true;
                }

                if (!book.IsImported && book.IsEnabled && book.IsSelected)
                {
                    newBooksSelected = true;
                }

                if (book.IsImported && book.IsEnabled && book.IsSelected)
                {
                    oldBooksSelected = true;
                }

                if (somethingSelected && newBooksSelected && oldBooksSelected)
                {
                    break;
                }
            }
        }


        public async void SendUsfmBooks()
        {
            if (SelectedBookManager.SelectedBooks.Count == 0)
            {
                return;
            }

            var result = await ExecuteRequest(new GetUsfmFilePathQuery(SelectedProject.Id), CancellationToken.None);
            if (result.Success)
            {
                var list = result.Data;
                var selectedBooks = SelectedBookManager.SelectedBooks.Where(b => b.IsSelected && b.IsEnabled).ToList();

                // get the list of books to send
                var booksToSend = new List<string>();
                foreach (var book in selectedBooks)
                {
                    var bookPath = list.FirstOrDefault(b => b.BookNameShort == book.BookName);

                    if (bookPath != null)
                    {
                        booksToSend.Add(bookPath.FilePath);
                    }
                }

                // add in the settings file
                if (booksToSend.Count > 0)
                {
                    var filePath = booksToSend[0];
                    FileInfo fileInfo = new(filePath);
                    var directory = fileInfo.DirectoryName;
                    var settingsFile = Path.Combine(directory!, "settings.xml");
                    if (File.Exists(settingsFile))
                    {
                        booksToSend.Add(settingsFile);
                    }
                }

                // zip up the files
                var guid = Guid.NewGuid().ToString();
                var zipPath = Path.Combine(Path.GetTempPath(), $"{guid}.zip");
                ZipFiles zipFiles = new(booksToSend, zipPath);
                var succcess = zipFiles.Zip();

                if (succcess == false)
                {
                    _logger.LogError("Error zipping files");
                }
                else
                {
                    // send the zip file to slack
                    SendUsfmFilesMessage(booksToSend);

                }
            }







        }

        private async Task SendUsfmFilesMessage(List<string> booksToSend)
        {
            var bRet = await NetworkHelper.IsConnectedToInternet();
            // check internet connection
            if (bRet == false)
            {
                _logger.LogWarning("No internet connection available");
                return;
            }


            ShowSlackSendButton = false;
            WorkingMessage = "Sending Message...";
            await Task.Delay(200);


            // add in attached files
            if (AttachedFiles.Count > 0)
            {
                foreach (var file in booksToSend)
                {
                    Files.Add(file);
                }

                ZipFiles();
            }


            var thisVersion = Assembly.GetEntryAssembly()!.GetName().Version;
            var versionNumber = $"{thisVersion!.Major}.{thisVersion.Minor}.{thisVersion.Build}.{thisVersion.Revision}";

            string msg =
                $"*Dashboard User:* {DashboardUser.FullName} \n*Paratext User:* {ParatextUser} \n*Github User:* {GitLabUser.RemoteUserName} \n*Version*: {versionNumber} \n*Message:* \nProject USFM Files for: {SelectedProject.Name}";

            var logger = LifetimeScope!.Resolve<ILogger<SlackMessage>>();


            SlackMessage slackMessage = new SlackMessage(msg, this.ZipPathAttachment, logger, SlackMessageType.BugReport);
            var bSuccess = await slackMessage.SendFileToSlackAsync();

            if (bSuccess)
            {
                ShowSlackSendButton = false;
                SendErrorVisibility = Visibility.Collapsed;
                WorkingMessage = "Message Sent Successfully";
                ShowEmailIcon = true;
            }
            else
            {
                ShowSlackSendButton = true;
                SendErrorVisibility = Visibility.Visible;
                WorkingMessage = "Message Sending Problem";
                ShowEmailIcon = false;
            }

        }

        #endregion

        #endregion // Methods


    }



    public class FileItem
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
    }
}
