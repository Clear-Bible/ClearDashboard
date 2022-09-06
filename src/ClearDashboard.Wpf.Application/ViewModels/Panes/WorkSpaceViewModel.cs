using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using AvalonDock.Themes;
using Caliburn.Micro;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearDashboard.DAL.ViewModels;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.ParatextPlugin.CQRS.Features.Verse;
using ClearDashboard.Wpf.Application.Models;
using ClearDashboard.Wpf.Application.ViewModels.ParatextViews;
using MediatR;
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
using ClearDashboard.Wpf.Application.Properties;
using ClearDashboard.Wpf.Application.ViewModels.Menus;
using ClearDashboard.Wpf.Application.Views.Project;

namespace ClearDashboard.Wpf.Application.ViewModels.Panes
{
    public class WorkSpaceViewModel : Conductor<IScreen>.Collection.AllActive,
                    IHandle<VerseChangedMessage>,
                    IHandle<ProjectChangedMessage>,
                    IHandle<ProgressBarVisibilityMessage>,
                    IHandle<ProgressBarMessage>
    {
#nullable disable
        #region Member Variables
        private IEventAggregator EventAggregator { get; }
        private DashboardProjectManager ProjectManager { get; }
        private ILogger<WorkSpaceViewModel> Logger { get; }
        private INavigationService NavigationService { get; }
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
                    ActiveDocumentChanged(this, EventArgs.Empty);
                }
            }
        }

        public bool IsRtl { get; set; }

        private bool InComingChangesStarted { get; set; }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => Set(ref _isBusy, value, nameof(IsBusy));
        }


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




        //private ObservableCollection<int> _verseNumbers = new();
        //public ObservableCollection<int> VerseNumbers
        //{
        //    get => _verseNumbers;
        //    set
        //    {
        //        _verseNumbers = value;
        //        NotifyOfPropertyChange(() => VerseNumbers);
        //    }
        //}

        private Dictionary<string, string> _bcvDictionary;

        private Dictionary<string, string> BCVDictionary
        {
            get => _bcvDictionary;
            set
            {
                _bcvDictionary = value;
                NotifyOfPropertyChange(() => BCVDictionary);
            }
        }

        //private ObservableCollection<string> _bookNames = new();
        //public ObservableCollection<string> BookNames
        //{
        //    get => _bookNames;
        //    set
        //    {
        //        _bookNames = value;
        //        NotifyOfPropertyChange(() => BookNames);
        //    }
        //}


        //private string _verseRef;
        //public string VerseRef
        //{
        //    get => _verseRef;
        //    set
        //    {
        //        _verseRef = value;
        //        NotifyOfPropertyChange(() => VerseRef);
        //    }
        //}

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
                        case "CorpusTokensID":
                            _windowIdToLoad = "CORPUSTOKENS";
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
                        case "ProjectDesignSurfaceID":
                            _windowIdToLoad = "PROJECTDESIGNSURFACETOOL";
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

                    UnHideWindow(WindowIdToLoad);
                }

                NotifyOfPropertyChange(() => WindowIdToLoad);
            }
        }

        private ObservableCollection<MenuItemViewModel> _menuItems = new();
        public ObservableCollection<MenuItemViewModel> MenuItems
        {
            get => _menuItems;
            private set
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

        private List<Tuple<string, Theme>> Themes { get; }

        //private readonly Tuple<string, Theme> _selectedTheme;
        private Tuple<string, Theme> _selectedTheme;
        public Tuple<string, Theme> SelectedTheme
        {
            get => _selectedTheme;
            //private init
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
            private set
            {
                _fileLayouts = value;
                NotifyOfPropertyChange(() => FileLayouts);
            }
        }

        private readonly FlowDirection _windowFlowDirection = FlowDirection.LeftToRight;
        public FlowDirection WindowFlowDirection
        {
            get => _windowFlowDirection;
            init => Set(ref _windowFlowDirection, value, nameof(WindowFlowDirection));
        }

        #endregion //Observable Properties

        #region Constructor

        /// <summary>
        /// Required for design-time support
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public WorkSpaceViewModel()
        {

        }


        // ReSharper disable once UnusedMember.Global
        public WorkSpaceViewModel(INavigationService navigationService,
            ILogger<WorkSpaceViewModel> logger, DashboardProjectManager projectManager, IEventAggregator eventAggregator)

        {
            EventAggregator = eventAggregator;
            ProjectManager = projectManager;
            Logger = logger;
            NavigationService = navigationService;
            WindowFlowDirection = ProjectManager.CurrentLanguageFlowDirection;

#pragma warning disable CA1416 // Validate platform compatibility
            Themes = new List<Tuple<string, Theme>>
            {
                new(nameof(Vs2013DarkTheme),new Vs2013DarkTheme()),
                new(nameof(Vs2013LightTheme),new Vs2013LightTheme()),
                new(nameof(AeroTheme),new AeroTheme()),
                new(nameof(Vs2013BlueTheme),new Vs2013BlueTheme()),
                new(nameof(GenericTheme), new GenericTheme()),
                new(nameof(ExpressionDarkTheme),new ExpressionDarkTheme()),
                new(nameof(ExpressionLightTheme),new ExpressionLightTheme()),
                new(nameof(MetroTheme),new MetroTheme()),
                new(nameof(VS2010Theme),new VS2010Theme()),
            };
#pragma warning restore CA1416 // Validate platform compatibility


            // TODO - UNREMARK THIS FOR THEME SWITCHING
            
//            this.SelectedTheme = Settings.Default.Theme == MaterialDesignThemes.Wpf.BaseTheme.Dark ? Themes[0] : Themes[1];
            
//            // check if we are in design mode or not
//            if (System.Windows.Application.Current != null)
//            {
//                // subscribe to change events in the parent's theme
//                ((App)(System.Windows.Application.Current).ThemeChanged += WorkSpaceViewModel_ThemeChanged;
                
//                if ((System.Windows.Application.Current is App)
//                {
//#pragma warning disable CS8601 // Possible null reference assignment.
//                    DashboardProject = (System.Windows.Application.Current as App)?.SelectedDashboardProject;
//#pragma warning restore CS8601 // Possible null reference assignment.
//                }
//            }
        }


        /// <summary>
        /// Binds the viewmodel to it's view prior to activating so that the OnViewAttached method of the
        /// child viewmodel are called.
        /// </summary>
        /// <typeparam name="TViewModel"></typeparam>
        /// <returns></returns>
        private async Task ActivateItemAsync<TViewModel>(CancellationToken cancellationToken = default) where TViewModel : class, IScreen
        {
            var viewModel = IoC.Get<TViewModel>();
            var view = ViewLocator.LocateForModel(viewModel, null, null);
            ViewModelBinder.Bind(viewModel, view, null);
            await ActivateItemAsync(viewModel, cancellationToken);
        }

        protected override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            // subscribe to the event aggregator so that we can listen to messages
            EventAggregator.SubscribeOnUIThread(this);

            return base.OnActivateAsync(cancellationToken);
        }

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            if (_lastLayout == "")
            {
                SelectedLayoutText = "Last Saved";
                OkSave();
            }

            // unsubscribe to the event aggregator
            EventAggregator?.Unsubscribe(this);
            return base.OnDeactivateAsync(close, cancellationToken);
        }

        protected override void OnViewAttached(object view, object context)
        {
            base.OnViewAttached(view, context);

            // hook up a reference to the windows dock manager
            if (view is WorkSpaceView currentView)
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                _dockingManager = (DockingManager)currentView.FindName("dockManager");
            }

            Init();
        }

        private async void Init()
        {
            ReBuildMenu();


            Items.Clear();
            // documents
            await ActivateItemAsync<AlignmentToolViewModel>();
            //await ActivateItemAsync<ConcordanceViewModel>();
            await ActivateItemAsync<CorpusTokensViewModel>();
            //await ActivateItemAsync<DashboardViewModel>();
            //await ActivateItemAsync<StartPageViewModel>();
            //await ActivateItemAsync<TreeDownViewModel>();

            // tools
            await ActivateItemAsync<BiblicalTermsViewModel>();
            //await ActivateItemAsync<NotesViewModel>();
            await ActivateItemAsync<PinsViewModel>();
            await ActivateItemAsync<ProjectDesignSurfaceViewModel>();
            //await ActivateItemAsync<SourceContextViewModel>();
            //await ActivateItemAsync<TargetContextViewModel>();
            await ActivateItemAsync<TextCollectionsViewModel>();
            await ActivateItemAsync<WordMeaningsViewModel>();

            // remove all existing windows
            var layoutSerializer = new XmlLayoutSerializer(_dockingManager);

            if (Settings.Default.LastLayout == "")
            {
                // bring up the default layout
                LoadLayout(layoutSerializer, FileLayouts[0].LayoutPath);
            }
            else
            {
                //var filePath = Path.Combine(Environment.CurrentDirectory, @"Resources\Layouts\Dashboard.Layout.config");
                //Settings.Default.LastLayout = filePath;
                // check to see if the layout exists
                string layoutPath = Settings.Default.LastLayout;
                LoadLayout(layoutSerializer, File.Exists(layoutPath) ? layoutPath : FileLayouts[0].LayoutPath);
            }

            // grab the dictionary of all the verse lookups
            if (ProjectManager?.CurrentParatextProject is not null)
            {
                BCVDictionary = ProjectManager.CurrentParatextProject.BcvDictionary;
            }
            else
            {
                BCVDictionary = new Dictionary<string, string>();
            }


            InComingChangesStarted = true;

            // set the CurrentBcv prior to listening to the event
            CurrentBcv.SetVerseFromId(ProjectManager?.CurrentVerse);

            CalculateBooks();
            CalculateChapters();
            CalculateVerses();
            InComingChangesStarted = false;

            // Subscribe to changes of the Book Chapter Verse data object.
            CurrentBcv.PropertyChanged += BcvChanged;
        }

        #endregion //Constructor

        #region Methods

        private Task<TResponse> ExecuteRequest<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
        {
            IsBusy = true;
            try
            {
                return ProjectManager?.ExecuteRequest(request, cancellationToken);
            }
            finally
            {
                IsBusy = false;
            }
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

                foreach (var file in files.Where(f => !f.StartsWith("Project")))
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
            if (ProjectManager?.CurrentDashboardProject?.TargetProject != null)
            {
                path = Path.Combine(ProjectManager?.CurrentDashboardProject?.TargetProject?.DirectoryPath, "shared");
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
            ObservableCollection<MenuItemViewModel> layouts = new()
            {
                // add in the standard menu items
                new MenuItemViewModel
                    { Header = "🖫 Save Current Layout", Id = "SaveID", ViewModel = this, Icon = null },
                new MenuItemViewModel { Header = "🗑 Delete Saved Layout", Id = "DeleteID", ViewModel = this, },
                new MenuItemViewModel { Header = "---- STANDARD LAYOUTS ----", Id = "SeparatorID", ViewModel = this, }
            };


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
                new()
                {
                    Header = "Layouts", Id = "LayoutID", ViewModel = this,
                    MenuItems = layouts,
                },
                new()
                {
                    Header = "Windows", Id = "WindowID", ViewModel = this,
                    MenuItems = new ObservableCollection<MenuItemViewModel>
                    {
                        new() { Header = "⳼ Alignment Tool", Id = "AlignmentToolID", ViewModel = this, },
                        new() { Header = "🕮 Biblical Terms", Id = "BiblicalTermsID", ViewModel = this, },
                        new() { Header = "🆎 Concordance Tool", Id = "ConcordanceToolID", ViewModel = this, },
                        new() { Header = "🗟 Corpus Tokens", Id = "CorpusTokensID", ViewModel = this, },
                        new() { Header = "📐 Dashboard", Id = "DashboardID", ViewModel = this, },
                        new() { Header = "🖉 Notes", Id = "NotesID", ViewModel = this, },
                        new() { Header = "⍒ PINS", Id = "PINSID", ViewModel = this, },
                        new() { Header = "🖧 ProjectDesignSurface", Id = "ProjectDesignSurfaceID", ViewModel = this,  },
                        new() { Header = "⬒ Source Context", Id = "SourceContextID", ViewModel = this, },
                        new() { Header = "⌂ Start Page", Id = "StartPageID", ViewModel = this, },
                        new() { Header = "⬓ Target Context", Id = "TargetContextID", ViewModel = this, },
                        new() { Header = "🗐 Text Collection", Id = "TextCollectionID", ViewModel = this, },
                        new() { Header = "⯭ Treedown", Id = "TreedownID", ViewModel = this, },
                        new() { Header = "⌺ Word Meanings", Id = "WordMeaningsID", ViewModel = this, },
                    }
                },
                new() { Header = "Help", Id = "HelpID", ViewModel = this, }
            };
        }

        /// <summary>
        /// Save the layout
        /// </summary>
        public void OkSave()
        {
            string filePath = "";
            if (SelectedLayout == null)
            {
                // create a new layout
                if (SelectedLayoutText != "")
                {
                    // get the project layouts
                    filePath = Path.Combine(ProjectManager.CurrentDashboardProject.TargetProject.DirectoryPath, "shared");
                    filePath = Path.Combine(filePath, Helpers.Helpers.SanitizeFileName(SelectedLayoutText) + ".Layout.config");
                }
            }
            else
            {
                // overwrite a layout
                filePath = SelectedLayout.LayoutPath;
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
            finally
            {
                GridIsVisible = Visibility.Collapsed;

                ReBuildMenu();
            }

        }

        //public void DeleteLayout(LayoutFile layoutFile)
        //{
        //    if (layoutFile.LayoutType == LayoutFile.eLayoutType.Standard)
        //    {
        //        return;
        //    }

        //    try
        //    {
        //        File.Delete(layoutFile.LayoutPath);
        //    }
        //    catch (Exception e)
        //    {
        //        Logger.LogError(e.Message);
        //        return;
        //    }

        //    FileLayouts.Remove(layoutFile);
        //    ReBuildMenu();
        //}

        public void CancelSave()
        {
            GridIsVisible = Visibility.Collapsed;
        }

        //public void CancelDelete()
        //{
        //    DeleteGridIsVisible = Visibility.Collapsed;
        //}

        private void WorkSpaceViewModel_ThemeChanged()
        {
            GridIsVisible = Visibility.Collapsed;
            SelectedTheme = Settings.Default.Theme == MaterialDesignThemes.Wpf.BaseTheme.Dark ? Themes[0] : Themes[1];
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


        private void LoadLayout(XmlLayoutSerializer layoutSerializer, string filePath)
        {
            // Here I've implemented the LayoutSerializationCallback just to show
            //  a way to feed layout deserialization with content loaded at runtime
            // Actually I could in this case let AvalonDock to attach the contents
            // from current layout using the content ids
            // LayoutSerializationCallback should anyway be handled to attach contents
            // not currently loaded
#pragma warning disable CA1416 // Validate platform compatibility
            layoutSerializer.LayoutSerializationCallback += (_, e) =>
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

                        case WorkspaceLayoutNames.CorpusTokens:
                            e.Content = GetPaneViewModelFromItems("CorpusTokensViewModel");
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
                            e.Content = GetToolViewModelFromItems("TextCollectionsViewModel");
                            break;
                        case WorkspaceLayoutNames.ProjectDesignSurface:
                            e.Content = GetToolViewModelFromItems("ProjectDesignSurfaceViewModel");
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

                    foreach (var t in Items)
                    {
                        var type = t;
                        switch (type)
                        {
                            case AlignmentToolViewModel:
                            //case ConcordanceViewModel:
                            case CorpusTokensViewModel:
                            //case DashboardViewModel:
                            //case StartPageViewModel:
                            //case TreeDownViewModel:
                                _documents.Add((PaneViewModel)t);
                                break;

                            case BiblicalTermsViewModel:
                            //case NotesViewModel:
                            case PinsViewModel:
                            case ProjectDesignSurfaceViewModel:
                            //case SourceContextViewModel:
                            //case TargetContextViewModel:
                            case TextCollectionsViewModel:
                            case WordMeaningsViewModel:
                                _tools.Add((ToolViewModel)t);
                                break;
                        }
                    }

                    NotifyOfPropertyChange(() => Documents);
                    //NotifyOfPropertyChange(() => BookNames);
                }
            }

            // save to settings
            Settings.Default.LastLayout = filePath;
            _lastLayout = filePath;

            // save the layout
            //var layoutSerializer = new XmlLayoutSerializer(this._dockingManager);
            layoutSerializer.Serialize(filePath);
        }

        /// <summary>
        /// return the correct existing vm from Items list - DOCUMENTS
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        private PaneViewModel GetPaneViewModelFromItems(string vm)
        {
            foreach (var t in Items)
            {
                var type = t;
                if (type.GetType().Name == vm)
                {
                    switch (type)
                    {
                        case AlignmentToolViewModel:
                        //case ConcordanceViewModel:
                        case CorpusTokensViewModel:
                        //case DashboardViewModel:
                        //case StartPageViewModel:
                        //case TreeDownViewModel:
                            return (PaneViewModel)t;
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
        private ToolViewModel GetToolViewModelFromItems(string vm)
        {
            foreach (var t in Items)
            {
                var type = t;
                if (type.GetType().Name == vm)
                {
                    switch (type)
                    {
                        case BiblicalTermsViewModel:
                        //case NotesViewModel:
                        case PinsViewModel:
                        case ProjectDesignSurfaceViewModel:
                        //case SourceContextViewModel:
                        //case TargetContextViewModel:
                        case TextCollectionsViewModel:
                        case WordMeaningsViewModel:
                            return (ToolViewModel)t;
                    }
                }
            }

            return (ToolViewModel)Items[0];
        }

        private (object vm, string title, PaneViewModel.EDockSide dockSide) LoadWindow(string windowTag)
        {
            // window has been closed so we need to reopen it
            switch (windowTag)
            {
                // Documents
                case WorkspaceLayoutNames.AlignmentTool:
                    var vm10 = GetPaneViewModelFromItems("AlignmentToolViewModel");
                    return (vm10, vm10.Title, vm10.DockSide);
                //case WorkspaceLayoutNames.ConcordanceTool:
                //    var vm2 = GetPaneViewModelFromItems("ConcordanceViewModel");
                //    return (vm2, vm2.Title, vm2.DockSide);
                case WorkspaceLayoutNames.CorpusTokens:
                    var vm12 = GetPaneViewModelFromItems("CorpusTokensViewModel");
                    return (vm12, vm12.Title, vm12.DockSide);
                //case WorkspaceLayoutNames.Dashboard:
                //    var vm1 = GetPaneViewModelFromItems("DashboardViewModel");
                //    return (vm1, vm1.Title, vm1.DockSide);
                case WorkspaceLayoutNames.Pins:
                    var vm7 = GetPaneViewModelFromItems("PinsViewModel");
                    return (vm7, vm7.Title, vm7.DockSide);
                case WorkspaceLayoutNames.StartPage:
                    var vm9 = GetPaneViewModelFromItems("StartPageViewModel");
                    return (vm9, vm9.Title, vm9.DockSide);
                //case WorkspaceLayoutNames.TreeDown:
                //    var vm11 = GetPaneViewModelFromItems("TreeDownViewModel");
                //    return (vm11, vm11.Title, vm11.DockSide);


                // Tools
                case WorkspaceLayoutNames.BiblicalTerms:
                    var vm = GetToolViewModelFromItems("BiblicalTermsViewModel");
                    return (vm, vm.Title, vm.DockSide);
                //case WorkspaceLayoutNames.Notes:
                //    var vm6 = GetToolViewModelFromItems("NotesViewModel");
                //    return (vm6, vm6.Title, vm6.DockSide);
                case WorkspaceLayoutNames.ProjectDesignSurface:
                    var vm13 = GetToolViewModelFromItems("ProjectDesignSurfaceViewModel");
                    return (vm13, vm13.Title, vm13.DockSide);
                //case WorkspaceLayoutNames.SourceContext:
                //    var vm4 = GetToolViewModelFromItems("SourceContextViewModel");
                //    return (vm4, vm4.Title, vm4.DockSide);
                //case WorkspaceLayoutNames.TargetContext:
                //    var vm5 = GetToolViewModelFromItems("TargetContextViewModel");
                //    return (vm5, vm5.Title, vm5.DockSide);
                case WorkspaceLayoutNames.TextCollection:
                    var vm8 = GetToolViewModelFromItems("TextCollectionsViewModel");
                    return (vm8, vm8.Title, vm8.DockSide);
                case WorkspaceLayoutNames.WordMeanings:
                    var vm3 = GetToolViewModelFromItems("WordMeaningsViewModel");
                    return (vm3, vm3.Title, vm3.DockSide);
            }
            return (null, null, PaneViewModel.EDockSide.Bottom);
        }

        /// <summary>
        /// Un hide window
        /// </summary>
        /// <param name="windowTag"></param>
        private void UnHideWindow(string windowTag)
        {
            // find the pane in the dockmanager with this contentID
#pragma warning disable CA1416 // Validate platform compatibility

            // test for tool window
            var windowPane = _dockingManager.Layout.Descendents()
                .OfType<LayoutAnchorable>()
                .SingleOrDefault(a =>
                {
                    if (a.ContentId is not null)
                    {
                        Debug.WriteLine(a.ContentId);
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
                // test for document window
                var windowDockable = _dockingManager.Layout.Descendents()
                    .OfType<LayoutDocument>()
                    .SingleOrDefault(a =>
                    {
                        if (a.ContentId is not null)
                        {
                            Debug.WriteLine(a.ContentId);
                            return a.ContentId.ToUpper() == windowTag.ToUpper();
                        }
                        return false;
                    });

                if (windowDockable == null)
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
                else
                {
                    windowDockable.IsActive = true;
                }

            }
#pragma warning restore CA1416 // Validate platform compatibility
        }

        private void BcvChanged(object sender, PropertyChangedEventArgs e)
        {
            if (ParatextSync && InComingChangesStarted == false)
            {
                string verseId;
                bool somethingChanged = false;
                if (e.PropertyName == "BookNum")
                {
                    // book switch so find the first chapter and verse for that book
                    verseId = BCVDictionary.Values.First(b => b[..3] == CurrentBcv.Book);
                    if (verseId != "")
                    {
                        InComingChangesStarted = true;
                        CurrentBcv.SetVerseFromId(verseId);

                        CalculateChapters();
                        CalculateVerses();
                        InComingChangesStarted = false;
                        somethingChanged = true;
                    }
                }
                else if (e.PropertyName == "Chapter")
                {
                    // ReSharper disable once InconsistentNaming
                    var BBBCCC = CurrentBcv.Book + CurrentBcv.ChapterIdText;

                    // chapter switch so find the first verse for that book and chapter
                    verseId = BCVDictionary.Values.First(b => b.Substring(0, 6) == BBBCCC);
                    if (verseId != "")
                    {
                        InComingChangesStarted = true;
                        CurrentBcv.SetVerseFromId(verseId);

                        CalculateVerses();
                        InComingChangesStarted = false;
                        somethingChanged = true;
                    }
                }
                else if (e.PropertyName == "Verse")
                {
                    InComingChangesStarted = true;
                    CurrentBcv.SetVerseFromId(CurrentBcv.BBBCCCVVV);
                    InComingChangesStarted = false;
                    somethingChanged = true;
                }

                if (somethingChanged)
                {
                    // send to the event aggregator for everyone else to hear about a verse change
                    EventAggregator.PublishOnUIThreadAsync(new VerseChangedMessage(CurrentBcv.BBBCCCVVV));

                    // push to Paratext
                    if (ParatextSync)
                    {
                        _ = Task.Run(() => ExecuteRequest(new SetCurrentVerseCommand(CurrentBcv.BBBCCCVVV), CancellationToken.None));
                    }
                }

            }
        }

        private void CalculateBooks()
        {
            CurrentBcv.BibleBookList?.Clear();

            var books = BCVDictionary.Values.GroupBy(b => b.Substring(0, 3))
                .Select(g => g.First())
                .ToList();

            foreach (var book in books)
            {
                var bookId = book.Substring(0, 3);

                var bookName = BookChapterVerseViewModel.GetShortBookNameFromBookNum(bookId);

                CurrentBcv.BibleBookList?.Add(bookName);
            }

        }

        private void CalculateChapters()
        {
            // CHAPTERS
            var bookId = CurrentBcv.Book;
            var chapters = BCVDictionary.Values.Where(b => bookId != null && b.StartsWith(bookId)).ToList();
            for (int i = 0; i < chapters.Count; i++)
            {
                chapters[i] = chapters[i].Substring(3, 3);
            }

            chapters = chapters.DistinctBy(v => v).ToList().OrderBy(b => b).ToList();
            // invoke to get it to run in STA mode
            System.Windows.Application.Current.Dispatcher.Invoke(delegate
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
            var bookId = CurrentBcv.Book;
            var chapId = CurrentBcv.ChapterIdText;
            var verses = BCVDictionary.Values.Where(b => b.StartsWith(bookId + chapId)).ToList();

            for (int i = 0; i < verses.Count; i++)
            {
                verses[i] = verses[i].Substring(6);
            }

            verses = verses.DistinctBy(v => v).ToList().OrderBy(b => b).ToList();
            // invoke to get it to run in STA mode
            System.Windows.Application.Current.Dispatcher.Invoke(delegate
            {
                List<int> verseNumbers = new List<int>();
                foreach (var verse in verses)
                {
                    verseNumbers.Add(Convert.ToInt16(verse));
                }

                CurrentBcv.VerseNumbers = verseNumbers;
            });
        }

        public async Task HandleAsync(VerseChangedMessage message, CancellationToken cancellationToken)
        {
            if (CurrentBcv.BibleBookList.Count == 0)
            {
                return;
            }

            if (message.Verse != "" && CurrentBcv.BBBCCCVVV != message.Verse.PadLeft(9, '0'))
            {
                // send to log
                await EventAggregator.PublishOnUIThreadAsync(new LogActivityMessage($"{this.DisplayName}: Project Change"), cancellationToken);

                InComingChangesStarted = true;
                CurrentBcv.SetVerseFromId(message.Verse);

                CalculateChapters();
                CalculateVerses();
                InComingChangesStarted = false;
            }

            return;
        }

        // ReSharper disable once UnusedMember.Global
        public async Task HandleAsync(ProjectChangedMessage message, CancellationToken cancellationToken)
        {
            if (ProjectManager?.CurrentParatextProject is not null)
            {
                // send to log
                await EventAggregator.PublishOnUIThreadAsync(new LogActivityMessage($"{this.DisplayName}: Project Change"), cancellationToken);


                BCVDictionary = ProjectManager.CurrentParatextProject.BcvDictionary;
                InComingChangesStarted = true;

                // add in the books to the dropdown list
                CalculateBooks();

                // set the CurrentBcv prior to listening to the event
                CurrentBcv.SetVerseFromId(ProjectManager.CurrentVerse);

                CalculateChapters();
                CalculateVerses();

                NotifyOfPropertyChange(() => CurrentBcv);
                InComingChangesStarted = false;
            }
            else
            {
                BCVDictionary = new Dictionary<string, string>();
            }

            return;
        }

        #endregion // Methods

        private bool _showProgressBar;
        private string _message;
        public bool ShowProgressBar
        {
            get => _showProgressBar;
            set => Set(ref _showProgressBar, value);
        }

        public string Message
        {
            get => _message;
            set => Set(ref _message, value);
        }

        public async Task HandleAsync(ProgressBarVisibilityMessage message, CancellationToken cancellationToken)
        {
            OnUIThread(() => ShowProgressBar = message.Show);
            await Task.CompletedTask;
        }

        public async Task HandleAsync(ProgressBarMessage message, CancellationToken cancellationToken)
        {
            OnUIThread(() => Message = message.Message);
            await Task.CompletedTask;
        }

    }

    public static class WorkspaceLayoutNames
    {
        public const string AlignmentTool = "ALIGNMENTTOOL";
        public const string BiblicalTerms = "BIBLICALTERMS";
        public const string ConcordanceTool = "CONCORDANCETOOL";
        public const string CorpusTokens = "CORPUSTOKENS";
        public const string Dashboard = "DASHBOARD";
        public const string Notes = "NOTES";
        public const string Pins = "PINS";
        public const string ProjectDesignSurface = "PROJECTDESIGNSURFACETOOL";
        public const string SourceContext = "SOURCECONTEXT";
        public const string StartPage = "STARTPAGE";
        public const string TargetContext = "TARGETCONTEXT";
        public const string TextCollection = "TEXTCOLLECTION";
        public const string TreeDown = "TREEDOWN";
        public const string WordMeanings = "WORDMEANINGS";
    }
}
