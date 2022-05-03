using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using AvalonDock.Themes;
using Caliburn.Micro;
using ClearDashboard.Common.Models;
using ClearDashboard.DataAccessLayer.NamedPipes;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.ParatextPlugin.Data;
using ClearDashboard.Wpf.Models;
using ClearDashboard.Wpf.ViewModels.Menus;
using ClearDashboard.Wpf.ViewModels.Panes;
using ClearDashboard.Wpf.Views;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;


namespace ClearDashboard.Wpf.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class WorkSpaceViewModel : Conductor<IScreen>.Collection.AllActive//ApplicationScreen
    {
        #region Member Variables

        private static WorkSpaceViewModel _this;
        public static WorkSpaceViewModel This => _this;

        public DashboardProject DashboardProject { get; set; }
        private DashboardViewModel _dashboardViewModel;

        private DockingManager _dockingManager = new DockingManager();
        
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


        #endregion  //Commands

        #region Observable Properties

        private bool _gridIsVisible;
        public bool GridIsVisible
        {
            get => _gridIsVisible;
            set => Set(ref _gridIsVisible, value);
          
        }

      

        private string _selectedLayoutText;

        public string SelectedLayoutText
        {
            get => _selectedLayoutText;
            set
            {
                _selectedLayoutText = value; 
                NotifyOfPropertyChange(() => SelectedLayoutText);
            }
        }




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
                if (value.StartsWith("Layout:"))
                {
                    LoadLayoutByID(value);
                }
                else
                {
                    switch (value)
                    {
                        case "LayoutID":
                            break;
                        case "AlignmentToolID":
                            _windowIdToLoad = "ALIGNMENTTOOL";
                            break;
                        case "BiblicalTermsID":
                            _windowIdToLoad = "BIBLICALTERMS";
                            break;
                        case "ConcordanceToolID":
                            _windowIdToLoad = "CONCORDANCETOOL";
                            break;
                        case "DashboardID":
                            _windowIdToLoad = "DASHBOARD";
                            break;
                        case "NotesID":
                            _windowIdToLoad = "NOTES";
                            break;
                        case "PINSID":
                            _windowIdToLoad = "PINS";
                            break;
                        case "WordMeaningsID":
                            _windowIdToLoad = "WORDMEANINGS";
                            break;
                        case "SourceContextID":
                            _windowIdToLoad = "SOURCECONTEXT";
                            break;
                        case "StartPageID":
                            _windowIdToLoad = "STARTPAGE";
                            break;
                        case "TargetContextID":
                            _windowIdToLoad = "TARGETCONTEXT";
                            break;
                        case "TextCollectionID":
                            _windowIdToLoad = "TEXTCOLLECTION";
                            break;
                        case "SaveID":
                            GridIsVisible = true;
                            break;
                        case "LoadID":

                            break;
                        default:
                            _windowIdToLoad = value;
                            break;
                    }
                }

                
                NotifyOfPropertyChange(() => WindowIDToLoad);
            }
        }

        private ObservableCollection<MenuItemViewModel> _menuItems = new ();
        public ObservableCollection<MenuItemViewModel> MenuItems
        {
            get => _menuItems;
            set
            {
                _menuItems = value;
                NotifyOfPropertyChange(() => MenuItems);
            }
        }


        ObservableCollection<ToolViewModel> _tools = new ();
        public ObservableCollection<ToolViewModel> Tools
        {
            get => _tools;
            set
            {
                _tools = value;
                NotifyOfPropertyChange(() => Tools);
            }
        }


        ObservableCollection<PaneViewModel> _files = new ();
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

        private LayoutFile _SelectedLayout;
        public LayoutFile SelectedLayout
        {
            get => _SelectedLayout;
            set
            {
                _SelectedLayout = value;
                NotifyOfPropertyChange(nameof(SelectedLayout));
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

        private DashboardProjectManager ProjectManager { get; set; }
        private ILogger<WorkSpaceViewModel> Logger { get; set; }
        private INavigationService NavigationService { get; set; }

        private FlowDirection _flowDirection = FlowDirection.LeftToRight;
        public FlowDirection FlowDirection
        {
            get => _flowDirection;
            set => Set(ref _flowDirection, value, nameof(FlowDirection));
        }

        private ObservableCollection<LayoutFile> _fileLayouts = new();
        public ObservableCollection<LayoutFile> FileLayouts
        {
            get => _fileLayouts;
            set
            {
                _fileLayouts = value;
                NotifyOfPropertyChange(() => FileLayouts);
            }
        }

        public WorkSpaceViewModel(INavigationService navigationService, 
            ILogger<WorkSpaceViewModel> logger, DashboardProjectManager projectManager) 
            
        {
            ProjectManager = projectManager;
            Logger = logger;  
            NavigationService = navigationService;

            FlowDirection = ProjectManager.CurrentLanguageFlowDirection;

            //ProjectManager.NamedPipeChanged += HandleEventAsync;

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


        private ObservableCollection<LayoutFile> GetFileLayouts()
        {
            int id = 0;
            ObservableCollection<LayoutFile> fileLayouts = new();
            // add in the default layouts
            var path = Path.Combine(Environment.CurrentDirectory, @"Resources\Layouts");
            if (Directory.Exists(path))
            {
                var files = Directory.GetFiles(path, "*.Layout.config");

                foreach (var file in files)
                {
                    FileInfo fileInfo = new FileInfo(file);
                    string name = fileInfo.Name.Substring(0, fileInfo.Name.Length - ".Layout.config".Length);

                    fileLayouts.Add(new LayoutFile
                    {
                        LayoutName = name,
                        LayoutID = "Layout:" + id.ToString(),
                        LayoutPath = file,
                    });
                    id++;
                }
            }

            // get the project layouts
            if (ProjectManager is not null)
            {
                path = ProjectManager.CurrentDashboardProject.TargetProject.DirectoryPath;
                if (Directory.Exists(path))
                {
                    var files = Directory.GetFiles(path, "*.Layout.config");

                    foreach (var file in files)
                    {
                        FileInfo fileInfo = new FileInfo(file);
                        string name = fileInfo.Name.Substring(0, fileInfo.Name.Length - ".Layout.config".Length);

                        fileLayouts.Add(new LayoutFile
                        {
                            LayoutName = name,
                            LayoutID = "ProjectLayout:" + id.ToString(),
                            LayoutPath = file,
                        });
                        id++;
                    }
                }
            }

            return fileLayouts;
        }
        public async void Init()
        {
            FileLayouts = GetFileLayouts();
            ObservableCollection<MenuItemViewModel> layouts = new ();


            // add in the standard menu items
            layouts.Add(new MenuItemViewModel { Header = "🖫 Save Current Layout", Id = "SaveID", ViewModel = this, Icon=null });
            layouts.Add(new MenuItemViewModel { Header = "🗑 Delete Saved Layout", Id = "DeleteID", ViewModel = this, });
            layouts.Add(new MenuItemViewModel { Header = "_________________________", Id = "SeparatorID", ViewModel = this, });

            foreach (var fileLayout in FileLayouts)
            {
                layouts.Add(new MenuItemViewModel
                {
                    Header = "🝆 " + fileLayout.LayoutName,
                    Id = fileLayout.LayoutID,
                    ViewModel = this,
                });
            }

            // initiate the menu system
            MenuItems.Clear();
            MenuItems = new ObservableCollection<MenuItemViewModel>
            {
                new MenuItemViewModel
                {
                    Header = "Layouts", Id = "LayoutID", ViewModel = this,
                    MenuItems = layouts,
                },
                new MenuItemViewModel
                {
                    Header = "Windows", Id = "WindowID", ViewModel = this,
                    MenuItems = new ObservableCollection<MenuItemViewModel>
                    {
                        new MenuItemViewModel { Header = "⳼ Alignment Tool", Id = "AlignmentToolID", ViewModel = this,},
                        new MenuItemViewModel { Header = "🕮 Biblical Terms", Id = "BiblicalTermsID", ViewModel = this, },
                        new MenuItemViewModel { Header = "🆎 Concordance Tool", Id = "ConcordanceToolID", ViewModel = this, },
                        new MenuItemViewModel { Header = "📐 Dashboard", Id = "DashboardID", ViewModel = this, },
                        new MenuItemViewModel { Header = "🖉 Notes", Id = "NotesID", ViewModel = this, },
                        new MenuItemViewModel { Header = "⍒ PINS", Id = "PINSID", ViewModel = this, },
                        new MenuItemViewModel { Header = "⌺ Word Meanings", Id = "WordMeaningsID", ViewModel = this, },
                        new MenuItemViewModel { Header = "⬒ Source Context", Id = "SourceContextID", ViewModel = this, },
                        new MenuItemViewModel { Header = "⌂ Start Page", Id = "StartPageID", ViewModel = this, },
                        new MenuItemViewModel { Header = "⬓ Target Context", Id = "TargetContextID", ViewModel = this, },
                        new MenuItemViewModel { Header = "🗐 Text Collection", Id = "TextCollectionID", ViewModel = this, },
                        new MenuItemViewModel { Header = "⯭ Treedown", Id = "TreedownID", ViewModel = this, },
                    }
                },
                new MenuItemViewModel { Header = "Help", Id = "HelpID", ViewModel = this, }
            };


            // add in the document panes
            _files.Clear();
            _files.Add(IoC.Get<DashboardViewModel>());
            _files.Add(IoC.Get<ConcordanceViewModel>());
            _files.Add(IoC.Get<StartPageViewModel>());
            _files.Add(IoC.Get<AlignmentToolViewModel>());
            _files.Add(IoC.Get<TreeDownViewModel>());
            NotifyOfPropertyChange(() => Files);

            Items.Clear();

            await ActivateItemAsync(IoC.Get<BiblicalTermsViewModel>());
            await ActivateItemAsync(IoC.Get<WordMeaningsViewModel>());
            await ActivateItemAsync(IoC.Get<SourceContextViewModel>());
            await ActivateItemAsync(IoC.Get<TargetContextViewModel>());
            await ActivateItemAsync(IoC.Get<NotesViewModel>());
            await ActivateItemAsync(IoC.Get<PinsViewModel>());
            await ActivateItemAsync(IoC.Get<TextCollectionViewModel>());



            NotifyOfPropertyChange(() => BookNames);

            //await ProjectManager.SendPipeMessage(PipeAction.GetCurrentVerse).ConfigureAwait(false);

        }

        protected override void OnViewAttached(object view, object context)
        {
            base.OnViewAttached(view, context);

            // hook up a reference to the windows dock manager
            if (view is WorkSpaceView currentView)
            {
                _dockingManager = (DockingManager)currentView.FindName("dockManager");
            }

            Init();
        }

        //protected override void Dispose(bool disposing)
        //{
        //    ProjectManager.NamedPipeChanged -= HandleEventAsync;
        //    base.Dispose(disposing);
        //}

        #endregion //Constructor

        #region Methods

        public void OkSave()
        {
            // todo
            if (SelectedLayout is not null)
            {
                Debug.WriteLine(SelectedLayout.LayoutPath);
            }
            else
            {
                Debug.WriteLine(SelectedLayoutText);
            }
            GridIsVisible = false;
        }

        public void CancelSave()
        {
            GridIsVisible = false;
        }

        private void WorkSpaceViewModel_ThemeChanged()
        {
            GridIsVisible = false;
        }

        private void LoadLayoutByID(string layoutId)
        {
            var layoutFile = FileLayouts.SingleOrDefault(f => f.LayoutID == layoutId);

            if (layoutFile is null)
            {
                return;
            }

            // remove all existing windows
            _dockingManager.AnchorablesSource = null;
            _dockingManager.DocumentsSource = null;
            var layoutSerializer = new XmlLayoutSerializer(_dockingManager);
            LoadLayout(layoutSerializer, layoutFile.LayoutPath);

        }

      


        public void LoadLayout(XmlLayoutSerializer layoutSerializer, string filePath)
        {
            // Here I've implemented the LayoutSerializationCallback just to show
            //  a way to feed layout deserialization with content loaded at runtime
            // Actually I could in this case let AvalonDock to attach the contents
            // from current layout using the content ids
            // LayoutSerializationCallback should anyway be handled to attach contents
            // not currently loaded
            layoutSerializer.LayoutSerializationCallback += (s, e) =>
            {
                if (e.Model.ContentId is not null)
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
                }
            };
            try
            {
                layoutSerializer.Deserialize(filePath);
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message);
            }
            finally
            {
                filePath = Path.Combine(Environment.CurrentDirectory, @"Resources\Layouts\Dashboard.Layout.config");
                if (! File.Exists(filePath))
                {
                    layoutSerializer.Deserialize(filePath);
                }
            }

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

        /// <summary>
        /// Unhide window
        /// </summary>
        /// <param name="windowTag"></param>
        private void UnHideWindow(string windowTag)
        {
            // find the pane in the dockmanager with this contentID
            var windowPane = _dockingManager.Layout.Descendents()
                .OfType<LayoutAnchorable>()
                .SingleOrDefault(a =>
                {
                    Debug.WriteLine(a.Title);
                    if (a.ContentId is not null)
                    {
                        return a.ContentId.ToUpper() == windowTag.ToUpper();
                    }
                    else
                    {
                        return false;
                    }

                });

            if (windowPane != null)
            {
                if (windowPane.IsAutoHidden)
                {
                    windowPane.ToggleAutoHide();
                }
                else if (windowPane.IsHidden)
                {
                    windowPane.Show();
                }
                else if (windowPane.IsVisible)
                {
                    windowPane.IsActive = true;
                }
            }
            else
            {
                // window has been closed so reload it
                windowPane = new LayoutAnchorable
                {
                    ContentId = windowTag
                };

                // setup the right ViewModel for the pane
                var obj = LoadWindow(windowTag);
                windowPane.Content = obj.vm;
                windowPane.Title = obj.title;
                windowPane.IsActive = true;
                // set where it will doc on layout
                if (obj.dockSide == PaneViewModel.EDockSide.Bottom)
                {
                    windowPane.AddToLayout(_dockingManager, AnchorableShowStrategy.Bottom);
                }
                else if (obj.dockSide == PaneViewModel.EDockSide.Left)
                {
                    windowPane.AddToLayout(_dockingManager, AnchorableShowStrategy.Left);
                }
            }
        }

   


        public void SetLayoutSaveName(string cboNamesText)
        {
            // todo
            Console.WriteLine();
        }

        public override event PropertyChangedEventHandler PropertyChanged;


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
