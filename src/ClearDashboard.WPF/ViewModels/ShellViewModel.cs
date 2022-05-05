using AvalonDock.Properties;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Events;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.Helpers;
using ClearDashboard.Wpf.Models;
using ClearDashboard.Wpf.Views;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace ClearDashboard.Wpf.ViewModels
{
    public class ShellViewModel : ApplicationScreen, IHandle<ParatextConnectedMessage>
    {
        private readonly TranslationSource _translationSource;
       
        #region Properties
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
            Properties.Settings.Default.language_code = language;
            Properties.Settings.Default.Save();
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

        #region events

        /// <summary>
        /// Capture the current Paratext username
        /// </summary>
        /// <param abbr="e"></param>
        private void HandleSetParatextUserNameEvent(object sender, EventArgs e)
        {
            var args = (CustomEvents.ParatextUsernameEventArgs)e;
            ParatextUserName = args.ParatextUserName;
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
            ILogger<ShellViewModel> logger, DashboardProjectManager projectManager, IEventAggregator eventAggregator) 
            : base(navigationService, logger, projectManager, eventAggregator)
        {
            _translationSource = translationSource;

            Logger.LogInformation("'ShellViewModel' ctor called.");

            //get the assembly version
            var thisVersion = Assembly.GetEntryAssembly().GetName().Version;
            Version = $"Version: {thisVersion.Major}.{thisVersion.Minor}.{thisVersion.Build}.{thisVersion.Revision}";
            ProjectManager.ParatextUserNameEventHandler += HandleSetParatextUserNameEvent;
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

        public override Task<bool> CanCloseAsync(CancellationToken cancellationToken = default)
        {
            //ProjectManager.ParatextUserNameEventHandler -= HandleSetParatextUserNameEvent;
            //ProjectManager.NamedPipeChanged -= HandleNamedPipeChanged;
            
            return base.CanCloseAsync(cancellationToken);
        }

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            ProjectManager.ParatextUserNameEventHandler -= HandleSetParatextUserNameEvent;
            ProjectManager.Dispose();
            return base.OnDeactivateAsync(close, cancellationToken);
        }

        #endregion

        #region Methods

        private void HandleNamedPipeChanged(object sender, EventArgs args)
        {
            //TODO:  Refactor to use EventAggregator

            //if (args == null) return;
            //var pipeMessage = args.PipeMessage;
            //switch (pipeMessage.Action)
            //{
            //    case ActionType.OnConnected:
            //        this.Connected = true;
            //        break;
            //    case ActionType.OnDisconnected:
            //        this.Connected = false;
            //        break;
            //}
            //Logger.LogDebug(pipeMessage.Text);

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
            var culture = Properties.Settings.Default.language_code;
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

            FlowDirection = ProjectManager.CurrentLanguageFlowDirection;
        }

        #endregion

        public async Task HandleAsync(ParatextConnectedMessage message, CancellationToken cancellationToken)
        {
            Connected = message.Connected;
            await Task.CompletedTask;
        }
    }
}
