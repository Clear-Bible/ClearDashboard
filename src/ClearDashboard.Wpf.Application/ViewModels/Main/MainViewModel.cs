using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using AvalonDock.Themes;
using Caliburn.Micro;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearDashboard.DAL.ViewModels;
using ClearDashboard.DataAccessLayer.Features.Bcv;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.ParatextPlugin.CQRS.Features.Verse;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Models;
using ClearDashboard.Wpf.Application.Properties;
using ClearDashboard.Wpf.Application.ViewModels.Menus;
using ClearDashboard.Wpf.Application.ViewModels.Panes;
using ClearDashboard.Wpf.Application.ViewModels.ParatextViews;
using ClearDashboard.Wpf.Application.ViewModels.Project;
using ClearDashboard.Wpf.Application.Views.Main;
using ClearDashboard.Wpf.Application.Views.Project;
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
using Autofac;
using Autofac.Core.Lifetime;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.Wpf.Application.ViewModels.Startup;
using Microsoft.Xaml.Behaviors.Core;
using SIL.Reporting;
using ClearDashboard.DataAccessLayer;
using DockingManager = AvalonDock.DockingManager;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.ParatextPlugin.CQRS.Features.Projects;

namespace ClearDashboard.Wpf.Application.ViewModels.Main
{
    public class MainViewModel : Conductor<IScreen>.Collection.AllActive,
                IHandle<VerseChangedMessage>,
                IHandle<ProjectChangedMessage>,
                IHandle<ProgressBarVisibilityMessage>,
                IHandle<ProgressBarMessage>,
                IHandle<ShowTokenizationWindowMessage>,
                IHandle<UiLanguageChangedMessage>,
                IHandle<ActiveDocumentMessage>,
                IHandle<NewProjectPickerMessage>
    {
        private ILifetimeScope LifetimeScope { get; }
        private IWindowManager WindowManager { get; }
#nullable disable
        #region Member Variables
        private IEventAggregator EventAggregator { get; }
        private DashboardProjectManager ProjectManager { get; }
        private ILogger<MainViewModel> Logger { get; }

#pragma warning disable CA1416 // Validate platform compatibility
        private DockingManager _dockingManager = new();
        private ProjectDesignSurfaceView _projectDesignSurfaceControl;
        private ProjectDesignSurfaceViewModel _projectDesignSurfaceViewModel;
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

        private bool InComingChangesStarted { get; set; }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => Set(ref _isBusy, value);
        }

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public DashboardProject Parameter { get; set; }
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
                        case "BiblicalTermsID":
                            _windowIdToLoad = "BIBLICALTERMS";
                            break;
                        case "EnhancedCorpusID":
                            _windowIdToLoad = "ENHANCEDCORPUS";
                            break;
                        case "PINSID":
                            _windowIdToLoad = "PINS";
                            break;
                        case "WordMeaningsID":
                            _windowIdToLoad = "WORDMEANINGS";
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

        private async Task StartDashboardAsync(int secondsToWait = 10)
        {
            _ = Process.GetProcessesByName("ClearDashboard.Wpf.Application");

            Logger.LogInformation("Opening a new instance of Dashboard.");
            _ = await InternalStartDashboardAsync();


            Logger.LogInformation($"Waiting {secondsToWait} seconds for Dashboard to completely start");
            await Task.Delay(TimeSpan.FromSeconds(secondsToWait));
        }

        private async Task<Process> InternalStartDashboardAsync()
        {
            string programFiles = Environment.ExpandEnvironmentVariables("%ProgramW6432%");
            var dashboardInstallDirectory = Path.Combine(programFiles, "Clear Dashboard");
            var process = Process.Start($"{dashboardInstallDirectory}\\ClearDashboard.Wpf.Application.exe");

            return await Task.FromResult(process);
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
                NotifyOfPropertyChange(() => SelectedLayout);
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
            init => Set(ref _windowFlowDirection, value);
        }

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

        private string _projectName;
        public string ProjectName
        {
            get => _projectName;
            set
            {
                _projectName = value;
                NotifyOfPropertyChange(nameof(ProjectName));
            }
        }

        #endregion //Observable Properties

        #region Constructor

        /// <summary>
        /// Required for design-time support
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public MainViewModel()
        {
            // no-op for design time support
        }


        // ReSharper disable once UnusedMember.Global
        public MainViewModel(INavigationService navigationService, ILogger<MainViewModel> logger, DashboardProjectManager projectManager, IEventAggregator eventAggregator, IWindowManager windowManager, ILifetimeScope lifetimeScope)
        {
            LifetimeScope = lifetimeScope;
            WindowManager = windowManager;
            EventAggregator = eventAggregator;
            ProjectManager = projectManager;
            Logger = logger;
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

            this.SelectedTheme = Settings.Default.Theme == MaterialDesignThemes.Wpf.BaseTheme.Dark ? Themes[0] : Themes[1];

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

        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            EventAggregator.SubscribeOnUIThread(this);
            Logger.LogInformation($"Subscribing {nameof(MainViewModel)} to the EventAggregator");


            await base.OnActivateAsync(cancellationToken);
        }

        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            if (Parameter != null)
            {

                Logger.LogInformation($"Received {Parameter.ProjectName}.");
                if (Parameter.IsNew)
                {
                    await ProjectManager.CreateNewProject(Parameter.ProjectName);
                }
                else
                {
                    await ProjectManager.LoadProject(Parameter.ProjectName);
                }
                ProjectName = ProjectManager.CurrentProject.ProjectName;
            }



            await base.OnInitializeAsync(cancellationToken);
        }

        protected override async void OnViewLoaded(object view)
        {
            // send out a notice that the project is loaded up
            await EventAggregator.PublishOnUIThreadAsync(new ProjectLoadCompleteMessage(true));

            base.OnViewLoaded(view);
        }

        protected override async Task<Task> OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            Logger.LogInformation($"{nameof(MainViewModel)} is deactivating.");

            if (_lastLayout == "")
            {
                SelectedLayoutText = "Last Saved";
                OkSave();
            }

            // save the design surface
            await _projectDesignSurfaceViewModel.SaveCanvas();

            //we need to cancel running background processes
            //check a bool to see if it already cancelled or already completed
            if (_projectDesignSurfaceViewModel.LongProcessRunning)
            {
#pragma warning disable CS8602
                // ReSharper disable once PossibleNullReferenceException
                _projectDesignSurfaceViewModel.CancellationTokenSource.Cancel();
#pragma warning restore CS8602
                await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
                {
                    Name = "Corpus",
                    Description = "Task was cancelled",
                    EndTime = DateTime.Now,
                    TaskStatus = StatusEnum.Completed
                }), cancellationToken);
            }



            // unsubscribe to the event aggregator
            Logger.LogInformation($"Unsubscribing {nameof(MainViewModel)} to the EventAggregator");
            EventAggregator?.Unsubscribe(this);
            return base.OnDeactivateAsync(close, cancellationToken);
        }

        protected override async void OnViewAttached(object view, object context)
        {
            base.OnViewAttached(view, context);

            // hook up a reference to the windows dock manager
            if (view is MainView currentView)
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                _dockingManager = (DockingManager)currentView.FindName("DockManager");
                _projectDesignSurfaceControl = (ProjectDesignSurfaceView)currentView.FindName("ProjectDesignSurfaceControl");

                // subscribe to the event aggregator
                //_dockingManager.ActiveContentChanged += new EventHandler(OnActiveDocumentChanged);
            }

            await Task.Delay(250);
            Init();
        }

        private async void Init()
        {
            RebuildMenu();

            _projectDesignSurfaceViewModel = IoC.Get<ProjectDesignSurfaceViewModel>();
            var view = ViewLocator.LocateForModel(_projectDesignSurfaceViewModel, null, null);
            ViewModelBinder.Bind(_projectDesignSurfaceViewModel, view, null);
            _projectDesignSurfaceControl.DataContext = _projectDesignSurfaceViewModel;

            // force a load to happen as it is getting swallowed up elsewhere
            _projectDesignSurfaceViewModel.LoadCanvas();

            Items.Clear();

            // documents
            await ActivateItemAsync<EnhancedCorpusViewModel>();

            // tools
            await ActivateItemAsync<BiblicalTermsViewModel>();
            await ActivateItemAsync<PinsViewModel>();
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

            //CalculateBooks();
            //CalculateChapters();
            //CalculateVerses();
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
            if (ProjectManager.CurrentParatextProject.DirectoryPath != null)
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                path = Path.Combine(ProjectManager.CurrentParatextProject.DirectoryPath, "shared");
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

        private void RebuildMenu()
        {
            FileLayouts = GetFileLayouts();
            ObservableCollection<MenuItemViewModel> layouts = new()
            {
                // add in the standard menu items

                // Save Current Layout
                new MenuItemViewModel
                {
                    Header = "🖫 " + LocalizationStrings.Get("MainView_LayoutsSave", Logger), Id = "SaveID",
                    ViewModel = this
                },
                
                // Delete Saved Layout
                new MenuItemViewModel
                {
                    Header = "🗑 " + LocalizationStrings.Get("MainView_LayoutsDelete", Logger), Id = "DeleteID",
                    ViewModel = this,
                },

                // STANDARD LAYOUTS
                new MenuItemViewModel
                {
                    Header = "---- " + LocalizationStrings.Get("MainView_LayoutsStandardLayouts", Logger) + " ----",
                    Id = "SeparatorID", ViewModel = this,
                }
            };

            var bFound = false;
            foreach (var fileLayout in FileLayouts)
            {
                // PROJECT LAYOUTS
                if (fileLayout.LayoutID.StartsWith("ProjectLayout:") && bFound == false)
                {
                    layouts.Add(new MenuItemViewModel
                    {
                        Header = "---- " + LocalizationStrings.Get("MainView_LayoutsProjectLayouts", Logger) + " ----",
                        Id = "SeparatorID",
                        ViewModel = this,
                    });
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
                // File
                new()
                {
                    Header = LocalizationStrings.Get("MainView_File", Logger), Id = "FileID", ViewModel = this,
                    MenuItems = new ObservableCollection<MenuItemViewModel>
                    {
                        // New
                        new() { Header = LocalizationStrings.Get("MainView_FileNew", Logger), Id = "NewID", ViewModel = this, }
                    }
                },
                new()
                {
                    // Layouts
                    Header = LocalizationStrings.Get("MainView_Layouts", Logger), Id = "LayoutID", ViewModel = this,
                    MenuItems = layouts,
                },
                new()
                {
                    // Windows
                    Header = LocalizationStrings.Get("MainView_Windows", Logger), Id = "WindowID", ViewModel = this,
                    MenuItems = new ObservableCollection<MenuItemViewModel>
                    {
                        // Biblical Terms
                        new() { Header = "🕮 " + LocalizationStrings.Get("MainView_WindowsBiblicalTerms", Logger), Id = "BiblicalTermsID", ViewModel = this, },
                        
                        // Enhanced Corpus
                        new() { Header = "⳼ " + LocalizationStrings.Get("MainView_WindowsEnhancedCorpus", Logger), Id = "EnhancedCorpusID", ViewModel = this, },
                        
                        // PINS
                        new() { Header = "⍒ " + LocalizationStrings.Get("MainView_WindowsPINS", Logger), Id = "PINSID", ViewModel = this, },
                        
                        // Text Collection
                        new() { Header = "🗐 " + LocalizationStrings.Get("MainView_WindowsTextCollections", Logger), Id = "TextCollectionID", ViewModel = this, },
                        
                        // Word Meanings
                        new() { Header = "⌺ " + LocalizationStrings.Get("MainView_WindowsWordMeanings", Logger), Id = "WordMeaningsID", ViewModel = this, },
                    }
                },
                
                // HELP
                new() { Header = LocalizationStrings.Get("MainView_Help", Logger), Id =  "HelpID", ViewModel = this, }
            };
        }

        /// <summary>
        /// Save the layout
        /// </summary>
        public async void OkSave()
        {
            var filePath = string.Empty;
            if (SelectedLayout == null)
            {
                // create a new layout
                if (SelectedLayoutText != string.Empty)
                {
                    filePath = Path.Combine(ProjectManager.CurrentParatextProject.DirectoryPath, "shared");
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

                RebuildMenu();
            }

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
            RebuildMenu();
        }

        public void CancelSave()
        {
            GridIsVisible = Visibility.Collapsed;
        }

        public void CancelDelete()
        {
            DeleteGridIsVisible = Visibility.Collapsed;
        }

        //private void WorkSpaceViewModel_ThemeChanged()
        //{
        //    GridIsVisible = Visibility.Collapsed;
        //    SelectedTheme = Settings.Default.Theme == MaterialDesignThemes.Wpf.BaseTheme.Dark ? Themes[0] : Themes[1];
        //}

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
                        case WorkspaceLayoutNames.EnhancedCorpus:
                            e.Content = GetPaneViewModelFromItems("EnhancedCorpusViewModel");
                            break;

                        // Tools
                        case WorkspaceLayoutNames.BiblicalTerms:
                            e.Content = GetToolViewModelFromItems("BiblicalTermsViewModel");
                            break;
                        case WorkspaceLayoutNames.WordMeanings:
                            e.Content = GetToolViewModelFromItems("WordMeaningsViewModel");
                            break;
                        case WorkspaceLayoutNames.Pins:
                            e.Content = GetToolViewModelFromItems("PinsViewModel");
                            break;
                        case WorkspaceLayoutNames.TextCollection:
                            e.Content = GetToolViewModelFromItems("TextCollectionsViewModel");
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

                            case EnhancedCorpusViewModel:
                                _documents.Add((PaneViewModel)t);
                                break;

                            case BiblicalTermsViewModel:
                            case PinsViewModel:
                            case TextCollectionsViewModel:
                            case WordMeaningsViewModel:
                                _tools.Add((ToolViewModel)t);
                                break;
                        }
                    }

                    NotifyOfPropertyChange(() => Documents);
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
                        case EnhancedCorpusViewModel:
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
                        case PinsViewModel:
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
                // DOCUMENTS
                case WorkspaceLayoutNames.EnhancedCorpus:
                    var vm14 = GetPaneViewModelFromItems("EnhancedCorpusViewModel");
                    return (vm14, vm14.Title, vm14.DockSide);

                // TOOLS
                case WorkspaceLayoutNames.BiblicalTerms:
                    var vm = GetToolViewModelFromItems("BiblicalTermsViewModel");
                    return (vm, vm.Title, vm.DockSide);
                case WorkspaceLayoutNames.Pins:
                    var vm7 = GetToolViewModelFromItems("PinsViewModel");
                    return (vm7, vm7.Title, vm7.DockSide);
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
                    switch (windowTag.ToUpper())
                    {
                        // Documents
                        case WorkspaceLayoutNames.EnhancedCorpus:
                            windowDockable = new LayoutDocument
                            {
                                ContentId = windowTag
                            };
                            // setup the right ViewModel for the pane
                            var obj = LoadWindow(windowTag);
                            windowDockable.Content = obj.vm;
                            windowDockable.Title = obj.title;
                            windowDockable.IsActive = true;

                            var documentPane = _dockingManager.Layout.Descendents().OfType<LayoutDocumentPane>().FirstOrDefault();

                            if (documentPane != null)
                            {
                                documentPane.Children.Add(windowDockable);
                            }
                            break;

                        // Tools
                        case WorkspaceLayoutNames.BiblicalTerms:
                        case WorkspaceLayoutNames.WordMeanings:
                        case WorkspaceLayoutNames.Pins:
                        case WorkspaceLayoutNames.TextCollection:

                            // window has been closed so reload it
                            windowPane = new LayoutAnchorable
                            {
                                ContentId = windowTag
                            };

                            // setup the right ViewModel for the pane
                            obj = LoadWindow(windowTag);
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
                            break;

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
            //if (ParatextSync && InComingChangesStarted == false)
            //{
            //    string verseId;
            //    bool somethingChanged = false;
            //    if (e.PropertyName == "BookNum")
            //    {
            //        // book switch so find the first chapter and verse for that book
            //        verseId = BCVDictionary.Values.First(b => b[..3] == CurrentBcv.Book);
            //        if (verseId != "")
            //        {
            //            InComingChangesStarted = true;
            //            CurrentBcv.SetVerseFromId(verseId);

            //            CalculateChapters();
            //            CalculateVerses();
            //            InComingChangesStarted = false;
            //            somethingChanged = true;
            //        }
            //    }
            //    else if (e.PropertyName == "Chapter")
            //    {
            //        // ReSharper disable once InconsistentNaming
            //        var BBBCCC = CurrentBcv.Book + CurrentBcv.ChapterIdText;

            //        // chapter switch so find the first verse for that book and chapter
            //        verseId = BCVDictionary.Values.First(b => b.Substring(0, 6) == BBBCCC);
            //        if (verseId != "")
            //        {
            //            InComingChangesStarted = true;
            //            CurrentBcv.SetVerseFromId(verseId);

            //            CalculateVerses();
            //            InComingChangesStarted = false;
            //            somethingChanged = true;
            //        }
            //    }
            //    else if (e.PropertyName == "Verse")
            //    {
            //        InComingChangesStarted = true;
            //        CurrentBcv.SetVerseFromId(CurrentBcv.BBBCCCVVV);
            //        InComingChangesStarted = false;
            //        somethingChanged = true;
            //    }

            //    if (somethingChanged)
            //    {
            //        // send to the event aggregator for everyone else to hear about a verse change
            //        EventAggregator.PublishOnUIThreadAsync(new VerseChangedMessage(CurrentBcv.BBBCCCVVV));

            //        // push to Paratext
            //        if (ParatextSync)
            //        {
            //            _ = Task.Run(() => ExecuteRequest(new SetCurrentVerseCommand(CurrentBcv.BBBCCCVVV), CancellationToken.None));
            //        }
            //    }

            //}
        }

        //private void CalculateBooks()
        //{
        //    CurrentBcv.BibleBookList?.Clear();

        //    var books = BCVDictionary.Values.GroupBy(b => b.Substring(0, 3))
        //        .Select(g => g.First())
        //        .ToList();

        //    foreach (var book in books)
        //    {
        //        var bookId = book.Substring(0, 3);

        //        var bookName = BookChapterVerseViewModel.GetShortBookNameFromBookNum(bookId);

        //        CurrentBcv.BibleBookList?.Add(bookName);
        //    }

        //}

        //private void CalculateChapters()
        //{
        //    // CHAPTERS
        //    var bookId = CurrentBcv.Book;
        //    var chapters = BCVDictionary.Values.Where(b => bookId != null && b.StartsWith(bookId)).ToList();
        //    for (var i = 0; i < chapters.Count; i++)
        //    {
        //        chapters[i] = chapters[i].Substring(3, 3);
        //    }

        //    chapters = chapters.DistinctBy(v => v).ToList().OrderBy(b => b).ToList();
        //    // invoke to get it to run in STA mode
        //    System.Windows.Application.Current.Dispatcher.Invoke(delegate
        //    {
        //        var chapterNumbers = new List<int>();
        //        foreach (var chapter in chapters)
        //        {
        //            chapterNumbers.Add(Convert.ToInt16(chapter));
        //        }

        //        CurrentBcv.ChapterNumbers = chapterNumbers;
        //    });
        //}

        //private void CalculateVerses()
        //{
        //    // VERSES
        //    var bookId = CurrentBcv.Book;
        //    var chapId = CurrentBcv.ChapterIdText;
        //    var verses = BCVDictionary.Values.Where(b => b.StartsWith(bookId + chapId)).ToList();

        //    for (int i = 0; i < verses.Count; i++)
        //    {
        //        verses[i] = verses[i].Substring(6);
        //    }

        //    verses = verses.DistinctBy(v => v).ToList().OrderBy(b => b).ToList();
        //    // invoke to get it to run in STA mode
        //    System.Windows.Application.Current.Dispatcher.Invoke(delegate
        //    {
        //        List<int> verseNumbers = new List<int>();
        //        foreach (var verse in verses)
        //        {
        //            verseNumbers.Add(Convert.ToInt16(verse));
        //        }

        //        CurrentBcv.VerseNumbers = verseNumbers;
        //    });
        //}

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

                //CalculateChapters();
                //CalculateVerses();
                InComingChangesStarted = false;
            }
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
                //CalculateBooks();

                // set the CurrentBcv prior to listening to the event
                CurrentBcv.SetVerseFromId(ProjectManager.CurrentVerse);

                //CalculateChapters();
                //CalculateVerses();

                NotifyOfPropertyChange(() => CurrentBcv);
                InComingChangesStarted = false;
            }
            else
            {
                BCVDictionary = new Dictionary<string, string>();
            }
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

        /// <summary>
        /// Pop open a new Corpus Tokization window and pass in the current corpus
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task HandleAsync(ShowTokenizationWindowMessage message, CancellationToken cancellationToken)
        {

            // the user wants to add to the currently active window
            if (message.IsNewWindow == false)
            {
                var dockableWindows = _dockingManager.Layout.Descendents()
                    .OfType<LayoutDocument>().ToList();
                if (dockableWindows.Count == 1)
                {
                    // there is only one doc window open, so we can just add to it
                    var enhancedCorpusViewModels =
                        Items.First(items => items.GetType() == typeof(EnhancedCorpusViewModel)) as
                            EnhancedCorpusViewModel;
                    if (enhancedCorpusViewModels is not null)
                    {
                        await enhancedCorpusViewModels.ShowCorpusTokens(message, cancellationToken);
                    }

                    return;
                }

                // more than one enhanced corpus window is open and active
                foreach (var document in dockableWindows)
                {
                    if (document.IsActive && document.Content is EnhancedCorpusViewModel)
                    {
                        var vm = document.Content as EnhancedCorpusViewModel;
                        // ReSharper disable once PossibleNullReferenceException
                        var guid = vm.Guid;

                        var enhancedCorpusViewModels =
                            Items.Where(items => items.GetType() == typeof(EnhancedCorpusViewModel))
                                    // ReSharper disable once UsePatternMatching
                                    .First(item => ((EnhancedCorpusViewModel)item).Guid == guid) as
                                EnhancedCorpusViewModel;
                        if (enhancedCorpusViewModels is not null)
                        {
                            await enhancedCorpusViewModels.ShowCorpusTokens(message, cancellationToken);
                            return;
                        }
                    }
                }
            }

            // unactivate any other doc windows before we add in the new one
            var docWindows = _dockingManager.Layout.Descendents()
                .OfType<LayoutDocument>();
            foreach (var document in docWindows)
            {
                document.IsActive = false;
            }


            string tokenizationType = message.TokenizationType;
            string paratextId = message.ParatextProjectId;

            EnhancedCorpusViewModel viewModel = IoC.Get<EnhancedCorpusViewModel>();
            viewModel.CurrentCorpusName = message.ProjectName;
            viewModel.Title = message.ProjectName + " (" + tokenizationType + ")";
            viewModel.BcvDictionary = ProjectManager.CurrentParatextProject.BcvDictionary;
            viewModel.CurrentBcv.SetVerseFromId(ProjectManager.CurrentVerse);
            viewModel.VerseChange = ProjectManager.CurrentVerse;

            // add vm to conductor
            Items.Add(viewModel);

            // make a new document for the windows
            var windowDockable = new LayoutDocument
            {
                ContentId = paratextId,
                Content = viewModel,
                Title = message.ProjectName + " (" + tokenizationType + ")",
                IsActive = true
            };

            var documentPane = _dockingManager.Layout.Descendents().OfType<LayoutDocumentPane>().FirstOrDefault();
            documentPane?.Children.Add(windowDockable);

            await viewModel.ShowCorpusTokens(message, cancellationToken);
        }

        public Task HandleAsync(UiLanguageChangedMessage message, CancellationToken cancellationToken)
        {
            // pass up to the Project Design Surface the message
            if (_projectDesignSurfaceViewModel is not null)
            {
                _projectDesignSurfaceViewModel.UiLanguageChangedMessage(message);
            }

            // rebuild the menu system with the new language
            RebuildMenu();

            return Task.CompletedTask;
        }

        public async Task ExecuteMenuCommand(MenuItemViewModel menuItem)
        {
            switch (menuItem.Id)
            {
                case "NewID":
                    //var startupDialogViewModel = IoC.Get<StartupDialogViewModel>();
                    var startupDialogViewModel = LifetimeScope!.Resolve<StartupDialogViewModel>();
                    startupDialogViewModel.MimicParatextConnection = true;
                    var result = await WindowManager.ShowDialogAsync(startupDialogViewModel);
                    if (result.HasValue && result.Value)
                    {
                        var dashboardProject = startupDialogViewModel.ExtraData as DashboardProject;
                        if (dashboardProject.IsNew)
                        {
                            await ProjectManager.CreateNewProject(dashboardProject.ProjectName);
                        }
                        else
                        {
                            await ProjectManager.LoadProject(dashboardProject.ProjectName);
                        }

                        ProjectName = dashboardProject.ProjectName;
                    }
                    break;
            }
        }

        #endregion // Methods

        public Task HandleAsync(ActiveDocumentMessage message, CancellationToken cancellationToken)
        {
            var guid = message.Guid;

            var dockableWindows = _dockingManager.Layout.Descendents()
                .OfType<LayoutDocument>();
            foreach (var pane in dockableWindows)
            {
                var content = pane.Content as EnhancedCorpusViewModel;
                // ReSharper disable once PossibleNullReferenceException
                if (content.Guid != guid)
                {
                    pane.IsActive = false;
                }
            }

            return Task.CompletedTask;
        }

        public Task HandleAsync(NewProjectPickerMessage message, CancellationToken cancellationToken)
        {
            
            //update properties here
            throw new NotImplementedException();
        }
    }

    public static class WorkspaceLayoutNames
    {
        public const string EnhancedCorpus = "ENHANCEDCORPUS";

        public const string BiblicalTerms = "BIBLICALTERMS";
        public const string Pins = "PINS";
        public const string TextCollection = "TEXTCOLLECTION";
        public const string WordMeanings = "WORDMEANINGS";
    }
}
