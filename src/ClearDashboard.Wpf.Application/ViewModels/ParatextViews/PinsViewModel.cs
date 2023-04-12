using Autofac;
using Caliburn.Micro;
using ClearDashboard.DAL.ViewModels;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.DataAccessLayer.Features.PINS;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Paratext;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.ParatextPlugin.CQRS.Features.Verse;
using ClearDashboard.ParatextPlugin.CQRS.Features.VerseText;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Messages;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.Panes;
using ClearDashboard.Wpf.Application.Views.ParatextViews;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml;
using ClearApplicationFoundation.Framework.Input;

namespace ClearDashboard.Wpf.Application.ViewModels.ParatextViews
{
    public class PinsViewModel : ToolViewModel, IHandle<FilterPinsMessage>
    {

        #region Member Variables
        public Dictionary<string, string> LexMatRef = new();
        public string[] BibleBooks = { "01GEN", "02EXO", "03LEV", "04NUM", "05DEU", "06JOS", "07JDG", "08RUT", "091SA", "102SA", "111KI", "122KI", "131CH", "142CH", "15EZR", "16NEH", "17EST", "18JOB", "19PSA", "20PRO", "21ECC", "22SNG", "23ISA", "24JER", "25LAM", "26EZK", "27DAN", "28HOS", "29JOL", "30AMO", "31OBA", "32JON", "33MIC", "34NAM", "35HAB", "36ZEP", "37HAG", "38ZEC", "39MAL", "41MAT", "42MRK", "43LUK", "44JHN", "45ACT", "46ROM", "471CO", "482COR", "49GAL", "50EPH", "51PHP", "52COL", "531TH", "542TH", "551TI", "562TI", "57TIT", "58PHM", "59HEB", "60JAS", "611PE", "622PE", "631JN", "642JN", "653JN", "66JUD", "67REV", "70TOB", "71JDT", "72ESG", "73WIS", "74SIR", "75BAR", "76LJE", "77S3Y", "78SUS", "79BEL", "80MAN", "81PS2" };
        public Dictionary<string, string> BibleBookDict;
        public string[] projectBookFileData;
        public string ProjectDir;
        private TermRenderingsList _termRenderingsList = new();
        private BiblicalTermsList _biblicalTermsList = new();
        private BiblicalTermsList _allBiblicalTermsList = new();
        private SpellingStatus _spellingStatus = new();
        private DataAccessLayer.Models.Lexicon _lexicon = new();
        private string _paratextInstallPath = "";

        private bool _generateDataRunning = false;
      //  private CancellationTokenSource _cancellationTokenSource;
        private string _taskName = "PINS";

        private readonly ILogger<PinsViewModel> _logger;
        private readonly DashboardProjectManager? _projectManager;
        private readonly IMediator _mediator;
        private readonly LongRunningTaskManager _longRunningTaskManager;

        private BindableCollection<PinsDataTable> GridData { get; } = new();

        #endregion //Member Variables

        #region Public Properties


        #endregion //Public Properties

        #region Observable Properties

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
            get => _verseRefDialogOpen;
            set
            {
                _verseRefDialogOpen = value;
                NotifyOfPropertyChange(() => VerseRefDialogOpen);
            }
        }

        public ObservableCollection<PinsVerseList> SelectedItemVerses { get; } = new();

        public string FontFamily { get; set; } = FontNames.DefaultFontFamily;

        public float FontSize { get; } = 12;

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
            get => _filterString;
            set
            {
                value ??= string.Empty;

                _filterString = value;
                NotifyOfPropertyChange(() => FilterString);

                if (GridData != null && GridCollectionView is not null)
                {
                    GridCollectionView.Refresh();
                }
            }
        }

        public ICollectionView GridCollectionView { get; set; }

        #endregion //Observable Properties

        #region Commands

        public RelayCommand ClearFilterCommand { get; }
        public RelayCommand VerseButtonCommand { get; }

        public ICommand VerseClickCommand { get; set; }

        #endregion //Commands


        #region Constructor

        // ReSharper disable once UnusedMember.Global
        public PinsViewModel()
        {
        }

        // ReSharper disable once UnusedMember.Global
        public PinsViewModel(INavigationService navigationService, ILogger<PinsViewModel> logger,
            DashboardProjectManager? projectManager, IEventAggregator? eventAggregator, IMediator mediator, ILifetimeScope lifetimeScope, LongRunningTaskManager longRunningTaskManager, ILocalizationService localizationService)
            : base(navigationService, logger, projectManager, eventAggregator, mediator, lifetimeScope,localizationService)
        {
            Title = "⍒ " + LocalizationService!.Get("Windows_PINS");
            this.ContentId = "PINS";

            _logger = logger;
            _projectManager = projectManager;
            _mediator = mediator;
            _longRunningTaskManager = longRunningTaskManager;
            
            // wire up the commands
            ClearFilterCommand = new RelayCommand(ClearFilter);
            VerseButtonCommand = new RelayCommand(VerseButtonClick);
            VerseClickCommand = new RelayCommand(VerseClick);


            // pull out the project font family
            if (ProjectManager!.CurrentParatextProject is not null)
            {
                var paratextProject = ProjectManager.CurrentParatextProject;
                FontFamily = paratextProject.Language.FontFamily;
                FontSize = paratextProject.Language.Size;
                IsRtl = paratextProject.Language.IsRtol;
            }
        }

        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
#pragma warning disable CS4014
            // Do not await this....let it run in the background otherwise
            // it freezes the UI

            // send to the task started event aggregator for everyone else to hear about a background task starting
            await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
            {
                Name = _taskName,
                Description = "Loading PINS data...",
                StartTime = DateTime.Now,
                TaskLongRunningProcessStatus = LongRunningTaskStatus.Running
            }), cancellationToken);

            // ReSharper disable once MethodSupportsCancellation
            _ = Task.Run(async () =>
            {
                await GenerateData().ConfigureAwait(false);
            }).ConfigureAwait(true);
#pragma warning restore CS4014

            _ = base.OnActivateAsync(cancellationToken);
        }

        protected override async  Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
          
            //we need to cancel this process here
            //check a bool to see if it already cancelled or already completed
            if (_generateDataRunning)
            {
                var cancelled = _longRunningTaskManager.CancelTask(_taskName);
                
                await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
                {
                    Name = _taskName,
                    Description = "Task was cancelled",
                    EndTime = DateTime.Now,
                    TaskLongRunningProcessStatus = LongRunningTaskStatus.Completed
                }), cancellationToken);
            }

            EventAggregator?.Unsubscribe(this);
            await base.OnDeactivateAsync(close, cancellationToken);
        }


        /// <summary>
        /// Main logic for building the data
        /// </summary>
        /// <returns></returns>
        private async Task<bool> GenerateData()
        {
            _generateDataRunning = true;
            
            var longRunningTask = _longRunningTaskManager.Create(_taskName, LongRunningTaskStatus.Running);
            var cancellationToken = longRunningTask.CancellationTokenSource.Token;

            try
            {
                var logger = LifetimeScope.Resolve<ILogger<ParatextProxy>>();
                ParatextProxy paratextUtils = new ParatextProxy(logger);
                if (paratextUtils.IsParatextInstalled())
                {
                    _paratextInstallPath = paratextUtils.ParatextInstallPath;

                    //run getting and deserializing all of these resources in parallel
                    await Task.WhenAll(
                        GetTermRenderings(),
                        GetBiblicalTerms(_paratextInstallPath),
                        GetAllBiblicalTerms(_paratextInstallPath),
                        GetSpellingStatus(),
                        GetLexicon());
                }
                else
                {
                    // send to the task started event aggregator for everyone else to hear about a task error
                    await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(
                        new BackgroundTaskStatus
                        {
                            Name = _taskName,
                            EndTime = DateTime.Now,
                            ErrorMessage = "Paratext is not installed",
                            TaskLongRunningProcessStatus = LongRunningTaskStatus.Failed
                        }), cancellationToken);

                    _logger!.LogError("Paratext Not Installed in PINS viewmodel");

                    // turn off the progress bar
                    ProgressBarVisibility = Visibility.Collapsed;
                    return false;
                }


                // fix the greek renderings which are inconsistent
                for (var i = _termRenderingsList.TermRendering.Count - 1; i >= 0; i--)
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

                    cancellationToken.ThrowIfCancellationRequested();
                }

                if (_biblicalTermsList.Term != null)
                {
                    for (var i = _biblicalTermsList.Term.Count - 1; i >= 0; i--)
                    {
                        if (_biblicalTermsList.Term[i].Id != "")
                        {
                            _biblicalTermsList.Term[i].Id =
                                CorrectUnicode(_biblicalTermsList.Term[i].Id);
                        }

                        cancellationToken.ThrowIfCancellationRequested();
                    }
                }

                if (_allBiblicalTermsList.Term != null)
                {
                    for (var i = _allBiblicalTermsList.Term.Count - 1; i >= 0; i--)
                    {
                        if (_allBiblicalTermsList.Term[i].Id != "")
                        {
                            _allBiblicalTermsList.Term[i].Id =
                                CorrectUnicode(_allBiblicalTermsList.Term[i].Id);
                        }

                        cancellationToken.ThrowIfCancellationRequested();
                    }
                }


                // build the data for display
                foreach (var terms in _termRenderingsList.TermRendering)
                {
                    var targetRendering = terms.Renderings;
                    targetRendering = targetRendering.Replace("||", "; ");

                    var sourceWord = terms.Id;

                    string biblicalTermsSense;
                    string biblicalTermsSpelling;
                    if (sourceWord.Contains("."))
                    {
                        // Sense number uses "." in gateway language; this Sense number will not match anything in abt or bbt
                        // place Sense in braces  
                        biblicalTermsSense = sourceWord[..sourceWord.IndexOf(".", StringComparison.Ordinal)] + " {" +
                                             sourceWord[(sourceWord.IndexOf(".", StringComparison.Ordinal) + 1)..] +
                                             "}";

                        // remove the Sense number from word/phrase for correct matching with AllBiblicalTerms
                        biblicalTermsSpelling =
                            sourceWord = sourceWord[..sourceWord.IndexOf(".", StringComparison.Ordinal)];
                    }
                    else if (sourceWord.Contains("-")) // Sense number uses "-" in Gk & Heb, this will match bbt
                    {
                        // place Sense in braces
                        biblicalTermsSense = sourceWord.Trim().Replace("-", " {") + "}";

                        // remove the Sense number from word/phrase for correct matching with Spelling
                        biblicalTermsSpelling = sourceWord[..sourceWord.IndexOf("-", StringComparison.Ordinal)];
                    }
                    else
                    {
                        biblicalTermsSpelling = biblicalTermsSense = sourceWord;
                    }
                    
                    // CHECK AGAINST SPELLING
                    List<Status> spellingRecords = new();
                    try
                    {
                       spellingRecords = _spellingStatus.Status?.FindAll(s => string.Equals(s.Word,
                            biblicalTermsSpelling, StringComparison.OrdinalIgnoreCase));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                    if (spellingRecords is null || spellingRecords.Count == 0)
                    {
                        biblicalTermsSpelling = "";
                    }
                    else
                    {
                        if (spellingRecords[0].State == "W") // apparently a "W" means misspelled
                        {
                            // misspelled
                            biblicalTermsSpelling = " [misspelled]";
                        }
                        else if (spellingRecords[0].SpecificCase != biblicalTermsSpelling)
                        {
                            // has wrong upper/lower case
                            biblicalTermsSpelling = " [" + spellingRecords[0].SpecificCase + "] wrong case";
                        }
                        else
                        {
                            biblicalTermsSpelling = "";
                        }
                    }

                    // peel off the notes
                    var notes = terms.Notes;
                    var noteList = "";
                    if (notes.GetType() == typeof(XmlNode[]))
                    {
                        // convert a list of xmlnodes to at List<>
                        var listNotes = ((IEnumerable)notes).Cast<XmlNode>().ToList();
                        foreach (var note in listNotes)
                        {
                            // append all the notes together and clean up the formatting
                            noteList += note.Value.Replace("\n", "").Replace("\r", "") + "; ";
                        }

                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    var verseList = new List<string>();

                    // check against the BiblicalTermsList
                    var bt = _biblicalTermsList.Term.FindAll(t => t.Id == sourceWord);
                    var gloss = "";
                    if (bt.Count > 0)
                    {
                        gloss = bt[0].Gloss;

                        // BiblicalTerms contains the verses associated with the TermRenderings
                        foreach (var verse in bt[0].References.Verse)
                        {
                            verseList.Add(verse);
                            cancellationToken.ThrowIfCancellationRequested();
                        }

                        GridData.Add(new PinsDataTable
                        {
                            Id = Guid.NewGuid(),
                            XmlSource = "BT",
                            XmlPath = Path.Combine(_paratextInstallPath, @"Terms\Lists\BiblicalTerms.xml"),
                            Code = "KeyTerm",
                            OriginID = terms.Id,
                            Gloss = gloss,
                            Lang = "",
                            Lform = "",
                            Match = "KeyTerm" + targetRendering,
                            Notes = noteList,
                            Phrase = "",
                            Prefix = "",
                            Refs = "",
                            SimpRefs = verseList.Count.ToString(),
                            Source = targetRendering + biblicalTermsSpelling,
                            Stem = "",
                            Suffix = "",
                            Word = "",
                            VerseList = verseList
                        });
                    }
                    else
                    {
                        // if not found in the Biblical Terms list, now check AllBiblicalTerms second
                        var abt = _allBiblicalTermsList.Term.FindAll(t => t.Id == sourceWord);
                        if (abt.Count > 0)
                        {
                            gloss = abt[0].Gloss;

                            // AllBiblicalTerms contains the verses associated with the TermRenderings
                            foreach (var verse in abt[0].References.Verse)
                            {
                                verseList.Add(verse);
                                cancellationToken.ThrowIfCancellationRequested();
                            }

                            GridData.Add(new PinsDataTable
                            {
                                Id = Guid.NewGuid(),
                                XmlSource = "ABT",
                                XmlPath = Path.Combine(_paratextInstallPath, @"Terms\Lists\AllBiblicalTerms.xml"),
                                Code = "KeyTerm",
                                OriginID = terms.Id,
                                Gloss = gloss,
                                Lang = "",
                                Lform = "",
                                Match = "KeyTerm" + targetRendering,
                                Notes = noteList,
                                Phrase = "",
                                Prefix = "",
                                Refs = "",
                                SimpRefs = verseList.Count.ToString(),
                                Source = targetRendering + biblicalTermsSpelling,
                                Stem = "",
                                Suffix = "",
                                Word = "",
                                VerseList = verseList
                            });
                        }
                        else
                        {
                            // not found in either the BiblicalTerms or AllBiblicalTerms.xml lists
                            gloss = biblicalTermsSense == "" ? sourceWord : biblicalTermsSense;

                            Dispatcher.CurrentDispatcher.Invoke(() =>
                            {
                                GridData.Add(new PinsDataTable
                                {
                                    Id = Guid.NewGuid(),
                                    XmlSource = "TR",
                                    XmlPath = Path.Combine(_projectManager.CurrentParatextProject?.DirectoryPath, "TermRenderings.xml"),
                                    Code = "KeyTerm",
                                    OriginID = terms.Id,
                                    Gloss = gloss,
                                    Lang = "",
                                    Lform = "",
                                    Match = "KeyTerm" + targetRendering,
                                    Notes = noteList,
                                    Phrase = "",
                                    Prefix = "",
                                    Refs = "",
                                    SimpRefs = "0",
                                    Source = targetRendering + biblicalTermsSpelling,
                                    Stem = "",
                                    Suffix = "",
                                    Word = "",
                                });
                            });
                          
                        }
                    }

                    cancellationToken.ThrowIfCancellationRequested();
                }


                if (_lexicon.Entries != null)
                {
                    // populate the data grid
                    foreach (var entry in _lexicon.Entries.Item)
                    {
                        foreach (var senseEntry in entry.Entry.Sense)
                        {
                            GridData.Add(new PinsDataTable
                            {
                                Id = Guid.NewGuid(),
                                XmlSource = "LX",
                                XmlPath = Path.Combine(_projectManager.CurrentParatextProject.DirectoryPath, "Lexicon.xml"),
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
                            cancellationToken.ThrowIfCancellationRequested();
                        }
                    }
                }


                if (GridData.Count > 0)
                {
                    BibleBookDict = BibleBooks.ToDictionary(item => item[2..5], item => item[..2]);
                    var reg = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Wow6432Node\\Paratext\\8");
                    ProjectDir = (reg.GetValue("Settings_Directory") ?? "").ToString();
                    var bookfiles = Directory.GetFiles(ProjectDir, "Interlinear_*.xml", SearchOption.AllDirectories)
                        .ToList();
                    string tref, lx, lt, li;
                    var bcnt = 0;
                    var lexlang = ProjectManager.CurrentParatextProject.Name;
                    var bookfilesfiltered = bookfiles.Where(s => s.ToString().Contains("\\" + lexlang)).ToList();

                    foreach (var f in bookfilesfiltered) // loop through books f
                    {
                        projectBookFileData = File.ReadAllLines(f); // read file and check
                        for (var k = 0; k < projectBookFileData.Count(); k++)
                        {
                            if (projectBookFileData[k].Contains("<item>"))
                            {
                                if (projectBookFileData[++k].Contains("<string>"))
                                {
                                    tref = GetTagValue(projectBookFileData[k]);
                                    do
                                    {
                                        if (projectBookFileData[++k]
                                            .Contains(
                                                "<Lexeme")) // build a dictionary where key = lexeme+gloss, and where value = references
                                        {
                                            lx = lt = li = "";
                                            Lexparse(projectBookFileData[k], ref lx, ref lt, ref li);
                                            if (LexMatRef.ContainsKey(li +
                                                                      lx)) // key already exists so add references to previous value
                                                LexMatRef[li + lx] = LexMatRef[li + lx] + ", " + tref;
                                            else // this is a new key so create a new key, value pair
                                                LexMatRef.Add(li + lx, tref);
                                        }
                                    } while (!projectBookFileData[k].Contains("</item>"));
                                }
                            }
                        }
                    }

                    List<string> rs;
                    string ky, vl;
                    int ndx2;
                    var pndx = 0;
                    var simrefs = "";
                    var results = new List<PinsDataTable>();
                    PinsDataTable datrow;
                    int lmrIndex = 0;
                    foreach (var LMR in LexMatRef)
                    {
                        
                        try
                        {
                            rs = LMR.Value.Split(',')
                                .ToList(); // change dictionary values from comma delimited string to List for sorting
                            ky = LMR.Key;
                            SortRefs(ref rs); // sort the List  
                            vl = string.Join(", ", rs); // change List back to comma delimited string

                            if (!vl.Contains("missing"))
                            {
                                var objectToFind = GridData.Where(s => s.Match == ky).FirstOrDefault();
                                ndx2 = GridData.IndexOf(objectToFind); //.FindIndex(s => s.Match == ky);
                                if (ndx2 >= 0)
                                {
                                    datrow = GridData[ndx2];
                                    datrow.Refs = vl;

                                    if (datrow.Refs != "")
                                    {
                                        //SimplifyRefs(datrow.Refs.Split(',').ToList(), ref simrefs);
                                        var longrefs = datrow.Refs.Split(',').ToList();
                                        var simprefs = new List<string>();
                                        foreach (var longref in longrefs)
                                        {
                                            lmrIndex++;
                                            var booksplit = longref.Trim().Split(' ').ToList();
                                            var bookNum = BookChapterVerseViewModel.GetBookNumFromBookName(booksplit[0]);
                                                //BibleBookDict[booksplit[0]].PadLeft(3, '0');
                                            var chapterVerseSplit = booksplit[1].Split(':').ToList();
                                            var chapterNum = chapterVerseSplit[0].PadLeft(3, '0');
                                            var verseNum = chapterVerseSplit[1].PadLeft(3, '0');
                                            simprefs.Add(bookNum + chapterNum + verseNum);
                                        }

                                        var verseList = datrow.VerseList;
                                        verseList.AddRange(simprefs);
                                        datrow.VerseList = verseList;
                                        datrow.SimpRefs = datrow.VerseList.Count.ToString();
                                    }
                                    else
                                    {
                                        datrow.SimpRefs = "0";
                                        datrow.VerseList = null;

                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            var theThingThatFailed = LMR;
                            _logger.LogError(ex, "Adding Verse References from Interlinear_*.xml failed");
                        }
                    }
                }

                // bind the data grid to the collection view
                GridCollectionView = CollectionViewSource.GetDefaultView(GridData);
                // setup the filtering routine to determine what gets displayed
                GridCollectionView.Filter = new Predicate<object>(FilterTerms);
                NotifyOfPropertyChange(() => GridCollectionView);

                // turn off the progress bar
                ProgressBarVisibility = Visibility.Collapsed;

                // send to the task started event aggregator for everyone else to hear about a task completion
                await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(
                    new BackgroundTaskStatus
                    {
                        Name = _taskName,
                        EndTime = DateTime.Now,
                        Description = "Loading PINS data...Complete",
                        TaskLongRunningProcessStatus = LongRunningTaskStatus.Completed
                    }), cancellationToken);
            }
            catch (OperationCanceledException ex)
            {
                _logger!.LogInformation("PinsViewModel.GenerateData() - an exception was thrown -> cancellation was requested.");
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "An unpected error occurred while generating the PINS data.");
                if (!cancellationToken.IsCancellationRequested)
                {
                    await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(
                        new BackgroundTaskStatus
                        {
                            Name = _taskName,
                            EndTime = DateTime.Now,
                            ErrorMessage = $"{ex}",
                            TaskLongRunningProcessStatus = LongRunningTaskStatus.Failed
                        }), cancellationToken);
                }
            
            }
            finally
            {
                _generateDataRunning = false;
               
                _longRunningTaskManager.TaskComplete(_taskName);
               
               

            }
            return false;
        }

        private string GetTagValue(string t)
        {
            var ttrim = t.TrimStart(' ', '<');            // remove leading blanks and < leaving "tagname>VALUE</tagname>"
            var words = ttrim.Split('<', '>');          // value of simple tag is words[1] (tagname, VALUE, /tagname)
            return words[1];                                 // return second word only
        }

        private void Lexparse(string lexin, ref string lex, ref string lext, ref string lexi)
        {
            var tmp = lexin.Split('"');
            if (tmp.Length > 3)
                lexi = tmp[3];
            else
                lexi = "missing";
            if (tmp.Length > 1)
            {
                var tmp2 = tmp[1].Split(':');
                lext = tmp2[0];
                lex = tmp2[1];
            }
            else
            {
                lext = "missing";
                lex = "missing";
            }
        }

        // sort the references in the list to their Biblical order
        private void SortRefs(ref List<string> refs)
        {
            List<CoupleOfStrings> references = new();
            foreach (var r in refs)
            {
                var tmp = r.Trim();
                if (tmp.Length >= 3)
                {
                    var book = tmp.Substring(0, 3);//the issue is we are assuming the string is a certian length etc.  do more checking
                    var bookNum = BookChapterVerseViewModel.GetBookNumFromBookName(book);
                    if (bookNum.Length > 0)
                    {
                        if(tmp.Length >= 4)
                        {
                            tmp = tmp.Substring(3).Trim();
                            var parts = tmp.Split(':');
                            if (parts.Length > 1)
                            {
                                string chapter = parts[0].Trim();
                                string verse = parts[1].Trim();
                                if (verse.IndexOf("-") > 0)
                                {
                                    verse = verse.Substring(0, verse.IndexOf("-"));
                                }

                                if (verse.IndexOf(".") > 0)
                                {
                                    verse = verse.Substring(0, verse.IndexOf("."));
                                }

                                references.Add(new CoupleOfStrings
                                {
                                    stringA = $"{bookNum}{chapter.PadLeft(3, '0')}{verse.PadLeft(3, '0')}",
                                    stringB = r
                                });
                            }
                        }
                        //else
                        //{
                        //    Logger.LogWarning("The reference was less than 4 characters.  It was "+ tmp.Length);
                        //}
                    }
                }
                //else
                //{
                //    Logger.LogWarning("The reference was less than 3 characters.  It was "+ tmp.Length);
                //}
            }

            var orderedList = references.OrderBy(x => x.stringA).ToList();
            orderedList = orderedList.DistinctBy(x => x.stringA).ToList();

            refs.Clear();
            foreach (var reference in orderedList)
            {
                refs.Add(reference.stringB);
            }
        }

        private async Task<bool> GetLexicon()
        {
            // load in the lexicon.xml for the project
            var queryLexiconResult = await ExecuteRequest(new GetLexiconQuery(), CancellationToken.None).ConfigureAwait(false);
            if (queryLexiconResult.Success == false)
            {
                _logger!.LogError(queryLexiconResult.Message);
                return true;
            }

            if (queryLexiconResult.Data == null)
            {
                return true;
            }

            _lexicon = queryLexiconResult.Data;
            return false;
        }

        private async Task<bool> GetSpellingStatus()
        {
            // load in the 'spellingstatus.xml'
            var querySsResult =
                await ExecuteRequest(new GetSpellingStatusQuery(), CancellationToken.None).ConfigureAwait(false);
            if (querySsResult.Success == false)
            {
                _logger!.LogError(querySsResult.Message);
                return true;
            }

            if (querySsResult.Data == null)
            {
                return true;
            }

            _spellingStatus = querySsResult.Data;
            return false;
        }

        private async Task<bool> GetAllBiblicalTerms(string paratextInstallPath)
        {
            // load in the AllBiblicalTerms.xml file
            var queryAbtResult =
                await ExecuteRequest(
                    new GetBiblicalTermsQuery(paratextInstallPath, BTtype.allBT),
                    CancellationToken.None).ConfigureAwait(false);
            if (queryAbtResult.Success == false)
            {
                _logger!.LogError(queryAbtResult.Message);
                return true;
            }

            if (queryAbtResult.Data == null)
            {
                return true;
            }

            _allBiblicalTermsList = queryAbtResult.Data;
            return false;
        }

        private async Task<bool> GetBiblicalTerms(string paratextInstallPath)
        {
            // load in the BiblicalTerms.xml file
            var queryBtResult =
                await ExecuteRequest(
                    new GetBiblicalTermsQuery(paratextInstallPath, BTtype.BT),
                    CancellationToken.None).ConfigureAwait(false);
            if (queryBtResult.Success == false)
            {
                _logger!.LogError(queryBtResult.Message);
                return true;
            }

            if (queryBtResult.Data == null)
            {
                return true;
            }

            _biblicalTermsList = queryBtResult.Data;
            return false;
        }

        private async Task<bool> GetTermRenderings()
        {
            // load in the TermRenderings.xml file
            var queryResult = await ExecuteRequest(new GetTermRenderingsQuery(), CancellationToken.None).ConfigureAwait(false);

            if (queryResult.Success == false)
            {
                _logger!.LogError(queryResult.Message);
                return true;
            }

            if (queryResult.Data == null)
            {
                return true;
            }

            _termRenderingsList = queryResult.Data;
            return false;
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


        private void ClearFilter(object obj)
        {
            FilterString = "";
        }

        private bool FilterTerms(object item)
        {
            var itemDt = (PinsDataTable)item;

            if (itemDt.Gloss is null)
            {
                return itemDt.Source.Contains(_filterString) || itemDt.Notes.Contains(_filterString);
            }

            return itemDt.Source.Contains(_filterString) || itemDt.Gloss.Contains(_filterString) ||
                   itemDt.Notes.Contains(_filterString);

        }

        /// <summary>
        /// User has clicked on a verse button in the grid
        /// </summary>
        /// <param name="obj"></param>
        private async void VerseButtonClick(object obj)
        {
            ProgressBarVisibility = Visibility.Visible;

            if (obj is PinsDataTable dataRow)
            {
                await LoadVerseText(dataRow);
            }
            
            ProgressBarVisibility = Visibility.Collapsed;
        }

        private async Task<bool> LoadVerseText(PinsDataTable dataRow)
        {
            if (dataRow.VerseList.Count == 0)
            {
                return true;
            }

            SelectedItemVerses.Clear();

            // sort these BBBCCCVVV so that they are arranged properly
            dataRow.VerseList.Sort();

            // create a list for doing versification processing
            var verseList = new List<VersificationList>();
            foreach (var verse in dataRow.VerseList)
            {
                verseList.Add(new VersificationList
                {
                    SourceBBBCCCVV = verse.Substring(0, 9),
                    TargetBBBCCCVV = "",
                });
            }

            // this data from the BiblicalTerms & AllBiblicalTerms XML files has versification from the org.vrs
            // convert it over to the current project versification format.
            verseList = Helpers.Versification.GetVersificationFromOriginal(verseList, _projectManager.CurrentParatextProject);

            // create the list to display
            foreach (var verse in verseList)
            {
                var verseIdShort = BookChapterVerseViewModel.GetVerseStrShortFromBBBCCCVVV(verse.TargetBBBCCCVV);

                var bookNum = int.Parse(verse.TargetBBBCCCVV.Substring(0, 3));
                var chapterNum = int.Parse(verse.TargetBBBCCCVV.Substring(3, 3));
                var verseNum = int.Parse(verse.TargetBBBCCCVV.Substring(6, 3));

                var verseTextResult = await ExecuteRequest(new GetParatextVerseTextQuery(bookNum, chapterNum, verseNum),
                    CancellationToken.None);
                var verseText = "";
                if (verseTextResult.Success)
                    verseText = verseTextResult.Data.Name;
                else
                {
                    verseText = "There was an issue getting the text for this verse.";
                    _logger.LogInformation("Failure to GetParatextVerseTextQuery");
                }

                SelectedItemVerses.Add(new PinsVerseList
                {
                    BBBCCCVVV = verse.TargetBBBCCCVV,
                    VerseIdShort = verseIdShort,
                    VerseText = verseText
                });
            }

            NotifyOfPropertyChange(() => SelectedItemVerses);
            VerseRefDialogOpen = true;
            return false;
        }

        /// <summary>
        /// User has clicked on a verse link.  The VersePopUp window comes up with
        /// all the various verse renderings
        /// </summary>
        /// <param name="obj"></param>
        private async void VerseClick(object obj)
        {
            if (obj is null)
            {
                return;
            }
            
            await ExecuteRequest(new SetCurrentVerseCommand(obj.ToString()), CancellationToken.None);
        }

        public void LaunchMirrorView(double actualWidth, double actualHeight)
        {
            LaunchMirrorView<PinsView>.Show(this, actualWidth, actualHeight);
        }

        public async Task HandleAsync(FilterPinsMessage message, CancellationToken cancellationToken)
        {
            FilterString = message.Message;
        }

        #endregion // Methods
    }

}
