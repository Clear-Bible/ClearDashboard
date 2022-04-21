using System;
using Caliburn.Micro;
using ClearDashboard.Common.Models;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.DataAccessLayer.NamedPipes;
using ClearDashboard.Wpf.Helpers;
using ClearDashboard.Wpf.ViewModels.Panes;
using Microsoft.Extensions.Logging;
using Pipes_Shared;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using ClearDashboard.DataAccessLayer.Slices.ManuscriptVerses;
using ClearDashboard.DataAccessLayer.Slices.MarbleDataRequests;
using ClearDashboard.DataAccessLayer.Wpf;
using Action = System.Action;

namespace ClearDashboard.Wpf.ViewModels
{
    public class WordMeaningsViewModel : ToolViewModel
    {

        #region Member Variables

        //private readonly ILogger _logger;
        //private readonly ProjectManager _projectManager;
        private readonly TranslationSource _translationSource;

        private string _currentVerse = "";

        List<MARBLEresource> _whatIsThisWord = new List<MARBLEresource>();

        #endregion //Member Variables

        #region Public Properties

        private Visibility _buttonVisibility = Visibility.Hidden;
        public Visibility ButtonVisibility
        {
            get => _buttonVisibility;
            set
            {
                _buttonVisibility = value;
                NotifyOfPropertyChange(() => ButtonVisibility);
            }
        }


        #endregion //Public Properties

        #region Observable Properties

       
        private bool _isOT;
        public bool IsOT
        {
            get => _isOT;
            set
            {
                _isOT = value;
                NotifyOfPropertyChange(() => IsOT);
            }
        }

        private BookChapterVerse _currentBcv = new();
        public BookChapterVerse CurrentBcv
        {
            get => _currentBcv;
            set
            {
                _currentBcv = value;
                NotifyOfPropertyChange(() => CurrentBcv);

                if (_currentBcv.BookNum < 40)
                {
                    IsOT = true;
                }
                else
                {
                    IsOT = false;
                }
            }
        }

        private ObservableCollection<MARBLEresource> _wordData = new ObservableCollection<MARBLEresource>();
        public ObservableCollection<MARBLEresource> WordData
        {
            get => _wordData;
            set
            {
                _wordData = value;
                NotifyOfPropertyChange(() => WordData);
            }
        }

        ObservableCollection<Inline> _targetInlinesText = new ObservableCollection<Inline>();
        public ObservableCollection<Inline> TargetInlinesText
        {
            get
            {
                return _targetInlinesText;
            }
            set
            {
                _targetInlinesText = value;
                NotifyOfPropertyChange(() => TargetInlinesText);
            }
        }

        private string _targetHTML;
        public string TargetHTML
        {
            get => _targetHTML;
            set
            {
                _targetHTML = value;
                NotifyOfPropertyChange(() => TargetHTML);
            }
        }

        ObservableCollection<Inline> _sourceInlinesText = new ObservableCollection<Inline>();
        public ObservableCollection<Inline> SourceInlinesText
        {
            get
            {
                return _sourceInlinesText;
            }
            set
            {
                _sourceInlinesText = value;
                NotifyOfPropertyChange(() => SourceInlinesText);
            }
        }

        private string _sourceVerses = "";
        public string SourceVerses
        {
            get => _sourceVerses;
            set
            {
                _sourceVerses = value;
                NotifyOfPropertyChange(() => SourceVerses);
            }
        }


        #endregion //Observable Properties

        #region Commands

        public ICommand LaunchLogosCommand { get; set; }
        public ICommand LaunchSensesCommand { get; set; }
        public ICommand GoBackCommand { get; set; }

        #endregion

        #region Constructor
        public WordMeaningsViewModel()
        {

        }

        public WordMeaningsViewModel(INavigationService navigationService, 
            ILogger<WordMeaningsViewModel> logger, ProjectManager projectManager, TranslationSource translationSource)
            : base(navigationService, logger, projectManager)
        {
            this.Title = "⌺ WORD MEANINGS";
            this.ContentId = "WORDMEANINGS";
            this.DockSide = EDockSide.Left;

            _translationSource = translationSource;

            CurrentBcv.SetVerseFromId(ProjectManager.CurrentVerse);

            // listen to the DAL event messages coming in
            ProjectManager.NamedPipeChanged += HandleEventAsync;

            // wire up the commands
            LaunchLogosCommand = new RelayCommand(ShowLogos);
            LaunchSensesCommand = new RelayCommand(ShowSenses);
            GoBackCommand = new RelayCommand(RefreshWords);
        }

        #endregion //Constructor

        #region Methods

        protected override void Dispose(bool disposing)
        {
            // unsubscribe to the pipes listener
            ProjectManager.NamedPipeChanged -= HandleEventAsync;

            base.Dispose(disposing);    
        }

        private void ShowSenses(object obj)
        {
            if (obj is null)
            {
                return;
            }

            MARBLEresource mr = (MARBLEresource)obj;
            if (mr.TotalSenses == 1)
            {
                return;
            }

            if (this.ButtonVisibility == Visibility.Visible)
            {
                // we are in the details already...jump back up a level
                this.RefreshWords(null);
                return;
            }


            int ID = mr.ID;

            var items = _whatIsThisWord.Where(p => p.ID == ID);

            WordData.Clear();
            foreach (var item in items)
            {
                WordData.Add(item);
            }

            ButtonVisibility = Visibility.Visible;
        }

        private void ShowLogos(object obj)
        {
            if (obj is null)
            {
                return;
            }

            MARBLEresource mr = (MARBLEresource)obj;

            // OT or NT?
            if (CurrentBcv.BookNum < 40)
            {
                // Hebrew link
                string hebrewPrefix = "logos4:Guide;t=My_Bible_Word_Study;lemma=lbs$2Fhe$2F";
                LaunchWebPage.TryOpenUrl(hebrewPrefix + mr.LogosRef);
            }
            else
            {
                // Greek link
                string greekPrefix = "logos4:Guide;t=My_Bible_Word_Study;lemma=lbs$2Fel$2F";
                LaunchWebPage.TryOpenUrl(greekPrefix + mr.LogosRef);
            }

        }


        /// <summary>
        /// Listen for changes in the DAL regarding any messages coming in
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public async void HandleEventAsync(object sender, PipeEventArgs args)
        {
            if (args == null) return;

            PipeMessage pipeMessage = args.PipeMessage;

            switch (pipeMessage.Action)
            {
                case ActionType.CurrentVerse:
                    if (_currentVerse != pipeMessage.Text)
                    {
                        _currentVerse = pipeMessage.Text;
                        CurrentBcv.SetVerseFromId(_currentVerse);
                        if (_currentVerse.EndsWith("000"))
                        {
                            // a zero based verse
                            TargetInlinesText.Clear();
                            NotifyOfPropertyChange(() => TargetInlinesText);
                            TargetHTML = "";
                            WordData.Clear();
                            NotifyOfPropertyChange(() => WordData);
                        }
                        else
                        {
                            // a normal verse
                            Verse  verse = new Verse();
                            verse.VerseBBCCCVVV = _currentVerse;

                            if (verse.BookNum < 40)
                            {
                                _isOT = true;
                            }
                            else
                            {
                                _isOT = false;
                            }

                            _ = ReloadWordMeanings();
                        }
                    }

                    break;
            }
        }

        /// <summary>
        /// Get the Biblical Words from 
        /// </summary>
        /// <returns></returns>
        private async Task ReloadWordMeanings()
        {
            // SDBH & SDBG support the following language codes:
            // en, fr, sp, pt, sw, zht, zhs

            string languageCode = "";

            switch (_translationSource.Language)
            {
                case "es":
                    languageCode = "sp";
                    break;
                case "fr":
                    languageCode = "fr";
                    break;
                case "zh-CN":
                    languageCode = "zhs";
                    break;
                case "zh-TW":
                    languageCode = "zht";
                    break;
                default:
                    // default to English for everyone else
                    languageCode = "en";
                    break;
            }

            var queryResult = await ExecuteCommand(new GetWhatIsThisWord.GetWhatIsThisWordByBcvQuery(CurrentBcv, languageCode), CancellationToken.None).ConfigureAwait(false);
            if (queryResult.Success == false)
            {
                Logger.LogError(queryResult.Message);
                return;
            }


            if (queryResult.Data == null)
            {
                WordData.Clear();
                return;
            }

            _whatIsThisWord = queryResult.Data;

            //GetWhatIsThisWord sd = new GetWhatIsThisWord();
            //_whatIsThisWord = sd.GetSemanticDomainData(CurrentBcv, languageCode);

            // invoke to get it to run in STA mode
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                _wordData.Clear();
                foreach (var marbleResource in _whatIsThisWord)
                {
                    if (marbleResource.IsSense)
                    {
                        _wordData.Add(marbleResource);
                    }
                }
            });

            NotifyOfPropertyChange(() => WordData);
        }

        private void RefreshWords(object obj)
        {
            _wordData.Clear();
            foreach (var marbleResource in _whatIsThisWord)
            {
                if (marbleResource.IsSense)
                {
                    _wordData.Add(marbleResource);
                }
            }
            NotifyOfPropertyChange(() => WordData);

            ButtonVisibility = Visibility.Hidden;
        }

        #endregion // Methods
    }
}
