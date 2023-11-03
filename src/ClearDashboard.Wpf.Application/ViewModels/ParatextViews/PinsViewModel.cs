using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.Framework.Input;
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
using ClearDashboard.Wpf.Application.Properties;
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
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using Brushes = System.Windows.Media.Brushes;
using Point = System.Windows.Point;

namespace ClearDashboard.Wpf.Application.ViewModels.ParatextViews
{
    public class PinsViewModel : ToolViewModel, IHandle<FilterPinsMessage>, IHandle<ProjectLoadedMessage>
    {

        #region Member Variables
        public Dictionary<string, string> LexMatRef = new();
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

        private Stopwatch _watch = new Stopwatch();

        private Collection<PinsDataTable> _gridData { get; } = new();
        private PinsVerseViewModel? _pinsVerseViewModel;

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


        private ObservableCollection<PinsVerseListViewModel> _selectedItemVerses = new();
        public ObservableCollection<PinsVerseListViewModel> SelectedItemVerses
        {
            get => _selectedItemVerses;
            set
            {
                _selectedItemVerses = value;
                NotifyOfPropertyChange(() => SelectedItemVerses);
            }
        }

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

        private bool _isAll = true;
        public bool IsAll
        {
            get => _isAll;
            set
            {
                _isAll = value;
                NotifyOfPropertyChange(() => IsAll);
                CheckAndRefreshGrid();
            }
        }

        private bool _isBt;
        public bool IsBt
        {
            get => _isBt;
            set
            {
                _isBt = value;
                NotifyOfPropertyChange(() => IsBt);
                _selectedXmlSourceRadioDictionary[XmlSource.BiblicalTerms] = value;
                CheckAndRefreshGrid();
            }
        }

        private bool _isAbt;
        public bool IsAbt
        {
            get => _isAbt;
            set
            {
                _isAbt = value;
                NotifyOfPropertyChange(() => IsAbt);
                _selectedXmlSourceRadioDictionary[XmlSource.AllBiblicalTerms] = value;
                CheckAndRefreshGrid();
            }
        }

        private bool _isTr;
        public bool IsTr
        {
            get => _isTr;
            set
            {
                _isTr = value;
                NotifyOfPropertyChange(() => IsTr);
                _selectedXmlSourceRadioDictionary[XmlSource.TermsRenderings] = value;
                CheckAndRefreshGrid();
            }
        }

        private bool _isLx;
        public bool IsLx
        {
            get => _isLx;
            set
            {
                _isLx = value;
                NotifyOfPropertyChange(() => IsLx);
                _selectedXmlSourceRadioDictionary[XmlSource.Lexicon] = value;
                CheckAndRefreshGrid();
            }
        }

        private Dictionary<XmlSource, bool> _selectedXmlSourceRadioDictionary;
        public Dictionary<XmlSource, bool> SelectedXmlSourceRadioDictionary
        {
            get => _selectedXmlSourceRadioDictionary;
            set
            {
                _selectedXmlSourceRadioDictionary = value;
                NotifyOfPropertyChange(() => SelectedXmlSourceRadioDictionary);
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

                CheckAndRefreshGrid();
            }
        }


        private string _lastSelectedPinsDataTableSource = "";
        public string LastSelectedPinsDataTableSource
        {
            get => _lastSelectedPinsDataTableSource;
            set
            {
                value ??= string.Empty;

                _lastSelectedPinsDataTableSource = value;
                NotifyOfPropertyChange(() => LastSelectedPinsDataTableSource);
            }
        }

        private string _selectedItemFilterString = "";
        public string SelectedItemFilterString
        {
            get => _selectedItemFilterString;
            set
            {
                value ??= string.Empty;

                _selectedItemFilterString = value;
                NotifyOfPropertyChange(() => SelectedItemFilterString);

                CheckAndRefreshGrid();
            }
        }

        private string _verseFilterText;
        public string VerseFilterText
        {
            get
            {
                return _verseFilterText;
            }
            set
            {
                _verseFilterText = value;
                this._verseCollection.View.Refresh();
                NotifyOfPropertyChange(() => VerseFilterText);
            }
        }

        private CollectionViewSource _verseCollection;
        public ICollectionView VerseCollection
        {
            get
            {
                return this._verseCollection.View;
            }
        }

        private bool _backTranslationFound = false;
        public bool BackTranslationFound
        {
            get => _backTranslationFound;

            set
            {
                _backTranslationFound = value;
                NotifyOfPropertyChange(() => BackTranslationFound);
            }
        }

        private bool _showBackTranslation = false;
        public bool ShowBackTranslation
        {
            get => _showBackTranslation;

            set
            {
                _showBackTranslation = value;

                foreach (var verse in SelectedItemVerses)
                {
                    if (verse.BackTranslation != string.Empty)
                    {
                        verse.ShowBackTranslation = value;
                    }
                }

                Settings.Default.PinsShowBackTranslation = value;

                NotifyOfPropertyChange(() => ShowBackTranslation);
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
            : base(navigationService, logger, projectManager, eventAggregator, mediator, lifetimeScope, localizationService)
        {
            Title = "⍒ " + LocalizationService!.Get("Windows_PINS");
            this.ContentId = "PINS";

            _logger = logger;
            _projectManager = projectManager;
            _mediator = mediator;
            _longRunningTaskManager = longRunningTaskManager;

            _verseCollection = new CollectionViewSource();
            _verseCollection.Source = SelectedItemVerses;
            _verseCollection.Filter += VerseCollection_Filter;

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

            _selectedXmlSourceRadioDictionary = new Dictionary<XmlSource, bool>();
            _selectedXmlSourceRadioDictionary.Add(XmlSource.BiblicalTerms, _isBt);
            _selectedXmlSourceRadioDictionary.Add(XmlSource.AllBiblicalTerms, _isAbt);
            _selectedXmlSourceRadioDictionary.Add(XmlSource.TermsRenderings, _isTr);
            _selectedXmlSourceRadioDictionary.Add(XmlSource.Lexicon, _isLx);

            ShowBackTranslation = Settings.Default.PinsShowBackTranslation;
        }

        public async Task Initialize(CancellationToken cancellationToken)
        {
            if (ProjectManager.IsParatextConnected == false)
            {
                ProgressBarVisibility = Visibility.Collapsed;
                return;
            }

            _watch.Start();
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

            try
            {
                _generateDataRunning = true;
                var longRunningTask = _longRunningTaskManager.Create(_taskName, LongRunningTaskStatus.Running);
                var cancellationToken2 = longRunningTask.CancellationTokenSource.Token;


                var isDataGenerated = await GenerateInitialData(cancellationToken2);
                if (!isDataGenerated)
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
                }

                await GenerateData(cancellationToken2);

                await GenerateLexiconData(cancellationToken2);

                await ConnectDataToUi(cancellationToken2).ConfigureAwait(true);
            }
            catch (OperationCanceledException ex)
            {
                _logger!.LogInformation(
                    "PinsViewModel.GenerateData() - an exception was thrown -> cancellation was requested.");
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
                var time = _watch.Elapsed.Seconds;
            }

#pragma warning restore CS4014

            //_ = base.OnActivateAsync(cancellationToken);
        }

        protected override async Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
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

        #endregion //Constructor

        #region Methods

        /// <summary>
        /// Main logic for building the data
        /// </summary>
        /// <returns></returns>
        private async Task<bool> GenerateInitialData(CancellationToken cancellationToken)
        {
            //ReSharper disable once MethodSupportsCancellation
            _ = await Task.Run<bool>(async () =>
            {
                var logger = LifetimeScope.Resolve<ILogger<ParatextProxy>>();
                ParatextProxy paratextUtils = new ParatextProxy(logger);
                if (paratextUtils.IsParatextInstalled())
                {

                    if (paratextUtils.ParatextInstallPath != string.Empty)
                    {
                        _paratextInstallPath = paratextUtils.ParatextInstallPath;
                    }
                    else if (paratextUtils.ParatextBetaInstallPath != string.Empty)
                    {
                        _paratextInstallPath = paratextUtils.ParatextBetaInstallPath;
                    }


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
                    return false;
                }

                return true;
            }).ConfigureAwait(true);


            return true;
        }

        private async Task<bool> GenerateData(CancellationToken cancellationToken)
        {
            //ReSharper disable once MethodSupportsCancellation
            _ = await Task.Run(async () =>
            {
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
                    var bt = _biblicalTermsList.Term?.FindAll(t => t.Id == sourceWord);

                    if (bt is null)
                    {
                        bt = new List<Term>();
                    }

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

                        var xmlSource = XmlSource.BiblicalTerms;

                        _gridData.Add(new PinsDataTable
                        {
                            Id = Guid.NewGuid(),
                            XmlSource = xmlSource,
                            XmlSourceAbbreviation = xmlSource.GetDescription(),
                            XmlSourceDisplayName = "Key Terms",//Path.Combine(_paratextInstallPath, @"Terms\Lists\BiblicalTerms.xml"),
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
                        var abt = _allBiblicalTermsList.Term?.FindAll(t => t.Id == sourceWord);
                        if (abt is null)
                        {
                            abt = new List<Term>();
                        }


                        if (abt.Count > 0)
                        {
                            gloss = abt[0].Gloss;

                            // AllBiblicalTerms contains the verses associated with the TermRenderings
                            foreach (var verse in abt[0].References.Verse)
                            {
                                verseList.Add(verse);
                                cancellationToken.ThrowIfCancellationRequested();
                            }

                            var xmlSource = XmlSource.AllBiblicalTerms;

                            _gridData.Add(new PinsDataTable
                            {
                                Id = Guid.NewGuid(),
                                XmlSource = xmlSource,
                                XmlSourceAbbreviation = xmlSource.GetDescription(),
                                XmlSourceDisplayName = "All Biblical Terms",//Path.Combine(_paratextInstallPath, @"Terms\Lists\AllBiblicalTerms.xml"),
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

                            var xmlSource = XmlSource.TermsRenderings;

                            _gridData.Add(new PinsDataTable
                            {
                                Id = Guid.NewGuid(),
                                XmlSource = xmlSource,
                                XmlSourceAbbreviation = xmlSource.GetDescription(),
                                XmlSourceDisplayName = "Term Renderings", //Path.Combine(_projectManager.CurrentParatextProject?.DirectoryPath,"TermRenderings.xml"),
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

                        }
                    }

                    cancellationToken.ThrowIfCancellationRequested();
                }
                return true;
            }).ConfigureAwait(true);


            return true;
        }


        private async Task<bool> GenerateLexiconData(CancellationToken cancellationToken)
        {
            _ = await Task.Run<bool>(async () =>
            {
                await GenerateLexiconDataCalculations(cancellationToken);
                return true;
            }).ConfigureAwait(true);

            return false;
        }

        private async Task<bool> ConnectDataToUi(CancellationToken cancellationToken)
        {

            // bind the data grid to the collection view
            GridCollectionView = CollectionViewSource.GetDefaultView(_gridData);
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

            return false;
        }

        private async Task<bool> GenerateLexiconDataCalculations(CancellationToken cancellationToken)
        {
            if (ProjectManager.CurrentParatextProject is null)
            {
                return false;
            }


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

            if (_lexicon.Entries != null)
            {
                // populate the data grid
                foreach (var entry in _lexicon.Entries.Item)
                {
                    foreach (var senseEntry in entry.Entry.Sense)
                    {
                        string vl = string.Empty;
                        var numRefCount = "0";
                        List<string> verseList = new List<string>();

                        try
                        {
                            if (LexMatRef.ContainsKey(senseEntry.Id + entry.Lexeme.Form))
                            {
                                var verseReferences = LexMatRef[senseEntry.Id + entry.Lexeme.Form].Split(',').ToList();

                                var orderedList = SortRefs(verseReferences); // sort the List  

                                verseReferences.Clear();
                                foreach (var orderedReference in orderedList)
                                {
                                    verseReferences.Add(orderedReference.stringB);
                                    verseList.Add(orderedReference.stringA);

                                }

                                vl = string.Join(", ", verseReferences);
                                numRefCount = verseList.Count.ToString();
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Adding Verse References from Interlinear_*.xml failed");
                        }

                        var xmlSource = XmlSource.Lexicon;

                        _gridData.Add(new PinsDataTable
                        {
                            Id = Guid.NewGuid(),
                            XmlSource = xmlSource,
                            XmlSourceAbbreviation = xmlSource.GetDescription(),
                            XmlSourceDisplayName = "Lexicon",//Path.Combine(_projectManager.CurrentParatextProject.DirectoryPath, "Lexicon.xml"),
                            Code = senseEntry.Id,
                            Gloss = senseEntry.Gloss.Text,
                            Lang = senseEntry.Gloss.Language,
                            Lform = entry.Lexeme.Type,
                            Match = senseEntry.Id + entry.Lexeme.Form,
                            Notes = "",
                            LexemeType = entry.Lexeme.Type,
                            //Phrase = (entry.Lexeme.Type == "Phrase") ? "Phr" : "",
                            //Prefix = (entry.Lexeme.Type == "Prefix") ? "pre-" : "",
                            Refs = vl,
                            SimpRefs = numRefCount,
                            Source = entry.Lexeme.Form,
                            //Stem = (entry.Lexeme.Type == "Stem") ? "Stem" : "",
                            //Suffix = (entry.Lexeme.Type == "Suffix") ? "-suf" : "",
                            VerseList = verseList,
                            //Word = (entry.Lexeme.Type == "Word") ? "Wrd" : "",
                        });
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                }
            }

            return true;
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
        private List<CoupleOfStrings> SortRefs(List<string> verseReferences)
        {
            List<CoupleOfStrings> bothReferences = new();
            foreach (var verseReference in verseReferences)
            {
                var tmp = verseReference.Trim();
                if (tmp.Length >= 3 && !verseReference.Contains("missing") && verseReference != "")
                {
                    var book = tmp.Substring(0, 3);//the issue is we are assuming the string is a certian length etc.  do more checking
                    var bookNum = BookChapterVerseViewModel.GetBookNumFromBookName(book);
                    if (bookNum.Length > 0)
                    {
                        if (tmp.Length >= 4)
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

                                var numRef = $"{bookNum}{chapter.PadLeft(3, '0')}{verse.PadLeft(3, '0')}";

                                bothReferences.Add(new CoupleOfStrings
                                {
                                    stringA = numRef,
                                    stringB = verseReference
                                });
                            }
                        }
                    }
                }
            }
            var orderedList = bothReferences.OrderBy(x => x.stringA).ToList();
            orderedList = orderedList.DistinctBy(x => x.stringA).ToList();

            return orderedList;
        }

        private async Task<bool> GetLexicon()
        {
            // load in the lexicon.xml for the project
            var queryLexiconResult = await ExecuteRequest(new GetLexiconQuery(), CancellationToken.None);
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
                await ExecuteRequest(new GetSpellingStatusQuery(), CancellationToken.None);
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
                    CancellationToken.None);
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
                    CancellationToken.None);
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
            var queryResult = await ExecuteRequest(new GetTermRenderingsQuery(), CancellationToken.None);

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

        private void CheckAndRefreshGrid()
        {
            if (_gridData != null && GridCollectionView is not null)
            {
                try
                {
                    GridCollectionView.Refresh();
                }
                catch
                {
                    _logger.LogError("PINS Clear Filter failed.");
                }
            }
        }

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
                return (itemDt.Source.Contains(_filterString, StringComparison.InvariantCultureIgnoreCase)
                        || itemDt.Notes.Contains(_filterString, StringComparison.InvariantCultureIgnoreCase)
                        || itemDt.OriginID.Contains(_filterString, StringComparison.InvariantCultureIgnoreCase))
                    && (_selectedXmlSourceRadioDictionary[itemDt.XmlSource] || _isAll);
            }

            return (itemDt.Source.Contains(_filterString, StringComparison.InvariantCultureIgnoreCase)
                    || itemDt.Gloss.Contains(_filterString, StringComparison.InvariantCultureIgnoreCase)
                    || itemDt.Notes.Contains(_filterString, StringComparison.InvariantCultureIgnoreCase)
                    || itemDt.OriginID.Contains(_filterString, StringComparison.InvariantCultureIgnoreCase))
                && (_selectedXmlSourceRadioDictionary[itemDt.XmlSource] || _isAll);
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
            if (_pinsVerseViewModel != null)
            {
                await _pinsVerseViewModel.TryCloseAsync();
            }

            LastSelectedPinsDataTableSource = dataRow.Source;
            VerseFilterText = string.Empty;
            //_showBackTranslation = false;
            BackTranslationFound = false;

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

                var verseTextResult = await ExecuteRequest(new GetParatextVerseTextQuery(bookNum, chapterNum, verseNum), CancellationToken.None);
                var verseText = "";
                if (verseTextResult.Success)
                    verseText = verseTextResult.Data.Name;
                else
                {
                    verseText = "There was an issue getting the text for this verse.";
                    //_logger.LogInformation("Failure to GetParatextVerseTextQuery");
                }

                var backTranslationResult = await ExecuteRequest(new GetParatextVerseTextQuery(bookNum, chapterNum, verseNum, true), CancellationToken.None);
                var backTranslation = "";
                var showBackTranslation = false;
                if (backTranslationResult.Success)
                {
                    backTranslation = backTranslationResult.Data.Name;

                    if (Settings.Default.PinsShowBackTranslation)
                        showBackTranslation = true;
                    //showBackTranslation = true;
                    //if(!ShowBackTranslation)
                    //    ShowBackTranslation = true;
                    BackTranslationFound = true;
                }
                else
                {
                    //_logger.LogInformation("Failure to GetParatextVerseTextQuery");
                }

                SelectedItemVerses.Add(new PinsVerseListViewModel
                {
                    VerseBBCCCVVV = verse.TargetBBBCCCVV,
                    VerseIdShort = verseIdShort,
                    VerseText = verseText,
                    BackTranslation = backTranslation,
                    ShowBackTranslation = showBackTranslation
                });
            }

            CreateInlines(dataRow);
            NotifyOfPropertyChange(() => SelectedItemVerses);
            //VerseRefDialogOpen = true;


            dynamic settings = new ExpandoObject();
            settings.SizeToContent = SizeToContent.Manual;
            settings.Title = "PINS Verse List";

            var parameters = new List<Autofac.Core.Parameter>
            {
                new NamedParameter("lastSelectedPinsDataTableSource", LastSelectedPinsDataTableSource),
                new NamedParameter("verseCollection", _verseCollection),
                new NamedParameter("showBackTranslation", Settings.Default.PinsShowBackTranslation),
                new NamedParameter("backTranslationFound", BackTranslationFound),
                new NamedParameter("selectedItemVerses", SelectedItemVerses),
            };

            _pinsVerseViewModel = LifetimeScope?.Resolve<PinsVerseViewModel>(parameters);

            IWindowManager manager = new WindowManager();
            manager.ShowWindowAsync(_pinsVerseViewModel, null, settings);
            return false;
        }

        private void CreateInlines(PinsDataTable dataRow)
        {
            var puncs = Punctuation.LoadPunctuation();

            // create inlines of the selected word
            foreach (var verse in _selectedItemVerses)
            {
                var renderings = dataRow.Source.Split("; ").Distinct().ToList();

                var output = TextUtils.HighLightWordsRtf(renderings, verse.VerseText, null, puncs, FontFamily, FontSize);
                verse.RichVerseText = output.Item1;
                verse.Found = output.Item2;
            }

            NotifyOfPropertyChange(() => VerseCollection);
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
            LaunchMirrorView<PinsView>.Show(this, actualWidth, actualHeight, this.Title);
        }

        void VerseCollection_Filter(object sender, FilterEventArgs e)
        {
            if (string.IsNullOrEmpty(VerseFilterText))
            {
                e.Accepted = true;
                return;
            }

            PinsVerseListViewModel pinsVerse = e.Item as PinsVerseListViewModel;
            if (pinsVerse.VerseText.ToUpper().Contains(VerseFilterText.ToUpper()) ||
                pinsVerse.VerseIdShort.ToUpper().Contains(VerseFilterText.ToUpper()))
            {
                e.Accepted = true;
            }
            else
            {
                e.Accepted = false;
            }
        }

        #endregion // Methods

        #region IHandle

        public async Task HandleAsync(FilterPinsMessage message, CancellationToken cancellationToken)
        {
            FilterString = message.Message;
            switch (message.XmlSource)
            {
                case XmlSource.All:
                    IsAll = true;
                    break;
                case XmlSource.BiblicalTerms:
                    IsBt = true;
                    break;
                case XmlSource.AllBiblicalTerms:
                    IsAbt = true;
                    break;
                case XmlSource.TermsRenderings:
                    IsTr = true;
                    break;
                case XmlSource.Lexicon:
                    IsLx = true;
                    break;
            }
        }

        #endregion // IHandle
    }

}
