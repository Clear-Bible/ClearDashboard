using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.Framework.Input;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Models.Common;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Messages;
using ClearDashboard.Wpf.Application.Models;
using ClearDashboard.Wpf.Application.Properties;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.Main;
using ClearDashboard.Wpf.Application.Views.Shell;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;
using ClearDashboard.Collaboration.Services;
using Resources = ClearDashboard.Wpf.Application.Strings.Resources;
using static ClearDashboard.Wpf.Application.Helpers.Telemetry;
using System.Runtime.InteropServices;

namespace ClearDashboard.Wpf.Application.ViewModels.Shell
{
    public class ShellViewModel : DashboardApplicationScreen, IShellViewModel,
        IHandle<ParatextConnectedMessage>,
        IHandle<UserMessage>,
        IHandle<GetApplicationWindowSettings>,
        IHandle<UiLanguageChangedMessage>,
        IHandle<PerformanceModeMessage>,
        IHandle<DashboardProjectNameMessage>,
        IHandle<ProjectChangedMessage>,
        IHandle<RefreshCheckGitLab>,
        IHandle<DashboardProjectPermissionLevelMessage>
    {

        //[DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        //public static extern int GetDeviceCaps(IntPtr hDC, int nIndex);

        //public enum DeviceCap
        //{
        //    VERTRES = 10,
        //    DESKTOPVERTRES = 117
        //}


        #region Properties

        private System.Timers.Timer _timer;
        private long _elapsedSeconds;
        nint _backgroundIndicatorValue = 0;
        private double _timerInterval = 15000;

        public BackgroundTasksViewModel BackgroundTasksViewModel { get; }

        private readonly TranslationSource? _translationSource;

        private UpdateFormat? _updateData;

        private readonly ILocalizationService _localizationServices;
        private readonly CollaborationManager _collaborationManager;

        private bool _verseChangeInProgress;

        private string? _paratextUserName;
        public string? ParatextUserName
        {
            get => _paratextUserName;
            set => Set(ref _paratextUserName, value);
        }

        private string? _dashboardProjectName;
        public string? DashboardProjectName
        {
            get => _dashboardProjectName;
            set => Set(ref _dashboardProjectName, value);
        }

        private string? _paratextProjectName;
        public string? ParatextProjectName
        {
            get => _paratextProjectName;
            set => Set(ref _paratextProjectName, value);
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

        private PermissionLevel _permissionLevel = PermissionLevel.None;
        public PermissionLevel PermissionLevel
        {
            get => _permissionLevel;
            set => Set(ref _permissionLevel, value);
        }


        private string _elapsedTime = "";
        public string ElapsedTime
        {
            get => _elapsedTime;
            set
            {
                _elapsedTime = value;
                NotifyOfPropertyChange(() => ElapsedTime);
            }
        }



        private Visibility _showHighPerformanceMode = Visibility.Collapsed;
        public Visibility ShowHighPerformanceMode
        {
            get => _showHighPerformanceMode;
            set
            {
                _showHighPerformanceMode = value;
                NotifyOfPropertyChange(() => ShowHighPerformanceMode);
            }
        }



        private WindowSettings _windowSettings;
        public WindowSettings WindowSettings
        {
            get => _windowSettings;
            set => Set(ref _windowSettings, value);
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
            set => Set(ref _showSpinner, value);
        }

        private Visibility _showTaskView = Visibility.Collapsed;
        public Visibility ShowTaskView
        {
            get => _showTaskView;
            set => Set(ref _showTaskView, value);
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


        //public bool GitLabUpdateNeeded
        //{
        //    get => ProjectManager != null && ProjectManager.CurrentDashboardProject != null && ProjectManager.CurrentDashboardProject.GitLabUpdateNeeded;
        //}


        private bool _gitLabUpdateNeeded;
        public bool GitLabUpdateNeeded
        {
            get => _gitLabUpdateNeeded;
            set => Set(ref _gitLabUpdateNeeded, value);
        }

        private bool _loadingApplication;


        public bool LoadingApplication
        {
            get => _loadingApplication;
            set
            {
                Set(ref _loadingApplication, value);
                //SetLanguage();
            }
        }

        private double _popupHorizontalOffset;
        public double PopupHorizontalOffset
        {
            get => _popupHorizontalOffset;
            set => Set(ref _popupHorizontalOffset, value);
        }

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
        
        public ICommand HelpCommand => new RelayCommand(Help);

        private void Help(object? commandParameter)
        {
            var mainViewModel = LifetimeScope?.Resolve<MainViewModel>();
            mainViewModel!.LaunchGettingStartedGuide();
        }
        
        public ICommand PreviousVerseCommand => new RelayCommand(PreviousVerse);

        private async void PreviousVerse(object? commandParameter)
        {
            await EventAggregator.PublishOnUIThreadAsync(new BcvArrowMessage(BcvArrow.PreviousVerse));
        }
        
        public ICommand NextVerseCommand => new RelayCommand(NextVerse);

        private async void NextVerse(object? commandParameter)
        {
            await EventAggregator.PublishOnUIThreadAsync(new BcvArrowMessage(BcvArrow.NextVerse));
        }
        
        public ICommand NextChapterCommand => new RelayCommand(NextChapter);

        private async void NextChapter(object? commandParameter)
        {
            await EventAggregator.PublishOnUIThreadAsync(new BcvArrowMessage(BcvArrow.NextChapter));
        }
        
        public ICommand NextBookCommand => new RelayCommand(NextBook);

        private async void NextBook(object? commandParameter)
        {
            await EventAggregator.PublishOnUIThreadAsync(new BcvArrowMessage(BcvArrow.NextBook));
        }
        
        public ICommand OpenBiblicalTermsCommand => new RelayCommand(OpenBiblicalTerms);

        private void OpenBiblicalTerms(object? commandParameter)
        {
            var mainViewModel = LifetimeScope?.Resolve<MainViewModel>();
            mainViewModel!.MenuItems[2].MenuItems[2].Command.Execute(null);
        }
        
        public ICommand OpenProjectPickerCommand => new RelayCommand(OpenProjectPicker);

        private void OpenProjectPicker(object commandParameter)
        {
            var mainViewModel = LifetimeScope?.Resolve<MainViewModel>();
            mainViewModel!.MenuItems[0].MenuItems[1].Command.Execute(null);
        }

        #endregion


        #region Startup


        /// <summary>
        /// Required for design-time support
        /// </summary>
        public ShellViewModel()
        {
            // no-op
        }

        public ShellViewModel(TranslationSource? translationSource, INavigationService navigationService,
            ILogger<ShellViewModel> logger, DashboardProjectManager? projectManager, IEventAggregator eventAggregator,
            IWindowManager windowManager, IMediator mediator, ILifetimeScope lifetimeScope, BackgroundTasksViewModel backgroundTasksViewModel, 
            ILocalizationService localizationService, CollaborationManager collaborationManager)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
        {
            BackgroundTasksViewModel = backgroundTasksViewModel;
            _translationSource = translationSource;
            _localizationServices = localizationService;
            _collaborationManager = collaborationManager;

            Logger.LogInformation("'ShellViewModel' ctor called.");

            //get the assembly version
            var thisVersion = Assembly.GetEntryAssembly().GetName().Version;
            var versionText = _localizationServices.Get("ProjectPicker_Version");
            Version = $"{versionText} {thisVersion.Major}.{thisVersion.Minor}.{thisVersion.Build}.{thisVersion.Revision}";


            LoadingApplication = true;
            NavigationService!.Navigated += NavigationServiceOnNavigated;
        }


        //protected override void OnUIThread(Action action)
        //{
        //    SetLanguage();
        //}




        #endregion

        #region Caliburn.Micro overrides

        protected override void OnViewReady(object view)
        {
            DeterminePopupHorizontalOffset((ShellView)view);
            SetLanguage();

            base.OnViewReady(view);
        }

        protected override void OnViewLoaded(object view)
        {
            OnTimedEvent(null, null);

            StartTimer();

            base.OnViewLoaded(view);
        }


        private void StartTimer()
        {
            _timer = new System.Timers.Timer(15000);
            _timer.Elapsed += OnTimedEvent;
            _timer.Enabled = true;
            _timer.AutoReset = true;
        }

        private async void OnTimedEvent(object? sender, ElapsedEventArgs e)
        {
            if (ProjectManager!.CurrentProject is null)
            {
                return;
            }

            var changeNeeded =  _collaborationManager.AreUnmergedChanges();

            ProjectManager.CurrentDashboardProject.GitLabUpdateNeeded = changeNeeded;
            GitLabUpdateNeeded = changeNeeded;
            if (changeNeeded)
            {
                await EventAggregator.PublishOnBackgroundThreadAsync(new RebuildMainMenuMessage());
            }
            
            if (ApplicationIsActivated())
            {
                Telemetry.IncrementMetric(TelemetryDictionaryKeys.ActiveAppMinutes, _timerInterval/60000);
            }
        }

        /// <summary>Returns true if the current application has focus, false otherwise</summary>
        public static bool ApplicationIsActivated()
        {
            var activatedHandle = GetForegroundWindow();
            if (activatedHandle == IntPtr.Zero)
            {
                return false;       // No window is currently activated
            }

            var procId = System.Diagnostics.Process.GetCurrentProcess().Id;
            int activeProcId;
            GetWindowThreadProcessId(activatedHandle, out activeProcId);

            return activeProcId == procId;
        }


        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);

        //private static double GetWindowsScreenScalingFactor(bool percentage = true)
        //{
        //    //Create Graphics object from the current windows handle
        //    Graphics GraphicsObject = Graphics.FromHwnd(IntPtr.Zero);
        //    //Get Handle to the device context associated with this Graphics object
        //    IntPtr DeviceContextHandle = GraphicsObject.GetHdc();
        //    //Call GetDeviceCaps with the Handle to retrieve the Screen Height
        //    int LogicalScreenHeight = GetDeviceCaps(DeviceContextHandle, (int)DeviceCap.VERTRES);
        //    int PhysicalScreenHeight = GetDeviceCaps(DeviceContextHandle, (int)DeviceCap.DESKTOPVERTRES);
        //    //Divide the Screen Heights to get the scaling factor and round it to two decimals
        //    double ScreenScalingFactor = Math.Round((double)PhysicalScreenHeight / (double)LogicalScreenHeight, 2);
        //    //If requested as percentage - convert it
        //    if (percentage)
        //    {
        //        ScreenScalingFactor *= 100.0;
        //    }
        //    //Release the Handle and Dispose of the GraphicsObject object
        //    GraphicsObject.ReleaseHdc(DeviceContextHandle);
        //    GraphicsObject.Dispose();
        //    //Return the Scaling Factor
        //    return ScreenScalingFactor;
        //}


        protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            SetLanguage();
            return base.OnInitializeAsync(cancellationToken);
        }

        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            InitializeProjectManager();
            await BackgroundTasksViewModel.ActivateAsync();
            await base.OnActivateAsync(cancellationToken);
        }

        protected override async Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            NavigationService!.Navigated -= NavigationServiceOnNavigated;
            Logger!.LogInformation($"{nameof(ShellViewModel)} is deactivating.");

            // HACK:  Force the MainViewModel singleton to properly deactivate
            var mainViewModel = IoC.Get<MainViewModel>();
            await mainViewModel!.DeactivateAsync(true);

            // deactivate the BackgroundTaskViewModel
            await BackgroundTasksViewModel.DeactivateAsync(true);

            await base.OnDeactivateAsync(close, cancellationToken);
        }

        #endregion

        #region Methods

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

        private void DeterminePopupHorizontalOffset(Visual view)
        {
            var source = PresentationSource.FromVisual(view);

            PopupHorizontalOffset = 5;

            //var scalingFactor = GetWindowsScreenScalingFactor(false);
            //PopupHorizontalOffset = scalingFactor * 5;

            //var verticalFactor = source.CompositionTarget.TransformToDevice.M11;
            //var horizontalFactor = source.CompositionTarget.TransformToDevice.M22;
            //PopupHorizontalOffset = horizontalFactor switch
            //{
            //    < 1.25 => 5,
            //    >= 1.25 => 270,
            //    _ => 5
            //};

            //Logger!.LogInformation($"VerticalFactor is {verticalFactor}");
            //Logger!.LogInformation($"HorizontalFactor is {horizontalFactor}");
            Logger!.LogInformation($"Setting PopupHorizontalOffset to {PopupHorizontalOffset}");
        }

        private void NavigationServiceOnNavigated(object sender, NavigationEventArgs e)
        {
            //SetLanguage();
            var uri = e.Uri;

            if (uri.OriginalString.Contains("MainView.xaml"))
            {
                LoadingApplication = false;
            }
        }


        /// <summary>
        /// Button click for the background tasks on the status bar
        /// </summary>
        public async void BackgroundTasks()
        {
            await EventAggregator!.PublishOnUIThreadAsync(new ToggleBackgroundTasksVisibilityMessage());
        }


        private bool SettingLanguage { get; set; }
        public void SetLanguage()
        {
            SettingLanguage = true;
            try
            {
                var culture = Settings.Default.language_code;
                if (string.IsNullOrEmpty(culture))
                {
                    var cultureName = "";
                    var currentCulture = Thread.CurrentThread.CurrentCulture;
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
                //SelectedLanguage = (LanguageTypeValue)Enum.Parse(typeof(LanguageTypeValue), culture.Replace("-", string.Empty));
                SelectedLanguage = (LanguageTypeValue)Enum.Parse(typeof(LanguageTypeValue), culture.Replace("-", string.Empty));

                var languageFlowDirection = SelectedLanguage.GetAttribute<RTLAttribute>();
                if (languageFlowDirection.isRTL)
                {
                    ProjectManager!.CurrentLanguageFlowDirection = FlowDirection.RightToLeft;
                }
                else
                {
                    ProjectManager!.CurrentLanguageFlowDirection = FlowDirection.LeftToRight;
                }

                WindowFlowDirection = ProjectManager.CurrentLanguageFlowDirection;
            }
            finally
            {
                SettingLanguage = false;
            }

        }


        private async Task SendUiLanguageChangeMessage(string language)
        {
            if (!SettingLanguage)
            {
                await EventAggregator.PublishOnUIThreadAsync(new UiLanguageChangedMessage(language));
            }
        }

        public void CopyText(BackgroundTaskStatus status)
        {
            Clipboard.SetText(status.Name + ": " + status.Description);
        }

        #endregion


        #region EventAggregator message handling
        public async Task HandleAsync(ParatextConnectedMessage message, CancellationToken cancellationToken)
        {
            if (!message.Connected && Connected) //play sound only when going from connected to not connected
            {
                PlaySound.PlaySoundFromResource(SoundType.Disconnected);
            }

            Connected = message.Connected;

            await Task.CompletedTask;
        }

        public async Task HandleAsync(UserMessage message, CancellationToken cancellationToken)
        {
            ParatextUserName = message.User.ParatextUserName;
            await Task.CompletedTask;
        }

        public async Task HandleAsync(GetApplicationWindowSettings message, CancellationToken cancellationToken)
        {
            await EventAggregator.PublishOnUIThreadAsync(new ApplicationWindowSettings(_windowSettings), cancellationToken);
        }


        public async Task HandleAsync(UiLanguageChangedMessage message, CancellationToken cancellationToken)
        {
            if (!SettingLanguage && SelectedLanguage.ToString()!=message.LanguageCode)
            {
                SetLanguage();
            }

            await Task.CompletedTask;
        }


        public Task HandleAsync(PerformanceModeMessage message, CancellationToken cancellationToken)
        {
            if (message.IsActive)
            {
                ShowHighPerformanceMode = Visibility.Visible;
            }
            else
            {
                ShowHighPerformanceMode = Visibility.Collapsed;
            }

            return Task.CompletedTask;
        }

        public async Task HandleAsync(ProjectChangedMessage message, CancellationToken cancellationToken)
        {
            ParatextProjectName = message.Project.ShortName;
            await Task.CompletedTask;
        }

        public async Task HandleAsync(DashboardProjectNameMessage message, CancellationToken cancellationToken)
        {
            DashboardProjectName = message.ProjectName;
            await Task.CompletedTask;
        }

        public Task HandleAsync(RefreshCheckGitLab message, CancellationToken cancellationToken)
        {
            OnTimedEvent(null, null);

            return Task.CompletedTask;
        }

        public Task HandleAsync(DashboardProjectPermissionLevelMessage message, CancellationToken cancellationToken)
        {
            ProjectManager.CurrentDashboardProject.PermissionLevel = message.PermissionLevel;
            
            PermissionLevel = message.PermissionLevel;
            return Task.CompletedTask;
        }

        #endregion
    }
}
