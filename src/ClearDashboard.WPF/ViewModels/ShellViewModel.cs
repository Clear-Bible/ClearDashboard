using AvalonDock.Properties;
using Caliburn.Micro;
using ClearDashboard.Wpf.Helpers;
using ClearDashboard.Wpf.Models;
using ClearDashboard.Wpf.Views;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.DataAccessLayer.Events;
using ClearDashboard.DataAccessLayer.NamedPipes;
using Pipes_Shared;

namespace ClearDashboard.Wpf.ViewModels
{
    public class ShellViewModel : ApplicationScreen
    {
        private readonly TranslationSource _translationSource;

        #region Properties

        //Connection to the DAL
        private ProjectManager ProjectManager { get; set; }

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

        /// <summary>
        /// Overload for DI of the logger
        /// </summary>
        /// <param name="logger"></param>
        public ShellViewModel(TranslationSource translationSource, INavigationService navigationService, ILogger<ShellViewModel> logger, ProjectManager projectManager) : base(navigationService, logger)
        {
            _translationSource = translationSource;

            Logger.LogInformation("'ShellViewModel' ctor called.");

            //get the assembly version
            var thisVersion = Assembly.GetEntryAssembly().GetName().Version;
            Version = $"Version: {thisVersion.Major}.{thisVersion.Minor}.{thisVersion.Build}.{thisVersion.Revision}";


            // wire up the commands
            ColorStylesCommand = new RelayCommand(ShowColorStyles);

            // listen for username changes in Paratext
            ProjectManager = projectManager;
            ProjectManager.ParatextUserNameEventHandler += HandleSetParatextUserNameEvent;
            //_DAL = new StartUp();
            ProjectManager.NamedPipeChanged += HandleNamedPipeChanged;

            ProjectManager.GetParatextUserName();
        }

        protected override void Dispose(bool disposing)
        {
            ProjectManager.ParatextUserNameEventHandler -= HandleSetParatextUserNameEvent;
            ProjectManager.NamedPipeChanged -= HandleNamedPipeChanged;

            base.Dispose(disposing);
        }


        protected override void OnViewLoaded(object view)
        {
            SetLanguage();
        }

        #endregion

        #region Caliburn.Micro overrides

        public override Task<bool> CanCloseAsync(CancellationToken cancellationToken = default)
        {
            ProjectManager.OnClosing();

            return base.CanCloseAsync(cancellationToken);
        }

        #endregion

        #region Methods

        private void HandleNamedPipeChanged(object sender, PipeEventArgs args)
        {
            if (args == null) return;

            PipeMessage pipeMessage = args.PM;

            switch (pipeMessage.Action)
            {
                case ActionType.OnConnected:
                    this.Connected = true;
                    break;
                case ActionType.OnDisconnected:
                    this.Connected = false;
                    break;
            }

            Debug.WriteLine($"{pipeMessage.Text}");
        }
        
        /// <summary>
        /// Show the ColorStyles form
        /// </summary>
        /// <param abbr="obj"></param>
        private void ShowColorStyles(object obj)
        {
            ColorStyles frm = new ColorStyles();
            frm.Show();
        }

        public void SetLanguage()
        {
            var culture = Properties.Settings.Default.language_code;
            // strip out any "-" characters so the string can be propey parsed into the target enum
            SelectedLanguage = (LanguageTypeValue)Enum.Parse(typeof(LanguageTypeValue), culture.Replace("-", string.Empty));
        }

        #endregion


    }
}
