﻿using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.ViewModels.Infrastructure;
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
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;
using ClearDashboard.DAL.ViewModels;
using ClearDashboard.DataAccessLayer.Models;
using Resources = ClearDashboard.Wpf.Application.Strings.Resources;

namespace ClearDashboard.Wpf.Application.ViewModels.Shell
{
    public class ShellViewModel : DashboardApplicationScreen, IShellViewModel,
        IHandle<ParatextConnectedMessage>,
        IHandle<UserMessage>,
        IHandle<GetApplicationWindowSettings>, IHandle<UiLanguageChangedMessage>
    {

        //[DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        //public static extern int GetDeviceCaps(IntPtr hDC, int nIndex);

        //public enum DeviceCap
        //{
        //    VERTRES = 10,
        //    DESKTOPVERTRES = 117
        //}


        #region Properties

        public BackgroundTasksViewModel BackgroundTasksViewModel { get; }

        private readonly TranslationSource? _translationSource;

        private UpdateFormat? _updateData;

        private bool _verseChangeInProgress;

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
            set => Set(ref _showSpinner,value);
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

        private void Help(object commandParameter)
        {
            var programFiles = Environment.ExpandEnvironmentVariables("%ProgramW6432%");
            var path = Path.Combine(programFiles, "Clear Dashboard", "Dashboard_Instructions.pdf");
            if (File.Exists(path))
            {
                var p = new Process();
                p.StartInfo = new ProcessStartInfo(path)
                {
                    UseShellExecute = true
                };
                p.Start();
            }
            else
            {
                Logger?.LogInformation("Dashboard_Instructions.pdf missing.");
            }
        }
        
        public ICommand PreviousVerseCommand => new RelayCommand(PreviousVerse);

        private async void PreviousVerse(object commandParameter)
        {
            await EventAggregator.PublishOnUIThreadAsync(new BcvArrowMessage(BcvArrow.PreviousVerse));
        }
        
        public ICommand NextVerseCommand => new RelayCommand(NextVerse);

        private async void NextVerse(object commandParameter)
        {
            await EventAggregator.PublishOnUIThreadAsync(new BcvArrowMessage(BcvArrow.NextVerse));
        }
        
        public ICommand NextChapterCommand => new RelayCommand(NextChapter);

        private async void NextChapter(object commandParameter)
        {
            await EventAggregator.PublishOnUIThreadAsync(new BcvArrowMessage(BcvArrow.NextChapter));
        }
        
        public ICommand NextBookCommand => new RelayCommand(NextBook);

        private async void NextBook(object commandParameter)
        {
            await EventAggregator.PublishOnUIThreadAsync(new BcvArrowMessage(BcvArrow.NextBook));
        }
        
        public ICommand OpenBiblicalTermsCommand => new RelayCommand(OpenBiblicalTerms);

        private void OpenBiblicalTerms(object commandParameter)
        {
            var mainViewModel = LifetimeScope.Resolve<MainViewModel>();
            mainViewModel.MenuItems[2].MenuItems[2].Command.Execute(null);
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
            IWindowManager windowManager, IMediator mediator, ILifetimeScope lifetimeScope, BackgroundTasksViewModel backgroundTasksViewModel, ILocalizationService localizationService)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope,localizationService)
        {
            BackgroundTasksViewModel = backgroundTasksViewModel;
            _translationSource = translationSource;

            Logger.LogInformation("'ShellViewModel' ctor called.");

            //get the assembly version
            var thisVersion = Assembly.GetEntryAssembly().GetName().Version;
            Version = $"Version: {thisVersion.Major}.{thisVersion.Minor}.{thisVersion.Build}.{thisVersion.Revision}";


            LoadingApplication = true;
            NavigationService!.Navigated += NavigationServiceOnNavigated;
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

        private void NavigationServiceOnNavigated(object sender, NavigationEventArgs e)
        {
            //SetLanguage();
            var uri = e.Uri;

            if (uri.OriginalString.Contains("MainView.xaml"))
            {
                LoadingApplication = false;
            }
        }

        protected override void OnViewReady(object view)
        {
            DeterminePopupHorizontalOffset((ShellView)view);
            SetLanguage();
            base.OnViewReady(view);
        }

        private double _popupHorizontalOffset;
        public double PopupHorizontalOffset
        {
            get => _popupHorizontalOffset;
            set => Set(ref _popupHorizontalOffset, value);
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
            await mainViewModel!.DeactivateAsync(true);

            // deactivate the BackgroundTaskViewModel
            await BackgroundTasksViewModel.DeactivateAsync(true);

            await base.OnDeactivateAsync(close, cancellationToken);
        }

        #endregion

        #region Methods

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

        public async Task HandleAsync(GetApplicationWindowSettings message, CancellationToken cancellationToken)
        {
            await EventAggregator.PublishOnUIThreadAsync(new ApplicationWindowSettings(_windowSettings), cancellationToken);
        }


        public async  Task HandleAsync(UiLanguageChangedMessage message, CancellationToken cancellationToken)
        {
            if (!SettingLanguage && SelectedLanguage.ToString()!=message.LanguageCode)
            {
                 SetLanguage();
            }
           
            await Task.CompletedTask;
        }

        #endregion
    }
}
