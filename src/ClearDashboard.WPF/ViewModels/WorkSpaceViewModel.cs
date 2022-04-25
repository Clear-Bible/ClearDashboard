using AvalonDock.Layout.Serialization;
using AvalonDock.Themes;
using Caliburn.Micro;
using ClearDashboard.Common.Models;
using ClearDashboard.DataAccessLayer.NamedPipes;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.ViewModels.Menus;
using ClearDashboard.Wpf.ViewModels.Panes;
using Microsoft.Extensions.Logging;
using Pipes_Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;


namespace ClearDashboard.Wpf.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class WorkSpaceViewModel : ApplicationScreen
    {
        #region Member Variables

        private static WorkSpaceViewModel _this;
        public static WorkSpaceViewModel This => _this;

        public DashboardProject DashboardProject { get; set; }
        private DashboardViewModel _dashboardViewModel;


        #endregion //Member Variables

        #region Public Properties

        private bool _paratextSync = true;
        public bool ParatextSync
        {
            get => _paratextSync;
            set
            {
                _paratextSync = value;
                NotifyOfPropertyChange(() => ParatextSync);
            }
        }


        public event EventHandler ActiveDocumentChanged;
        private FileViewModel _activeDocument = null;
        public FileViewModel ActiveDocument
        {
            get => _activeDocument;
            set
            {
                if (_activeDocument != value)
                {
                    _activeDocument = value;
                    NotifyOfPropertyChange(() => MenuItems);
                    if (ActiveDocumentChanged != null)
                        ActiveDocumentChanged(this, EventArgs.Empty);
                }
            }
        }

        public bool IsRtl { get; set; } = false;

        public bool OutGoingChangesStarted { get; set; } = false;

        #endregion //Public Properties

        #region Commands

        //private RelayCommand _openCommand = null;
        //public ICommand OpenCommand
        //{
        //    get
        //    {
        //        if (_openCommand == null)
        //        {
        //            _openCommand = new RelayCommand((p) => LoadLayout(p), null);
        //        }

        //        return _openCommand;
        //    }
        //}


        #endregion  //Commands

        #region Observable Properties

        private ObservableCollection<int> _verseNums = new();
        public ObservableCollection<int> VerseNums
        {
            get => _verseNums;
            set
            {
                _verseNums = value;
                NotifyOfPropertyChange(() => VerseNums);
            }
        }
        
        private Dictionary<string, string> _BCVDictionary;
        public Dictionary<string, string> BCVDictionary
        {
            get => _BCVDictionary;
            set
            {
                _BCVDictionary = value;
                NotifyOfPropertyChange(() => BCVDictionary);
            }
        }

        private ObservableCollection<string> _bookNames = new();
        public ObservableCollection<string> BookNames
        {
            get => _bookNames;
            set
            {
                _bookNames = value;
                NotifyOfPropertyChange(() => BookNames);
            }
        }


        private string _verseRef;
        public string VerseRef
        {
            get => _verseRef;
            set
            {
                _verseRef = value;
                NotifyOfPropertyChange(() => VerseRef);
            }
        }

        private string _windowIdToLoad;
        public string WindowIDToLoad
        {
            get => _windowIdToLoad;
            set
            {
                _windowIdToLoad = value;
                NotifyOfPropertyChange(() => WindowIDToLoad);
                OnPropertyChanged("WindowIDToLoad");
            }
        }

        private ObservableCollection<MenuItemViewModel> _menuItems = new ObservableCollection<MenuItemViewModel>
        {
            new MenuItemViewModel{ Header="Layouts"},
            new MenuItemViewModel{ Header="Windows"},
            new MenuItemViewModel{ Header="Help"},
        };
        public ObservableCollection<MenuItemViewModel> MenuItems
        {
            get => _menuItems;
            set
            {
                _menuItems = value;
                NotifyOfPropertyChange(() => MenuItems);
            }
        }


        ObservableCollection<ToolViewModel> _tools = new ObservableCollection<ToolViewModel>();
        public ObservableCollection<ToolViewModel> Tools
        {
            get => _tools;
            set
            {
                _tools = value;
                NotifyOfPropertyChange(() => Tools);
            }
        }


        ObservableCollection<PaneViewModel> _files = new ObservableCollection<PaneViewModel>();
        public ObservableCollection<PaneViewModel> Files
        {
            get => _files;
            set
            {
                _files = value;
                NotifyOfPropertyChange(() => Files);
            }
        }
        public List<Tuple<string, Theme>> Themes { get; set; }

        private Tuple<string, Theme> _selectedTheme;
        public Tuple<string, Theme> SelectedTheme
        {
            get => _selectedTheme;
            set
            {
                _selectedTheme = value;
                NotifyOfPropertyChange(() => SelectedTheme);
            }
        }

        private BookChapterVerse _currentBcv = new();
        public BookChapterVerse CurrentBcv
        {
            get => _currentBcv;
            set
            {
                _currentBcv = value;
                NotifyOfPropertyChange(() => CurrentBcv);
            }
        }




        #endregion //Observable Properties

        #region Constructor

        /// <summary>
        /// Required for design-time support
        /// </summary>
        public WorkSpaceViewModel()
        {

        }

        public WorkSpaceViewModel(INavigationService navigationService,
            ILogger<WorkSpaceViewModel> logger, ProjectManager projectManager)
            : base(navigationService, logger, projectManager)
        {

            ProjectManager.NamedPipeChanged += HandleEventAsync;

            _this = this;

            Themes = new List<Tuple<string, Theme>>
            {
                new Tuple<string, Theme>(nameof(Vs2013DarkTheme),new Vs2013DarkTheme()),
                new Tuple<string, Theme>(nameof(Vs2013LightTheme),new Vs2013LightTheme()),
                new Tuple<string, Theme>(nameof(AeroTheme),new AeroTheme()),
                new Tuple<string, Theme>(nameof(Vs2013BlueTheme),new Vs2013BlueTheme()),
                new Tuple<string, Theme>(nameof(GenericTheme), new GenericTheme()),
                new Tuple<string, Theme>(nameof(ExpressionDarkTheme),new ExpressionDarkTheme()),
                new Tuple<string, Theme>(nameof(ExpressionLightTheme),new ExpressionLightTheme()),
                new Tuple<string, Theme>(nameof(MetroTheme),new MetroTheme()),
                new Tuple<string, Theme>(nameof(VS2010Theme),new VS2010Theme()),
            };

            if (Properties.Settings.Default.Theme == MaterialDesignThemes.Wpf.BaseTheme.Dark)
            {
                // toggle the Dark theme for AvalonDock
                this.SelectedTheme = Themes[0];
            }
            else
            {
                // toggle the light theme for AvalonDock
                this.SelectedTheme = Themes[1];
            }

            // check if we are in design mode or not
            if (Application.Current != null)
            {
                // TODO

                // subscribe to change events in the parent's theme
                ((App)Application.Current).ThemeChanged += WorkSpaceViewModel_ThemeChanged;

                if (Application.Current is App)
                {
                    DashboardProject = (Application.Current as App)?.SelectedDashboardProject;
                }
            }


            // Subscribe to changes of the Book Chapter Verse data object.
            CurrentBcv.PropertyChanged += BcvChanged;
        }

        public async void Init()
        {
            // initiate the menu system
            MenuItems.Clear();
            MenuItems = new ObservableCollection<MenuItemViewModel>
            {
                new MenuItemViewModel { Header = "Layouts", Id = "LayoutID", ViewModel = this, },
                new MenuItemViewModel
                {
                    Header = "Windows", Id = "WindowID", ViewModel = this,
                    MenuItems = new ObservableCollection<MenuItemViewModel>
                    {
                        new MenuItemViewModel { Header = "Alignment Tool", Id = "AlignmentToolID", ViewModel = this, },
                        new MenuItemViewModel { Header = "Biblical Terms", Id = "BiblicalTermsID", ViewModel = this, },
                        new MenuItemViewModel { Header = "Concordance Tool", Id = "ConcordanceToolID", ViewModel = this, },
                        new MenuItemViewModel { Header = "Dashboard", Id = "DashboardID", ViewModel = this, },
                        new MenuItemViewModel { Header = "Notes", Id = "NotesID", ViewModel = this, },
                        new MenuItemViewModel { Header = "PINS", Id = "PINSID", ViewModel = this, },
                        new MenuItemViewModel { Header = "Word Meanings", Id = "WordMeaningsID", ViewModel = this, },
                        new MenuItemViewModel { Header = "Source Context", Id = "SourceContextID", ViewModel = this, },
                        new MenuItemViewModel { Header = "Start Page", Id = "StartPageID", ViewModel = this, },
                        new MenuItemViewModel { Header = "Target Context", Id = "TargetContextID", ViewModel = this, },
                        new MenuItemViewModel
                            { Header = "Text Collection", Id = "TextCollectionID", ViewModel = this, },
                    }
                },
                new MenuItemViewModel { Header = "Help", Id = "HelpID", ViewModel = this, }
            };


            // add in the document panes
            _files.Clear();


            Debug.WriteLine(DashboardProject.Name);
            _dashboardViewModel = IoC.Get<DashboardViewModel>();
            _files.Add(_dashboardViewModel);

            _files.Add(IoC.Get<ConcordanceViewModel>());
            _files.Add(IoC.Get<StartPageViewModel>());
            _files.Add(IoC.Get<AlignmentToolViewModel>());
            // trigger property changed event
            Files.Add(IoC.Get<TreeDownViewModel>());


            // add in the tool panes
            _tools.Clear();
            _tools.Add(IoC.Get<BiblicalTermsViewModel>());
            _tools.Add(IoC.Get<WordMeaningsViewModel>());
            _tools.Add(IoC.Get<SourceContextViewModel>());
            _tools.Add(IoC.Get<TargetContextViewModel>());
            _tools.Add(IoC.Get<NotesViewModel>());
            _tools.Add(IoC.Get<PinsViewModel>());
            // trigger property changed event
            Tools.Add(new TextCollectionViewModel());


            if (ProjectManager.ParatextProject is not null)
            {
                // unique ids for all the verses in the project
                BCVDictionary = ProjectManager.ParatextProject.BCVDictionary;
            }

            // add in the books
            var books = Helpers.Helpers.GetBookIdDictionary();

            // ensure that the book is in the project
            foreach (var book in ProjectManager.CurrentDashboardProject.TargetProject.BooksList)
            {
                if (book.Available)
                {
                    var bookID = book.BookId.ToString();
                    if (books.ContainsKey(bookID))
                    {
                        _bookNames.Add(books[bookID]);
                    }
                }
            }

            NotifyOfPropertyChange(() => BookNames);

            await ProjectManager.SendPipeMessage(ProjectManager.PipeAction.GetCurrentVerse).ConfigureAwait(false);
        }

        protected override void OnViewAttached(object view, object context)
        {
            base.OnViewAttached(view, context);
            Init();
        }

        protected override void Dispose(bool disposing)
        {
            ProjectManager.NamedPipeChanged -= HandleEventAsync;
            base.Dispose(disposing);
        }

        #endregion //Constructor

        #region Methods

        private void WorkSpaceViewModel_ThemeChanged()
        {
            // TODO

            var newTheme = ((App)Application.Current).Theme;
            if (newTheme == MaterialDesignThemes.Wpf.BaseTheme.Dark)
            {
                // toggle the Dark theme for AvalonDock
                this.SelectedTheme = Themes[0];
            }
            else
            {
                // toggle the light theme for AvalonDock
                this.SelectedTheme = Themes[1];
            }
        }


        public void LoadLayout(XmlLayoutSerializer layoutSerializer)
        {
            // Here I've implemented the LayoutSerializationCallback just to show
            //  a way to feed layout deserialization with content loaded at runtime
            // Actually I could in this case let AvalonDock to attach the contents
            // from current layout using the content ids
            // LayoutSerializationCallback should anyway be handled to attach contents
            // not currently loaded
            layoutSerializer.LayoutSerializationCallback += (s, e) =>
            {
                switch (e.Model.ContentId.ToUpper())
                {
                    case WorkspaceLayoutNames.Dashboard:
                        e.Content = _dashboardViewModel ?? new DashboardViewModel();
                        break;
                    case WorkspaceLayoutNames.ConcordanceTool:
                        e.Content = new ConcordanceViewModel();
                        break;
                    case WorkspaceLayoutNames.BiblicalTerms:
                        e.Content = IoC.Get<BiblicalTermsViewModel>();
                        break;
                    case WorkspaceLayoutNames.WordMeanings:
                        e.Content = new WordMeaningsViewModel();
                        break;
                    case WorkspaceLayoutNames.SourceContext:
                        e.Content = new SourceContextViewModel();
                        break;
                    case WorkspaceLayoutNames.TargetContext:
                        e.Content = new TargetContextViewModel();
                        break;
                    case WorkspaceLayoutNames.Notes:
                        e.Content = new NotesViewModel();
                        break;
                    case WorkspaceLayoutNames.Pins:
                        e.Content = new PinsViewModel();
                        break;
                    case WorkspaceLayoutNames.TextCollection:
                        e.Content = new TextCollectionViewModel();
                        break;
                    case WorkspaceLayoutNames.StartPage:
                        e.Content = new StartPageViewModel();
                        break;
                    case WorkspaceLayoutNames.AlignmentTool:
                        e.Content = new AlignmentToolViewModel();
                        break;
                    case WorkspaceLayoutNames.TreeDown:
                        e.Content = new TreeDownViewModel();
                        break;

                }
            };
            layoutSerializer.Deserialize(@".\AvalonDock.Layout.config");
        }

        public (object vm, string title, PaneViewModel.EDockSide dockSide) LoadWindow(string windowTag)
        {
            // window has been closed so we need to reopen it
            switch (windowTag)
            {
                case WorkspaceLayoutNames.BiblicalTerms:
                    var vm = IoC.Get<BiblicalTermsViewModel>();
                    return (vm, vm.Title, vm.DockSide);
                case WorkspaceLayoutNames.Dashboard:
                    var vm1 = new DashboardViewModel();
                    return (vm1, vm1.Title, vm1.DockSide);
                case WorkspaceLayoutNames.ConcordanceTool:
                    var vm2 = new ConcordanceViewModel();
                    return (vm2, vm2.Title, vm2.DockSide);
                case WorkspaceLayoutNames.WordMeanings:
                    var vm3 = new WordMeaningsViewModel();
                    return (vm3, vm3.Title, vm3.DockSide);
                case WorkspaceLayoutNames.SourceContext:
                    var vm4 = new SourceContextViewModel();
                    return (vm4, vm4.Title, vm4.DockSide);
                case WorkspaceLayoutNames.TargetContext:
                    var vm5 = new TargetContextViewModel();
                    return (vm5, vm5.Title, vm5.DockSide);
                case WorkspaceLayoutNames.Notes:
                    var vm6 = new NotesViewModel();
                    return (vm6, vm6.Title, vm6.DockSide);
                case WorkspaceLayoutNames.Pins:
                    var vm7 = new PinsViewModel();
                    return (vm7, vm7.Title, vm7.DockSide);
                case WorkspaceLayoutNames.TextCollection:
                    var vm8 = new TextCollectionViewModel();
                    return (vm8, vm8.Title, vm8.DockSide);
                case WorkspaceLayoutNames.StartPage:
                    var vm9 = new StartPageViewModel();
                    return (vm9, vm9.Title, vm9.DockSide);
                case WorkspaceLayoutNames.AlignmentTool:
                    var vm10 = new AlignmentToolViewModel();
                    return (vm10, vm10.Title, vm10.DockSide);
                case WorkspaceLayoutNames.TreeDown:
                    var vm11 = new TreeDownViewModel();
                    return (vm11, vm11.Title, vm11.DockSide);
            }
            return (null, null, PaneViewModel.EDockSide.Bottom);
        }


        private void HandleEventAsync(object sender, PipeEventArgs args)
        {
            if (args == null) return;

            var pipeMessage = args.PipeMessage;

            switch (pipeMessage.Action)
            {
                case ActionType.CurrentVerse:
                    this.VerseRef = pipeMessage.Text;
                    if (ParatextSync)
                    {
                        OutGoingChangesStarted = true;
                        if (pipeMessage.Text != CurrentBcv.VerseLocationId)
                        {
                            if (BCVDictionary is null)
                            {
                                return;
                            }

                            if (BCVDictionary.Count == 0 || CurrentBcv is null)
                            {
                                return;
                            }

                            CurrentBcv.SetVerseFromId(pipeMessage.Text);

                            CalculateChapters();

                            CalculateVerses();

                            // during the resetting of all the chapters & verse lists from the above,
                            // this defaults back to {book}001001
                            // so we need to reset it again with this call
                            CurrentBcv.SetVerseFromId(pipeMessage.Text);

                            NotifyOfPropertyChange(() => CurrentBcv);
                        }
                        OutGoingChangesStarted = false;
                    }

                    break;
                case ActionType.OnConnected:
                    break;
                case ActionType.OnDisconnected:
                    break;
            }

            Logger.LogInformation($"{pipeMessage.Text}");
        }


        private void CalculateChapters()
        {
            // CHAPTERS
            string bookID = CurrentBcv.Book;
            var chapters = BCVDictionary.Values.Where(b => b.StartsWith(bookID)).ToList();
            for (int i = 0; i < chapters.Count; i++)
            {
                chapters[i] = chapters[i].Substring(2, 3);
            }

            chapters = chapters.DistinctBy(v => v).ToList().OrderBy(b => b).ToList();
            // invoke to get it to run in STA mode
            Application.Current.Dispatcher.Invoke(delegate
            {
                List<int> chapterNumbers = new List<int>();
                foreach (var chapter in chapters)
                {
                    chapterNumbers.Add(Convert.ToInt16(chapter));
                }

                CurrentBcv.ChapterNumbers = chapterNumbers;
            });
        }

        private async void BcvChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (ParatextSync && OutGoingChangesStarted == false)
            {
                var newVerse = CurrentBcv.GetVerseId();
                await ProjectManager.SendPipeMessage(ProjectManager.PipeAction.SetCurrentVerse, newVerse).ConfigureAwait(false);
                Console.WriteLine("");
            }
        }

        private void CalculateVerses()
        {
            // VERSES
            string bookID = CurrentBcv.Book;
            string chapID = CurrentBcv.ChapterIdText;
            var verses = BCVDictionary.Values.Where(b => b.StartsWith(bookID + chapID)).ToList();

            for (int i = 0; i < verses.Count; i++)
            {
                verses[i] = verses[i].Substring(5);
            }

            verses = verses.DistinctBy(v => v).ToList().OrderBy(b => b).ToList();
            // invoke to get it to run in STA mode
            Application.Current.Dispatcher.Invoke(delegate
            {
                List<int> verseNumbers = new List<int>();
                foreach (var verse in verses)
                {
                    verseNumbers.Add(Convert.ToInt16(verse));
                }

                CurrentBcv.VerseNumbers = verseNumbers;
            });
        }

        public override event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion // Methods
    }

    public class WorkspaceLayoutNames
    {
        public const string AlignmentTool = "ALIGNMENTTOOL";
        public const string BiblicalTerms = "BIBLICALTERMS";
        public const string ConcordanceTool = "CONCORDANCETOOL";
        public const string Dashboard = "DASHBOARD";
        public const string Notes = "NOTES";
        public const string Pins = "PINS";
        public const string SourceContext = "SOURCECONTEXT";
        public const string StartPage = "STARTPAGE";
        public const string TargetContext = "TARGETCONTEXT";
        public const string TextCollection = "TEXTCOLLECTION";
        public const string TreeDown = "TREEDOWN";
        public const string WordMeanings = "WORDMEANINGS";
    }
}
