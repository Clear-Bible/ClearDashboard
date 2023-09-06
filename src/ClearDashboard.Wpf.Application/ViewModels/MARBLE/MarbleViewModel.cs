using Autofac;
using Caliburn.Micro;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.ViewModels;
using ClearDashboard.DataAccessLayer.Features.MarbleDataRequests;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Models.ViewModels.WordMeanings;
using ClearDashboard.ParatextPlugin.CQRS.Features.Verse;
using ClearDashboard.ParatextPlugin.CQRS.Features.VerseText;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Messages;
using ClearDashboard.Wpf.Application.UserControls;
using ClearDashboard.Wpf.Application.ViewModels.Panes;
using ClearDashboard.Wpf.Application.Views.Marble;
using MaterialDesignThemes.Wpf;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ClearApplicationFoundation.Framework.Input;
using ClearDashboard.Wpf.Application.Services;
using wpfKeyBoard;


#pragma warning disable CS8618

namespace ClearDashboard.Wpf.Application.ViewModels.Marble
{
    public class MarbleViewModel : ToolViewModel, IHandle<VerseChangedMessage>
    {
        #region Commands

        public ICommand LaunchLogosCommand { get; set; }
        public ICommand LaunchLogosStrongNumberCommand { get; set; }
        public ICommand SearchSenseCommand { get; set; }
        public ICommand SetContextCommand { get; set; }
        public ICommand GetVerseDetailCommand { get; set; }
        public ICommand ShowDrawerCommand { get; set; }
        public ICommand GotoSourceWordCommand { get; set; }

        // used by the MarbleLinkControl
        private ICommand _linkCommand;
        public ICommand LinkCommand
        {
            get => _linkCommand;
            set
            {
                _linkCommand = value;
                OnPropertyChanged();
            }
        }

        #endregion Commands

        #region Member Variables   

        private List<SemanticDomainLookup>? _lookup;
        private string _currentVerse = "";

        private readonly ILogger<MarbleViewModel> _logger;
        private readonly DashboardProjectManager? _projectManager;
        private readonly IEventAggregator? _eventAggregator;
        private readonly TranslationSource _translationSource;
        private readonly ILocalizationService _localizationService;

        private enum FiterReference
        {
            All,
            Book,
            Chapter,
        }

        private FiterReference _currentFilter = FiterReference.All;

        private DrawerHost _drawerHost;



        #region BCV
        private bool _paratextSync;
        public bool ParatextSync
        {
            get => _paratextSync;
            set
            {
                if (value)
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

        private int _verseOffsetRange;
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

        private List<CoupleOfStrings> _searchResults = new();
        public List<CoupleOfStrings> SearchResults
        {
            get => _searchResults;
            set
            {
                _searchResults = value;
                NotifyOfPropertyChange(() => SearchResults);
            }
        }

        private string _searchEnglish = string.Empty;
        public string SearchEnglish
        {
            get => _searchEnglish;
            set
            {
                if (value is null)
                {
                    value = string.Empty;
                }

                _searchEnglish = value;
                NotifyOfPropertyChange(() => SearchEnglish);

                if (_searchEnglish.Length > 2)
                {
                    _ = SearchEnglishDatabase(_searchEnglish);
                }
                else
                {
                    SearchResults = new List<CoupleOfStrings>();
                }
            }
        }

        private string _searchSource = string.Empty;
        public string SearchSource
        {
            get => _searchSource;
            set
            {
                if (value is null)
                {
                    value = string.Empty;
                }
                _searchSource = value;
                NotifyOfPropertyChange(() => SearchSource);


                
                if (_searchSource.Length > 1)
                {
                    _ = SearchSourceDatabase(_searchSource);
                }
                else
                {
                    SearchResults = new List<CoupleOfStrings>();
                }
            }
        }




        private Visibility _drawerVisibility;
        public Visibility DrawerVisibility
        {
            get => _drawerVisibility;
            set
            {
                _drawerVisibility = value;
                NotifyOfPropertyChange(() => DrawerVisibility);
            }
        }


        private bool _isTargetRtl;
        public bool IsTargetRtl
        {
            get => _isTargetRtl;
            set
            {
                _isTargetRtl = value;
                NotifyOfPropertyChange(() => IsTargetRtl);
            }
        }


        private ObservableCollection<Senses> _senses = new();
        public ObservableCollection<Senses> Senses
        {
            get => _senses;
            set
            {
                _senses = value;

                VerseText = "";
                SourceText = "";
                VerseTextId = "";

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

        private string _selectedWord = "";
        public string SelectedWord
        {
            get => _selectedWord;
            set
            {
                _selectedWord = value;
                NotifyOfPropertyChange(() => SelectedWord);
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
                    SelectedWord = _selectedLexicalLink.Word;
                    _ = GetWord();
                }

                NotifyOfPropertyChange(() => SelectedLexicalLink);
            }
        }

        private bool _isOt;
        public bool IsOt
        {
            get => _isOt;
            set
            {
                _isOt = value;
                NotifyOfPropertyChange(() => IsOt);
            }
        }

        private string _verseTextFontFamily;
        public string VerseTextFontFamily
        {
            get => _verseTextFontFamily;
            set
            {
                _verseTextFontFamily = value;
                NotifyOfPropertyChange(() => VerseTextFontFamily);
            }
        }

        private string _sourceTextFontFamily = "SBL Hebrew";
        public string SourceTextFontFamily
        {
            get => _sourceTextFontFamily;
            set
            {
                _sourceTextFontFamily = value;
                NotifyOfPropertyChange(() => SourceTextFontFamily);
            }
        }

        private string _verseText;
        public string VerseText
        {
            get => _verseText;
            set
            {
                _verseText = value;
                NotifyOfPropertyChange(() => VerseText);
            }
        }

        private string _verseTextId;
        public string VerseTextId
        {
            get => _verseTextId;
            set
            {
                _verseTextId = value;
                NotifyOfPropertyChange(() => VerseTextId);
            }
        }



        private string _sourceText;
        public string SourceText
        {
            get => _sourceText;
            set
            {
                _sourceText = value;
                NotifyOfPropertyChange(() => SourceText);
            }
        }


        #region Constructor

        public MarbleViewModel()
        {
            // no-op
        }

        public MarbleViewModel(INavigationService navigationService, ILogger<MarbleViewModel> logger,
            DashboardProjectManager? projectManager, TranslationSource translationSource,
            IEventAggregator? eventAggregator, IMediator mediator, ILifetimeScope lifetimeScope, ILocalizationService localizationService)
            : base(navigationService, logger, projectManager, eventAggregator, mediator, lifetimeScope,localizationService)
        {
            _logger = logger;
            _projectManager = projectManager;
            _eventAggregator = eventAggregator;
            _translationSource = translationSource;
            _localizationService = localizationService;


            Title = "◕ " + _localizationService!.Get("MainView_WindowsMarble");
            ContentId = "MARBLE";
            DockSide = DockSide.Bottom;

            // wire up the commands
            LaunchLogosCommand = new RelayCommand(LaunchLogos);
            LaunchLogosStrongNumberCommand = new RelayCommand(LaunchLogosStrongNumber);
            SearchSenseCommand = new RelayCommandAsync(SearchSense);
            SetContextCommand = new RelayCommandAsync(SetContext);
            GetVerseDetailCommand = new RelayCommandAsync(GetVerseDetail);
            ShowDrawerCommand = new RelayCommand(ShowDrawer);
            GotoSourceWordCommand = new RelayCommand(GotoSourceWord);

            LinkCommand = new DelegateCommand<string>(OnLinkCommand);
        }


        protected override void OnViewAttached(object view, object context)
        {
            // hook up a reference to the windows drawer host so we can close
            // it after a search
            if (view is MarbleView currentView)
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                _drawerHost = (DrawerHost)currentView.FindName("DrawerHost");
            }
            
            
            
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

                IsTargetRtl = ProjectManager.CurrentParatextProject.IsRTL;
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
            await LoadSearchCsv();

            if (ProjectManager?.CurrentVerse != string.Empty)
            {
                CurrentBcv.SetVerseFromId(ProjectManager?.CurrentVerse);
                _currentVerse = CurrentBcv.BBBCCCVVV;

                await LoadUpVerse();
            }

            //// start collecting the gloss to Heb/Greek words
            //_ = Task.Run(async () =>
            //{
            //    await ObtainGlosses();
            //});

            if (ProjectManager.CurrentParatextProject != null)
            {
                var paratextProject = ProjectManager.CurrentParatextProject;
                // pull out the project font family
                _verseTextFontFamily = paratextProject.Language.FontFamily;
                IsTargetRtl = paratextProject.Language.IsRtol;
            }

            base.OnViewReady(view);
        }


        /// <summary>
        /// Load up the CSV file with results of which Heb/Greek word are in which Semantic Dictionary File
        /// </summary>
        /// <returns></returns>
        private async Task LoadSearchCsv()
        {
            var queryResult = await ExecuteRequest(new LoadSemanticDictionaryLookupSlice.LoadSemanticDictionaryLookupQuery(), CancellationToken.None);
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


        /// <summary>
        /// Iterate through the glosses and obtain the Heb/Greek words
        /// </summary>
        /// <returns></returns>
        //private async Task ObtainGlosses()
        //{
        //    //var queryResult =
        //    //    await ExecuteRequest(new LoadSemanticDictionaryGlosses.LoadSemanticDictionaryGlossesQuery(),
        //    //        CancellationToken.None);
        //    //if (queryResult.Success == false)
        //    //{
        //    //    Logger!.LogError(queryResult.Message);
        //    //    return;
        //    //}
        //}

        #endregion //Constructor


        #region Methods

        public void VirtualKeyPressed(object sender, RoutedEventArgs e)
        {
            var args = (VirtualKeyPressedEventArgs)e;
            //Console.WriteLine(args.VKey.Value);

            var unicodeChar = CharToUnicodeFormat(args.VKey.Value[0]);

            switch (unicodeChar)
            {
                case "U+f177": // backspace
                    if (SearchSource.Length > 0)
                    {
                        SearchSource = SearchSource.Remove(SearchSource.Length - 1, 1);
                    }
                    break;
                case "U+f149": // enter key
                case "U+f062": // left shift key
                case "U+0308": // empty key
                case "U+0026": // number key
                case "U+0020": // space key
                case "U+f11c": // keyboard select key
                    break;
                default: // regular characters
                    SearchSource += args.VKey.Value;
                    break;
            }

        }


        private string CharToUnicodeFormat(char c)
        {
            return string.Format(@"U+{0:x4}", (int)c);
        }

        private void GotoSourceWord(object? obj)
        {
            if (obj is Button button)
            {
                SelectedWord = button.Content.ToString();
                if (button.Tag.ToString() == "1")
                {
                    _ = GetWord(true, true);
                }
                else
                {
                    _ = GetWord(true, false);
                }

                //if (_drawerHost is not null)
                //{
                //    // CODE NOT WORKING - WHY??
                //    _drawerHost.IsTopDrawerOpen = false;
                //}
            }
        }
        
        private async Task SearchSourceDatabase(string searchSource)
        {
            var queryResult = await ExecuteRequest(new GetConsonantsSliceQuery(searchSource), CancellationToken.None);
            if (queryResult.Success == false)
            {
                Logger!.LogError(queryResult.Message);
                return;
            }


            if (queryResult.Data == null)
            {
                return;
            }

            var list = new List<CoupleOfStrings>();

            foreach (var item in queryResult.Data)
            {
                if (item.stringB == "1")
                {
                    list.Add(new CoupleOfStrings
                    {
                        stringA = item.stringA,
                        stringB = "1"
                    });
                }
                else
                {
                    list.Add(new CoupleOfStrings
                    {
                        stringA = item.stringA,
                        stringB = ""
                    });
                }


            }

            SearchResults = list;
        }


        private async Task SearchEnglishDatabase(string searchEnglish)
        {
            var queryResult = await ExecuteRequest(new GetEnglishGlossSliceQuery(searchEnglish), CancellationToken.None);
            if (queryResult.Success == false)
            {
                Logger!.LogError(queryResult.Message);
                return;
            }


            if (queryResult.Data == null)
            {
                return;
            }
            
            SearchResults = queryResult.Data;
        }


        private void ShowDrawer(object? obj)
        {
            DrawerVisibility = Visibility.Visible;
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
                case FiterReference.All:
                    Verses = tempSense.Verses;
                    break;
                case FiterReference.Book:
                    string bbb = CurrentBcv.BBBCCCVVV.Substring(0, 3);

                    Verses = tempSense.Verses.Where(x =>
                    {
                        return x.stringA.StartsWith(bbb);
                    }).ToList();
                    break;
                case FiterReference.Chapter:
                    string bbbccc = CurrentBcv.BBBCCCVVV;
                    bbbccc = bbbccc.Substring(0, 6);

                    Verses = tempSense.Verses.Where(x => x.stringA.StartsWith(bbbccc)).ToList();
                    break;
            }
        }

        private async Task GetVerseDetail(object? arg)
        {
            if (arg is CoupleOfStrings coupleOfStrings)
            {
                var verseId = coupleOfStrings.stringA;

                // create a list for doing versification processing
                var verseList = new List<VersificationList>();

                verseList.Add(new VersificationList
                {
                    SourceBBBCCCVV = verseId.Substring(0, 9),
                    TargetBBBCCCVV = "",
                });

                // this data from the BiblicalTerms & AllBiblicalTerms XML files has versification from the org.vrs
                // convert it over to the current project versification format.
                verseList = Helpers.Versification.GetVersificationFromOriginal(verseList, _projectManager.CurrentParatextProject);

                // create the list to display
                var verse = verseList[0];

                var bookNum = int.Parse(verse.TargetBBBCCCVV.Substring(0, 3));
                var chapterNum = int.Parse(verse.TargetBBBCCCVV.Substring(3, 3));
                var verseNum = int.Parse(verse.TargetBBBCCCVV.Substring(6, 3));

                // call paratext to get this verse
                var verseTextResult = await ExecuteRequest(new GetParatextVerseTextQuery(bookNum, chapterNum, verseNum), CancellationToken.None);
                var verseText = "";
                if (verseTextResult.Success)
                    verseText = verseTextResult.Data.Name;
                else
                {
                    verseText = "There was an issue getting the text for this verse.";
                    Logger.LogInformation("Failure to GetParatextVerseTextQuery");
                }

                // get the Heb/Greek text
                var sourceWords = LexicalLinks.Select(x => x.Word + " ").ToList();
                var sourceText = string.Join("", sourceWords);
                if (LexicalLinks[0].IsRtl)
                {
                    SourceTextFontFamily = "SBL Hebrew";
                }
                else
                {
                    SourceTextFontFamily = "SBL Greek";
                }

                var project = ProjectManager!.ProjectMetadata.FirstOrDefault(x => x.Id == ProjectManager.CurrentParatextProject.Guid);

                VerseText = verseText;
                VerseTextFontFamily = project.FontFamily;
                SourceText = sourceText;
                VerseTextId = coupleOfStrings.stringB;
            }
        }


        private async Task LoadUpVerse()
        {
            var queryResult = await ExecuteRequest(new GetVerseDataFromSemanticDatabaseQuery(CurrentBcv), CancellationToken.None);
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
                SelectedLexicalLink = _lexicalLinks[0];
                SelectedWord = _lexicalLinks[0].Word;
                await GetWord();
            }
            NotifyOfPropertyChange(() => LexicalLinks);
        }



        /// <summary>
        /// Get the Biblical Words from 
        /// </summary>
        /// <returns></returns>
        private async Task GetWord(bool defineTestament = false, bool isHebrew = false)
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
                    languageCode = "zhS";
                    break;
                case "zh-TW":
                    languageCode = "zhT";
                    break;
                case "pt":
                    languageCode = "pt";
                    break;
                case "hi":
                    languageCode = "hi";
                    break;

                default:
                    // default to English for everyone else
                    languageCode = "en";
                    break;
            }

            RequestResult<ObservableCollection<Senses>> queryResult;

            if (defineTestament)  
            {
                // call coming in from the search window so we don't want the current BCV
                BookChapterVerseViewModel bcv = new BookChapterVerseViewModel();
                
                // send with the knowledge that we know which testament it is
                if (isHebrew)
                {
                    bcv.SetVerseFromId("001001001"); // set to OT
                    IsOt = true;
                }
                else
                {
                    bcv.SetVerseFromId("040001001"); // set to NT
                    IsOt = false;
                }

                queryResult = await ExecuteRequest(new GetWordMeaningsQuery(bcv, languageCode, _selectedWord, _lookup), CancellationToken.None);
            }
            else
            {
                // send with the actual current BCV verse
                queryResult = await ExecuteRequest(new GetWordMeaningsQuery(CurrentBcv, languageCode, _selectedWord, _lookup), CancellationToken.None);
            }
            
            if (queryResult.Success == false)
            {
                Logger!.LogError(queryResult.Message);
                Senses.Clear();
                return;
            }


            if (queryResult.Data == null)
            {
                Logger!.LogError($"MABLE DB Query returned null for {CurrentBcv.BBBCCCVVV} Word: {_selectedWord}");
                return;
            }


            Senses = queryResult.Data;
            if (Senses.Count == 0)
            {
                SelectedSense = Senses[0];
            }
            else
            {
                for (int i = 0; i < Senses.Count; i++)
                {
                    if (Senses[i].IsCurrentVerseSense)
                    {
                        SelectedSense = Senses[i];
                        break;
                    }
                }
            }

            if (defineTestament == false)
            {
                // get ot/nt
                var bookNum = int.Parse(CurrentBcv.BBBCCCVVV.Substring(0, 3));
                if (bookNum < 40)
                {
                    IsOt = true;
                }
                else
                {
                    IsOt = false;
                }
            }
        }

        private Task SetContext(object? arg)
        {
            if (arg.ToString() == "All")
            {
                _currentFilter = FiterReference.All;
            }
            else if (arg.ToString() == "Book")
            {
                _currentFilter = FiterReference.Book;
            }
            else
            {
                _currentFilter = FiterReference.Chapter;
            }

            VerseText = "";
            SourceText = "";
            VerseTextId = "";

            FilterSenses();
            return Task.CompletedTask;
        }



        private async Task SearchSense(object? obj)
        {
            if (obj is RelatedLemma lemma)
            {
                _selectedWord = lemma.Lemma;
                await GetWord();
            }
        }


        private void LaunchLogos(object? obj)
        {
            if (obj is null)
            {
                return;
            }


            var safeUrl = Uri.EscapeDataString(SelectedWord).Replace('%', '$');

            // OT or NT?
            if (CurrentBcv.BookNum < 40)
            {
                // Hebrew link
                var hebrewPrefix = "logos4:Guide;t=My_Bible_Word_Study;lemma=lbs$2Fhe$2F";
                LaunchWebPage.TryOpenUrl(hebrewPrefix + safeUrl);
            }
            else
            {
                // Greek link
                var greekPrefix = "logos4:Guide;t=My_Bible_Word_Study;lemma=lbs$2Fel$2F";
                LaunchWebPage.TryOpenUrl(greekPrefix + safeUrl);
            }
        }

        private void LaunchLogosStrongNumber(object? obj)
        {
            if (SelectedWord == "")
            {
                return;
            }

            var strongNum = (string)obj;
            // remove all the alphabetic characters
            var logosRef = Regex.Replace(strongNum, "[^0-9.]", "");

            //// OT or NT?
            if (strongNum.StartsWith("H"))
            {
                // Hebrew link
                var hebrewPrefix = "logosres:strongs;ref=HebrewStrongs.";
                LaunchWebPage.TryOpenUrl(hebrewPrefix + logosRef);
            }
            else if (strongNum.StartsWith("G"))
            {
                // Greek link
                var greekPrefix = "logosres:strongs;ref=GreekStrongs.";
                LaunchWebPage.TryOpenUrl(greekPrefix + logosRef);
            }
            else if (strongNum.StartsWith("A"))
            {
                // Aramaic link
                var greekPrefix = "logosres:strongs;ref=HebrewStrongs.";
                LaunchWebPage.TryOpenUrl(greekPrefix + logosRef);
            }
        }

        public void LaunchMirrorView(double actualWidth, double actualHeight)
        {
            LaunchMirrorView<MarbleView>.Show(this, actualWidth, actualHeight, this.Title);
        }

        /// <summary>
        /// Go to linked word in short description
        /// </summary>
        /// <param name="word"></param>
        private void OnLinkCommand(string word)
        {
            SelectedWord = word;
            _ = GetWord();
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
                _ = new VerseObject
                {
                    VerseBBBCCCVVV = _currentVerse
                };

                int bookNum;
                try
                {
                    _currentBcv.SetVerseFromId(message.Verse);
                    bookNum = _currentBcv.BookNum;
                }
                catch (Exception)
                {
                    Logger?.LogError($"Error converting [{message.Verse}] to book integer in WordMeanings");
                    bookNum = 01;
                }


                if (bookNum < 40)
                {
                    _isOt = true;
                }
                else
                {
                    _isOt = false;
                }

                _ = LoadUpVerse();

                //_ = GetWord();

            }

            return Task.CompletedTask;
        }

        #endregion // Methods


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
