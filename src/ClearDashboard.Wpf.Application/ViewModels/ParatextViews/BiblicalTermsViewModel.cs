using Autofac;
using Caliburn.Micro;
using ClearDashboard.DAL.ViewModels;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.ParatextPlugin.CQRS.Features.BiblicalTerms;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.Panes;
using ClearDashboard.Wpf.Application.ViewModels.PopUps;
using ClearDashboard.Wpf.Application.Views.ParatextViews;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using ClearApplicationFoundation.Framework.Input;
using Point = System.Windows.Point;

// ReSharper disable InconsistentNaming

namespace ClearDashboard.Wpf.Application.ViewModels.ParatextViews
{
    /// <summary>
    /// 
    /// </summary>
    public class BiblicalTermsViewModel : ToolViewModel
    {
        private readonly LongRunningTaskManager _longRunningTaskManager;

        #region Member Variables

        BiblicalTermsView _view;

        private bool _getBiblicalTermsRunning = false;
        private const string TaskName = "BiblicalTerms";


        public enum SelectedBtEnum
        {
            // ReSharper disable once UnusedMember.Local
            OptionAll,
            // ReSharper disable once UnusedMember.Local
            OptionProject
        }

        public enum FilterWordEnum
        {
            // ReSharper disable once UnusedMember.Local
            Gloss,
            // ReSharper disable once UnusedMember.Local
            Rendering
        }

        private enum FilterScopeEnum
        {
            // DO NOT CHANGE THESE IDENTIFIER NAMES - THEY ARE LINKED TO THE
            // LOCALIZATION SPREADSHEET LOOKUPS
            BiblicalTermsBcv_All,
            // ReSharper disable once UnusedMember.Local
            BiblicalTermsBcv_Book,
            // ReSharper disable once UnusedMember.Local
            BiblicalTermsBcv_Chapter,
            // ReSharper disable once UnusedMember.Local
            BiblicalTermsBcv_Verse
        }

        private enum RenderingFilterEnum
        {
            // DO NOT CHANGE THESE IDENTIFIER NAMES - THEY ARE LINKED TO THE
            // LOCALIZATION SPREADSHEET LOOKUPS
            // ReSharper disable once UnusedMember.Local            
            BiblicalTermsRendering_AllTerms,
            // ReSharper disable once UnusedMember.Local
            BiblicalTermsRendering_MissingRenderings
        }

        private enum SemanticDomainEnum
        {
            // DO NOT CHANGE THESE IDENTIFIER NAMES - THEY ARE LINKED TO THE
            // LOCALIZATION SPREADSHEET LOOKUPS
            // ReSharper disable once UnusedMember.Local
            BiblicalTermsDomains_All = 0,
            // ReSharper disable once UnusedMember.Local            
            BiblicalTermsDomains_Affection = 1,
            // ReSharper disable once UnusedMember.Local
            BiblicalTermsDomains_Agriculture = 2,
            // ReSharper disable once UnusedMember.Local
            BiblicalTermsDomains_Animals = 3,
            // ReSharper disable once UnusedMember.Local            
            BiblicalTermsDomains_Area = 4,
            // ReSharper disable once UnusedMember.Local
            BiblicalTermsDomains_AreaNature = 5,
            // ReSharper disable once UnusedMember.Local
            BiblicalTermsDomains_Association = 6,
            // ReSharper disable once UnusedMember.Local
            BiblicalTermsDomains_ConstructionReligiousActivities = 7,
            // ReSharper disable once UnusedMember.Local
            BiblicalTermsDomains_ConstructionsAnimalHusbandry = 8,
            // ReSharper disable once UnusedMember.Local
            BiblicalTermsDomains_ContainersAnimalHusbandry = 9,
            // ReSharper disable once UnusedMember.Local            
            BiblicalTermsDomains_CraftsCloth = 10,
            // ReSharper disable once UnusedMember.Local
            BiblicalTermsDomains_Fruits = 11,
            // ReSharper disable once UnusedMember.Local
            BiblicalTermsDomains_Gemstones = 12,
            // ReSharper disable once UnusedMember.Local
            BiblicalTermsDomains_Grasses = 13,
            // ReSharper disable once UnusedMember.Local            
            BiblicalTermsDomains_Group = 14,
            // ReSharper disable once UnusedMember.Local
            BiblicalTermsDomains_GroupArea = 15,
            // ReSharper disable once UnusedMember.Local
            BiblicalTermsDomains_HonorRespectStatus = 16,
            // ReSharper disable once UnusedMember.Local
            BiblicalTermsDomains_Locale = 17,
            // ReSharper disable once UnusedMember.Local
            BiblicalTermsDomains_MammalsDomesticAnimals = 18,
            // ReSharper disable once UnusedMember.Local
            BiblicalTermsDomains_MammalsWildAnimals = 19,
            // ReSharper disable once UnusedMember.Local
            BiblicalTermsDomains_Monument = 20,
            // ReSharper disable once UnusedMember.Local
            BiblicalTermsDomains_MoralsAndEthics = 21,
            // ReSharper disable once UnusedMember.Local
            BiblicalTermsDomains_Mourning = 22,
            // ReSharper disable once UnusedMember.Local
            BiblicalTermsDomains_Nature = 23,
            // ReSharper disable once UnusedMember.Local
            BiblicalTermsDomains_Paganism = 24,
            // ReSharper disable once UnusedMember.Local
            BiblicalTermsDomains_People = 25,
            // ReSharper disable once UnusedMember.Local
            BiblicalTermsDomains_PeopleAuthority = 26,
            // ReSharper disable once UnusedMember.Local
            BiblicalTermsDomains_PeopleHonorRespectStatus = 27,
            // ReSharper disable once UnusedMember.Local
            BiblicalTermsDomains_Person = 28,
            // ReSharper disable once UnusedMember.Local
            BiblicalTermsDomains_Purpose = 29,
            // ReSharper disable once UnusedMember.Local
            BiblicalTermsDomains_ReligiousActivities = 30,
            // ReSharper disable once UnusedMember.Local
            BiblicalTermsDomains_SacrificesAndOfferings = 31,
            // ReSharper disable once UnusedMember.Local
            BiblicalTermsDomains_Settlement = 32,
            // ReSharper disable once UnusedMember.Local            
            BiblicalTermsDomains_SignsAndWonders = 33,
            // ReSharper disable once UnusedMember.Local
            BiblicalTermsDomains_SupernaturalBeingsAndPowers = 34,
            // ReSharper disable once UnusedMember.Local            
            BiblicalTermsDomains_SupernaturalBeingsAndPowersTitles = 35,
            // ReSharper disable once UnusedMember.Local
            BiblicalTermsDomains_Tools = 36,
            // ReSharper disable once UnusedMember.Local            
            BiblicalTermsDomains_ToolsChildbirth = 37,
            // ReSharper disable once UnusedMember.Local
            BiblicalTermsDomains_ToolsWeightCommerce = 38,
            // ReSharper disable once UnusedMember.Local            
            BiblicalTermsDomains_TreesFruits = 39,
            // ReSharper disable once UnusedMember.Local            
            BiblicalTermsDomains_TreesPerfumesAndSpices = 40,
            // ReSharper disable once UnusedMember.Local
            BiblicalTermsDomains_WisdomUnderstanding = 41,
        }

        private string _currentVerse = "";


        //Dictionary<string, object> filters = new Dictionary<string, object>();
        #endregion //Member Variables

        #region Public Properties

        // public bool IsRtl { get; set; }


        private FlowDirection _windowFlowDirection = FlowDirection.LeftToRight;
        public new FlowDirection WindowFlowDirection
        {
            get => _windowFlowDirection;
            set
            {
                _windowFlowDirection = value;
                NotifyOfPropertyChange(() => WindowFlowDirection);
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
                //SelectedDomain = null;
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

        private Brush _randomBackColorBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0xDD, 0xDD, 0xDD));
        public Brush RandomBackColorBrush
        {
            get { return _randomBackColorBrush; }
            set
            {
                _randomBackColorBrush = value;
                NotifyOfPropertyChange(() => RandomBackColorBrush);
            }
        }


        private string _fontFamily = FontNames.DefaultFontFamily;
        public string FontFamily
        {
            get => _fontFamily;
            set
            {
                _fontFamily = value;
                NotifyOfPropertyChange(() => FontFamily);
            }
        }

        private float _fontSize = 12;

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

        public ICollectionView BiblicalTermsCollectionView { get; set; }


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

        private ObservableCollection<VerseViewModel> _selectedItemVerses = new();
        public ObservableCollection<VerseViewModel> SelectedItemVerses
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
                try
                {
                    NotifyOfPropertyChange(() => ProgressBarVisibility);
                }
                catch (Exception ex)
                {
                    Logger.LogWarning("Setting BiblicalTerms progress bar visibility task was cancelled.", ex);
                }
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



        #endregion //Observable Properties

        #region Commands

        public ICommand NotesCommand { get; set; }
        public ICommand VerseClickCommand { get; set; }
        public RelayCommand ClearFilterCommand { get; }


        #endregion

        #region Constructor

        public BiblicalTermsViewModel()
        {
            // used by Caliburn Micro for design time    
        }

        public BiblicalTermsViewModel(INavigationService navigationService, ILogger<BiblicalTermsViewModel> logger,
            DashboardProjectManager? projectManager, IEventAggregator? eventAggregator, IMediator mediator, ILifetimeScope lifetimeScope, LongRunningTaskManager longRunningTaskManager, ILocalizationService localizationService)
            : base(navigationService, logger, projectManager, eventAggregator, mediator, lifetimeScope, localizationService)
        {
            _longRunningTaskManager = longRunningTaskManager;
            Title = "🕮 " + LocalizationService!.Get("Windows_BiblicalTerms");
            ContentId = "BIBLICALTERMS";
            DockSide = DockSide.Bottom;

            // wire up the commands
            ClearFilterCommand = new RelayCommand(ClearFilter);
            NotesCommand = new RelayCommand(ShowNotes);
            VerseClickCommand = new RelayCommand(VerseClick);
        }

        protected override void OnViewAttached(object view, object context)
        {

            _view = (BiblicalTermsView)view;
            Logger!.LogInformation("OnViewAttached");
            base.OnViewAttached(view, context);
        }

        protected override async void OnViewReady(object view)
        {
            await GetBiblicalTerms(BiblicalTermsType.Project);

            // populate the combo box for scope
            SetupScopes();
            // select the first one
            var drv = Scopes.DefaultView[Scopes.Rows.IndexOf(Scopes.Rows[0])];
            SelectedScope = drv;

            // populate the combo box for rendering filters
            SetupRenderingFilters();
            // select the first one
            drv = RenderingsFilters.DefaultView[RenderingsFilters.Rows.IndexOf(RenderingsFilters.Rows[0])];
            RenderingFilter = drv;

            // populate the combo box for semantic domains
            SetupSemanticDomains();
            // select the first one
            drv = Domains.DefaultView[Domains.Rows.IndexOf(Domains.Rows[0])];
            SelectedDomain = drv;

            try
            {
                // setup the collectionview that binds to the data grid
                OnUIThread(() =>
                {
                    BiblicalTermsCollectionView = CollectionViewSource.GetDefaultView(_biblicalTerms);
                    
                    // setup the method that we go to for filtering
                    BiblicalTermsCollectionView.Filter = FilterGridItems;
                });
            }
            catch (Exception ex)
            {
                Logger.LogWarning("Setting BiblicalTerms progress bar visibility task was cancelled.", ex);
            }


            NotifyOfPropertyChange(() => BiblicalTermsCollectionView);



            if (ProjectManager.CurrentParatextProject != null)
            {
                var paratextProject = ProjectManager.CurrentParatextProject;
                // pull out the project font family
                _fontFamily = paratextProject.Language.FontFamily;
                _fontSize = paratextProject.Language.Size;
                IsRtl = paratextProject.Language.IsRtol;
            }


            Logger.LogInformation("OnViewReady");
            base.OnViewReady(view);
        }

        protected override async Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            //we need to cancel this process here
            //check a bool to see if it already cancelled or already completed
            if (_getBiblicalTermsRunning)
            {
                var cancelled = _longRunningTaskManager.CancelTask(TaskName);

                await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
                {
                    Name = TaskName,
                    Description = "Task was cancelled",
                    EndTime = DateTime.Now,
                    TaskLongRunningProcessStatus = LongRunningTaskStatus.Completed
                }), cancellationToken);
            }
            await base.OnDeactivateAsync(close, cancellationToken);
        }


        #endregion //Constructor

        #region Methods

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="obj"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void ShowNotes(object obj)
        {
#if DEBUG
            throw new NotImplementedException();
#endif
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
            var verses = SelectedItemVerses.Where(v => v.VerseBBCCCVVV.Equals(verseBBCCCVVV)).ToList();

            if (verses.Count > 0)
            {
                var logger = LifetimeScope.Resolve<ILogger<VersePopUpViewModel>>();
                IWindowManager manager = new WindowManager();
                manager.ShowWindowAsync(
                    new VersePopUpViewModel(NavigationService, logger, ProjectManager, EventAggregator, Mediator,
                        verses[0], LifetimeScope, LocalizationService), null, null);
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

                //try
                //{
                OnUIThread(() =>
                {
                    BiblicalTerms.Clear();
                });
                //}
                //catch (Exception ex)
                //{

                //}
                await SetProgressBarVisibilityAsync(Visibility.Visible);

                if (_selectedBiblicalTermsType == SelectedBtEnum.OptionProject)
                {
                    await GetBiblicalTerms(BiblicalTermsType.Project);
                }
                else
                {
                    await GetBiblicalTerms(BiblicalTermsType.All);

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
            await Task.Run(() => { ProgressBarVisibility = visibility; });
            // TODO: COMEBACKHERE
            //System.Windows.Forms.Application.DoEvents();
        }

        /// <summary>
        /// On BiblicalTerms Click - these generate the bottom verse & verse references
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

                for (var i = 0; i < selectedBiblicalTermsData.ReferencesLong.Count; i++)
                {
                    var verseRef = selectedBiblicalTermsData.ReferencesLong[i];
                    var verseText = "";
                    if (i < selectedBiblicalTermsData.ReferencesListText.Count)
                    {
                        verseText = selectedBiblicalTermsData.ReferencesListText[i];
                    }

                    var loc = verseRef.Split(' ');
                    var localizedString = verseRef;

                    if (loc.Length > 1)
                    {
                        localizedString = LocalizationService!.Get(loc[0]) + $" {loc[1]}";
                    }

                    _selectedItemVerses.Add(new VerseViewModel
                    {
                        VerseId = localizedString,
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
                    var sBlank = "".PadLeft(punc.Length, ' ');

                    verseText = verseText.Replace(punc, sBlank);
                }

                // create a list of all the matches within the verse
                var points = new List<Point>();
                var words = new List<string>();

                // get only the distinct renderings otherwise we end up having errors
                var renderings = selectedBiblicalTermsData.Renderings.Distinct().ToList();
                renderings = SortByLength(renderings);

                foreach (var render in renderings)
                {
                    // do the same for the target word that we are trying to test against
                    var puncLessWord = render;
                    foreach (var punc in puncs)
                    {
                        // we need to maintain the same verse length so we need to pad
                        // out the replacement spaces
                        var sBlank = "".PadLeft(punc.Length, ' ');

                        puncLessWord = puncLessWord.Replace(punc, sBlank);
                    }


                    try
                    {
                        // look for full word and case sensitive
                        var pattern = new Regex(@"\b" + puncLessWord + @"\b");
                        var matchResults = pattern.Match(verseText);
                        while (matchResults.Success)
                        {
                            // matched text: matchResults.Value
                            // match start: matchResults.Index
                            // match length: matchResults.Length
                            var point = new Point(matchResults.Index, matchResults.Index + matchResults.Length);
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
                }

                verseText = verse.VerseText;

                // organize the points in lowest to highest
                points = points.OrderBy(o => o.X).Distinct().ToList();

                // iterate through in reverse
                for (var i = points.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        var endPart = verseText.Substring((int)points[i].Y);
                        var startPart = verseText.Substring(0, (int)points[i].X);

                        //var a = new Run(startPart) { FontWeight = FontWeights.Normal };
                        verse.Inlines.Insert(0, new Run(endPart) { FontWeight = FontWeights.Normal });
                        verse.Inlines.Insert(0, new Run(words[i]) { FontWeight = FontWeights.Bold, Foreground = Brushes.Black});

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

        private List<string> SortByLength(List<string> e)
        {
            // Use LINQ to sort the array received and return a copy.
            var sorted = from s in e
                orderby s.Length ascending
                select s;
            return sorted.ToList();
        }


        private void TriggerFilterTermsByWord()
        {
            //refresh the biblical terms collection so the filter runs
            BiblicalTermsCollectionView.Refresh();
        }

        /// <summary>
        /// This is the filter callback for the grid that runs through all the user's filtering
        /// and updates the grid with only those things the user wants to see
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool FilterGridItems(object obj)
        {
            // first check if the row is filtered by the book or not
            var isBcvFound = false;
            if (SelectedScope is { } rowView)
            {
                var selectedScope = rowView[1].ToString();

                // filter down to scope if present
                if (selectedScope != FilterScopeEnum.BiblicalTermsBcv_All.ToString())
                {
                    if (ProjectManager.CurrentVerse.Length != 9)
                    {
                        return false;
                    }

                    if (obj is BiblicalTermsData terms)
                    {
                        switch (selectedScope)
                        {
                            case "BiblicalTermsBcv_Book":
                                foreach (var term in terms.References)
                                {
                                    _currentBcv.SetVerseFromId(term);
                                    var book = _currentBcv.Book.ToString();
                                    if (book == ProjectManager.CurrentVerse.Substring(0, 3))
                                    {
                                        // found the book
                                        isBcvFound = true;
                                        break;
                                    }
                                }

                                break;
                            case "BiblicalTermsBcv_Chapter":
                                foreach (var term in terms.References)
                                {
                                    _currentBcv.SetVerseFromId(term);
                                    var book = _currentBcv.Book.ToString();
                                    var chapter = _currentBcv.ChapterIdText.ToString();
                                    if (book + chapter == ProjectManager.CurrentVerse.Substring(0, 6))
                                    {
                                        // found the chapter
                                        isBcvFound = true;
                                        break;
                                    }
                                }

                                break;
                            case "BiblicalTermsBcv_Verse":
                                foreach (var term in terms.References)
                                {
                                    if (term.PadLeft(9, '0') == ProjectManager.CurrentVerse)
                                    {
                                        // found the verse
                                        isBcvFound = true;
                                        break;
                                    }
                                }
                                break;
                        }
                    }

                    if (!isBcvFound)
                    {
                        return false;
                    }
                }
            }

            // filter based on semantic domain (only in )
            if (SelectedBiblicalTermsType == SelectedBtEnum.OptionProject)
            {
                var bFoundSemanticDomain = false;
                if (obj is BiblicalTermsData bt)
                {
                    if (SelectedDomain is not null && bt.SemanticDomain is not null)
                    {
                        bFoundSemanticDomain = SelectedDomain[1].ToString() == "BiblicalTermsDomains_All" ||
                                               bt.SemanticDomain.Contains(SelectedDomain[0].ToString() ?? string.Empty);
                    }

                    if (bFoundSemanticDomain == false)
                    {
                        return false;
                    }
                }
            }

            // filter based on rendering filter
            if (obj is BiblicalTermsData renderingFilter)
            {
                if (RenderingFilter is not null)
                {
                    if (RenderingFilter[1].ToString() == "BiblicalTermsRendering_MissingRenderings")
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
                        if (btFilter.Gloss.IndexOf(FilterText, StringComparison.InvariantCultureIgnoreCase) > -1)
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


        /// <summary>
        /// populates the Scopes list for the view's combobox
        /// </summary>
        private void SetupScopes()
        {
            _scope = new DataTable();
            _scope.Columns.Add("Display");
            _scope.Columns.Add("Value");

            var filter = Enum.GetNames(typeof(FilterScopeEnum));
            foreach (var loc in filter)
            {
                // get the localized rendering for this
                var localizedString = LocalizationService!.Get(loc);
                _scope.Rows.Add(localizedString, loc);
            }
            NotifyOfPropertyChange(() => Scopes);   
        }

        /// <summary>
        /// populates the rendering filters for the view's combobox
        /// </summary>
        private void SetupRenderingFilters()
        {
            _renderingsFilters = new DataTable();
            _renderingsFilters.Columns.Add("Display");
            _renderingsFilters.Columns.Add("Value");

            var filter = Enum.GetNames(typeof(RenderingFilterEnum));
            foreach (var loc in filter)
            {
                // get the localized rendering for this
                var localizedString = LocalizationService!.Get(loc);
                _renderingsFilters.Rows.Add(localizedString, loc);
            }
            NotifyOfPropertyChange(() => RenderingsFilters);
        }

        /// <summary>
        /// Combo box for the domains
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
                var localizedString = LocalizationService!.Get(loc);
                _domains.Rows.Add(localizedString, loc);
            }

            try
            {
                // will throw an error when the user shuts down the program
                // shortly after it appears
                NotifyOfPropertyChange(() => Domains);
            }
            catch (Exception)
            {
                // no-op
            }
        }

        private async Task GetBiblicalTerms(BiblicalTermsType type = BiblicalTermsType.Project)
        {
            _getBiblicalTermsRunning = true;

            var task = _longRunningTaskManager.Create(TaskName, LongRunningTaskStatus.Running);
            var cancellationToken = task.CancellationTokenSource!.Token;

            // send to the task started event aggregator for everyone else to hear about a background task starting
            await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
            {
                Name = TaskName,
                Description = "Requesting BiblicalTerms data...",
                StartTime = DateTime.Now,
                TaskLongRunningProcessStatus = LongRunningTaskStatus.Running
            }), cancellationToken);

            try
            {
                await SetProgressBarVisibilityAsync(Visibility.Visible);

                OnUIThread(() => { _biblicalTerms.Clear(); });

                // deserialize the list
                var biblicalTermsList = new List<BiblicalTermsData>();
                try
                {
                    var result = await ExecuteRequest(new GetBiblicalTermsByTypeQuery(type), cancellationToken);
                    if (result.Success)
                    {
                        biblicalTermsList = result.Data;

                        // send to the task started event aggregator for everyone else to hear about a background task starting
                        await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
                        {
                            Name = TaskName,
                            Description = "BiblicalTerms Loaded",
                            EndTime = DateTime.Now,
                            TaskLongRunningProcessStatus = LongRunningTaskStatus.Completed
                        }), cancellationToken);


                        OnUIThread(() =>
                        {
                            if (biblicalTermsList.Count > 0)
                            {
                                for (var i = 0; i < biblicalTermsList.Count; i++)
                                {
                                    _biblicalTerms.Add(biblicalTermsList[i]);

                                    var renderings = biblicalTermsList[i].Renderings.Distinct().OrderBy(x => x).ToList();
                                    // group similar words if the list is long
                                    if (renderings.Count > 6)
                                    {

                                        renderings = SortByLength(renderings);
                                        int shortest = renderings[0].Length;

                                        // group by shortest length of string
                                        var renderingGroups = renderings
                                            .GroupBy(x => x.Substring(0, shortest));

                                        List<RenderingStringParts> sTemp = new();
                                        foreach (var renderingGroup in renderingGroups)
                                        {
                                            if (renderingGroup.Count() == 1)
                                            {
                                                var term = renderings.FirstOrDefault(x => x.StartsWith(renderingGroup.Key));

                                                if (term is not null)
                                                {
                                                    sTemp.Add(new RenderingStringParts { RenderingString = term });
                                                }
                                                else
                                                {
                                                    sTemp.Add(new RenderingStringParts { RenderingString = renderingGroup.Key });
                                                }
                                                
                                            }
                                            else
                                            {
                                                var terms = renderings.Where(x => x.StartsWith(renderingGroup.Key));

                                                if (terms is not null)
                                                {
                                                    var termsString = String.Join("\n", terms);

                                                    sTemp.Add(new RenderingStringParts
                                                    {
                                                        RenderingString = renderingGroup.Key + "*",
                                                        RenderingStringHover = termsString
                                                    });
                                                }
                                                else
                                                {
                                                    sTemp.Add(new RenderingStringParts
                                                    {
                                                        RenderingString = renderingGroup.Key + "*"
                                                    });
                                                }
                                                
                                            }
                                        }

                                        _biblicalTerms[i].RenderingString = sTemp;

                                        cancellationToken.ThrowIfCancellationRequested();
                                    }
                                    else
                                    {
                                        List<RenderingStringParts> sTemp = new();
                                        foreach (var rendering in renderings)
                                        {
                                            sTemp.Add(new RenderingStringParts
                                            {
                                                RenderingString = rendering
                                            });
                                        }

                                        if (_biblicalTerms[i].RenderingString != null)
                                        {
                                            _biblicalTerms[i].RenderingString.AddRange(sTemp);
                                        }
                                        else
                                        {
                                            _biblicalTerms[i].RenderingString = sTemp;
                                        }
                                        
                                    }

                                    // check to see if every verse has been accounted for
                                    if (_biblicalTerms[i].RenderingCount == _biblicalTerms[i].ReferencesLong.Count)
                                    {
                                        _biblicalTerms[i].Found = true;
                                    }

                                }

                                NotifyOfPropertyChange(() => BiblicalTerms);
                            }
                        });
                    }

                }
                catch (OperationCanceledException ex)
                {
                    Logger!.LogInformation("BiblicalTermsViewModel.GetBiblicalTerms() - an exception was thrown -> cancellation was requested.");
                }
                catch (Exception ex)
                {
                    Logger!.LogError($"BiblicalTermsViewModel Deserialize BiblicalTerms: {ex.Message}");

                    if (!cancellationToken.IsCancellationRequested)
                    {
                        await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(
                            new BackgroundTaskStatus
                            {
                                Name = TaskName,
                                EndTime = DateTime.Now,
                                ErrorMessage = $"{ex}",
                                TaskLongRunningProcessStatus = LongRunningTaskStatus.Failed
                            }), cancellationToken);
                    }
                }


            }
            //catch (Exception ex)
            //{

            //}
            finally
            {
                await SetProgressBarVisibilityAsync(Visibility.Hidden);
                _getBiblicalTermsRunning = false;
                _longRunningTaskManager.TaskComplete(TaskName);
            }
        }

        private void ClearFilter(object obj)
        {
            FilterText = "";
        }


        public void LaunchMirrorView(double actualWidth, double actualHeight)
        {
            LaunchMirrorView<BiblicalTermsView>.Show(this, actualWidth, actualHeight, this.Title);
        }
        #endregion // Methods
    }
}
