using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.ViewModels.Panes;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml;
using System.Xml.Serialization;
using ClearDashboard.DAL.ViewModels;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Models.Common;
using ClearDashboard.DataAccessLayer.Models.Helpers;
using ClearDashboard.DataAccessLayer.Paratext;
using ClearDashboard.Wpf.Helpers;
using SIL.ObjectModel;

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

        private DashboardProjectManager _projectManager;
        private ILogger<PinsViewModel> _logger;
        private INavigationService _navigationService;
        private IEventAggregator _eventAggregator;

        #endregion //Member Variables

        #region Public Properties


        #endregion //Public Properties

        #region Observable Properties

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

        protected override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            Debug.WriteLine("");
            return base.OnActivateAsync(cancellationToken);
        }

        protected override async void OnViewAttached(object view, object context)
        {
            base.OnViewAttached(view, context);
        }


        protected override async void OnViewReady(object view)
        {
            // load in the TermRenderings.xml file
            await Task.Run(() =>
            {
                string xmlPath = Path.Combine(ProjectManager.CurrentDashboardProject.DirectoryPath,
                    "TermRenderings.xml");

                if (File.Exists(xmlPath))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(xmlPath);
                    XmlNodeReader reader = new XmlNodeReader(doc);

                    using (reader)
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(TermRenderingsList));
                        try
                        {
                            _termRenderingsList = (TermRenderingsList)serializer.Deserialize(reader);
                        }
                        catch (Exception e)
                        {
                            Logger.LogError("Error in PINS deserialization of TermRenderings.xml: " + e.Message);
                        }
                    }
                }
                else
                {
                    Logger.LogError("Missing file in PINS viewmodel: " + xmlPath);
                }
            }).ConfigureAwait(false);



            ParatextProxy paratextUtils = new ParatextProxy(Logger as ILogger<ParatextProxy>);
            string paratextInstallPath = "";
            if (paratextUtils.IsParatextInstalled())
            {
                paratextInstallPath = paratextUtils.ParatextInstallPath;

                // load in the BiblicalTerms.xml file
                await Task.Run(() =>
                {
                    string xmlPath = Path.Combine(paratextInstallPath, @"Terms\Lists\BiblicalTerms.xml");

                    if (File.Exists(xmlPath))
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.Load(xmlPath);
                        XmlNodeReader reader = new XmlNodeReader(doc);

                        using (reader)
                        {
                            XmlSerializer serializer = new XmlSerializer(typeof(BiblicalTermsList));
                            try
                            {
                                _biblicalTermsList = (BiblicalTermsList)serializer.Deserialize(reader);
                            }
                            catch (Exception e)
                            {
                                Logger.LogError("Error in PINS deserialization of BibilicalTerms.xml: " + e.Message);
                            }

                        }
                    }
                    else
                    {
                        Logger.LogError("Missing file in PINS viewmodel: " + xmlPath);
                    }
                }).ConfigureAwait(false);


                // load in the AllBiblicalTerms.xml file
                await Task.Run(() =>
                {
                    string xmlPath = Path.Combine(paratextInstallPath, @"Terms\Lists\AllBiblicalTerms.xml");

                    if (File.Exists(xmlPath))
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.Load(xmlPath);
                        XmlNodeReader reader = new XmlNodeReader(doc);

                        using (reader)
                        {
                            XmlSerializer serializer = new XmlSerializer(typeof(BiblicalTermsList));
                            try
                            {
                                _allBiblicalTermsList = (BiblicalTermsList)serializer.Deserialize(reader);
                            }
                            catch (Exception e)
                            {
                                Logger.LogError("Error in PINS deserialization of AllBibilicalTerms.xml: " + e.Message);
                            }
                        }
                    }
                    else
                    {
                        Logger.LogError("Missing file in PINS viewmodel: " + xmlPath);
                    }
                }).ConfigureAwait(false);
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
            await Task.Run(() =>
            {
                string xmlPath = Path.Combine(ProjectManager.CurrentDashboardProject.DirectoryPath,
                    "SpellingStatus.xml");

                if (File.Exists(xmlPath))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(xmlPath);
                    XmlNodeReader reader = new XmlNodeReader(doc);

                    using (reader)
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(SpellingStatus));
                        try
                        {
                            _spellingStatus = (SpellingStatus)serializer.Deserialize(reader);
                        }
                        catch (Exception e)
                        {
                            Logger.LogError("Error in PINS deserialization of AllBibilicalTerms.xml: " + e.Message);
                        }
                    }
                }
                else
                {
                    Logger.LogError("Missing file in PINS viewmodel: " + xmlPath);
                }
            }).ConfigureAwait(false);

            Console.WriteLine();

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
                {                                                                                                               // Sense number uses "." in gateway language; this Sense number will not match anything in abt or bbt
                    pbtsense = source[..source.IndexOf(".")] + " {" + source[(source.IndexOf(".") + 1)..] + "}";    // place Sense in braces  
                    pbtspell = source = source[..source.IndexOf(".")];                                                 // remove the Sense number from word/phrase for correct matching with AllBiblicalTerms
                }
                else if (source.Contains("-"))                                                                               // Sense number uses "-" in Gk & Heb, this will match bbt
                {
                    pbtsense = source.Trim().Replace("-", " {") + "}";                                                       // place Sense in braces
                    pbtspell = source[..source.IndexOf("-")];                                                             // remove the Sense number from word/phrase for correct matching with Spelling
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
            await Task.Run(() =>
            {
                string xmlPath = Path.Combine(ProjectManager.CurrentDashboardProject.DirectoryPath,
                    "Lexicon.xml");

                if (File.Exists(xmlPath))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(xmlPath);
                    XmlNodeReader reader = new XmlNodeReader(doc);

                    using (reader)
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(Lexicon));
                        try
                        {
                            _lexicon = (Lexicon)serializer.Deserialize(reader);
                        }
                        catch (Exception e)
                        {
                            Logger.LogError("Error in PINS deserialization of Lexicon.xml: " + e.Message);
                        }
                    }
                }
                else
                {
                    Logger.LogError("Missing file in PINS viewmodel: " + xmlPath);
                }
            }).ConfigureAwait(false);

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

            base.OnViewReady(view);
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





    [XmlRoot(ElementName = "TermRendering")]
    public class TermRendering
    {

        [XmlElement(ElementName = "Changes")]
        public Changes Changes { get; set; }

        [XmlElement(ElementName = "Notes")]
        public object Notes { get; set; }

        [XmlElement(ElementName = "Denials")]
        public Denials Denials { get; set; }

        [XmlAttribute(AttributeName = "Id")]
        public string Id { get; set; }

        [XmlAttribute(AttributeName = "Guess")]
        public bool Guess { get; set; }

        [XmlText]
        public string Text { get; set; }

        [XmlElement(ElementName = "Renderings")]
        public string Renderings { get; set; }

        [XmlElement(ElementName = "Glossary")]
        public object Glossary { get; set; }
    }

    [XmlRoot(ElementName = "Change")]
    public class Change
    {

        [XmlElement(ElementName = "Before")]
        public object Before { get; set; }

        [XmlElement(ElementName = "After")]
        public string After { get; set; }

        [XmlAttribute(AttributeName = "UserName")]
        public string UserName { get; set; }

        [XmlAttribute(AttributeName = "TermId")]
        public string TermId { get; set; }

        [XmlAttribute(AttributeName = "Date")]
        public DateTime Date { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "Changes")]
    public class Changes
    {

        [XmlElement(ElementName = "Change")]
        public List<Change> Change { get; set; }
    }

    [XmlRoot(ElementName = "Denials")]
    public class Denials
    {

        [XmlElement(ElementName = "Denial")]
        public List<int> Denial { get; set; }
    }

    [XmlRoot(ElementName = "TermRenderingsList")]
    public class TermRenderingsList
    {

        [XmlElement(ElementName = "TermRendering")]
        public List<TermRendering> TermRendering { get; set; }
    }








    // using System.Xml.Serialization;
    // XmlSerializer serializer = new XmlSerializer(typeof(BiblicalTermsList));
    // using (StringReader reader = new StringReader(xml))
    // {
    //    var test = (BiblicalTermsList)serializer.Deserialize(reader);
    // }

    [XmlRoot(ElementName = "References")]
    public class References
    {

        [XmlElement(ElementName = "Verse")]
        public List<string> Verse { get; set; }
    }

    [XmlRoot(ElementName = "Term")]
    public class Term
    {
        [XmlElement(ElementName = "Strong")]
        public string Strong { get; set; }

        [XmlElement(ElementName = "Transliteration")]
        public string Transliteration { get; set; }

        [XmlElement(ElementName = "Category")]
        public string Category { get; set; }

        [XmlElement(ElementName = "Domain")]
        public string Domain { get; set; }

        [XmlElement(ElementName = "Language")]
        public string Language { get; set; }

        [XmlElement(ElementName = "Definition")]
        public string Definition { get; set; }

        [XmlElement(ElementName = "Gloss")]
        public string Gloss { get; set; }

        [XmlElement(ElementName = "References")]
        public References References { get; set; }

        [XmlAttribute(AttributeName = "Id")]
        public string Id { get; set; }

        [XmlText]
        public string Text { get; set; }

        [XmlElement(ElementName = "Link")]
        public string Link { get; set; }
    }

    [XmlRoot(ElementName = "BiblicalTermsList")]
    public class BiblicalTermsList
    {

        [XmlElement(ElementName = "Term")]
        public List<Term> Term { get; set; }

        [XmlAttribute(AttributeName = "xsi")]
        public string Xsi { get; set; }

        [XmlAttribute(AttributeName = "xsd")]
        public string Xsd { get; set; }

        [XmlText]
        public string Text { get; set; }
    }




    // using System.Xml.Serialization;
    // XmlSerializer serializer = new XmlSerializer(typeof(SpellingStatus));
    // using (StringReader reader = new StringReader(xml))
    // {
    //    var test = (SpellingStatus)serializer.Deserialize(reader);
    // }

    [XmlRoot(ElementName = "Status")]
    public class Status
    {

        [XmlAttribute(AttributeName = "Word")]
        public string Word { get; set; }

        [XmlAttribute(AttributeName = "State")]
        public string State { get; set; }

        [XmlElement(ElementName = "SpecificCase")]
        public string SpecificCase { get; set; }

        [XmlText]
        public string Text { get; set; }

        [XmlElement(ElementName = "Correction")]
        public string Correction { get; set; }
    }

    [XmlRoot(ElementName = "SpellingStatus")]
    public class SpellingStatus
    {

        [XmlElement(ElementName = "Status")]
        public List<Status> Status { get; set; }
    }






    // using System.Xml.Serialization;
    // XmlSerializer serializer = new XmlSerializer(typeof(Lexicon));
    // using (StringReader reader = new StringReader(xml))
    // {
    //    var test = (Lexicon)serializer.Deserialize(reader);
    // }

    [XmlRoot(ElementName = "Lexeme")]
    public class Lexeme
    {

        [XmlAttribute(AttributeName = "Type")]
        public string Type { get; set; }

        [XmlAttribute(AttributeName = "Form")]
        public string Form { get; set; }

        [XmlAttribute(AttributeName = "Homograph")]
        public int Homograph { get; set; }
    }

    [XmlRoot(ElementName = "Gloss")]
    public class Gloss
    {

        [XmlAttribute(AttributeName = "Language")]
        public string Language { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "Sense")]
    public class Sense
    {

        [XmlElement(ElementName = "Gloss")]
        public Gloss Gloss { get; set; }

        [XmlAttribute(AttributeName = "Id")]
        public string Id { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "Entry")]
    public class Entry
    {

        [XmlElement(ElementName = "Sense")]
        public List<Sense> Sense { get; set; }
    }

    [XmlRoot(ElementName = "item")]
    public class Item
    {

        [XmlElement(ElementName = "Lexeme")]
        public Lexeme Lexeme { get; set; }

        [XmlElement(ElementName = "Entry")]
        public Entry Entry { get; set; }
    }

    [XmlRoot(ElementName = "Entries")]
    public class Entries
    {

        [XmlElement(ElementName = "item")]
        public List<Item> Item { get; set; }
    }

    [XmlRoot(ElementName = "Lexicon")]
    public class Lexicon
    {

        [XmlElement(ElementName = "Language")]
        public string Language { get; set; }

        [XmlElement(ElementName = "FontName")]
        public string FontName { get; set; }

        [XmlElement(ElementName = "FontSize")]
        public int FontSize { get; set; }

        [XmlElement(ElementName = "Analyses")]
        public object Analyses { get; set; }

        [XmlElement(ElementName = "Entries")]
        public Entries Entries { get; set; }
    }



}
