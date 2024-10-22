﻿using System;
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
using ClearDashboard.Wpf.Views;
using ClearDashboard.Wpf.Views.Project;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.ViewModels.Project
{
  
    public class ProjectWorkspaceViewModel : Conductor<Screen>.Collection.AllActive, IHandle<ProgressBarVisibilityMessage>, IHandle<ProgressBarMessage>
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
            SelectedTheme = Settings.Default.Theme == MaterialDesignThemes.Wpf.BaseTheme.Dark
                ? Themes[0]
                : Themes[1];
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
        private bool _showProgressBar;
        private string _message;
#pragma warning restore CA1416 // Validate platform compatibility

        /// <summary>
        /// Binds the viewmodel to it's view prior to activating so that the OnViewAttached method of the
        /// child viewmodel are called.
        /// </summary>
        /// <typeparam name="TViewModel"></typeparam>
        /// <returns></returns>
        private async Task ActivateItemAsync<TViewModel>(CancellationToken cancellationToken = default)
            where TViewModel : Screen
        {
            var viewModel = IoC.Get<TViewModel>();
            viewModel.Parent = this;
            viewModel.ConductWith(this);
            var view = ViewLocator.LocateForModel(viewModel, null, null);
            ViewModelBinder.Bind(viewModel, view, null);
            await ActivateItemAsync(viewModel, cancellationToken);
        }

        public Task ActivateOrCreate<T>(string displayName)
            where T : Screen
        {
            var item = Items.OfType<T>().FirstOrDefault(x => x.DisplayName == displayName);
            if (item == null)
            {
                item = (T)Activator.CreateInstance(typeof(T));
                item.Parent = this;
                item.ConductWith(this);
                item.DisplayName = displayName;
                //item.IsDirty = ++_createCount % 2 > 0;
            }
            return ActivateItemAsync(item, CancellationToken.None);
        }

        protected override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            // subscribe to the event aggregator so that we can listen to messages
            Logger.LogInformation($"Subscribing {this.GetType().Name} to the EventAggregator");
            EventAggregator.SubscribeOnUIThread(this);

            return base.OnActivateAsync(cancellationToken);
        }

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            if (_lastLayout == "")
            {
                SaveLayout();
            }

            // unsubscribe to the event aggregator
            Logger.LogInformation($"Unsubscribing {this.GetType().Name} from the EventAggregator");
            EventAggregator.Unsubscribe(this);
            return base.OnDeactivateAsync(close, cancellationToken);
        }

        public void SaveLayout()
        {
            var layoutSerializer = new XmlLayoutSerializer(this._dockingManager);
            var filePath = Path.Combine(Environment.CurrentDirectory, @"Resources\Layouts\Project.Layout.config");
            layoutSerializer.Serialize(filePath);
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

           
        }

        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            await Initialize();
            await base.OnInitializeAsync(cancellationToken);
        }


        private async Task Initialize()
        {
            Items.Clear();
            //documents
            //await ActivateItemAsync<CorpusViewModel>();
            await ActivateItemAsync<CorpusTokensViewModel>();
           

            // tools
            await ActivateItemAsync<ProjectDesignSurfaceViewModel>();
            LoadWindows();


            var layoutSerializer = new XmlLayoutSerializer(_dockingManager);
            var filePath = Path.Combine(Environment.CurrentDirectory, @"Resources\Layouts\Project.Layout.config");
            LoadLayout(layoutSerializer, filePath);


           
        }

        private void LoadWindows()
        {
            // build a layout
            _tools.Clear();
            _documents.Clear();

            foreach (var t in Items)
            {
                var type = t;
                switch (type)
                {
                    case CorpusTokensViewModel:
                    case CorpusViewModel:
                        _documents.Add((PaneViewModel)t);
                        break;
                    case ProjectDesignSurfaceViewModel:
                        _tools.Add((ToolViewModel)t);
                        break;
                }
            }

            NotifyOfPropertyChange(() => Documents);
            NotifyOfPropertyChange(() => Tools);
        }


        private void LoadLayout(XmlLayoutSerializer layoutSerializer, string filePath)
        {
           
            layoutSerializer.LayoutSerializationCallback += async (_, e) =>
            {
                if (e.Model.ContentId is not null)
                {
                    var item = Items.Cast<IAvalonDockWindow>()
                        .FirstOrDefault(item => item.ContentId == e.Model.ContentId);

                    if (item != null)
                    {
                        //await ActivateItemAsync((Screen)item);
                        e.Content = item;
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
                filePath = Path.Combine(Environment.CurrentDirectory, @"Resources\Layouts\Project.Layout.config");
                if (File.Exists(filePath))
                {
                    // load in the default layout
                    layoutSerializer.Deserialize(filePath);
                }
                else
                {
                    LoadWindows();
                }
            }

            // save to settings
            Settings.Default.LastProjectLayout = filePath;
            _lastLayout = filePath;
        }

        public async Task ActiveAlignmentView()
        {
            await ActivateItemAsync<CorpusTokensViewModel>();
        }

        public static class WorkspaceLayoutNames
        {
            public const string AlignmentTool = "ALIGNMENTTOOL";
            public const string CorpusTool = "CORPUSTOOL";
            public const string ProjectDesignSurfaceTool = "PROJECTDESIGNSURFACETOOL";
        }

        public bool ShowProgressBar
        {
            get => _showProgressBar;
            set => Set(ref _showProgressBar,value);
        }

        public string Message
        {
            get => _message;
            set => Set(ref _message, value);
        }

        public async Task HandleAsync(ProgressBarVisibilityMessage message, CancellationToken cancellationToken)
        {
           OnUIThread(()=>ShowProgressBar = message.Show);
           await Task.CompletedTask;
        }

        public async Task HandleAsync(ProgressBarMessage message, CancellationToken cancellationToken)
        {
            OnUIThread(() => Message = message.Message);
            await Task.CompletedTask;
        }
    }
}
