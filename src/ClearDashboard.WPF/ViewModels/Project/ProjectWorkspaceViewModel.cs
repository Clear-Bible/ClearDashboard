using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using AvalonDock.Themes;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.Models;
using ClearDashboard.Wpf.Properties;
using ClearDashboard.Wpf.ViewModels.Panes;
using ClearDashboard.Wpf.Views.Project;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.ViewModels.Project
{
    public class ProjectWorkspaceViewModel : Conductor<IScreen>.Collection.AllActive
    {

        private IEventAggregator EventAggregator { get; }
        private DashboardProjectManager ProjectManager { get; }
        private ILogger<ProjectWorkspaceViewModel> Logger { get; }
        private INavigationService NavigationService { get; }
        private DashboardProject DashboardProject { get; }

        private string _lastLayout = "";

        /// <summary>
        /// Required for design-time support
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public ProjectWorkspaceViewModel()
        {

        }

        // ReSharper disable once UnusedMember.Global
        public ProjectWorkspaceViewModel(INavigationService navigationService,
            ILogger<ProjectWorkspaceViewModel> logger, DashboardProjectManager projectManager,
            IEventAggregator eventAggregator)

        {
            EventAggregator = eventAggregator;
            ProjectManager = projectManager;
            Logger = logger;
            NavigationService = navigationService;
            WindowFlowDirection = ProjectManager.CurrentLanguageFlowDirection;

#pragma warning disable CA1416 // Validate platform compatibility
            Themes = new List<Tuple<string, Theme>>
            {
                new(nameof(Vs2013DarkTheme), new Vs2013DarkTheme()),
                new(nameof(Vs2013LightTheme), new Vs2013LightTheme()),
                new(nameof(AeroTheme), new AeroTheme()),
                new(nameof(Vs2013BlueTheme), new Vs2013BlueTheme()),
                new(nameof(GenericTheme), new GenericTheme()),
                new(nameof(ExpressionDarkTheme), new ExpressionDarkTheme()),
                new(nameof(ExpressionLightTheme), new ExpressionLightTheme()),
                new(nameof(MetroTheme), new MetroTheme()),
                new(nameof(VS2010Theme), new VS2010Theme()),
            };
#pragma warning restore CA1416 // Validate platform compatibility

            this.SelectedTheme = Settings.Default.Theme == MaterialDesignThemes.Wpf.BaseTheme.Dark
                ? Themes[0]
                : Themes[1];

            // check if we are in design mode or not
            if (Application.Current != null)
            {
                // subscribe to change events in the parent's theme
                ((App)Application.Current).ThemeChanged += WorkSpaceViewModel_ThemeChanged;

                if (Application.Current is App)
                {
#pragma warning disable CS8601 // Possible null reference assignment.
                    DashboardProject = (Application.Current as App)?.SelectedDashboardProject;
#pragma warning restore CS8601 // Possible null reference assignment.
                }
            }
        }

        private void WorkSpaceViewModel_ThemeChanged()
        {
            //GridIsVisible = Visibility.Collapsed;
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

        private readonly Tuple<string, Theme> _selectedTheme;

        public Tuple<string, Theme> SelectedTheme
        {
            get => _selectedTheme;
            private init
            {
                _selectedTheme = value;
                NotifyOfPropertyChange(() => SelectedTheme);
            }
        }

        private DocumentViewModel _activeDocument;

        public DocumentViewModel ActiveDocument
        {
            get => _activeDocument;
            set
            {
                if (_activeDocument != value)
                {
                    _activeDocument = value;
                    //NotifyOfPropertyChange(() => MenuItems);
                    //ActiveDocumentChanged(this, EventArgs.Empty);
                }
            }
        }

#pragma warning disable CA1416 // Validate platform compatibility
        private DockingManager _dockingManager = new();
#pragma warning restore CA1416 // Validate platform compatibility

        /// <summary>
        /// Binds the viewmodel to it's view prior to activating so that the OnViewAttached method of the
        /// child viewmodel are called.
        /// </summary>
        /// <typeparam name="TViewModel"></typeparam>
        /// <returns></returns>
        private async Task ActivateItemAsync<TViewModel>(CancellationToken cancellationToken = default)
            where TViewModel : class, IScreen
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
            //if (_lastLayout == "")
            //{
            //    SelectedLayoutText = "Last Saved";
            //    OkSave();
            //}

            // unsubscribe to the event aggregator
            EventAggregator.Unsubscribe(this);
            return base.OnDeactivateAsync(close, cancellationToken);
        }

        protected override void OnViewAttached(object view, object context)
        {
            base.OnViewAttached(view, context);

            // hook up a reference to the windows dock manager
            if (view is ProjectWorkspaceView currentView)
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                _dockingManager = (DockingManager)currentView.FindName("dockManager");
            }

            Initialize();
        }


        private ObservableCollection<LayoutFile> GetFileLayouts()
        {
            int id = 0;
            ObservableCollection<LayoutFile> fileLayouts = new();
            // add in the default layouts
            var path = Path.Combine(Environment.CurrentDirectory, @"Resources\ProjectLayouts");
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
            path = Path.Combine("ProjectWorkspace", "shared");
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

            return fileLayouts;
        }

        private async void Initialize()
        {
            FileLayouts = GetFileLayouts();
            Items.Clear();
            // documents
            await ActivateItemAsync<AlignmentViewModel>();
            await ActivateItemAsync<CorpusViewModel>();

            // tools
            await ActivateItemAsync<ProjectDesignSurfaceViewModel>();

            // remove all existing windows
            var layoutSerializer = new XmlLayoutSerializer(_dockingManager);

            if (Settings.Default.LastProjectLayout == "")
            {
                // bring up the default layout
                LoadLayout(layoutSerializer, FileLayouts[0].LayoutPath);
            }
            else
            {
                // check to see if the layout exists
                string layoutPath = Settings.Default.LastProjectLayout;
                LoadLayout(layoutSerializer, File.Exists(layoutPath) ? layoutPath : FileLayouts[0].LayoutPath);
            }
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
                        case WorkspaceLayoutNames.AlignmentTool:
                            e.Content = GetPaneViewModelFromItems("AlignmentViewModel");
                            break;
                        case WorkspaceLayoutNames.CorpusTool:
                            e.Content = GetPaneViewModelFromItems("CorpusViewModel");
                            break;
                        case WorkspaceLayoutNames.ProjectDesignSurfaceTool:
                            e.Content = GetPaneViewModelFromItems("ProjectDesignSurfaceViewModel");
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
                            case DashboardViewModel:
                            case ConcordanceViewModel:
                            case StartPageViewModel:
                            case AlignmentToolViewModel:
                            case TreeDownViewModel:
                                _documents.Add((PaneViewModel)t);
                                break;

                            case BiblicalTermsViewModel:
                            case WordMeaningsViewModel:
                            case SourceContextViewModel:
                            case TargetContextViewModel:
                            case NotesViewModel:
                            case PinsViewModel:
                            case TextCollectionsViewModel:
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
                        case DashboardViewModel:
                        case ConcordanceViewModel:
                        case StartPageViewModel:
                        case AlignmentToolViewModel:
                        case TreeDownViewModel:
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
                        case WordMeaningsViewModel:
                        case SourceContextViewModel:
                        case TargetContextViewModel:
                        case NotesViewModel:
                        case PinsViewModel:
                        case TextCollectionsViewModel:
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
                    var vm1 = GetPaneViewModelFromItems("AlignmentViewModel");
                    return (vm1, vm1.Title, vm1.DockSide);
                case WorkspaceLayoutNames.CorpusTool:
                    var vm2 = GetPaneViewModelFromItems("CopusViewModel");
                    return (vm2, vm2.Title, vm2.DockSide);
                case WorkspaceLayoutNames.ProjectDesignSurfaceTool:
                    var vm7 = GetPaneViewModelFromItems("ProjectDesignSurfaceViewModel");
                    return (vm7, vm7.Title, vm7.DockSide);
                

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

        public static class WorkspaceLayoutNames
        {
            public const string AlignmentTool = "ALIGNMENTTOOL";
            public const string CorpusTool = "CORPUSTOOL";
            public const string ProjectDesignSurfaceTool = "PROJECTDESIGNSURFACETOOL";
        }
    }
}
