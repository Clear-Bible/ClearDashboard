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
        TimeSpan startTimeSpan = TimeSpan.Zero;
        TimeSpan periodTimeSpan = TimeSpan.FromMinutes(1);

        System.Threading.Timer timer;
        
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
            timer = new((e) =>
            {
                CleanUpOldBackgroundTasks();
            }, null, startTimeSpan, periodTimeSpan);
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
            Console.WriteLine();
        }

        /// <summary>
        /// Cleanup Background tasks that are completed and don't have errors
        /// </summary>
        private void CleanUpOldBackgroundTasks()
        {
            foreach (var backgroundTask in _backgroundTaskStatuses)
            {
                if (backgroundTask.Completed && backgroundTask.IsError == false)
                {
                    BackgroundTaskStatuses.Remove(backgroundTask);
                }
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
            var Status = message.Status;
            
            // todo check for duplicate entries
            BackgroundTaskStatuses.Add(Status);


            // check to see if all are completed so we can turn off spinner
            var runningTasks = BackgroundTaskStatuses.Where(p => p.Completed == false).ToList();
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
