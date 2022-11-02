using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearDashboard.DataAccessLayer.Models.Common;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.DataAccessLayer.Wpf.Infrastructure;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Models;
using ClearDashboard.Wpf.Application.Properties;
using ClearDashboard.Wpf.Application.Strings;
using ClearDashboard.Wpf.Application.ViewModels.Main;
using ClearDashboard.Wpf.Application.ViewModels.PopUps;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace ClearDashboard.Wpf.Application.ViewModels.Shell
{
    public class ShellViewModel : DashboardApplicationScreen, IShellViewModel,
        IHandle<ParatextConnectedMessage>,
        IHandle<UserMessage>,
        IHandle<BackgroundTaskChangedMessage>,
        IHandle<GetApplicationWindowSettings>
    {
        private readonly TranslationSource? _translationSource;
        private readonly LongRunningTaskManager _longRunningTaskManager;


        #region Properties
        private readonly TimeSpan _startTimeSpan = TimeSpan.Zero;
        private readonly TimeSpan _periodTimeSpan = TimeSpan.FromSeconds(5);
        private readonly int _completedRemovalSeconds = 45;
        private bool _collapseTasksView;

        private UpdateFormat? _updateData;

        private Timer? _timer;
        private bool _cleanUpOldBackgroundTasks;


        private string? _paratextUserName;
        public string? ParatextUserName
        {
            get => _paratextUserName;
            set => Set(ref _paratextUserName, value);
        }


        private string? _version;
        public string? Version
        {
            get => _version;
            set => Set(ref _version, value);
        }

        private bool _connected;
        public bool Connected
        {
            get => _connected;
            set => Set(ref _connected, value);
        }


        #endregion

        #region ObservableProps

        private WindowSettings _windowSettings;
        public WindowSettings WindowSettings
        {
            get => _windowSettings;
            set
            {
                Set(ref _windowSettings, value);
                
            }
        }

        public async Task SetWindowsSettings(WindowSettings windowSettings)
        {
            WindowSettings = windowSettings;
            await EventAggregator.PublishOnUIThreadAsync(new ApplicationWindowSettings(_windowSettings));
        }

        private Visibility _showSpinner = Visibility.Collapsed;
        public Visibility ShowSpinner
        {
            get => _showSpinner;
            set => Set(ref _showSpinner,value);
        }

        private Visibility _showTaskView = Visibility.Collapsed;
        public Visibility ShowTaskView
        {
            get => _showTaskView;
            set => Set(ref _showTaskView, value);
        }

        private ObservableCollection<BackgroundTaskStatus> _backgroundTaskStatuses = new();
        public ObservableCollection<BackgroundTaskStatus> BackgroundTaskStatuses
        {
            get => _backgroundTaskStatuses;
            set => Set(ref _backgroundTaskStatuses, value);
        }


        private LanguageTypeValue _selectedLanguage;
        public LanguageTypeValue SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {

                _selectedLanguage = value;

                var language = EnumHelper.GetDescription(_selectedLanguage);
                SaveUserLanguage(_selectedLanguage.ToString());
                _translationSource!.Language = language;


                Message = Resources.ResourceManager.GetString("language", Thread.CurrentThread.CurrentUICulture);
                NotifyOfPropertyChange(() => SelectedLanguage);

                SendUiLanguageChangeMessage(language);
            }
        }

        private static void SaveUserLanguage(string language)
        {
            Settings.Default.language_code = language;
            Settings.Default.Save();
        }

        private string? _message = Resources.ResourceManager.GetString("language", Thread.CurrentThread.CurrentUICulture);
        public string? Message
        {
            get => _message;
            set => Set(ref _message, value);
        }

        private Visibility _showUpdateLink = Visibility.Collapsed;
        public Visibility ShowUpdateLink
        {
            get => _showUpdateLink;
            set => Set(ref _showUpdateLink, value);
        }


        private Uri _updateUrl = new Uri("https://www.clear.bible");
        public Uri UpdateUrl
        {
            get => _updateUrl;
            set => Set(ref _updateUrl , value);
        }

        public List<ReleaseNote> UpdateNotes { get; set; }

        #endregion

        #region Commands

        private ICommand? _colorStylesCommand;
        public ICommand? ColorStylesCommand
        {
            get => _colorStylesCommand;
            set
            {
                _colorStylesCommand = value;
            }
        }

        #endregion


        #region Startup


        /// <summary>
        /// Required for design-time support
        /// </summary>
        public ShellViewModel()
        {
            // no-op

            BogusData();
        }

        public ShellViewModel(TranslationSource? translationSource, INavigationService navigationService,
            ILogger<ShellViewModel> logger, DashboardProjectManager? projectManager, IEventAggregator eventAggregator,
            IWindowManager windowManager, IMediator mediator, ILifetimeScope lifetimeScope, LongRunningTaskManager longRunningTaskManager)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope)
        {
            _translationSource = translationSource;
            _longRunningTaskManager = longRunningTaskManager;

            Logger.LogInformation("'ShellViewModel' ctor called.");

            //get the assembly version
            var thisVersion = Assembly.GetEntryAssembly().GetName().Version;
            Version = $"Version: {thisVersion.Major}.{thisVersion.Minor}.{thisVersion.Build}.{thisVersion.Revision}";

            // setup timer to clean up old background tasks
            _timer = new Timer(TimerElapsed, null, _startTimeSpan, _periodTimeSpan);

            LoadingApplication = true;
            NavigationService!.Navigated += NavigationServiceOnNavigated;

            //BogusData();
        }

        private void TimerElapsed(object? state)
        {
            if (_cleanUpOldBackgroundTasks)
            {
                CleanUpOldBackgroundTasks();
            }
            else
            {
                _cleanUpOldBackgroundTasks = true;
            }
        }

        private bool _loadingApplication;
        public bool LoadingApplication
        {
            get => _loadingApplication;
            set
            {
                Set(ref _loadingApplication, value);
                SetLanguage();
            }
        }

        private void NavigationServiceOnNavigated(object sender, NavigationEventArgs e)
        {
            SetLanguage();
            var uri = e.Uri;

            if (uri.OriginalString.Contains("HomeView.xaml"))
            {
                LoadingApplication = false;
            }
        }

        private void BogusData()
        {
            // make some bogus task data
            BackgroundTaskStatuses.Add(new BackgroundTaskStatus
            {
                Name = "Background Task 1",
                Description = "Something longer that goes in here that is pretty darn long",
                StartTime = DateTime.Now,
                TaskLongRunningProcessStatus = LongRunningTaskStatus.Running
            });
            BackgroundTaskStatuses.Add(new BackgroundTaskStatus
            {
                Name = "Background Task 2",
                Description = "Something longer that goes in here",
                StartTime = DateTime.Now,
                EndTime = DateTime.Now,
                TaskLongRunningProcessStatus = LongRunningTaskStatus.Failed
            });
            BackgroundTaskStatuses.Add(new BackgroundTaskStatus
            {
                Name = "Background Task 3",
                Description = "Something longer that goes in here",
                StartTime = DateTime.Now,
                EndTime = DateTime.Now,
                TaskLongRunningProcessStatus = LongRunningTaskStatus.Completed
            });
            BackgroundTaskStatuses.Add(new BackgroundTaskStatus
            {
                Name = "Background Task 4",
                Description = "Something longer that goes in here which is also pretty darn long",
                StartTime = DateTime.Now,
                TaskLongRunningProcessStatus = LongRunningTaskStatus.Running
            });
        }

        protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            return base.OnInitializeAsync(cancellationToken);
        }

        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            InitializeProjectManager();
            CheckForProgramUpdates();
            await base.OnActivateAsync(cancellationToken);
        }

        private void InitializeProjectManager()
        {
            // delay long enough for the application to be rendered before 
            // asking the ProjectManager to initialize.  This is due to
            // the blocking nature of connecting to SignalR.
            var dispatcherTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };

            async void OnTickHandler(object? sender, EventArgs args)
            {
                dispatcherTimer.Stop();
                await ProjectManager!.Initialize();
                dispatcherTimer.Tick -= OnTickHandler;
            };

            dispatcherTimer.Tick += OnTickHandler;
            dispatcherTimer.Start();
        }

        //protected override void OnUIThread(Action action)
        //{
        //    SetLanguage();
        //}

        #endregion

        #region Caliburn.Micro overrides


        protected override async  Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            NavigationService!.Navigated -= NavigationServiceOnNavigated;
            Logger!.LogInformation($"{nameof(ShellViewModel)} is deactivating.");

            // HACK:  Force the MainViewModel singleton to properly deactivate
            var mainViewModel = IoC.Get<MainViewModel>();
            mainViewModel?.DeactivateAsync(true);

            _longRunningTaskManager.CancelAllTasks();

            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            _longRunningTaskManager.Dispose();

            ProjectManager?.Dispose();
            await base.OnDeactivateAsync(close, cancellationToken);
        }

        #endregion

        #region Methods

        private async void CheckForProgramUpdates()
        {
            //var updateJson = new UpdateFormat
            //{
            //    Version = "0.4.0.0",
            //    ReleaseDate = DateTime.Now.ToString(),
            //    DownloadLink = "",
            //};

            //var releaseNote = new ReleaseNote
            //{
            //    NoteType = ReleaseNote.ReleaseNoteType.Added,
            //    Note = "Alignments can now be added to the EnhancedView.  Pressing shift while hovering over tokens in either source or target will highlight the corresponding aligned token in the other related corpus. Alt hover will clear selection."
            //};
            //updateJson.ReleaseNotes.Add(releaseNote);
            //releaseNote = new ReleaseNote
            //{
            //    NoteType = ReleaseNote.ReleaseNoteType.Added,
            //    Note = "On adding in a new Paratext Corpus, you can now search for the corpus name in the dropdown box."
            //};
            //updateJson.ReleaseNotes.Add(releaseNote);

            //var options = new JsonSerializerOptions { WriteIndented = true };
            //string jsonString = JsonSerializer.Serialize(updateJson, options);
            //File.WriteAllText(@"d:\temp\Dashboard.json", jsonString);

            var connectedToInternet = await NetworkHelper.IsConnectedToInternet();           // check internet connection
            if (!connectedToInternet)
            {
                return;
            }


            Stream stream;
            try
            {
                var webClient = new WebClient();
                stream = await webClient.OpenReadTaskAsync(new Uri("https://raw.githubusercontent.com/Clear-Bible/CLEAR_External_Releases/main/ClearDashboard.json", UriKind.Absolute));
            }
            catch (Exception)
            {
                return;
            }

            _updateData = await JsonSerializer.DeserializeAsync<UpdateFormat>(stream);
            bool isNewer = CheckWebVersion(_updateData.Version);

            if (isNewer)
            {
                ShowUpdateLink = Visibility.Visible;
                UpdateUrl = new Uri(_updateData.DownloadLink);
                UpdateNotes = _updateData.ReleaseNotes;
            }
        }



        private bool CheckWebVersion(string webVersion)
        {
            //convert string to version
            var ver = webVersion.Split('.');
            Version webVer;

            if (ver.Length == 4)
            {
                try
                {
                    webVer = new Version(Convert.ToInt32(ver[0]), Convert.ToInt32(ver[1]), Convert.ToInt32(ver[2]), Convert.ToInt32(ver[3]));
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else if (ver.Length == 3)
            {
                try
                {
                    webVer = new Version(Convert.ToInt32(ver[0]), Convert.ToInt32(ver[1]), Convert.ToInt32(ver[2]), 0);
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else if (ver.Length == 2)
            {
                try
                {
                    webVer = new Version(Convert.ToInt32(ver[0]), Convert.ToInt32(ver[1]), 0, 0);
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else if (ver.Length == 2)
            {
                try
                {
                    webVer = new Version(Convert.ToInt32(ver[0]), 0, 0, 0);
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }


            //get the assembly version
            Version thisVersion = Assembly.GetEntryAssembly().GetName().Version;

            // compare
            var result = webVer.CompareTo(thisVersion);

            if (result == 1)
            {
                //newer release present on the web
                return true;
            }
            return false;
        }

        public void ClickUpdateLink()
        {
            if (UpdateUrl.AbsoluteUri == "")
            {
                return;
            }

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = UpdateUrl.AbsoluteUri,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
            }
        }

        public void ShowNotes()
        {
            var localizedString = LocalizationStrings.Get("ShellView_ShowNotes", Logger);

            dynamic settings = new ExpandoObject();
            settings.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            settings.ResizeMode = ResizeMode.CanResize;
            settings.MinWidth = 600;
            settings.MinHeight = 600;
            settings.Title = $"{localizedString} - {_updateData?.Version}";

            var viewModel = IoC.Get<ShowUpdateNotesViewModel>();
            viewModel.ReleaseNotes = new ObservableCollection<ReleaseNote>(UpdateNotes);

            IWindowManager manager = new WindowManager();
            manager.ShowWindowAsync(viewModel, null, settings);
        }


        /// <summary>
        /// Button click for the background tasks on the status bar
        /// </summary>
        public void BackgroundTasks()
        {
            if (ShowTaskView == Visibility.Collapsed)
            {
                ShowTaskView = Visibility.Visible;
            }
            else
            {
                ShowTaskView = Visibility.Collapsed;
            }

        }

        public void CloseTaskBox()
        {
            ShowTaskView = Visibility.Collapsed;
        }

        /// <summary>
        /// Cleanup Background tasks that are completed and don't have errors
        /// </summary>
        private void CleanUpOldBackgroundTasks()
        {
            // auto close task view if nothing is in the queue
            if (_backgroundTaskStatuses.Count == 0)
            {
                if (_collapseTasksView)
                {
                    ShowTaskView = Visibility.Collapsed;
                    _collapseTasksView = false;
                }

                _collapseTasksView = true;
                return;
            }

            var taskRemoved = false;
            var presentTime = DateTime.Now;

            for (var index = _backgroundTaskStatuses.Count - 1; index >= 0; index--)
            {
                var timeSpan = presentTime - _backgroundTaskStatuses[index].EndTime;

                // if completed task remove it
                if (_backgroundTaskStatuses[index].TaskLongRunningProcessStatus == LongRunningTaskStatus.Completed && timeSpan.TotalSeconds > _completedRemovalSeconds)
                {
                    var index1 = index;
                    OnUIThread(() =>
                    {
                        if (index1 < _backgroundTaskStatuses.Count)
                        {
                            _backgroundTaskStatuses.RemoveAt(index1);
                        }
                    });

                    taskRemoved = true;
                }
            }

            if (taskRemoved)
            {
                NotifyOfPropertyChange(() => BackgroundTaskStatuses);
            }
        }



        public void SetLanguage()
        {
            var culture = Settings.Default.language_code;
            if (string.IsNullOrEmpty(culture))
            {
                var cultureName = "";
                CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
                if (currentCulture.Parent.Name is not "zh" or "pt")
                {
                    cultureName = currentCulture.Name;//.Parent
                }
                else
                {
                    cultureName = currentCulture.Name;
                }

                try
                {
                    culture = ((LanguageTypeValue)Enum.Parse(typeof(LanguageTypeValue), cultureName.Replace("-", string.Empty))).ToString();
                }
                catch
                {
                    culture = "en";
                }
            }
            // strip out any "-" characters so the string can be properly parsed into the target enum
            SelectedLanguage = (LanguageTypeValue)Enum.Parse(typeof(LanguageTypeValue), culture.Replace("-", string.Empty));

            var languageFlowDirection = SelectedLanguage.GetAttribute<RTLAttribute>();
            if (languageFlowDirection.isRTL)
            {
                ProjectManager.CurrentLanguageFlowDirection = FlowDirection.RightToLeft;
            }
            else
            {
                ProjectManager.CurrentLanguageFlowDirection = FlowDirection.LeftToRight;
            }

            WindowFlowDirection = ProjectManager.CurrentLanguageFlowDirection;
        }

        public async void CancelTask(BackgroundTaskStatus status)
        {
            // update the task entry to show cancelling
            var backgroundTaskStatus = _backgroundTaskStatuses.FirstOrDefault(t => t.Name == status.Name);
            if (backgroundTaskStatus != null)
            {
                //taskToCancel.TaskLongRunningProcessStatus = LongRunningTaskStatus.CancellationRequested;
                //taskToCancel.EndTime = DateTime.Now;

                var task = _longRunningTaskManager.CancelTask(status.Name);

                backgroundTaskStatus.EndTime = DateTime.Now;
                backgroundTaskStatus.TaskLongRunningProcessStatus = LongRunningTaskStatus.Completed;

                //TODO:  Localize
                backgroundTaskStatus.Description = "Task was cancelled";
                NotifyOfPropertyChange(() => BackgroundTaskStatuses);

                ToggleSpinner();
            }

            //await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(taskToCancel));
        }

        public async void StartBackgroundTask()
        {
            await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
            {
                Name = "BOGUS TASK TO CANCEL",
                Description = "Try cancelling me",
                StartTime = DateTime.Now,
                TaskLongRunningProcessStatus = LongRunningTaskStatus.Running
            }));
        }

        private async Task SendUiLanguageChangeMessage(string language)
        {
            await EventAggregator.PublishOnUIThreadAsync(new UiLanguageChangedMessage(language)).ConfigureAwait(false);
        }

        public void CopyText(BackgroundTaskStatus status)
        {
            Clipboard.SetText(status.Name + ": " + status.Description);
        }

        #endregion


        #region EventAggregator message handling
        public async Task HandleAsync(ParatextConnectedMessage message, CancellationToken cancellationToken)
        {
            Connected = message.Connected;
            await Task.CompletedTask;
        }

        public async Task HandleAsync(UserMessage message, CancellationToken cancellationToken)
        {
            ParatextUserName = message.User.ParatextUserName;
            await Task.CompletedTask;
        }

        public async Task HandleAsync(BackgroundTaskChangedMessage message, CancellationToken cancellationToken)
        {
            var incomingMessage = message.Status;

            // check for duplicate entries
            var taskExists = false;
            foreach (var status in BackgroundTaskStatuses)
            {
                if (status.Name == incomingMessage.Name)
                {
                    taskExists = true;

                    status.Description = incomingMessage.Description;
                    if (incomingMessage.TaskLongRunningProcessStatus == LongRunningTaskStatus.Failed)
                    {
                        status.Description = incomingMessage.ErrorMessage;
                    }
                    status.TaskLongRunningProcessStatus = incomingMessage.TaskLongRunningProcessStatus;

                    NotifyOfPropertyChange(() => BackgroundTaskStatuses);
                    break;
                }
            }

            if (taskExists == false)
            {
                BackgroundTaskStatuses.Add(incomingMessage);
            }


            ToggleSpinner();


            // check if the message is for the bogus task
            //if (incomingMessage.Name == "BOGUS TASK TO CANCEL" && incomingMessage.TaskLongRunningProcessStatus == LongRunningTaskStatus.CancellationRequested)
            //{
            //    // return that your task was cancelled
            //    incomingMessage.EndTime = DateTime.Now;
            //    incomingMessage.TaskLongRunningProcessStatus = LongRunningTaskStatus.Completed;
            //    incomingMessage.Description = "Task was cancelled";

            //    await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(incomingMessage), cancellationToken);

            //}

            await Task.CompletedTask;
        }

        private void ToggleSpinner()
        {
            // check to see if all are completed so we can turn off spinner
            var runningTasks = BackgroundTaskStatuses
                .Where(p => p.TaskLongRunningProcessStatus == LongRunningTaskStatus.Running).ToList();
            if (runningTasks.Count > 0)
            {
                ShowSpinner = Visibility.Visible;
            }
            else
            {
                ShowSpinner = Visibility.Collapsed;
            }
        }

        #endregion

        public async Task HandleAsync(GetApplicationWindowSettings message, CancellationToken cancellationToken)
        {
            await EventAggregator.PublishOnUIThreadAsync(new ApplicationWindowSettings(_windowSettings), cancellationToken);
        }
    }
}
