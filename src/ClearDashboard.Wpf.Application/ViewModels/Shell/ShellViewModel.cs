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
using System.IO;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;
using ClearDashboard.Wpf.Application.Views.Shell;

namespace ClearDashboard.Wpf.Application.ViewModels.Shell
{
    public class ShellViewModel : DashboardApplicationScreen, IShellViewModel,
        IHandle<ParatextConnectedMessage>,
        IHandle<UserMessage>,
        IHandle<GetApplicationWindowSettings>
    {

        #region Properties

        public BackgroundTasksViewModel BackgroundTasksViewModel { get; }

        private readonly TranslationSource? _translationSource;

        private UpdateFormat? _updateData;


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
        }

        public ShellViewModel(TranslationSource? translationSource, INavigationService navigationService,
            ILogger<ShellViewModel> logger, DashboardProjectManager? projectManager, IEventAggregator eventAggregator,
            IWindowManager windowManager, IMediator mediator, ILifetimeScope lifetimeScope, BackgroundTasksViewModel backgroundTasksViewModel )
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope)
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

        protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            return base.OnInitializeAsync(cancellationToken);
        }

        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            InitializeProjectManager();
            CheckForProgramUpdates();
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

        protected override void OnViewReady(object view)
        {
            DeterminePopupHorizontalOffset((ShellView)view);
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

            var horizontalFactor = source.CompositionTarget.TransformToDevice.M22;
            PopupHorizontalOffset = horizontalFactor switch
            {
                < 1.25 => 5,
                >= 1.25 => 270,
                _ => 5
            };
            Logger!.LogInformation($"Set PopupHorizontalOffset to {PopupHorizontalOffset}");
        }


        #endregion

        #region Caliburn.Micro overrides


        protected override async  Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            NavigationService!.Navigated -= NavigationServiceOnNavigated;
            Logger!.LogInformation($"{nameof(ShellViewModel)} is deactivating.");

            // HACK:  Force the MainViewModel singleton to properly deactivate
            var mainViewModel = IoC.Get<MainViewModel>();
            await mainViewModel!.DeactivateAsync(true);

            // deactivate the BackgroiundTaskViewModel
            await BackgroundTasksViewModel.DeactivateAsync(true);

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
            var isNewer = CheckWebVersion(_updateData.Version);

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

            switch (ver.Length)
            {
                case 4:
                    try
                    {
                        webVer = new Version(Convert.ToInt32(ver[0]), Convert.ToInt32(ver[1]), Convert.ToInt32(ver[2]), Convert.ToInt32(ver[3]));
                    }
                    catch (Exception)
                    {
                        return false;
                    }

                    break;
                case 3:
                    try
                    {
                        webVer = new Version(Convert.ToInt32(ver[0]), Convert.ToInt32(ver[1]), Convert.ToInt32(ver[2]), 0);
                    }
                    catch (Exception)
                    {
                        return false;
                    }

                    break;
                case 2:
                    try
                    {
                        webVer = new Version(Convert.ToInt32(ver[0]), Convert.ToInt32(ver[1]), 0, 0);
                    }
                    catch (Exception)
                    {
                        return false;
                    }

                    break;
                default:
                {
                    if (ver.Length == 2)
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

                    break;
                }
            }


            //get the assembly version
            var thisVersion = Assembly.GetEntryAssembly().GetName().Version;

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
                var psi = new ProcessStartInfo
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
        public async void BackgroundTasks()
        {
           await EventAggregator!.PublishOnUIThreadAsync(new ToggleBackgroundTasksVisibilityMessage());
        }

        public void SetLanguage()
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

        #endregion
    }
}
