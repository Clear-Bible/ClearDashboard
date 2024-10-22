﻿using AvalonDock.Properties;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.Helpers;
using ClearDashboard.Wpf.Models;
using ClearDashboard.Wpf.Properties;
using ClearDashboard.Wpf.Views;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.Wpf.ViewModels.Popups;
using ClearDashboard.DataAccessLayer.Models.Common;
using System.Xml.Linq;
using ClearDashboard.DataAccessLayer.Models;
using BackgroundTaskStatus = ClearDashboard.DataAccessLayer.Models.BackgroundTaskStatus;

namespace ClearDashboard.Wpf.ViewModels
{
    public class ShellViewModel : ApplicationScreen, IHandle<ParatextConnectedMessage>, IHandle<UserMessage>,
        IHandle<BackgroundTaskChangedMessage>
    {
        private readonly TranslationSource _translationSource;

        #region Properties
        private readonly TimeSpan _startTimeSpan = TimeSpan.Zero;
        private readonly TimeSpan _periodTimeSpan = TimeSpan.FromSeconds(5);
        private readonly int _completedRemovalSeconds = 45;
        private bool _firstPass = false;

        private Timer _timer;
        private bool _firstRun;

        
        private string _paratextUserName;
        public string ParatextUserName
        {
            get => _paratextUserName;

            set
            {
                _paratextUserName = value;
                NotifyOfPropertyChange(() => ParatextUserName);
            }
        }


        private string _version;
        public string Version
        {
            get => _version;
            set
            {
                _version = value;
                NotifyOfPropertyChange(() => Version);
            }
        }

        private bool _connected;
        public bool Connected
        {
            get => _connected;
            set
            {
                _connected = value;
                NotifyOfPropertyChange(() => Connected);
            }
        }


        #endregion

        #region ObservableProps

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

        private Visibility _showTaskView = Visibility.Collapsed;
        public Visibility ShowTaskView
        {
            get => _showTaskView;
            set
            {
                _showTaskView = value;
                NotifyOfPropertyChange(() => ShowTaskView);
            }
        }

        private ObservableCollection<BackgroundTaskStatus> _backgroundTaskStatuses = new();
        public ObservableCollection<BackgroundTaskStatus> BackgroundTaskStatuses
        {
            get => _backgroundTaskStatuses;
            set
            {
                _backgroundTaskStatuses = value;
                NotifyOfPropertyChange(() => BackgroundTaskStatuses);
            }
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
                _translationSource.Language = language;

                Message = Resources.ResourceManager.GetString("language", Thread.CurrentThread.CurrentUICulture);

                NotifyOfPropertyChange(() => SelectedLanguage);

            }
        }

        

        
        private static void SaveUserLanguage(string language)
        {
            Settings.Default.language_code = language;
            Settings.Default.Save();
        }

        private string _message = Resources.ResourceManager.GetString("language", Thread.CurrentThread.CurrentUICulture);
        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                NotifyOfPropertyChange(() => Message);
            }
        }

        #endregion

        #region Commands

        private ICommand _colorStylesCommand;
        public ICommand ColorStylesCommand
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

        public ShellViewModel(TranslationSource translationSource, INavigationService navigationService, 
            ILogger<ShellViewModel> logger, DashboardProjectManager projectManager, IEventAggregator eventAggregator, IWindowManager windowManager) 
            : base(navigationService, logger, projectManager, eventAggregator)
        {
            _translationSource = translationSource;

            Logger.LogInformation("'ShellViewModel' ctor called.");

            //get the assembly version
            var thisVersion = Assembly.GetEntryAssembly().GetName().Version;
            Version = $"Version: {thisVersion.Major}.{thisVersion.Minor}.{thisVersion.Build}.{thisVersion.Revision}";
            
            // setup timer to clean up old background tasks
            _timer = new((_) =>
            {
                if (_firstRun)
                {
                    CleanUpOldBackgroundTasks();
                }
                else
                {
                    _firstRun = true;
                }
            }, null, _startTimeSpan, _periodTimeSpan);

            //BogusData();
        }

        private void BogusData()
        {
            // make some bogus task data
            BackgroundTaskStatuses.Add(new BackgroundTaskStatus
            {
                Name = "Background Task 1",
                Description = "Something longer that goes in here that is pretty darn long",
                StartTime = DateTime.Now,
                TaskStatus = StatusEnum.Working
            });
            BackgroundTaskStatuses.Add(new BackgroundTaskStatus
            {
                Name = "Background Task 2",
                Description = "Something longer that goes in here",
                StartTime = DateTime.Now,
                EndTime = DateTime.Now,
                TaskStatus = StatusEnum.Error
            });
            BackgroundTaskStatuses.Add(new BackgroundTaskStatus
            {
                Name = "Background Task 3",
                Description = "Something longer that goes in here",
                StartTime = DateTime.Now,
                EndTime = DateTime.Now,
                TaskStatus = StatusEnum.Completed
            });
            BackgroundTaskStatuses.Add(new BackgroundTaskStatus
            {
                Name = "Background Task 4",
                Description = "Something longer that goes in here which is also pretty darn long",
                StartTime = DateTime.Now,
                TaskStatus = StatusEnum.Working
            });
        }

        protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
#if RELEASE
            ProjectManager.CheckLicense(IoC.Get<RegistrationDialogViewModel>());
#endif
            return base.OnInitializeAsync(cancellationToken);
        }

        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        {        

            InitializeProjectManager();
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

            async void OnTickHandler(object sender, EventArgs args)
            {
                dispatcherTimer.Stop();
                await ProjectManager.Initialize();
                dispatcherTimer.Tick -= OnTickHandler;
            };

            dispatcherTimer.Tick += OnTickHandler;
            dispatcherTimer.Start();
        }


        protected override void OnViewLoaded(object view)
        {
            SetLanguage();
        }

#endregion

#region Caliburn.Micro overrides

       
        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            ProjectManager.Dispose();
            return base.OnDeactivateAsync(close, cancellationToken);
        }

        #endregion

        #region Methods

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
                if (_firstPass)
                {
                    ShowTaskView = Visibility.Collapsed;
                    _firstPass = false;
                }

                _firstPass = true;
                return;
            }


            bool bFound = false;
            DateTime presentTime = DateTime.Now;

            for (int i = _backgroundTaskStatuses.Count - 1; i >= 0; i--)
            {
                TimeSpan ts = presentTime - _backgroundTaskStatuses[i].EndTime;

                // if completed task remove it
                if (_backgroundTaskStatuses[i].TaskStatus == StatusEnum.Completed && ts.TotalSeconds > _completedRemovalSeconds)
                {
                    OnUIThread(() =>
                    {
                        if (i < _backgroundTaskStatuses.Count)
                        {
                            _backgroundTaskStatuses.RemoveAt(i);
                        }
                    });
                    
                    bFound = true;
                }
            }

            if (bFound)
            {
                NotifyOfPropertyChange(() => BackgroundTaskStatuses);
            }
        }


        /// <summary>
        /// Show the ColorStyles form
        /// </summary>
        public void ShowColorStyles()
        {
            var frm = new ColorStyles();
            frm.Show();
        }

        public void SetLanguage()
        {
            var culture = Settings.Default.language_code;
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

        public async void CancelTask(BackgroundTaskStatus task)
        {
            // update the task entry to show cancelling
            var taskToCancel = _backgroundTaskStatuses.FirstOrDefault(t => t.Name == task.Name);
            if (taskToCancel != null)
            {
                taskToCancel.TaskStatus = StatusEnum.CancelTaskRequested;
                taskToCancel.EndTime = DateTime.Now;
                NotifyOfPropertyChange(() => BackgroundTaskStatuses);
            }
            
            await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(taskToCancel));
        }

        public async void StartBackgroundTask()
        {
            await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
            {
                Name = "BOGUS TASK TO CANCEL",
                Description = "Try cancelling me",
                StartTime = DateTime.Now,
                TaskStatus = StatusEnum.Working
            }));
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
            ParatextUserName = message.user.FullName;
            await Task.CompletedTask;
        }

        public async Task HandleAsync(BackgroundTaskChangedMessage message, CancellationToken cancellationToken)
        {
            var incomingMessage = message.Status;
            
            // check for duplicate entries
            bool bFound = false;
            foreach (var status in BackgroundTaskStatuses)
            {
                if (status.Name == incomingMessage.Name)
                {
                    bFound = true;

                    status.Description = incomingMessage.Description;
                    if (incomingMessage.TaskStatus == StatusEnum.Error)
                    {
                        status.Description = incomingMessage.ErrorMessage;
                    }
                    status.TaskStatus = incomingMessage.TaskStatus;

                    NotifyOfPropertyChange(() => BackgroundTaskStatuses);
                    break;
                }
            }

            if (bFound == false)
            {
                BackgroundTaskStatuses.Add(incomingMessage);
            }


            // check to see if all are completed so we can turn off spinner
            var runningTasks = BackgroundTaskStatuses.Where(p => p.TaskStatus ==  StatusEnum.Working).ToList();
            if (runningTasks.Count > 0)
            {
                ShowSpinner = Visibility.Visible;
            }
            else
            {
                ShowSpinner = Visibility.Collapsed;
            }


            // check if the message is for the bogus task
            if (incomingMessage.Name == "BOGUS TASK TO CANCEL" && incomingMessage.TaskStatus == StatusEnum.CancelTaskRequested)
            {
                // return that your task was cancelled
                incomingMessage.EndTime = DateTime.Now;
                incomingMessage.TaskStatus = StatusEnum.Completed;
                incomingMessage.Description = "Task was cancelled";

                await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(incomingMessage));

            }

            await Task.CompletedTask;
        }
        
        #endregion
    }
}
