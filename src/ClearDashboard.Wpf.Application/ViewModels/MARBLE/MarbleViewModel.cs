using Autofac;
using Caliburn.Micro;
using ClearDashboard.DAL.ViewModels;
using ClearDashboard.DataAccessLayer.Features.MarbleDataRequests;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Models.ViewModels.WordMeanings;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.ParatextPlugin.CQRS.Features.Verse;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.ViewModels.Panes;
using ClearDashboard.Wpf.Application.Views.Marble;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClearDashboard.Wpf.Application.ViewModels.Marble
{
    public class MarbleViewModel : ToolViewModel, IHandle<VerseChangedMessage>
    {
        #region Commands

        public ICommand LaunchLogosCommand { get; set; }
        public ICommand SearchSenseCommand { get; set; }
        public ICommand SetContextCommand { get; set; }

        #endregion Commands

        #region Member Variables   

        private List<SemanticDomainLookup>? _lookup;
        private string _currentVerse = "";

        private readonly ILogger<MarbleViewModel> _logger;
        private readonly DashboardProjectManager? _projectManager;
        private readonly IEventAggregator? _eventAggregator;
        private readonly TranslationSource _translationSource;

        private enum fiterReference
        {
            All,
            Book,
            Chapter,
        }

        private fiterReference _currentFilter = fiterReference.All;

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

        #endregion //Member Variables


        #region Public Properties

        #endregion //Public Properties


        #region Observable Properties

        #endregion //Observable Properties

        private ObservableCollection<Senses> _senses = new();
        public ObservableCollection<Senses> Senses
        {
            get => _senses;
            set
            {
                _senses = value;
                NotifyOfPropertyChange(() => Senses);
            }
        }

        private Senses _selectedSense;
        public Senses SelectedSense
        {
            get => _selectedSense;
            set
            {
                _selectedSense = value;
                FilterSenses();
                NotifyOfPropertyChange(() => SelectedSense);
            }
        }
        
        private List<CoupleOfStrings> _verses;
        public List<CoupleOfStrings> Verses
        {
            get => _verses;
            set
            {
                _verses = value;
                NotifyOfPropertyChange(() => Verses);
            }
        }


        


        private void FilterSenses()
        {
            var tempSense = Senses.FirstOrDefault(x => x == SelectedSense);
            if (tempSense is null)
            {
                return;
            }

            if (tempSense.Verses.Count == 0)
            {
                return;
            }
            
            switch (_currentFilter)
            {
                case fiterReference.All:
                    Verses = tempSense.Verses;
                    break;
                case fiterReference.Book:
                    string bbb = CurrentBcv.BBBCCCVVV.Substring(0, 3);

                    Verses = tempSense.Verses.Where(x =>
                    {
                        return x.stringA.StartsWith(bbb);
                    }).ToList();
                    break;
                case fiterReference.Chapter:
                    string bbbccc = CurrentBcv.BBBCCCVVV;
                    bbbccc = bbbccc.Substring(0,6);

                    Verses = tempSense.Verses.Where(x => x.stringA.StartsWith(bbbccc)).ToList();
                    break;
            }
        }

        private List<CoupleOfStrings> SelectedVerseReferences { get; set; } = new();



        private string _selectedHebrew = "";
        public string SelectedHebrew
        {
            get => _selectedHebrew;
            set
            {
                _selectedHebrew = value;
                NotifyOfPropertyChange(() => SelectedHebrew);
            }
        }

        private List<LexicalLink> _lexicalLinks;
        public List<LexicalLink> LexicalLinks
        {
            get => _lexicalLinks;
            set
            {
                _lexicalLinks = value;
                NotifyOfPropertyChange(() => LexicalLinks);
            }
        }

        private LexicalLink _selectedLexicalLink;
        public LexicalLink SelectedLexicalLink
        {
            get => _selectedLexicalLink;
            set
            {
                _selectedLexicalLink = value;

                if (_selectedLexicalLink is not null)
                {
                    SelectedHebrew = _selectedLexicalLink.Word;
                    GetWord();
                }

                NotifyOfPropertyChange(() => SelectedLexicalLink);
            }
        }

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

        #region Constructor

        public MarbleViewModel()
        {
            // no-op
        }

        public MarbleViewModel(INavigationService navigationService, ILogger<MarbleViewModel> logger,
            DashboardProjectManager? projectManager, TranslationSource translationSource,
            IEventAggregator? eventAggregator, IMediator mediator, ILifetimeScope lifetimeScope)
            : base(navigationService, logger, projectManager, eventAggregator, mediator, lifetimeScope)
        {
            _logger = logger;
            _projectManager = projectManager;
            _eventAggregator = eventAggregator;
            _translationSource = translationSource;


            Title = "◕ " + "MARBLE";
            ContentId = "MARBLE";
            DockSide = EDockSide.Bottom;

            // wire up the commands
            LaunchLogosCommand = new RelayCommand(LaunchLogosStrongNumber);
            SearchSenseCommand = new RelayCommandAsync(SearchSense);
            SetContextCommand = new RelayCommandAsync(SetContext);
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
            // load in all the word search lookups
            await LoadSearchCSV();
            
            if (ProjectManager.CurrentVerse != String.Empty)
            {
                CurrentBcv.SetVerseFromId(ProjectManager.CurrentVerse);
                _currentVerse = CurrentBcv.BBBCCCVVV;

                await LoadUpVerse().ConfigureAwait(false);
            }

            base.OnViewReady(view);
        }

        private async Task LoadSearchCSV()
        {
            var queryResult = await ExecuteRequest(new LoadSemanticDictionaryLookupSlice.LoadSemanticDictionaryLookupQuery(), CancellationToken.None).ConfigureAwait(false);
            if (queryResult.Success == false)
            {
                Logger!.LogError(queryResult.Message);
                return;
            }


            if (queryResult.Data == null)
            {
               
                return;
            }

            _lookup = queryResult.Data;
        }

        #endregion //Constructor


        #region Methods

        private async Task LoadUpVerse()
        {
            var queryResult = await ExecuteRequest(new GetVerseDataFromSemanticDatabaseQuery(CurrentBcv), CancellationToken.None).ConfigureAwait(false);
            if (queryResult.Success == false)
            {
                Logger!.LogError(queryResult.Message);
                return;
            }


            if (queryResult.Data == null)
            {
                LexicalLinks.Clear();
                return;
            }

            _lexicalLinks = queryResult.Data;

            if (_lexicalLinks.Count > 0)
            {
                SelectedHebrew = _lexicalLinks[0].Word;
                await GetWord().ConfigureAwait(false);
            }
            NotifyOfPropertyChange(() => LexicalLinks);
        }



        /// <summary>
        /// Get the Biblical Words from 
        /// </summary>
        /// <returns></returns>
        private async Task GetWord()
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

            var queryResult = await ExecuteRequest(new GetWordMeaningsQuery(CurrentBcv, languageCode, _selectedHebrew, _lookup), CancellationToken.None).ConfigureAwait(false);
            if (queryResult.Success == false)
            {
                Logger!.LogError(queryResult.Message);
                Senses.Clear();
                return;
            }


            if (queryResult.Data == null)
            {
                Logger!.LogError($"MABLE DB Query returned null for {CurrentBcv.BBBCCCVVV} Word: {_selectedHebrew}");
                return;
            }


            Senses = queryResult.Data;
            if (Senses.Count > 0)
            {
                SelectedSense = Senses[0];
            }
            
        }

        private Task SetContext(object arg)
        {
            if (arg.ToString() == "All")
            {
                _currentFilter = fiterReference.All;
            }
            else if (arg.ToString() == "Book")
            {
                _currentFilter = fiterReference.Book;
            }
            else
            {
                _currentFilter = fiterReference.Chapter;
            }

            FilterSenses();
            return Task.CompletedTask;
        }



        private async Task SearchSense(object obj)
        {
            if (obj is RelatedLemma lemma)
            {
                _selectedHebrew = lemma.Lemma;
                await GetWord().ConfigureAwait(false);
            }
        }

        private void LaunchLogosStrongNumber(object obj)
        {
            if (obj is null)
            {
                return;
            }

            var StrongNum = (string)obj;

            var LogosRef = Convert.ToInt32(StrongNum.Substring(1));

            //var LogosRef = Uri.EscapeDataString(chopped).Replace('%', '$');


            //// OT or NT?
            if (StrongNum.StartsWith("H"))
            {
                // Hebrew link
                var hebrewPrefix = "logosres:strongs;ref=HebrewStrongs.";
                LaunchWebPage.TryOpenUrl(hebrewPrefix + LogosRef);
            }
            else if (StrongNum.StartsWith("G"))
            {
                // Greek link
                var greekPrefix = "logosres:strongs;ref=GreekStrongs.";
                LaunchWebPage.TryOpenUrl(greekPrefix + LogosRef);
            }
            else if (StrongNum.StartsWith("A"))
            {
                // Aramaic link
                var greekPrefix = "logosres:strongs;ref=HebrewStrongs.";
                LaunchWebPage.TryOpenUrl(greekPrefix + LogosRef);
            }
        }

        public void LaunchMirrorView(double actualWidth, double actualHeight)
        {
            LaunchMirrorView<MarbleView>.Show(this, actualWidth, actualHeight);
        }


        public Task HandleAsync(VerseChangedMessage message, CancellationToken cancellationToken)
        {
            var incomingVerse = message.Verse.PadLeft(9, '0');

            if (_currentVerse == incomingVerse)
            {
                return Task.CompletedTask;
            }


            _currentVerse = incomingVerse;
            CurrentBcv.SetVerseFromId(_currentVerse);
            if (_currentVerse.EndsWith("000"))
            {

                // TODO
                
                // a zero based verse
                //TargetInlinesText.Clear();
                //NotifyOfPropertyChange(() => TargetInlinesText);
                //TargetHTML = "";
                //WordData.Clear();
                //NotifyOfPropertyChange(() => WordData);
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

                _ = LoadUpVerse();
                
                //_ = GetWord();
                
            }

            return Task.CompletedTask;
        }

        #endregion // Methods


    }
}
