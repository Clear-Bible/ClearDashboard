using AvalonDock.Properties;
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

namespace ClearDashboard.Wpf.ViewModels
{
    public class ShellViewModel : ApplicationScreen, IHandle<ParatextConnectedMessage>, IHandle<UserMessage>,
        IHandle<BackgroundTaskChangedMessage>
    {
        private readonly TranslationSource _translationSource;

        #region Properties
        private TimeSpan startTimeSpan = TimeSpan.Zero;
        private TimeSpan periodTimeSpan = TimeSpan.FromSeconds(30);

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

        private Visibility _showSpinner = Visibility.Visible;
        public Visibility ShowSpinner
        {
            get => _showSpinner;
            set
            {
                _showSpinner = value;
                NotifyOfPropertyChange(() => ShowSpinner);
            }
        }

        private Visibility _showTaskView = Visibility.Visible;
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

            BogusData();

            // setup timer to clean up old background tasks
            _timer = new((e) =>
            {
                if (_firstRun)
                {
                    CleanUpOldBackgroundTasks();
                }
                else
                {
                    _firstRun = true;
                }
            }, null, startTimeSpan, periodTimeSpan);
        }

        private void BogusData()
        {
            // TODO
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
            bool bFound = false;
            for (int i = _backgroundTaskStatuses.Count - 1; i >= 0; i--)
            {
                // if completed task remove it
                if (_backgroundTaskStatuses[i].TaskStatus == StatusEnum.Completed)
                {
                    OnUIThread(() =>
                    {
                        _backgroundTaskStatuses.RemoveAt(i);
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

            await Task.CompletedTask;
        }
        
        #endregion
    }
}
