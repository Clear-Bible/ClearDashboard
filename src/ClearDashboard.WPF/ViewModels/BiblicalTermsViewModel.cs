using Caliburn.Micro;
using ClearDashboard.Common.Models;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.DataAccessLayer.NamedPipes;
using ClearDashboard.Wpf.Helpers;
using ClearDashboard.Wpf.Interfaces;
using ClearDashboard.Wpf.ViewModels.Panes;
using Microsoft.Extensions.Logging;
using Pipes_Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using ClearDashboard.DataAccessLayer.Wpf;
using Point = System.Windows.Point;
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local

namespace ClearDashboard.Wpf.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class BiblicalTermsViewModel : ToolViewModel, IWorkspace
    {
        #region Member Variables

        public enum SelectedBtEnum
        {
            OptionAll,
            OptionProject
        }

        public enum FilterWordEnum
        {
            Gloss,
            Rendering
        }

        private enum FilterScopeEnum
        {
            // DO NOT CHANGE THESE IDENTIFIER NAMES - THEY ARE LINKED TO THE
            // LOCALIZATION SPREADSHEET LOOKUPS
            BtBcvAll,
            BtBcvBook,
            BtBcvChapter,
            BtBcvVerse
        }

        private enum RenderingFilterEnum
        {
            // DO NOT CHANGE THESE IDENTIFIER NAMES - THEY ARE LINKED TO THE
            // LOCALIZATION SPREADSHEET LOOKUPS
            BtRenderingAllTerms,
            BtRenderingMissingRenderings
        }

        private enum SemanticDomainEnum
        {
            // DO NOT CHANGE THESE IDENTIFIER NAMES - THEY ARE LINKED TO THE
            // LOCALIZATION SPREADSHEET LOOKUPS
            BtDomainsAll = 0,
            BtDomainsAffection = 1,
            BtDomainsAgriculture = 2,
            BtDomainsAnimals = 3,
            BtDomainsArea = 4,
            BtDomainsAreaNature = 5,
            BtDomainsAssociation = 6,
            BtDomainsConstructionReligiousActivities = 7,
            BtDomainsConstructionsAnimalHusbandry = 8,
            BtDomainsContainersAnimalHusbandry = 9,
            BtDomainsCraftsCloth = 10,
            BtDomainsFruits = 11,
            BtDomainsGemstones = 12,
            BtDomainsGrasses = 13,
            BtDomainsGroup = 14,
            BtDomainsGroupArea = 15,
            BtDomainsHonorRespectStatus = 16,
            BtDomainsLocale = 17,
            BtDomainsMammalsDomesticAnimals = 18,
            BtDomainsMammalsWildAnimals = 19,
            BtDomainsMonument = 20,
            BtDomainsMoralsAndEthics = 21,
            BtDomainsMourning = 22,
            BtDomainsNature = 23,
            BtDomainsPaganism = 24,
            BtDomainsPeople = 25,
            BtDomainsPeopleAuthority = 26,
            BtDomainsPeopleHonorRespectStatus = 27,
            BtDomainsPerson = 28,
            BtDomainsPurpose = 29,
            BtDomainsReligiousActivities = 30,
            BtDomainsSacrificesAndOfferings = 31,
            BtDomainsSettlement = 32,
            BtDomainsSignsAndWonders = 33,
            BtDomainsSupernaturalBeingsAndPowers = 34,
            BtDomainsSupernaturalBeingsAndPowersTitles = 35,
            BtDomainsTools = 36,
            BtDomainsToolsChildbirth = 37,
            BtDomainsToolsWeightCommerce = 38,
            BtDomainsTreesFruits = 39,
            BtDomainsTreesPerfumesAndSpices = 40,
            BtDomainsWisdomUnderstanding = 41,
        }

        public ILogger _logger { get; set; }
        public INavigationService _navigationService { get; set; }
        public ProjectManager _projectManager { get; set; }
        private string _currentVerse = "";




        //Dictionary<string, object> filters = new Dictionary<string, object>();
        #endregion //Member Variables

        #region Public Properties

       // public bool IsRtl { get; set; }


        private FlowDirection _flowDirection = FlowDirection.LeftToRight;
        public  FlowDirection FlowDirection
        {
            get => _flowDirection;
            set
            {
                _flowDirection = value; 
                NotifyOfPropertyChange(() => FlowDirection);
            }
        }



        private string _filterText;

        public string FilterText
        {
            get => _filterText;
            set
            {
                _filterText = value;
                NotifyOfPropertyChange(() => FilterText);
                TriggerFilterTermsByWord();
            }
        }


        private string _gloss;
        public string Gloss
        {
            get => _gloss;
            set
            {
                _gloss = value;
                NotifyOfPropertyChange(() => Gloss);
            }
        }


        // ALL, BOOK, CHAPTER, VERSE
        private DataTable _scope;
        public DataTable Scopes
        {
            get => _scope; 
            set
            {
                _scope = value;
                NotifyOfPropertyChange(() => Scopes);
            }
        }

        // ALL, BOOK, CHAPTER, VERSE
        private DataRowView _selectedScope;
        public DataRowView SelectedScope
        {
            get => _selectedScope; 
            set
            {
                _selectedScope = value;
                NotifyOfPropertyChange(() => SelectedScope);

                //refresh the biblicalterms collection so the filter runs
                if (BiblicalTermsCollectionView is not null)
                {
                    BiblicalTermsCollectionView.Refresh();
                }
            }
        }

        // ALL TERMS, MISSING RENDERINGS
        private DataTable _renderingsFilters;
        public DataTable RenderingsFilters
        {
            get => _renderingsFilters;
            set
            {
                _renderingsFilters = value;
                NotifyOfPropertyChange(() => RenderingsFilters);
            }
        }

        // ALL TERMS, MISSING RENDERINGS
        private DataRowView _renderingFilter;
        public DataRowView RenderingFilter
        {
            get => _renderingFilter;
            set
            {
                _renderingFilter = value;
                NotifyOfPropertyChange(() => RenderingFilter);

                //refresh the biblicalterms collection so the filter runs
                if (BiblicalTermsCollectionView is not null)
                {
                    BiblicalTermsCollectionView.Refresh();
                }
            }
        }


        private DataTable _domains;
        public DataTable Domains
        {
            get => _domains; 
            set
            {
                _domains = value;
                NotifyOfPropertyChange(() => Domains);
            }
        }

        private DataRowView _selectedDomain;
        public DataRowView SelectedDomain
        {
            get => _selectedDomain; 
            set
            {
                _selectedDomain = value;
                NotifyOfPropertyChange(() => SelectedDomain);

                //refresh the biblicalterms collection so the filter runs
                if (BiblicalTermsCollectionView is not null)
                {
                    BiblicalTermsCollectionView.Refresh();
                }
            }
        }

        private SelectedBtEnum _lastSelectedBtEnum = SelectedBtEnum.OptionProject;

        private SelectedBtEnum _selectedBiblicalTermsType = SelectedBtEnum.OptionProject;
        public SelectedBtEnum SelectedBiblicalTermsType
        {
            get => _selectedBiblicalTermsType;
            set
            {
                _selectedBiblicalTermsType = value;
                NotifyOfPropertyChange(() => SelectedBiblicalTermsType);

                // reset the semantic domains & filter
                FilterText = "";
                SelectedDomain = null;

                SwitchedBiblicalTermsType();
            }
        }

        private FilterWordEnum _selectedWordFilterEnum = FilterWordEnum.Gloss;
        public FilterWordEnum SelectedWordFilterEnum
        {
            get => _selectedWordFilterEnum;
            set
            {
                _selectedWordFilterEnum = value;
                NotifyOfPropertyChange(() => SelectedWordFilterEnum);

                TriggerFilterTermsByWord();
            }
        }

        #endregion //Public Properties

        #region Observable Properties

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

        public ICollectionView BiblicalTermsCollectionView { get; }

        private ObservableCollection<BiblicalTermsData> _biblicalTerms = new();
        public ObservableCollection<BiblicalTermsData> BiblicalTerms
        {
            get => _biblicalTerms;
            set
            {
                _biblicalTerms = value;
                NotifyOfPropertyChange(() => BiblicalTerms);
            }
        }

        private BiblicalTermsData _selectedBiblicalTermsData;
        public BiblicalTermsData SelectedBiblicalTermsData
        {
            get => _selectedBiblicalTermsData; 
            set
            {
                _selectedBiblicalTermsData = value;
                NotifyOfPropertyChange(() => SelectedBiblicalTermsData);
                UpdateSelectedTerm(SelectedBiblicalTermsData);
            }
        }

        private ObservableCollection<Verse> _selectedItemVerses = new();
        public ObservableCollection<Verse> SelectedItemVerses
        {
            get => _selectedItemVerses; 
            set
            {
                _selectedItemVerses = value;
                NotifyOfPropertyChange(() => SelectedItemVerses);
            }
        }

        private ObservableCollection<string> _renderings = new();
        public ObservableCollection<string> Renderings
        {
            get => _renderings;
            set
            {
                _renderings = value;
                NotifyOfPropertyChange(() => Renderings);
            }
        }

        private ObservableCollection<string> _renderingsText = new();
        public ObservableCollection<string> RenderingsText
        {
            get => _renderingsText;
            set
            {
                _renderingsText = value;
                NotifyOfPropertyChange(() => RenderingsText);
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


        #endregion //Observable Properties

        #region Commands

        public ICommand NotesCommand { get; set; }
        public ICommand VerseClickCommand { get; set; }

        #endregion

        #region Constructor
        public BiblicalTermsViewModel(INavigationService navigationService, ILogger<WorkSpaceViewModel> logger, ProjectManager projectManager)
        {
            _navigationService = navigationService;
            _logger = logger;
            _projectManager = projectManager;
            
            Title = "🕮 BIBLICAL TERMS";
            ContentId = "BIBLICALTERMS";
            DockSide = EDockSide.Left;



            // listen to the DAL event messages coming in
            _projectManager.NamedPipeChanged += HandleEventAsync;

            FlowDirection = _projectManager.CurrentLanguageFlowDirection;

            // populate the combo box for semantic domains
            SetupSemanticDomains();
            DataRowView drv = Domains.DefaultView[Domains.Rows.IndexOf(Domains.Rows[0])];
            SelectedDomain = drv;

            // populate the combo box for scope
            SetupScopes();
            // select the first one
            drv = Scopes.DefaultView[Scopes.Rows.IndexOf(Scopes.Rows[0])];
            SelectedScope = drv;

            // populate the combo box for rendering filters
            SetupRenderingFilters();
            drv = RenderingsFilters.DefaultView[RenderingsFilters.Rows.IndexOf(RenderingsFilters.Rows[0])];
            RenderingFilter = drv;

            // setup the collectionview that binds to the data grid
            BiblicalTermsCollectionView = CollectionViewSource.GetDefaultView(this._biblicalTerms);

            // setup the method that we go to for filtering
            BiblicalTermsCollectionView.Filter = FilterGridItems;

            // wire up the commands
            NotesCommand = new RelayCommand(ShowNotes);
            VerseClickCommand = new RelayCommand(VerseClick);

            if (projectManager.ParatextProject is not null)
            {
                // pull out the project font family
                _fontFamily = projectManager.ParatextProject.Language.FontFamily;
                _fontSize = projectManager.ParatextProject.Language.Size;
                IsRtl = projectManager.ParatextProject.Language.IsRtol;
            }
        }


        protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            return base.OnInitializeAsync(cancellationToken);
        }

        protected override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            return base.OnActivateAsync(cancellationToken);
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
                    if (_currentVerse == "")
                    {
                        _currentVerse = pipeMessage.Text;
                        // ask for Biblical Terms
                        await _projectManager.SendPipeMessage(ProjectManager.PipeAction.GetBiblicalTermsProject)
                            .ConfigureAwait(false);
                    }
                    else
                    {
                        _currentVerse = pipeMessage.Text;
                    }

                    break;
                case ActionType.SetBiblicalTerms:
                    await SetProgressBarVisibilityAsync(Visibility.Visible).ConfigureAwait(false);
                    // invoke to get it to run in STA mode
                    Application.Current.Dispatcher.Invoke(delegate
                    {
                        _biblicalTerms.Clear();

                        // deserialize the list
                        var biblicalTermsList = new List<BiblicalTermsData>();
                        try
                        {
                            string json = pipeMessage.Payload.ToString();
                            biblicalTermsList = JsonSerializer.Deserialize<List<BiblicalTermsData>>(json);
                        }
                        catch (Exception e)
                        {
                            _logger.LogError($"BiblicalTermsViewModel Deserialize BiblicalTerms: {e.Message}");
                        }

                        if (biblicalTermsList.Count > 0)
                        {
                            for (int i = 0; i < biblicalTermsList.Count; i++)
                            {
                                _biblicalTerms.Add(biblicalTermsList[i]);

                                foreach (var rendering in biblicalTermsList[i].Renderings)
                                {
                                    _biblicalTerms[i].RenderingString += rendering + " ";
                                }
                            }

                            NotifyOfPropertyChange(() => BiblicalTerms);
                        }
                    });

                    await SetProgressBarVisibilityAsync(Visibility.Hidden).ConfigureAwait(false);
                    break;
            }
        }



        protected override void OnViewAttached(object view, object context)
        {
            Logger.LogInformation("OnViewAttached");
            base.OnViewAttached(view, context);
        }

        protected override void OnViewLoaded(object view)
        {
            Logger.LogInformation("OnViewLoaded");
            base.OnViewLoaded(view);
        }

        protected override void OnViewReady(object view)
        {
            Logger.LogInformation("OnViewReady");
            base.OnViewReady(view);
        }

        protected override void Dispose(bool disposing)
        {
            // unsubscribe from events
            _projectManager.NamedPipeChanged -= HandleEventAsync;

            Logger.LogInformation("Dispose");
            base.Dispose(disposing);
        }

        #endregion //Constructor

        #region Methods

        private void ToggleCurrentVerse()
        {
            //refresh the biblical terms collection so the filter runs
            BiblicalTermsCollectionView.Refresh();
        }

        private void ShowNotes(object obj)
        {
            throw new NotImplementedException();
        }

        private void VerseClick(object obj)
        {
            if (obj is null)
            {
                return;
            }

            string verseBBCCCVVV = (string)obj;
            var verses = SelectedItemVerses.Where(v => v.VerseBBCCCVVV.Equals(verseBBCCCVVV)).ToList();

            if (verses.Count > 0)
            {
                IWindowManager manager = new WindowManager();
                manager.ShowWindowAsync(
                    new VersePopUpViewModel(_navigationService, _logger, _projectManager,
                        verses[0]), null, null);
            }
        }



        /// <summary>
        /// User has switched the toggle for All/Project Biblical Terms
        /// </summary>
        /// <returns></returns>
        private async Task SwitchedBiblicalTermsType()
        {
            if (_lastSelectedBtEnum != _selectedBiblicalTermsType)
            {
                BiblicalTerms.Clear();
                await SetProgressBarVisibilityAsync(Visibility.Visible).ConfigureAwait(false);

                if (_selectedBiblicalTermsType == SelectedBtEnum.OptionProject)
                {
                    await _projectManager.SendPipeMessage(ProjectManager.PipeAction.GetBiblicalTermsProject).ConfigureAwait(false);
                }
                else
                {
                    await _projectManager.SendPipeMessage(ProjectManager.PipeAction.GetBiblicalTermsAll).ConfigureAwait(false);
                }

                _lastSelectedBtEnum = _selectedBiblicalTermsType;
            }
        }

        /// <summary>
        /// Turn on/off the progress bar asynchronously so the UI can render it
        /// </summary>
        /// <param name="visibility"></param>
        /// <returns></returns>
        private async Task SetProgressBarVisibilityAsync(Visibility visibility)
        {
            await Task.Run(() => { ProgressBarVisibility = visibility; }).ConfigureAwait(false);
            System.Windows.Forms.Application.DoEvents();
        }

        /// <summary>
        /// On BiblicalTerms Click - these are the bottom verse/verse references
        /// </summary>
        /// <param name="selectedBiblicalTermsData"></param>
        private void UpdateSelectedTerm(BiblicalTermsData selectedBiblicalTermsData)
        {
            SelectedItemVerses.Clear();
            Renderings.Clear();
            Gloss = "";
            if (selectedBiblicalTermsData is not null)
            {
                Gloss = selectedBiblicalTermsData.Gloss;

                for (int i = 0; i < selectedBiblicalTermsData.ReferencesLong.Count; i++)
                {
                    string verseRef = selectedBiblicalTermsData.ReferencesLong[i];
                    string verseText = "";
                    if (i < selectedBiblicalTermsData.ReferencesListText.Count)
                    {
                        verseText = selectedBiblicalTermsData.ReferencesListText[i];
                    }

                    var loc = verseRef.Split(' ');
                    string localizedString = verseRef;

                    if (loc.Length > 1)
                    {
                        localizedString = GetLocalizationString.Get(loc[0], _logger) + $" {loc[1]}";
                    }

                    _selectedItemVerses.Add(new Verse
                    {
                        VerseID = localizedString,
                        VerseBBCCCVVV = selectedBiblicalTermsData.References[i],
                        VerseText = verseText
                    });
                }

                foreach (var render in selectedBiblicalTermsData.Renderings)
                {
                    _renderings.Add(render);
                }
            }

            // create inlines of the selected word
            foreach (var verse in _selectedItemVerses)
            {
                var verseText = verse.VerseText;

                // create a punctuation-less version of the verse
                var puncs = Punctuation.LoadPunctuation();
                foreach (var punc in puncs)
                {
                    // we need to maintain the same verse length so we need to pad
                    // out the replacement spaces
                    string sBlank = "".PadLeft(punc.Length, ' ');

                    verseText = verseText.Replace(punc, sBlank);
                }

                // create a list of all the matches within the verse
                List<Point> points = new List<Point>();
                List<string> words = new List<string>();
                foreach (var render in selectedBiblicalTermsData.Renderings)
                {
                    // do the same for the target word that we are trying to test against
                    string puncLessWord = render;
                    foreach (var punc in puncs)
                    {
                        // we need to maintain the same verse length so we need to pad
                        // out the replacement spaces
                        string sBlank = "".PadLeft(punc.Length, ' ');

                        puncLessWord = puncLessWord.Replace(punc, sBlank);
                    }


                    try
                    {
                        // look for full word and case sensitive
                        Regex pattern = new Regex(@"\b" + puncLessWord + @"\b");
                        Match matchResults = pattern.Match(verseText);
                        while (matchResults.Success)
                        {
                            // matched text: matchResults.Value
                            // match start: matchResults.Index
                            // match length: matchResults.Length
                            Point point = new Point(matchResults.Index, matchResults.Index + matchResults.Length);
                            points.Add(point);
                            words.Add(render);

                            // flag that we found the word
                            verse.Found = true;

                            matchResults = matchResults.NextMatch();
                        }
                    }
                    catch (ArgumentException)
                    {
                        // Syntax error in the regular expression
                    }

                    //// iterate through in while loop
                    //int index = verseText.IndexOf(render, StringComparison.InvariantCultureIgnoreCase);
                    //if (index == -1)
                    //{
                    //    verse.Inlines.Add(new Run(verseText));
                    //    verse.Found = false;
                    //}
                    //else
                    //{
                    //    verse.Found = true;
                    //    while (true)
                    //    {
                    //        verse.Inlines.AddRange(new Inline[]
                    //        {
                    //            new Run(verseText.Substring(0, index)),
                    //            new Run(verseText.Substring(index, render.Length))
                    //            {
                    //                FontWeight = FontWeights.Bold,
                    //                Foreground = Brushes.Orange
                    //            }
                    //        });

                    //        verseText = verseText.Substring(index + render.Length);
                    //        index = verseText.IndexOf(render, StringComparison.InvariantCultureIgnoreCase);

                    //        if (index < 0)
                    //        {
                    //            verse.Inlines.Add(new Run(verseText));
                    //            break;
                    //        }
                    //    }
                    //}

                }

                verseText = verse.VerseText;

                // organize the points in lowest to highest
                points = points.OrderBy(o => o.X).ToList();

                // iterate through in reverse
                for (int i = points.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        string endPart = verseText.Substring((int)points[i].Y);
                        string startPart = verseText.Substring(0, (int)points[i].X);

                        //var a = new Run(startPart) { FontWeight = FontWeights.Normal };
                        verse.Inlines.Insert(0, new Run(endPart) { FontWeight = FontWeights.Normal });
                        verse.Inlines.Insert(0, new Run(words[i]) { FontWeight = FontWeights.Bold, Foreground = Brushes.Orange });

                        // check if this was the last one
                        if (i == 0)
                        {
                            verse.Inlines.Insert(0, new Run(startPart) { FontWeight = FontWeights.Normal });
                        }
                        else
                        {
                            verseText = startPart;
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e, "Unexpected error occurred while updating the selected term.");
                        throw;
                    }
                }

                if (points.Count == 0)
                {
                    verse.Inlines.Add(new Run(verseText));
                }
            }

            NotifyOfPropertyChange(() => SelectedItemVerses);
            NotifyOfPropertyChange(() => Renderings);
            NotifyOfPropertyChange(() => RenderingsText);
        }

        private void TriggerFilterTermsByWord()
        {
            //refresh the biblical terms collection so the filter runs
            BiblicalTermsCollectionView.Refresh();
        }

        /// <summary>
        /// This is the filter callback for the grid
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool FilterGridItems(object obj)
        {
            // first check if the row is filtered by the book or not
            bool isBcbFound = false;
            if (SelectedScope is { } rowView)
            {
                var selectedScope = rowView[1].ToString();

                // filter down to scope if present
                if (selectedScope != FilterScopeEnum.BtBcvAll.ToString())
                {
                    if (_projectManager.CurrentVerse.Length != 8)
                    {
                        return false;
                    }

                    if (obj is BiblicalTermsData terms)
                    {
                        switch (selectedScope)
                        {
                            case "BtBcvBook":
                                foreach (var term in terms.References)
                                {
                                    string book = term.Substring(0, 2);
                                    if (book == _projectManager.CurrentVerse.Substring(0, 2))
                                    {
                                        // found the book
                                        isBcbFound = true;
                                        break;
                                    }
                                }

                                break;
                            case "BtBcvChapter":
                                foreach (var term in terms.References)
                                {
                                    string chapter = term.Substring(0, 5);
                                    if (chapter == _projectManager.CurrentVerse.Substring(0, 5))
                                    {
                                        // found the chapter
                                        isBcbFound = true;
                                        break;
                                    }
                                }

                                break;
                            case "BtBcvVerse":
                                foreach (var term in terms.References)
                                {
                                    if (term == _projectManager.CurrentVerse)
                                    {
                                        // found the verse
                                        isBcbFound = true;
                                        break;
                                    }
                                }
                                break;
                        }
                    }

                    if (!isBcbFound)
                    {
                        return false;
                    }
                }
            }

            // filter based on semantic domain
            bool bFoundSemanticDomain = false;
            if (obj is BiblicalTermsData bt)
            {
                //if (! filters.ContainsKey(bt.SemanticDomain))
                //{
                //    filters.Add(bt.SemanticDomain, bt.SemanticDomain);
                //    Debug.WriteLine($"SEMANTIC DOMAIN: {bt.SemanticDomain}");
                //}
                if (SelectedDomain is not null)
                {
                    bFoundSemanticDomain = SelectedDomain[1].ToString() == "BtDomainsAll" || bt.SemanticDomain.Contains(SelectedDomain[0].ToString() ?? string.Empty);
                }

                if (! bFoundSemanticDomain)
                {
                    return false;
                }
            }

            // filter based on rendering filter
            if (obj is BiblicalTermsData renderingFilter)
            {
                if (RenderingFilter is not null)
                {
                    if (RenderingFilter[1].ToString() == "BtRenderingMissingRenderings")
                    {
                        if (renderingFilter.RenderingCount > 0)
                        {
                            return false;
                        }
                    }
                }
            }

            // filter based on word
            if (this.FilterText != "" && FilterText is not null)
            {
                if (obj is BiblicalTermsData btFilter)
                {
                    if (SelectedWordFilterEnum == FilterWordEnum.Gloss)
                    {
                        // make this case insensitive
                        if (btFilter.LocalGloss.IndexOf(FilterText, StringComparison.InvariantCultureIgnoreCase) > -1)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        // make this case insensitive
                        foreach (var rendering in btFilter.Renderings)
                        {
                            if (rendering.IndexOf(FilterText, StringComparison.InvariantCultureIgnoreCase) > -1)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }
            }

            return true;
        }


        private void SetupScopes()
        {
            _scope = new DataTable();
            _scope.Columns.Add("Display");
            _scope.Columns.Add("Value");

            var filter = Enum.GetNames(typeof(FilterScopeEnum));
            foreach (var loc in filter)
            {
                // get the localized rendering for this
                var localizedString = GetLocalizationString.Get(loc, _logger);
                _scope.Rows.Add(localizedString, loc);
            }
        }

        private void SetupRenderingFilters()
        {
            _renderingsFilters = new DataTable();
            _renderingsFilters.Columns.Add("Display");
            _renderingsFilters.Columns.Add("Value");

            var filter = Enum.GetNames(typeof(RenderingFilterEnum));
            foreach (var loc in filter)
            {
                // get the localized rendering for this
                var localizedString = GetLocalizationString.Get(loc, _logger);
                _renderingsFilters.Rows.Add(localizedString, loc);
            }
        }

        /// <summary>
        /// Combo box for the domains
        /// TODO once Paratext fixes their API, we need to collect this information from the ALL Biblical Terms
        /// as right now, this information is null on ALL but filled in for Project
        /// </summary>
        private void SetupSemanticDomains()
        {
            _domains = new DataTable();
            _domains.Columns.Add("Display");
            _domains.Columns.Add("Value");

            var filter = Enum.GetNames(typeof(SemanticDomainEnum));
            foreach (var loc in filter)
            {
                // get the localized rendering for this
                var localizedString = GetLocalizationString.Get(loc, _logger);
                _domains.Rows.Add(localizedString, loc);
            }

            NotifyOfPropertyChange(() => Domains);
        }

        #endregion // Methods

    }
}
