using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using AvalonDock.Themes;
using Caliburn.Micro;
using ClearDashboard.DAL.ViewModels;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.Models;
using ClearDashboard.Wpf.Properties;
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
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ClearDashboard.Wpf.Extensions;

namespace ClearDashboard.Wpf.ViewModels
{

    public class WorkSpaceViewModel : Conductor<IScreen>.Collection.AllActive
    {
        #region Member Variables
        private DashboardProjectManager ProjectManager { get; set; }
        private ILogger<WorkSpaceViewModel> Logger { get; set; }
        private INavigationService NavigationService { get; set; }
        private DashboardProject DashboardProject { get; }

#pragma warning disable CA1416 // Validate platform compatibility
        private DockingManager _dockingManager = new();
#pragma warning restore CA1416 // Validate platform compatibility

        private string _lastLayout = "";

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
        private DocumentViewModel _activeDocument;
        public DocumentViewModel ActiveDocument
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

        private bool OutGoingChangesStarted { get; set; } = false;

        #endregion //Public Properties

        #region Commands


        #endregion  //Commands

        #region Observable Properties

        private Visibility _gridIsVisible = Visibility.Collapsed;
        public Visibility GridIsVisible
        {
            get => _gridIsVisible;
            set => Set(ref _gridIsVisible, value);

        }

        
        private Visibility _deleteGridIsVisible = Visibility.Collapsed;
        public Visibility DeleteGridIsVisible
        {
            get => _deleteGridIsVisible;
            set => Set(ref _deleteGridIsVisible, value);

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




        private ObservableCollection<int> _verseNumbers = new();
        public ObservableCollection<int> VerseNumbers
        {
            get => _verseNumbers;
            set
            {
                _verseNumbers = value;
                NotifyOfPropertyChange(() => VerseNumbers);
            }
        }

        private Dictionary<string, string> _bcvDictionary;
        public Dictionary<string, string> BCVDictionary
        {
            get => _bcvDictionary;
            set
            {
                _bcvDictionary = value;
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
        public string WindowIdToLoad
        {
            get => _windowIdToLoad;
            set
            {
                if (value.StartsWith("ProjectLayout:") || value.StartsWith("StandardLayout"))
                {
                    LoadLayoutById(value);
                }
                else if (value == "SeparatorID")
                {
                    // no op
                }
                else if (value == "SaveID")
                {
                    GridIsVisible = Visibility.Visible;
                    DeleteGridIsVisible = Visibility.Collapsed;
                }
                else if (value == "DeleteID")
                {
                    DeleteGridIsVisible = Visibility.Visible;
                    GridIsVisible = Visibility.Collapsed;
                }
                else
                {
                    switch (value)
                    {
                        case "LayoutID":
                            Console.WriteLine();
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

                        default:
                            _windowIdToLoad = value;
                            break;
                    }

                    if (_windowIdToLoad is not null)
                    {
                        UnHideWindow(WindowIdToLoad);
                    }
                }

                NotifyOfPropertyChange(() => WindowIdToLoad);
            }
        }

        private ObservableCollection<MenuItemViewModel> _menuItems = new();
        public ObservableCollection<MenuItemViewModel> MenuItems
        {
            get => _menuItems;
            set
            {
                _menuItems = value;
                NotifyOfPropertyChange(() => MenuItems);
            }
        }


        ObservableCollection<ToolViewModel> _tools = new();
        public ObservableCollection<ToolViewModel> Tools
        {
            get => _tools;
            set
            {
                _tools = value;
                NotifyOfPropertyChange(() => Tools);
            }
        }


        ObservableCollection<PaneViewModel> _documents = new();
        public ObservableCollection<PaneViewModel> Documents
        {
            get => _documents;
            set
            {
                _documents = value;
                NotifyOfPropertyChange(() => Documents);
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

        private LayoutFile _selectedLayout;
        public LayoutFile SelectedLayout
        {
            get => _selectedLayout;
            set
            {
                _selectedLayout = value;
                NotifyOfPropertyChange(nameof(SelectedLayout));
            }
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

        private FlowDirection _flowDirection = FlowDirection.LeftToRight;
        public FlowDirection FlowDirection
        {
            get => _flowDirection;
            set => Set(ref _flowDirection, value, nameof(FlowDirection));
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
            ILogger<WorkSpaceViewModel> logger, DashboardProjectManager projectManager)

        {
            ProjectManager = projectManager;
            Logger = logger;
            NavigationService = navigationService;
            FlowDirection = ProjectManager.CurrentLanguageFlowDirection;

#pragma warning disable CA1416 // Validate platform compatibility
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
#pragma warning restore CA1416 // Validate platform compatibility

            if (Settings.Default.Theme == MaterialDesignThemes.Wpf.BaseTheme.Dark)
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

        /// <summary>
        /// Binds the viewmodel to it's view prior to activating so that the OnViewAttached method of the
        /// child viewmodel are called.
        /// </summary>
        /// <typeparam name="TViewModel"></typeparam>
        /// <returns></returns>
        protected async Task ActivateItemAsync<TViewModel>(CancellationToken cancellationToken = default) where TViewModel : class, IScreen
        {
            var viewModel = IoC.Get<TViewModel>();
            var view = ViewLocator.LocateForModel(viewModel, null, null);
            ViewModelBinder.Bind(viewModel, view, null);
            await ActivateItemAsync(viewModel, cancellationToken);
        }

        public async void Init()
        {

            ReBuildMenu();

           
            Items.Clear();
            // documents
            await ActivateItemAsync<DashboardViewModel>();
            await ActivateItemAsync<ConcordanceViewModel>();
            await ActivateItemAsync<StartPageViewModel>();
            await ActivateItemAsync<AlignmentToolViewModel>();
            await ActivateItemAsync<TreeDownViewModel>();
            // tools
          
            await ActivateItemAsync<BiblicalTermsViewModel>();
            await ActivateItemAsync<WordMeaningsViewModel>();
            await ActivateItemAsync<SourceContextViewModel>();
            await ActivateItemAsync<TargetContextViewModel>();
            await ActivateItemAsync<NotesViewModel>();
            await ActivateItemAsync<PinsViewModel>();
            await ActivateItemAsync<TextCollectionViewModel>();


            // remove all existing windows
            var layoutSerializer = new XmlLayoutSerializer(_dockingManager);

            if (Settings.Default.LastLayout == "")
            {
                // bring up the default layout
                LoadLayout(layoutSerializer, FileLayouts[0].LayoutPath);
            }
            else
            {
                // check to see if the layout exists
                string layoutPath = Settings.Default.LastLayout;
                if (File.Exists(layoutPath))
                {
                    LoadLayout(layoutSerializer, layoutPath);
                }
                else
                {
                    LoadLayout(layoutSerializer, FileLayouts[0].LayoutPath);
                }
            }
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

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            if (_lastLayout == "")
            {
                SelectedLayoutText = "Last Saved";
                OkSave();
            }
            return base.OnDeactivateAsync(close, cancellationToken);
        }


        #endregion //Constructor

        #region Methods

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
                        LayoutID = "StandardLayout:" + id.ToString(),
                        LayoutPath = file,
                        LayoutType = LayoutFile.eLayoutType.Standard
                    });
                    id++;
                }
            }

            // get the project layouts
            if (ProjectManager is not null)
            {
                path = Path.Combine(ProjectManager.CurrentDashboardProject.TargetProject.DirectoryPath, "shared");
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
                            LayoutType = LayoutFile.eLayoutType.Project
                        });
                        id++;
                    }
                }
            }

            return fileLayouts;
        }

        private void ReBuildMenu()
        {
            FileLayouts = GetFileLayouts();
            ObservableCollection<MenuItemViewModel> layouts = new();


            // add in the standard menu items
            layouts.Add(new MenuItemViewModel
                { Header = "🖫 Save Current Layout", Id = "SaveID", ViewModel = this, Icon = null });
            layouts.Add(new MenuItemViewModel { Header = "🗑 Delete Saved Layout", Id = "DeleteID", ViewModel = this, });
            layouts.Add(new MenuItemViewModel { Header = "---- STANDARD LAYOUTS ----", Id = "SeparatorID", ViewModel = this, });

            bool bFound = false;
            foreach (var fileLayout in FileLayouts)
            {
                if (fileLayout.LayoutID.StartsWith("ProjectLayout:") && bFound == false)
                {
                    layouts.Add(new MenuItemViewModel
                        { Header = "---- PROJECT LAYOUTS ----", Id = "SeparatorID", ViewModel = this, });
                    bFound = true;
                }


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
                        new MenuItemViewModel { Header = "⳼ Alignment Tool", Id = "AlignmentToolID", ViewModel = this, },
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
        }

        /// <summary>
        /// Save the layout
        /// </summary>
        public void OkSave()
        {
            string filePath = "";
            if (SelectedLayout is not null)
            {
                // overwrite the current layout
                filePath = SelectedLayout.LayoutPath;
            }
            else
            {
                if (SelectedLayoutText != string.Empty)
                {
                    // use the 
                    SelectedLayoutText = Helpers.Helpers.SanitizeFileName(SelectedLayoutText);

                    if (SelectedLayoutText == "")
                    {
                        // no characters were returned that were valid
                        return;
                    }

                    var path = Path.Combine(ProjectManager.CurrentDashboardProject.TargetProject.DirectoryPath, "shared");

                    // check for the presence of a "shared" directory under the project.  NOTE: IS CASE SENSITIVE
                    // AND MUST BE LOWERCASE FOR MERCURIAL
                    if (! Directory.Exists(path))
                    {
                        try
                        {
                            Directory.CreateDirectory(path);
                        }
                        catch (Exception e)
                        {
                            Logger.LogError(e.Message);
                            return;
                        }
                    }


                    filePath = Path.Combine(path, SelectedLayoutText + ".Layout.config");
                }

            }

            try
            {
                // save the layout
                var layoutSerializer = new XmlLayoutSerializer(this._dockingManager);
                layoutSerializer.Serialize(filePath);
            }
            catch (Exception e)
            {
                this.Logger.LogError(e.Message);
            }

            GridIsVisible = Visibility.Collapsed;

            ReBuildMenu();
        }

        public void DeleteLayout(LayoutFile layoutFile)
        {
            if (layoutFile.LayoutType == LayoutFile.eLayoutType.Standard)
            {
                return;
            }

            try
            {
                File.Delete(layoutFile.LayoutPath);
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message);
                return;
            }

            FileLayouts.Remove(layoutFile);
            ReBuildMenu();
        }

        public void CancelSave()
        {
            GridIsVisible = Visibility.Collapsed;
        }

        public void CancelDelete()
        {
            DeleteGridIsVisible = Visibility.Collapsed;
        }

        private void WorkSpaceViewModel_ThemeChanged()
        {
            GridIsVisible = Visibility.Collapsed;
        }

        private void LoadLayoutById(string layoutId)
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
#pragma warning disable CA1416 // Validate platform compatibility
            layoutSerializer.LayoutSerializationCallback += (s, e) =>
            {
                if (e.Model.ContentId is not null)
                {
                    Debug.WriteLine(e.Model.ContentId);

                    switch (e.Model.ContentId.ToUpper())
                    {
                        // Documents
                        case WorkspaceLayoutNames.Dashboard:
                            e.Content = GetPaneViewModelFromItems("DashboardViewModel");
                            break;
                        case WorkspaceLayoutNames.ConcordanceTool:
                            e.Content = GetPaneViewModelFromItems("ConcordanceViewModel");
                            break;
                        case WorkspaceLayoutNames.StartPage:
                            e.Content = GetPaneViewModelFromItems("StartPageViewModel");
                            break;
                        case WorkspaceLayoutNames.AlignmentTool:
                            e.Content = GetPaneViewModelFromItems("AlignmentToolViewModel");
                            break;
                        case WorkspaceLayoutNames.TreeDown:
                            e.Content = GetPaneViewModelFromItems("TreeDownViewModel");
                            break;

                        // Tools
                        case WorkspaceLayoutNames.BiblicalTerms:
                            e.Content = GetToolViewModelFromItems("BiblicalTermsViewModel");
                            break;
                        case WorkspaceLayoutNames.WordMeanings:
                            e.Content = GetToolViewModelFromItems("WordMeaningsViewModel");
                            break;
                        case WorkspaceLayoutNames.SourceContext:
                            e.Content = GetToolViewModelFromItems("SourceContextViewModel");
                            break;
                        case WorkspaceLayoutNames.TargetContext:
                            e.Content = GetToolViewModelFromItems("TargetContextViewModel");
                            break;
                        case WorkspaceLayoutNames.Notes:
                            e.Content = GetToolViewModelFromItems("NotesViewModel");
                            break;
                        case WorkspaceLayoutNames.Pins:
                            e.Content = GetToolViewModelFromItems("PinsViewModel");
                            break;
                        case WorkspaceLayoutNames.TextCollection:
                            e.Content = GetToolViewModelFromItems("TextCollectionViewModel");
                            break;

                    }
                }
            };
#pragma warning restore CA1416 // Validate platform compatibility
            try
            {
                //throw new Exception();
                layoutSerializer.Deserialize(filePath);
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message);
                filePath = Path.Combine(Environment.CurrentDirectory, @"Resources\Layouts\Dashboard.Layout.config");
                if (File.Exists(filePath))
                {
                    // load in the default layout
                    layoutSerializer.Deserialize(filePath);
                }
                else
                {
                    // build a layout
                    _tools.Clear();
                    _documents.Clear();

                    for (int i = 0; i < Items.Count; i++)
                    {
                        var type = Items[i];
                        switch (type)
                        {
                            case DashboardViewModel:
                            case ConcordanceViewModel:
                            case StartPageViewModel:
                            case AlignmentToolViewModel:
                            case TreeDownViewModel:
                                _documents.Add((PaneViewModel)Items[i]);
                                break;

                            case BiblicalTermsViewModel:
                            case WordMeaningsViewModel:
                            case SourceContextViewModel:
                            case TargetContextViewModel:
                            case NotesViewModel:
                            case PinsViewModel:
                            case TextCollectionViewModel:
                                _tools.Add((ToolViewModel)Items[i]);
                                break;
                        }
                    }

                    NotifyOfPropertyChange(() => Documents);
                    NotifyOfPropertyChange(() => BookNames);
                }
            }

            // save to settings
            Settings.Default.LastLayout = filePath;
            _lastLayout = filePath;
        }

        /// <summary>
        /// return the correct existing vm from Items list - DOCUMENTS
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        public PaneViewModel GetPaneViewModelFromItems(string vm)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                var type = Items[i];
                if (type.GetType().Name == vm)
                {
                    switch (type)
                    {
                        case DashboardViewModel:
                        case ConcordanceViewModel:
                        case StartPageViewModel:
                        case AlignmentToolViewModel:
                        case TreeDownViewModel:
                            return (PaneViewModel)Items[i];
                    }
                }
            }

            return (PaneViewModel)Items[0];
        }

        /// <summary>
        /// return the correct existing vm from Items list - TOOLS
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        public ToolViewModel GetToolViewModelFromItems(string vm)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                var type = Items[i];
                if (type.GetType().Name == vm)
                {
                    switch (type)
                    {
                        case BiblicalTermsViewModel:
                        case WordMeaningsViewModel:
                        case SourceContextViewModel:
                        case TargetContextViewModel:
                        case NotesViewModel:
                        case PinsViewModel:
                        case TextCollectionViewModel:
                            return (ToolViewModel)Items[i];
                    }
                }
            }

            return (ToolViewModel)Items[0];
        }

        public (object vm, string title, PaneViewModel.EDockSide dockSide) LoadWindow(string windowTag)
        {
            // window has been closed so we need to reopen it
            switch (windowTag)
            {
                // Documents
                case WorkspaceLayoutNames.Dashboard:
                    var vm1 = GetPaneViewModelFromItems("DashboardViewModel");
                    return (vm1, vm1.Title, vm1.DockSide);
                case WorkspaceLayoutNames.ConcordanceTool:
                    var vm2 = GetPaneViewModelFromItems("ConcordanceViewModel");
                    return (vm2, vm2.Title, vm2.DockSide);
                case WorkspaceLayoutNames.Pins:
                    var vm7 = GetPaneViewModelFromItems("PinsViewModel");
                    return (vm7, vm7.Title, vm7.DockSide);
                case WorkspaceLayoutNames.StartPage:
                    var vm9 = GetPaneViewModelFromItems("StartPageViewModel");
                    return (vm9, vm9.Title, vm9.DockSide);
                case WorkspaceLayoutNames.AlignmentTool:
                    var vm10 = GetPaneViewModelFromItems("AlignmentToolViewModel");
                    return (vm10, vm10.Title, vm10.DockSide);
                case WorkspaceLayoutNames.TreeDown:
                    var vm11 = GetPaneViewModelFromItems("TreeDownViewModel");
                    return (vm11, vm11.Title, vm11.DockSide);

                // Tools
                case WorkspaceLayoutNames.BiblicalTerms:
                    var vm = GetToolViewModelFromItems("BiblicalTermsViewModel");
                    return (vm, vm.Title, vm.DockSide);
                case WorkspaceLayoutNames.WordMeanings:
                    var vm3 = GetToolViewModelFromItems("WordMeaningsViewModel");
                    return (vm3, vm3.Title, vm3.DockSide);
                case WorkspaceLayoutNames.SourceContext:
                    var vm4 = GetToolViewModelFromItems("SourceContextViewModel");
                    return (vm4, vm4.Title, vm4.DockSide);
                case WorkspaceLayoutNames.TargetContext:
                    var vm5 = GetToolViewModelFromItems("TargetContextViewModel");
                    return (vm5, vm5.Title, vm5.DockSide);
                case WorkspaceLayoutNames.Notes:
                    var vm6 = GetToolViewModelFromItems("NotesViewModel");
                    return (vm6, vm6.Title, vm6.DockSide);
                case WorkspaceLayoutNames.TextCollection:
                    var vm8 = GetToolViewModelFromItems("TextCollectionViewModel");
                    return (vm8, vm8.Title, vm8.DockSide);

            }
            return (null, null, PaneViewModel.EDockSide.Bottom);
        }

        /// <summary>
        /// Unhide window
        /// </summary>
        /// <param name="windowTag"></param>
        private void UnHideWindow(string windowTag)
        {
            // find the pane in the dockmanager with this contentID
#pragma warning disable CA1416 // Validate platform compatibility
            var windowPane = _dockingManager.Layout.Descendents()
                .OfType<LayoutAnchorable>()
                .SingleOrDefault(a =>
                {
                    if (a.ContentId is not null)
                    {
                        return a.ContentId.ToUpper() == windowTag.ToUpper();
                    }
                    return false;
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
#pragma warning restore CA1416 // Validate platform compatibility
        }

        private void BcvChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (ParatextSync && OutGoingChangesStarted == false)
            {
                var newVerse = CurrentBcv.GetVerseId();
            }
        }


        private void CalculateChapters()
        {
            // CHAPTERS
            string bookID = CurrentBcv.Book;
            var chapters = BCVDictionary.Values.Where(b => b.StartsWith(bookID)).ToList();
            for (int i = 0; i < chapters.Count; i++)
            {
                chapters[i] = chapters[i].Substring(3, 3);
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

        private void CalculateVerses()
        {
            // VERSES
            string bookID = CurrentBcv.Book;
            string chapID = CurrentBcv.ChapterIdText;
            var verses = BCVDictionary.Values.Where(b => b.StartsWith(bookID + chapID)).ToList();

            for (int i = 0; i < verses.Count; i++)
            {
                verses[i] = verses[i].Substring(6);
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
