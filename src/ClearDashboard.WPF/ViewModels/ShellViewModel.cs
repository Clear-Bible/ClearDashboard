using MvvmHelpers;
using Newtonsoft.Json;
using System;
using System.Reflection;
using System.Threading;
using System.Windows.Input;
using AvalonDock.Properties;
using Caliburn.Micro;
using ClearDashboard.DAL.Events;
using ClearDashboard.Wpf.Helpers;
using ClearDashboard.Wpf.Models;
using ClearDashboard.Wpf.Views;


namespace ClearDashboard.Wpf.ViewModels
{
    public class ShellViewModel: Screen
    {
        #region Props

        private readonly ILog _logger;


        //Connection to the DAL
        DAL.StartUp _startup;

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

        #endregion

        #region ObservableProps

        private LanguageTypeValue _selectedLanguage;
        public LanguageTypeValue SelectedLanguage
        {
            get { return _selectedLanguage; }
            set
            {
                _selectedLanguage = value;
                NotifyOfPropertyChange(() => SelectedLanguage);

                TranslationSource.Instance.Language = EnumHelper.GetDescription(_selectedLanguage);
                Message = Resources.ResourceManager.GetString("language", Thread.CurrentThread.CurrentUICulture);
            }
        }

        private string _message = Resources.ResourceManager.GetString("language", Thread.CurrentThread.CurrentUICulture);
        public string Message
        {
            get { return _message; }
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

        public ShellViewModel()
        {
            // set the combobox based on the current UI Thread Culture
            var culture = Thread.CurrentThread.CurrentUICulture.Name;
            switch (culture)
            {
                case "am":
                    SelectedLanguage = LanguageTypeValue.am;
                    break;
                case "de":
                    SelectedLanguage = LanguageTypeValue.de;
                    break;
                case "en-US":
                    SelectedLanguage = LanguageTypeValue.enUS;
                    break;
                case "es":
                    SelectedLanguage = LanguageTypeValue.es;
                    break;
                case "fr":
                    SelectedLanguage = LanguageTypeValue.fr;
                    break;
                case "hi":
                    SelectedLanguage = LanguageTypeValue.hi;
                    break;
                case "id":
                    SelectedLanguage = LanguageTypeValue.id;
                    break;
                case "km":
                    SelectedLanguage = LanguageTypeValue.km;
                    break;
                case "pt":
                    SelectedLanguage = LanguageTypeValue.pt;
                    break;
                case "pt-BR":
                    SelectedLanguage = LanguageTypeValue.ptBR;
                    break;
                case "ro":
                    SelectedLanguage = LanguageTypeValue.ro;
                    break;
                case "ru-RU":
                    SelectedLanguage = LanguageTypeValue.ruRU;
                    break;
                case "vi":
                    SelectedLanguage = LanguageTypeValue.vi;
                    break;
                case "zh-CN":
                    SelectedLanguage = LanguageTypeValue.zhCN;
                    break;
                case "zh-TW":
                    SelectedLanguage = LanguageTypeValue.zhTW;
                    break;
            }
        }



        /// <summary>
        /// Overload for DI of the logger
        /// </summary>
        /// <param name="logger"></param>
        public ShellViewModel(ILog logger)
        {
            _logger = logger;

            _logger.Info("In ShellViewModel ctor");

            //get the assembly version
            Version thisVersion = Assembly.GetEntryAssembly().GetName().Version;
            Version = $"Version: {thisVersion.Major}.{thisVersion.Minor}.{thisVersion.Build}.{thisVersion.Revision}";


            // wire up the commands
            ColorStylesCommand = new RelayCommand(ShowColorStyles);

            // listen for username changes in Paratext
            DAL.StartUp.ParatextUserNameEventHandler += HandleSetParatextUserNameEvent;
            _startup = new DAL.StartUp();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Show the ColorStyles form
        /// </summary>
        /// <param abbr="obj"></param>
        private void ShowColorStyles(object obj)
        {
            ColorStyles frm = new ColorStyles();
            frm.Show();
        }

        #endregion
    }
}
