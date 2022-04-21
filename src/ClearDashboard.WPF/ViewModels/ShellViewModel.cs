using AvalonDock.Properties;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.BackgroundServices;
using ClearDashboard.DataAccessLayer.Events;
using ClearDashboard.DataAccessLayer.NamedPipes;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.Helpers;
using ClearDashboard.Wpf.Models;
using ClearDashboard.Wpf.Views;
using Microsoft.Extensions.Logging;
using Pipes_Shared;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ClearDashboard.Wpf.ViewModels
{
    public class ShellViewModel : ApplicationScreen
    {
        private readonly TranslationSource _translationSource;
       
        #region Properties

        //Connection to the DAL
        private ClearEngineBackgroundService BackgroundService { get; set; }

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
            get { return _connected; }
            set
            {
                _connected = value;
                NotifyOfPropertyChange(() => Connected);
            }
        }


        #endregion

        #region ObservableProps

        private FlowDirection _flowDirection = FlowDirection.LeftToRight;
        public FlowDirection flowDirection
        {
            get => _flowDirection;
            set
            {
                _flowDirection = value;
                NotifyOfPropertyChange(() => flowDirection);
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
            ILogger<ShellViewModel> logger, ProjectManager projectManager, ClearEngineBackgroundService backgroundService) 
            : base(navigationService, logger, projectManager)
        {
            _translationSource = translationSource;
           
            BackgroundService = backgroundService;

            Logger.LogInformation("'ShellViewModel' ctor called.");

            //get the assembly version
            var thisVersion = Assembly.GetEntryAssembly().GetName().Version;
            Version = $"Version: {thisVersion.Major}.{thisVersion.Minor}.{thisVersion.Build}.{thisVersion.Revision}";
            ProjectManager.ParatextUserNameEventHandler += HandleSetParatextUserNameEvent;
            ProjectManager.NamedPipeChanged += HandleNamedPipeChanged;
        }

        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            ProjectManager.Initialize();
            await base.OnActivateAsync(cancellationToken);
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
            ProjectManager.NamedPipeChanged -= HandleNamedPipeChanged;

            ProjectManager.Dispose();
            return base.OnDeactivateAsync(close, cancellationToken);
        }

        #endregion

        #region Methods

        private void HandleNamedPipeChanged(object sender, PipeEventArgs args)
        {
            if (args == null) return;
            var pipeMessage = args.PipeMessage;
            switch (pipeMessage.Action)
            {
                case ActionType.OnConnected:
                    this.Connected = true;
                    break;
                case ActionType.OnDisconnected:
                    this.Connected = false;
                    break;
            }
            Logger.LogDebug(pipeMessage.Text);
            
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

            flowDirection = ProjectManager.CurrentLanguageFlowDirection;
        }

        #endregion

    }
}
