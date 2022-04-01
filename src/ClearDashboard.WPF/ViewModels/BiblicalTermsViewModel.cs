using Caliburn.Micro;
using ClearDashboard.Common.Models;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.DataAccessLayer.NamedPipes;
using ClearDashboard.Wpf.Helpers;
using ClearDashboard.Wpf.Interfaces;
using ClearDashboard.Wpf.ViewModels.Panes;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pipes_Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using AvalonDock.Controls;
using ClearDashboard.Wpf.Models;
using Point = System.Windows.Point;

namespace ClearDashboard.Wpf.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class BiblicalTermsViewModel : ToolViewModel, IWorkspace
    {
        #region Member Variables

        public enum SelectedBTtype
        {
            OptionAll,
            OptionProject
        }

        public enum FilterWordType
        {
            English,
            Rendering
        }

        public enum FilterScope
        {
            All,
            Book,
            Chapter,
            Verse
        }

        public ILogger Logger { get; set; }
        public INavigationService NavigationService { get; set; }
        public ProjectManager ProjectManager { get; set; }
        private string _currentVerse = "";




        //Dictionary<string, object> filters = new Dictionary<string, object>();
        #endregion //Member Variables

        #region Public Properties


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
            get { return _gloss; }
            set
            {
                _gloss = value;
                NotifyOfPropertyChange(() => Gloss);
            }
        }

        private DataTable _scope;
        public DataTable Scopes
        {
            get { return _scope; }
            set
            {
                _scope = value;
                NotifyOfPropertyChange(() => Scopes);
            }
        }

        private DataRowView _selectedScope;
        public DataRowView SelectedScope
        {
            get { return _selectedScope; }
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



        private BindableCollection<string> _domains;
        public BindableCollection<string> Domains
        {
            get { return _domains; }
            set
            {
                _domains = value;
                NotifyOfPropertyChange(() => Domains);
            }
        }

        private string _selectedDomain = string.Empty;
        public string SelectedDomain
        {
            get { return _selectedDomain; }
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

        private SelectedBTtype _lastSelectedBTtype = SelectedBTtype.OptionProject;

        private SelectedBTtype _selectedBiblicalTermsType = SelectedBTtype.OptionProject;
        public SelectedBTtype SelectedBiblicalTermsType
        {
            get => _selectedBiblicalTermsType;
            set
            {
                _selectedBiblicalTermsType = value;
                NotifyOfPropertyChange(() => SelectedBiblicalTermsType);

                // reset the semantic domains & filter
                FilterText = "";
                SelectedDomain = null;

                SwitchedBibilicalTermsType();
            }
        }

        private FilterWordType _selectedWordFilterType = FilterWordType.English;
        public FilterWordType SelectedWordFilterType
        {
            get { return _selectedWordFilterType; }
            set
            {
                _selectedWordFilterType = value;
                NotifyOfPropertyChange(() => SelectedWordFilterType);

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
            get { return _fontSize; }
            set
            {
                _fontSize = value;
                NotifyOfPropertyChange(() => FontSize);
            }
        }


        private bool _isRTL = false;
        public bool IsRTL
        {
            get => _isRTL;
            set
            {
                _isRTL = value;
                NotifyOfPropertyChange(() => IsRTL);
            }
        }

        public ICollectionView BiblicalTermsCollectionView { get; }

        private ObservableCollection<BiblicalTermsData> _biblicalTerms = new ObservableCollection<BiblicalTermsData>();
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
            get { return _selectedBiblicalTermsData; }
            set
            {
                _selectedBiblicalTermsData = value;
                NotifyOfPropertyChange(() => SelectedBiblicalTermsData);
                UpdateSelectedTerm(SelectedBiblicalTermsData);
            }
        }

        private ObservableCollection<Verse> _selectedItemVerses = new ObservableCollection<Verse>();
        public ObservableCollection<Verse> SelectedItemVerses
        {
            get { return _selectedItemVerses; }
            set
            {
                _selectedItemVerses = value;
                NotifyOfPropertyChange(() => SelectedItemVerses);
            }
        }

        private ObservableCollection<string> _renderings = new ObservableCollection<string>();
        public ObservableCollection<string> Renderings
        {
            get { return _renderings; }
            set
            {
                _renderings = value;
                NotifyOfPropertyChange(() => Renderings);
            }
        }

        private ObservableCollection<string> _renderingsText = new ObservableCollection<string>();
        public ObservableCollection<string> RenderingsText
        {
            get { return _renderingsText; }
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

        private ICommand _notesCommand;
        public ICommand NotesCommand
        {
            get => _notesCommand;
            set
            {
                _notesCommand = value;
            }
        }

        private ICommand _verseClickCommand;
        public ICommand VerseClickCommand
        {
            get => _verseClickCommand;
            set
            {
                _verseClickCommand = value;
            }
        }

        #endregion

        #region Constructor
        public BiblicalTermsViewModel(INavigationService navigationService, ILogger<WorkSpaceViewModel> logger, ProjectManager projectManager)
        {
            this.NavigationService = navigationService;
            this.Logger = logger;
            this.ProjectManager = projectManager;

            this.Title = "🕮 BIBLICAL TERMS";
            this.ContentId = "BIBLICALTERMS";
            this.DockSide = EDockSide.Left;

            // listen to the DAL event messages coming in
            ProjectManager.NamedPipeChanged += HandleEventAsync;

            // populate the combo box for semantic domains
            SetupSemanticDomains();
            SelectedDomain = Domains[0];

            // populate the combo box for scope
            SetupScopes();
            // select the first one
            //SelectedScope = new DataRowView() Scopes.Rows[0][0].ToString();


            // setup the collectionview that binds to the datagrid
            BiblicalTermsCollectionView = CollectionViewSource.GetDefaultView(this._biblicalTerms);

            // setup the method that we go to for filtering
            BiblicalTermsCollectionView.Filter = FilterGridItems;

            // wire up the commands
            NotesCommand = new RelayCommand(ShowNotes);
            VerseClickCommand = new RelayCommand(VerseClick);

            if (projectManager.Project is not null)
            {
                // pull out the project font family
                _fontFamily = projectManager.Project.Language.FontFamily;
                _fontSize = projectManager.Project.Language.Size;
                _isRTL = projectManager.Project.Language.IsRtol;
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

            PipeMessage pipeMessage = args.PM;

            switch (pipeMessage.Action)
            {
                case ActionType.CurrentVerse:
                    if (_currentVerse != pipeMessage.Text)
                    {
                        // ask for Biblical Terms
                        await ProjectManager.SendPipeMessage((ProjectManager.PipeAction)ActionType.GetBibilicalTermsProject)
                            .ConfigureAwait(false);
                    }

                    break;
                case ActionType.SetBiblicalTerms:
                    await SetProgBarVisibilityAsync(Visibility.Visible).ConfigureAwait(false);
                    // invoke to get it to run in STA mode
                    Application.Current.Dispatcher.Invoke(delegate
                    {
                        _biblicalTerms.Clear();

                        // deserialize the list
                        List<BiblicalTermsData> biblicalTermsList = new List<BiblicalTermsData>();
                        try
                        {
                            biblicalTermsList = JsonConvert.DeserializeObject<List<BiblicalTermsData>>((string)pipeMessage.Payload);
                        }
                        catch (Exception e)
                        {
                            Logger.LogError($"BiblicalTermsViewModel Deserialize BibilicalTerms: {e.Message}");
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

                    await SetProgBarVisibilityAsync(Visibility.Hidden).ConfigureAwait(false);
                    break;
            }
        }



        protected override void OnViewAttached(object view, object context)
        {
            Debug.WriteLine("OnViewAttached");
            base.OnViewAttached(view, context);
        }

        protected override void OnViewLoaded(object view)
        {
            Debug.WriteLine("OnViewLoaded");
            base.OnViewLoaded(view);
        }

        protected override void OnViewReady(object view)
        {
            Debug.WriteLine("OnViewReady");
            base.OnViewReady(view);
        }

        protected override void Dispose(bool disposing)
        {
            // unsubscribe from events
            ProjectManager.NamedPipeChanged -= HandleEventAsync;

            Debug.WriteLine("Dispose");
            base.Dispose(disposing);
        }

        #endregion //Constructor

        #region Methods

        private void ToggleCurrentVerse()
        {
            //refresh the biblicalterms collection so the filter runs
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
            Console.WriteLine();


            //LayoutAnchorableFloatingWindowControl lfwc = (LayoutAnchorableFloatingWindowControl)Activator.CreateInstance(typeof(LayoutAnchorableFloatingWindowControl), BindingFlags.NonPublic | BindingFlags.Instance, null, new object[] { lfw }, CultureInfo.InvariantCulture);
            //dm.UpdateLayout();
            //lfwc.Width = 300;
            //lfwc.Height = 300;
            //lfwc.Topmost = true;
            //lfwc.Show();
        }



        /// <summary>
        /// User has switched the toggle for All/Project Bibilical Terms
        /// </summary>
        /// <returns></returns>
        private async Task SwitchedBibilicalTermsType()
        {
            if (_lastSelectedBTtype != _selectedBiblicalTermsType)
            {
                BiblicalTerms.Clear();
                await SetProgBarVisibilityAsync(Visibility.Visible).ConfigureAwait(false);

                if (_selectedBiblicalTermsType == SelectedBTtype.OptionProject)
                {
                    await ProjectManager.SendPipeMessage(ProjectManager.PipeAction.GetBiblicalTermsProject).ConfigureAwait(false);
                }
                else
                {
                    await ProjectManager.SendPipeMessage(ProjectManager.PipeAction.GetBiblicalTermsAll).ConfigureAwait(false);
                }

                _lastSelectedBTtype = _selectedBiblicalTermsType;
            }
        }

        /// <summary>
        /// Turn on/off the progress bar asyncronously so the UI can render it
        /// </summary>
        /// <param name="visibility"></param>
        /// <returns></returns>
        private async Task SetProgBarVisibilityAsync(Visibility visibility)
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

                    _selectedItemVerses.Add(new Verse
                    {
                        VerseID = verseRef,
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

                // create a punctionless version of the verse
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
                    catch (ArgumentException ex)
                    {
                        // Syntax error in the regular expression
                    }

                    //// interate through in while loop
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

                // interate through in reverse
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
                        Console.WriteLine(e);
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
            //refresh the biblicalterms collection so the filter runs
            BiblicalTermsCollectionView.Refresh();
        }

        /// <summary>
        /// This is the filter callback for the grid
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool FilterGridItems(object obj)
        {
            if (SelectedScope is DataRowView rowView)
            {
                var selectedScope = rowView[1].ToString();

                // filter down to scope if present
                if (selectedScope != "All")
                {
                    if (ProjectManager.CurrentVerse.Length != 8)
                    {
                        return false;
                    }

                    if (obj is BiblicalTermsData)
                    {
                        var terms = (BiblicalTermsData)obj;
                        bool bFound = false;
                        switch (selectedScope)
                        {
                            case "Book":
                                foreach (var term in terms.References)
                                {
                                    string book = term.Substring(0, 2);
                                    if (book == ProjectManager.CurrentVerse.Substring(0, 2))
                                    {
                                        return true;
                                    }
                                }

                                break;
                            case "Chapter":
                                foreach (var term in terms.References)
                                {
                                    string chapter = term.Substring(0, 5);
                                    if (chapter == ProjectManager.CurrentVerse.Substring(0, 5))
                                    {
                                        return true;
                                    }
                                }

                                break;
                            case "Verse":
                                foreach (var term in terms.References)
                                {
                                    if (term == ProjectManager.CurrentVerse)
                                    {
                                        return true;
                                    }
                                }

                                break;
                        }
                    }

                    return false;
                }
            }


            // filter based on word
            if (this.FilterText != "" && FilterText is not null)
            {
                if (obj is BiblicalTermsData btFilter)
                {
                    if (SelectedWordFilterType == FilterWordType.English)
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

            // filter based on semantic domain
            if (obj is BiblicalTermsData bt)
            {
                //if (! filters.ContainsKey(bt.SemanticDomain))
                //{
                //    filters.Add(bt.SemanticDomain, bt.SemanticDomain);
                //    Debug.WriteLine($"SEMANTIC DOMAIN: {bt.SemanticDomain}");
                //}

                if (SelectedDomain == "" || SelectedDomain == "*" || SelectedDomain is null)
                {
                    return true;
                }

                return bt.SemanticDomain.Contains(SelectedDomain);
            }



            return false;
        }


        private void SetupScopes()
        {
            _scope = new DataTable();
            _scope.Columns.Add("Display");
            _scope.Columns.Add("Value");

            var scopes = Enum.GetNames(typeof(FilterScope));
            for (int i = 0; i < scopes.Length; i++)
            {
                _scope.Rows.Add(scopes[i], scopes[i]);
            }
        }

        /// <summary>
        /// Combo box for the domains
        /// TODO once Paratext fixes their API, we need to collect this information from the ALL Biblical Terms
        /// as right now, this information is null on ALL but filled in for Project
        /// </summary>
        private void SetupSemanticDomains()
        {
            _domains = new BindableCollection<string>();
            _domains.Add("*");
            _domains.Add("affection");
            _domains.Add("agriculture");
            _domains.Add("animals");
            _domains.Add("area");
            _domains.Add("area; nature");
            _domains.Add("association");
            _domains.Add("construction; religious activities");
            _domains.Add("constructions; animal husbandry");
            _domains.Add("containers; animal husbandry");
            _domains.Add("crafts; cloth");
            _domains.Add("fruits");
            _domains.Add("gemstones");
            _domains.Add("grasses");
            _domains.Add("group");
            _domains.Add("group; area");
            _domains.Add("honor, respect, status");
            _domains.Add("locale");
            _domains.Add("mammals; domestic animals");
            _domains.Add("mammals; wild animals");
            _domains.Add("monument");
            _domains.Add("morals and ethics");
            _domains.Add("mourning");
            _domains.Add("nature");
            _domains.Add("paganism");
            _domains.Add("people");
            _domains.Add("people; authority");
            _domains.Add("people; honor, respect, status");
            _domains.Add("person");
            _domains.Add("purpose");
            _domains.Add("religious activities");
            _domains.Add("sacrifices and offerings");
            _domains.Add("settlement");
            _domains.Add("signs and wonders");
            _domains.Add("supernatural beings and powers");
            _domains.Add("supernatural beings and powers; titles");
            _domains.Add("tools");
            _domains.Add("tools; childbirth");
            _domains.Add("tools; weight; commerce");
            _domains.Add("trees; fruits");
            _domains.Add("trees; perfumes and spices");
            _domains.Add("wisdom, understanding");
            NotifyOfPropertyChange(() => Domains);
        }

        #endregion // Methods

    }
}
