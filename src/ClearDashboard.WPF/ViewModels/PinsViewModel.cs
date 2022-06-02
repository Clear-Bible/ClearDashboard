using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Features.PINS;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Models.Common;
using ClearDashboard.DataAccessLayer.Models.Helpers;
using ClearDashboard.DataAccessLayer.Paratext;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.Helpers;
using ClearDashboard.Wpf.ViewModels.Panes;
using Microsoft.Extensions.Logging;
using SIL.ObjectModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml;

namespace ClearDashboard.Wpf.ViewModels
{
    public class PinsViewModel : ToolViewModel
    {

        #region Member Variables

        private TermRenderingsList _termRenderingsList = new();
        private BiblicalTermsList _biblicalTermsList = new();
        private BiblicalTermsList _allBiblicalTermsList = new();
        private SpellingStatus _spellingStatus = new();
        private Lexicon _lexicon = new();

        private readonly DashboardProjectManager _projectManager;
        private readonly ILogger<PinsViewModel> _logger;
        private readonly INavigationService _navigationService;
        private readonly IEventAggregator _eventAggregator;

        #endregion //Member Variables

        #region Public Properties


        #endregion //Public Properties

        #region Observable Properties

        public Point WindowLocation { get; set; }

        private ObservableList<PinsDataTable> _thedata = new();

        public ObservableList<PinsDataTable> TheData
        {
            get => _thedata;
            set
            {
                _thedata = value;
                NotifyOfPropertyChange(() => TheData);
            }
        }

        private Visibility _progressBarVisibility = Visibility.Visible;
        public Visibility ProgressBarVisibility
        {
            get => _progressBarVisibility;
            set
            {
                _progressBarVisibility = value;
                NotifyOfPropertyChange(() => ProgressBarVisibility);
            }
        }

        private bool _verseRefDialogOpen;
        public bool VerseRefDialogOpen
        {
            get { return _verseRefDialogOpen; }
            set
            {
                _verseRefDialogOpen = value;
                NotifyOfPropertyChange(() => VerseRefDialogOpen);
            }
        }

        private ObservableCollection<PinsVerseList> _selectedItemVerses = new();
        public ObservableCollection<PinsVerseList> SelectedItemVerses
        {
            get => _selectedItemVerses;
            set
            {
                _selectedItemVerses = value;
                NotifyOfPropertyChange(() => SelectedItemVerses);
            }
        }

        private string _fontFamily = "Segoe UI";
        public string FontFamily
        {
            get => _fontFamily;
            set
            {
                _fontFamily = value;
                NotifyOfPropertyChange(() => FontFamily);
            }
        }

        private float _fontSize;
        public float FontSize
        {
            get => _fontSize;
            set
            {
                _fontSize = value;
                NotifyOfPropertyChange(() => FontSize);
            }
        }

        private bool _isRtl;
        public bool IsRtl
        {
            get => _isRtl;
            set
            {
                _isRtl = value;
                NotifyOfPropertyChange(() => IsRtl);
            }
        }

        private string _filterString = "";
        public string FilterString
        {
            get
            {
                return _filterString;
            }
            set
            {
                _filterString = value;
                NotifyOfPropertyChange(() => FilterString);

                if (TheData != null && GridCollectionView is not null)
                {
                    GridCollectionView.Refresh();
                }
            }
        }

        #endregion //Observable Properties

        #region Commands

        public RelayCommand ClearFilterCommand { get; set; }
        public RelayCommand VerseButtonCommand { get; set; }

        public ICommand VerseClickCommand { get; set; }

        #endregion //Commands


        #region Constructor

        public PinsViewModel()
        {
        }

        public PinsViewModel(INavigationService navigationService, ILogger<PinsViewModel> logger, DashboardProjectManager projectManager, IEventAggregator eventAggregator) 
            : base(navigationService, logger, projectManager, eventAggregator)
        {
            this.Title = "⍒ PINS";
            this.ContentId = "PINS";

            _eventAggregator = eventAggregator;
            _navigationService = navigationService;
            _logger = logger;
            _projectManager = projectManager;

            // wire up the commands
            ClearFilterCommand = new RelayCommand(ClearFilter);
            VerseButtonCommand = new RelayCommand(VerseButtonClick);
            VerseClickCommand = new RelayCommand(VerseClick);

            if (ProjectManager.ParatextProject is not null)
            {
                // pull out the project font family
                _fontFamily = ProjectManager.ParatextProject.Language.FontFamily;
                _fontSize = ProjectManager.ParatextProject.Language.Size;
                IsRtl = ProjectManager.ParatextProject.Language.IsRtol;
            }
        }

        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
#pragma warning disable CS4014
            // Do not await this....let it run in the background otherwise
            // it freezes the UI
            Task.Run(() =>
            {
                GenerateData().ConfigureAwait(false);
            }).ConfigureAwait(true);
#pragma warning restore CS4014

            base.OnActivateAsync(cancellationToken);
        }

        protected override async void OnViewAttached(object view, object context)
        {
            base.OnViewAttached(view, context);
        }


        protected override async void OnViewReady(object view)
        {
            base.OnViewReady(view);
        }

        protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            return base.OnInitializeAsync(cancellationToken);
        }

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            return base.OnDeactivateAsync(close, cancellationToken);
        }

        /// <summary>
        /// Main logic for building the data
        /// </summary>
        /// <returns></returns>
        private async Task<bool> GenerateData()
        {
            // load in the TermRenderings.xml file
            var queryResult =
                await ExecuteRequest(
                    new GetTermRenderingsQuery(Path.Combine(ProjectManager.CurrentDashboardProject.DirectoryPath,
                        "TermRenderings.xml")),
                    CancellationToken.None).ConfigureAwait(false);
            if (queryResult.Success == false)
            {
                Logger.LogError(queryResult.Message);
                return true;
            }

            if (queryResult.Data == null)
            {
                return true;
            }

            _termRenderingsList = queryResult.Data;


            ParatextProxy paratextUtils = new ParatextProxy(Logger as ILogger<ParatextProxy>);
            string paratextInstallPath = "";
            if (paratextUtils.IsParatextInstalled())
            {
                paratextInstallPath = paratextUtils.ParatextInstallPath;

                // load in the BiblicalTerms.xml file
                var queryBTResult =
                    await ExecuteRequest(
                        new GetBiblicalTermsQuery(Path.Combine(paratextInstallPath, @"Terms\Lists\BiblicalTerms.xml")),
                        CancellationToken.None).ConfigureAwait(false);
                if (queryBTResult.Success == false)
                {
                    Logger.LogError(queryBTResult.Message);
                    return true;
                }

                if (queryBTResult.Data == null)
                {
                    return true;
                }

                _biblicalTermsList = queryBTResult.Data;


                // load in the AllBiblicalTerms.xml file
                var queryABTResult =
                    await ExecuteRequest(
                        new GetBiblicalTermsQuery(Path.Combine(paratextInstallPath, @"Terms\Lists\AllBiblicalTerms.xml")),
                        CancellationToken.None).ConfigureAwait(false);
                if (queryABTResult.Success == false)
                {
                    Logger.LogError(queryABTResult.Message);
                    return true;
                }

                if (queryABTResult.Data == null)
                {
                    return true;
                }

                _allBiblicalTermsList = queryABTResult.Data;
            }
            else
            {
                Logger.LogError("Paratext Not Installed in PINS viewmodel");
            }


            // fix the greek renderings which are inconsistent
            for (int i = _termRenderingsList.TermRendering.Count - 1; i >= 0; i--)
            {
                if (_termRenderingsList.TermRendering[i].Renderings == "")
                {
                    // remove any records without rendering data
                    _termRenderingsList.TermRendering.RemoveAt(i);
                }
                else
                {
                    _termRenderingsList.TermRendering[i].Id =
                        CorrectUnicode(_termRenderingsList.TermRendering[i].Id);
                }
            }

            for (int i = _biblicalTermsList.Term.Count - 1; i >= 0; i--)
            {
                if (_biblicalTermsList.Term[i].Id != "")
                {
                    _biblicalTermsList.Term[i].Id =
                        CorrectUnicode(_biblicalTermsList.Term[i].Id);
                }
            }

            for (int i = _allBiblicalTermsList.Term.Count - 1; i >= 0; i--)
            {
                if (_allBiblicalTermsList.Term[i].Id != "")
                {
                    _allBiblicalTermsList.Term[i].Id =
                        CorrectUnicode(_allBiblicalTermsList.Term[i].Id);
                }
            }


            // load in the spellingstatus.xml
            var querySSResult =
                await ExecuteRequest(
                    new GetSpellingStatusQuery(Path.Combine(ProjectManager.CurrentDashboardProject.DirectoryPath,
                        "SpellingStatus.xml")), CancellationToken.None).ConfigureAwait(false);
            if (querySSResult.Success == false)
            {
                Logger.LogError(querySSResult.Message);
                return true;
            }

            if (querySSResult.Data == null)
            {
                return true;
            }

            _spellingStatus = querySSResult.Data;


            // build the data for display
            foreach (var terms in _termRenderingsList.TermRendering)
            {
                string target = terms.Renderings;
                target = target.Replace("||", "; ");

                if (target == "Naan Ɗiihai")
                {
                    Console.WriteLine();
                }


                string source = terms.Id;

                string pbtsense;
                string pbtspell;
                if (source.Contains("."))
                {
                    // Sense number uses "." in gateway language; this Sense number will not match anything in abt or bbt
                    pbtsense = source[..source.IndexOf(".")] + " {" + source[(source.IndexOf(".") + 1)..] +
                               "}"; // place Sense in braces  
                    pbtspell = source =
                        source
                            [..source.IndexOf(".")]; // remove the Sense number from word/phrase for correct matching with AllBiblicalTerms
                }
                else if (source.Contains("-")) // Sense number uses "-" in Gk & Heb, this will match bbt
                {
                    pbtsense = source.Trim().Replace("-", " {") + "}"; // place Sense in braces
                    pbtspell = source[
                        ..source.IndexOf("-")]; // remove the Sense number from word/phrase for correct matching with Spelling
                }
                else
                    pbtspell = pbtsense = source;


                // CHECK AGAINST SPELLING
                var spellingRecords = _spellingStatus.Status.FindAll(s => s.Word.ToLower() == pbtspell.ToLower());
                if (spellingRecords.Count == 0)
                {
                    pbtspell = "";
                }
                else
                {
                    if (spellingRecords[0].State == "W")
                    {
                        // misspelled
                        pbtspell = " [misspelled]";
                    }
                    else if (spellingRecords[0].SpecificCase != pbtspell)
                    {
                        //   has wrong case
                        pbtspell = " [" + spellingRecords[0].SpecificCase + "] wrong case";
                    }
                    else
                    {
                        pbtspell = "";
                    }
                }

                // peel off the notes
                var notes = terms.Notes;
                string noteList = "";
                if (notes.GetType() == typeof(XmlNode[]))
                {
                    var listNotes = ((IEnumerable)notes).Cast<XmlNode>().ToList();
                    foreach (var note in listNotes)
                    {
                        noteList += note.Value.Replace("\n", "").Replace("\r", "") + "; ";
                    }
                }

                var denials = terms.Denials;
                var gloss = "";
                List<string> verselist = new List<string>();

                // check against the BiblicalTermsList
                var bt = _biblicalTermsList.Term.FindAll(t => t.Id == source);
                if (bt.Count > 0)
                {
                    gloss = bt[0].Gloss;

                    foreach (var verse in bt[0].References.Verse)
                    {
                        verselist.Add(verse);
                    }

                    _thedata.Add(new PinsDataTable
                    {
                        Id = Guid.NewGuid(),
                        XmlSource = "BT",
                        Code = "KeyTerm",
                        OriginID = terms.Id,
                        Gloss = gloss,
                        Lang = "",
                        Lform = "",
                        Match = "KeyTerm" + target,
                        Notes = noteList,
                        Phrase = "",
                        Prefix = "",
                        Refs = "",
                        SimpRefs = verselist.Count.ToString(),
                        Source = target + pbtspell,
                        Stem = "",
                        Suffix = "",
                        Word = "",
                        VerseList = verselist
                    });
                }
                else
                {
                    // now check AllBiblicalTerms
                    var abt = _allBiblicalTermsList.Term.FindAll(t => t.Id == source);
                    if (abt.Count > 0)
                    {
                        gloss = abt[0].Gloss;

                        foreach (var verse in abt[0].References.Verse)
                        {
                            verselist.Add(verse);
                        }

                        string simpleRefs = string.Join(", ", verselist);

                        _thedata.Add(new PinsDataTable
                        {
                            Id = Guid.NewGuid(),
                            XmlSource = "ABT",
                            Code = "KeyTerm",
                            OriginID = terms.Id,
                            Gloss = gloss,
                            Lang = "",
                            Lform = "",
                            Match = "KeyTerm" + target,
                            Notes = noteList,
                            Phrase = "",
                            Prefix = "",
                            Refs = "",
                            SimpRefs = verselist.Count.ToString(),
                            Source = target + pbtspell,
                            Stem = "",
                            Suffix = "",
                            Word = "",
                            VerseList = verselist
                        });
                    }
                    else
                    {
                        if (pbtsense == "")
                        {
                            gloss = source;
                        }
                        else
                        {
                            gloss = pbtsense;
                        }

                        _thedata.Add(new PinsDataTable
                        {
                            Id = Guid.NewGuid(),
                            XmlSource = "TR",
                            Code = "KeyTerm",
                            OriginID = terms.Id,
                            Gloss = gloss,
                            Lang = "",
                            Lform = "",
                            Match = "KeyTerm" + target,
                            Notes = noteList,
                            Phrase = "",
                            Prefix = "",
                            Refs = "",
                            SimpRefs = "0",
                            Source = target + pbtspell,
                            Stem = "",
                            Suffix = "",
                            Word = "",
                        });
                    }
                }
            }


            // load in the lexicon.xml
            var queryLexiconResult =
                await ExecuteRequest(
                    new GetLexiconQuery(Path.Combine(ProjectManager.CurrentDashboardProject.DirectoryPath, "Lexicon.xml")),
                    CancellationToken.None).ConfigureAwait(false);
            if (queryLexiconResult.Success == false)
            {
                Logger.LogError(queryLexiconResult.Message);
                return true;
            }

            if (queryLexiconResult.Data == null)
            {
                return true;
            }

            _lexicon = queryLexiconResult.Data;


            for (int i = 0; i < _lexicon.Entries.Item.Count; i++)
            {
                var entry = _lexicon.Entries.Item[i];
                foreach (var senseEntry in entry.Entry.Sense)
                {
                    _thedata.Add(new PinsDataTable
                    {
                        Id = Guid.NewGuid(),
                        XmlSource = "LX",
                        Code = senseEntry.Id,
                        Gloss = senseEntry.Gloss.Text,
                        Lang = senseEntry.Gloss.Language,
                        Lform = entry.Lexeme.Type,
                        Match = senseEntry.Id + entry.Lexeme.Form,
                        Notes = "",
                        Phrase = (entry.Lexeme.Type == "Phrase") ? "Phr" : "",
                        Prefix = (entry.Lexeme.Type == "Prefix") ? "pre-" : "",
                        Refs = "",
                        SimpRefs = "0",
                        Source = entry.Lexeme.Form,
                        Stem = (entry.Lexeme.Type == "Stem") ? "Stem" : "",
                        Suffix = (entry.Lexeme.Type == "Suffix") ? "-suf" : "",
                        Word = (entry.Lexeme.Type == "Word") ? "Wrd" : "",
                    });
                }
            }


            GridCollectionView = CollectionViewSource.GetDefaultView(TheData);
            GridCollectionView.Filter = new Predicate<object>(FiterTerms);
            NotifyOfPropertyChange(() => GridCollectionView);

            // turn off the progress bar
            ProgressBarVisibility = Visibility.Collapsed;
            return false;
        }

        protected override async void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
        }

        #endregion //Constructor

        #region Methods

        private string CorrectUnicode(string instr)
        {
            // There is a problem in Gk Unicode Vowels exhibiting in Paratext. See https://wiki.digitalclassicist.org/Greek_Unicode_duplicated_vowels
            // The basic code is preferred by the Unicode Consortium
            // AllBiblicalTerms.xml and Biblical Terms.xml tend to use the Extended codes while TermRenderings.xml tends to use the Basic codes
            //  Unicode Basic   Extend
            instr = instr.Replace('ά', 'ά')     //  ά 	    03AC 	1F71
                .Replace('έ', 'έ')     //  έ 	    03AD 	1F73
                .Replace('ή', 'ή')     //  ή 	    03AE 	1F75
                .Replace('ί', 'ί')     //  ί 	    03AF 	1F77
                .Replace('ό', 'ό')     //  ό 	    03CC 	1F79
                .Replace('ύ', 'ύ')     //  ύ 	    03CD 	1F7B
                .Replace('ώ', 'ώ')     //  ώ 	    03CE 	1F7D
                .Replace('Ά', 'Ά')     //  Ά 	    0386 	1FBB
                .Replace('Έ', 'Έ')     //  Έ 	    0388 	1FC9
                .Replace('Ή', 'Ή')     //  Ή 	    0389 	1FCB
                .Replace('Ί', 'Ί')     //  Ί 	    038A 	1FDB
                .Replace('Ό', 'Ό')     //  Ό 	    038C 	1FF9
                .Replace('Ύ', 'Ύ')     //  Ύ 	    038E 	1FEB
                .Replace('Ώ', 'Ώ')     //  Ώ 	    038F 	1FFB
                .Replace('ΐ', 'ΐ')     //  ΐ 	    0390 	1FD3
                .Replace('ΰ', 'ΰ');    //  ΰ 	    03B0 	1FE3
            return instr;
        }

        public ICollectionView GridCollectionView { get; set; }



        private void ClearFilter(object obj)
        {
            FilterString = "";
        }

        private bool FiterTerms(object item)
        {
            PinsDataTable? itemDT = item as PinsDataTable;

            return (itemDT.Source.Contains(_filterString) || itemDT.Gloss.Contains(_filterString) ||
                    itemDT.Notes.Contains(_filterString));
        }

        private void VerseButtonClick(object obj)
        {
            if (obj is PinsDataTable)
            {
                var dataRow = (PinsDataTable)obj;

                if (dataRow.VerseList.Count == 0)
                {
                    return;
                }

                SelectedItemVerses.Clear();

                dataRow.VerseList.Sort();


                List<VersificationList> verseList = new List<VersificationList>();
                foreach (var verse in dataRow.VerseList)
                {
                    verseList.Add(new VersificationList
                    {
                        SourceBBBCCCVV = verse.Substring(0, 9),
                        TargetBBBCCCVV = "",
                    });
                }

                // this data has versification from the org.vrs
                // convert it over to the current project versification format.
                verseList = Helpers.Versification.GetVersificationFromOriginal(verseList, _projectManager.ParatextProject);

                foreach (var verse in verseList)
                {
                    string verseIdShort = BibleRefUtils.GetVerseStrShortFromBBBCCCVVV(verse.TargetBBBCCCVV);

                    _selectedItemVerses.Add(new PinsVerseList
                    {
                        BBBCCCVVV = verse.TargetBBBCCCVV,
                        VerseIdShort = verseIdShort,
                        VerseText = "SOME VERSE TEXT TO LOOKUP ONCE THE DB IS COMPLETE"
                    });
                }
                NotifyOfPropertyChange(() => SelectedItemVerses);
                VerseRefDialogOpen = true;
            }
        }

        /// <summary>
        /// User has clicked on a verse link.  The VersePopUp window comes up with
        /// all the various verse renderings
        /// </summary>
        /// <param name="obj"></param>
        private void VerseClick(object obj)
        {
            if (obj is null)
            {
                return;
            }

            var verseBBCCCVVV = (string)obj;
            var verses = SelectedItemVerses.Where(v => v.BBBCCCVVV.Equals(verseBBCCCVVV)).ToList();

            if (verses.Count > 0)
            {
                IWindowManager manager = new WindowManager();
                manager.ShowWindowAsync(
                    new VersePopUpViewModel(NavigationService, Logger, ProjectManager, EventAggregator,
                        verses[0]), null, null);
            }
        }

        #endregion // Methods
    }

}
