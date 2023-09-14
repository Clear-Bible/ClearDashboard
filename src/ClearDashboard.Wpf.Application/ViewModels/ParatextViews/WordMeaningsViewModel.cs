using Autofac;
using Caliburn.Micro;
using ClearDashboard.DAL.ViewModels;
using ClearDashboard.DataAccessLayer.Features.MarbleDataRequests;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.ParatextPlugin.CQRS.Features.Verse;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.ViewModels.Panes;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using ClearDashboard.Wpf.Application.Views.ParatextViews;

namespace ClearDashboard.Wpf.Application.ViewModels.ParatextViews
{
    public class WordMeaningsViewModel : ToolViewModel, IHandle<VerseChangedMessage>
    {

        #region Member Variables

        //private readonly ILogger _logger;
        //private readonly ProjectManager _projectManager;
        private readonly DashboardProjectManager? _projectManager;
        private readonly TranslationSource _translationSource;

        private string _currentVerse = "";

        List<MarbleResource> _whatIsThisWord = new List<MarbleResource>();

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

        //private BookChapterVerseViewModel _currentBcv = new();
        //public BookChapterVerseViewModel CurrentBcv
        //{
        //    get => _currentBcv;
        //    set
        //    {
        //        _currentBcv = value;
        //        NotifyOfPropertyChange(() => CurrentBcv);

        //        if (_currentBcv.BookNum < 40)
        //        {
        //            IsOT = true;
        //        }
        //        else
        //        {
        //            IsOT = false;
        //        }
        //    }
        //}

        private ObservableCollection<MarbleResource> _wordData = new ObservableCollection<MarbleResource>();
        public ObservableCollection<MarbleResource> WordData
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
            get => _targetInlinesText;
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
            get => _sourceInlinesText;
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

        #region BCV
        private bool _paratextSync = false;
        public bool ParatextSync
        {
            get => _paratextSync;
            set
            {
                if (value == true)
                {
                    // TODO do we return back the control to what Paratext is showing
                    // or do we change Paratext to this new verse?  currently set to 
                    // change Paratext to this new verse
                    _ = Task.Run(() =>
                        ExecuteRequest(new SetCurrentVerseCommand(CurrentBcv.BBBCCCVVV), CancellationToken.None));
                }

                _paratextSync = value;
                NotifyOfPropertyChange(() => ParatextSync);
            }
        }

        private Dictionary<string, string> _bcvDictionary;
        public Dictionary<string, string> BcvDictionary
        {
            get => _bcvDictionary;
            set
            {
                _bcvDictionary = value;
                NotifyOfPropertyChange(() => BcvDictionary);
            }
        }

        private BookChapterVerseViewModel _currentBcv = new();
        public BookChapterVerseViewModel CurrentBcv
        {
            get => _currentBcv;
            set
            {
                _currentBcv = value;
                NotifyOfPropertyChange(() => CurrentBcv);
            }
        }

        private int _verseOffsetRange = 0;
        public int VerseOffsetRange
        {
            get => _verseOffsetRange;
            set
            {
                _verseOffsetRange = value;
                NotifyOfPropertyChange(() => _verseOffsetRange);
            }
        }



        private string _verseChange = string.Empty;
        public string VerseChange
        {
            get => _verseChange;
            set
            {
                if (_verseChange == "")
                {
                    _verseChange = value;
                    NotifyOfPropertyChange(() => VerseChange);
                }
                else if (_verseChange != value)
                {
                    // push to Paratext
                    if (ParatextSync)
                    {
                        _ = Task.Run(() =>
                            ExecuteRequest(new SetCurrentVerseCommand(CurrentBcv.BBBCCCVVV), CancellationToken.None));
                    }

                    _verseChange = value;
                    NotifyOfPropertyChange(() => VerseChange);
                }
            }
        }



        #endregion BCV


        #endregion //Observable Properties

        #region Commands

        public ICommand LaunchLogosCommand { get; set; }
        public ICommand LaunchSensesCommand { get; set; }
        public ICommand GoBackCommand { get; set; }

        #endregion

        #region Constructor
        public WordMeaningsViewModel()
        {
            // no-op retained for designer support
        }

        public WordMeaningsViewModel(INavigationService navigationService, ILogger<WordMeaningsViewModel> logger,
            DashboardProjectManager? projectManager, TranslationSource translationSource,
            IEventAggregator? eventAggregator, IMediator mediator, ILifetimeScope lifetimeScope)
            : base(navigationService, logger, projectManager, eventAggregator, mediator, lifetimeScope)
        {
            Title = "⌺ " + LocalizationStrings.Get("Windows_WordMeanings", Logger);
            ContentId = "WORDMEANINGS";
            DockSide = DockSide.Bottom;

            _projectManager = projectManager;
            _translationSource = translationSource;

            // wire up the commands
            LaunchLogosCommand = new RelayCommand(ShowLogos);
            LaunchSensesCommand = new RelayCommand(ShowSenses);
            GoBackCommand = new RelayCommand(RefreshWords);
        }

        protected override void OnViewAttached(object view, object context)
        {
            // grab the dictionary of all the verse lookups
            if (ProjectManager?.CurrentParatextProject is not null)
            {
                BcvDictionary = ProjectManager.CurrentParatextProject.BcvDictionary;

                var books = BcvDictionary.Values.GroupBy(b => b.Substring(0, 3))
                    .Select(g => g.First())
                    .ToList();

                foreach (var book in books)
                {
                    var bookId = book.Substring(0, 3);

                    var bookName = BookChapterVerseViewModel.GetShortBookNameFromBookNum(bookId);

                    CurrentBcv.BibleBookList?.Add(bookName);
                }
            }
            else
            {
                BcvDictionary = new Dictionary<string, string>();
            }
            
            CurrentBcv.SetVerseFromId(_projectManager.CurrentVerse);
            NotifyOfPropertyChange(() => CurrentBcv);
            VerseChange = _projectManager.CurrentVerse;
            
            base.OnViewAttached(view, context);
        }

        protected override async void OnViewReady(object view)
        {
            if (ProjectManager.CurrentVerse != String.Empty)
            {
                CurrentBcv.SetVerseFromId(ProjectManager.CurrentVerse);
                await ReloadWordMeanings();
            }

            base.OnViewReady(view);
        }

        #endregion //Constructor

        #region Methods

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        private void ShowSenses(object obj)
        {
            if (obj is null)
            {
                return;
            }

            var marbleResource = (MarbleResource)obj;
            if (marbleResource.TotalSenses == 1)
            {
                return;
            }

            if (this.ButtonVisibility == Visibility.Visible)
            {
                // we are in the details already...jump back up a level
                this.RefreshWords(null);
                return;
            }

            var items = _whatIsThisWord.Where(p => p.Id == marbleResource.Id);

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

            var marbleResource = (MarbleResource)obj;

            // OT or NT?
            if (CurrentBcv.BookNum < 40)
            {
                // Hebrew link
                var hebrewPrefix = "logos4:Guide;t=My_Bible_Word_Study;lemma=lbs$2Fhe$2F";
                LaunchWebPage.TryOpenUrl(hebrewPrefix + marbleResource.LogosRef);
            }
            else
            {
                // Greek link
                var greekPrefix = "logos4:Guide;t=My_Bible_Word_Study;lemma=lbs$2Fel$2F";
                LaunchWebPage.TryOpenUrl(greekPrefix + marbleResource.LogosRef);
            }
        }


        /// <summary>
        /// Listen for changes in the DAL regarding any messages coming in
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public async void HandleEventAsync(object sender, EventArgs args)
        {
            //TODO:  Refactor to use EventAggregator
            //if (args == null) return;

            //var pipeMessage = args.PipeMessage;

            //switch (pipeMessage.Action)
            //{
            //    case ActionType.CurrentVerse:
            //        if (_currentVerse != pipeMessage.Text)
            //        {
            //            _currentVerse = pipeMessage.Text;
            //            CurrentBcv.SetVerseFromId(_currentVerse);
            //            if (_currentVerse.EndsWith("000"))
            //            {
            //                // a zero based verse
            //                TargetInlinesText.Clear();
            //                NotifyOfPropertyChange(() => TargetInlinesText);
            //                TargetHTML = "";
            //                WordData.Clear();
            //                NotifyOfPropertyChange(() => WordData);
            //            }
            //            else
            //            {
            //                // a normal verse
            //                var  verse = new Verse
            //                {
            //                    VerseBBCCCVVV = _currentVerse
            //                };

            //                if (verse.BookNum < 40)
            //                {
            //                    _isOT = true;
            //                }
            //                else
            //                {
            //                    _isOT = false;
            //                }

            //                _ = ReloadWordMeanings();
            //            }
            //        }

            //        break;
            //}
        }

        /// <summary>
        /// Get the Biblical Words from 
        /// </summary>
        /// <returns></returns>
        private async Task ReloadWordMeanings()
        {
            // SDBH & SDBG support the following language codes:
            // en, fr, sp, pt, sw, zht, zhs

            var languageCode = "";

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

            var queryResult = await ExecuteRequest(new GetWhatIsThisWordByBcvQuery(CurrentBcv, languageCode), CancellationToken.None);
            if (queryResult.Success == false)
            {
                Logger!.LogError(queryResult.Message);
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

            OnUIThread(() =>
            {
                WordData.Clear();
                foreach (var marbleResource in _whatIsThisWord)
                {
                    if (marbleResource.IsSense)
                    {
                        _wordData.Add(marbleResource);
                    }
                }
            });
            //Application.Current.Dispatcher.Invoke((Action)delegate
            //{
            //    _wordData.Clear();
            //    foreach (var marbleResource in _whatIsThisWord)
            //    {
            //        if (marbleResource.IsSense)
            //        {
            //            _wordData.Add(marbleResource);
            //        }
            //    }
            //});

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

        public async Task HandleAsync(VerseChangedMessage message, CancellationToken cancellationToken)
        {
            var incomingVerse = message.Verse.PadLeft(9, '0');

            if (_currentVerse == incomingVerse)
            {
                return;
            }

            _currentVerse = incomingVerse;
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
                var verse = new Verse
                {
                    VerseBBBCCCVVV = _currentVerse
                };

                int BookNum;
                try
                {
                    _currentBcv.SetVerseFromId(message.Verse);
                    BookNum = _currentBcv.BookNum;
                }
                catch (Exception)
                {
                    Logger.LogError($"Error converting [{message.Verse}] to book integer in WordMeanings");
                    BookNum = 01;
                }


                if (BookNum < 40)
                {
                    _isOT = true;
                }
                else
                {
                    _isOT = false;
                }

                // send to log
                await EventAggregator.PublishOnUIThreadAsync(new LogActivityMessage($"{this.DisplayName}: Verse Change"), cancellationToken);

                _ = ReloadWordMeanings();
            }
            
        }

        public void LaunchMirrorView(double actualWidth, double actualHeight)
        {
            LaunchMirrorView<WordMeaningsView>.Show(this, actualWidth, actualHeight);
        }

        #endregion // Methods
    }
}
