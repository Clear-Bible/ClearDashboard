using AvalonDock.Layout.Serialization;
using AvalonDock.Themes;
using ClearDashboard.Common.Models;
using MvvmHelpers;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using ClearDashboard.Wpf.Helpers;
using ClearDashboard.Wpf.Models.Menus;


namespace ClearDashboard.Wpf.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class WorkSpaceViewModel : ObservableObject
    {
        #region Member Variables

        private readonly ILogger _logger;
        private static WorkSpaceViewModel _this;
        public static WorkSpaceViewModel This => _this;



        public DashboardProject DashboardProject { get; set; }

        private DashboardViewModel _dashboardViewModel;

        #endregion //Member Variables

        #region Public Properties

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


        private string _WindowIDToLoad;
        public string WindowIDToLoad
        {
            get => _WindowIDToLoad;
            set
            {
                SetProperty(ref _WindowIDToLoad, value, nameof(WindowIDToLoad)); 
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
            set { SetProperty(ref _menuItems, value, nameof(MenuItems)); }
        }


        ObservableCollection<ToolViewModel> _tools = new ObservableCollection<ToolViewModel>();
        public ObservableCollection<ToolViewModel> Tools
        {
            get=> _tools;
            set
            {
                SetProperty(ref _tools, value, nameof(Tools));
            }
        }


        ObservableCollection<PaneViewModel> _files = new ObservableCollection<PaneViewModel>();
        public ObservableCollection<PaneViewModel> Files
        {
            get => _files;
            set
            {
                SetProperty(ref _files, value, nameof(Files));
            }
        }
        public List<Tuple<string, Theme>> Themes { get; set; }

        private Tuple<string, Theme> selectedTheme;
        public Tuple<string, Theme> SelectedTheme
        {
            get { return selectedTheme; }
            set
            {
                SetProperty(ref selectedTheme, value, nameof(selectedTheme));
            }
        }


        #endregion //Observable Properties

        #region Constructor

        public WorkSpaceViewModel()
        {
            _this = this;

            // grab a copy of the current logger from the App.xaml.cs
           // _logger = (Application.Current as ClearDashboard.Wpf.App)?._logger;

            this.Themes = new List<Tuple<string, Theme>>
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

                //// subscribe to change events in the parent's theme
                //(Application.Current as ClearDashboard.Wpf.App).ThemeChanged += WorkSpaceViewModel_ThemeChanged;

                //if (Application.Current is ClearDashboard.Wpf.App)
                //{
                //    _logger = (Application.Current as ClearDashboard.Wpf.App)._logger;
                //    DashboardProject = (Application.Current as ClearDashboard.Wpf.App).SelectedDashboardProject;
                //}
            }
        }

        private void WorkSpaceViewModel_ThemeChanged()
        {
            // TODO

            //var newTheme = (Application.Current as ClearDashboard.Wpf.App).Theme;
            //if (newTheme == MaterialDesignThemes.Wpf.BaseTheme.Dark)
            //{
            //    // toggle the Dark theme for AvalonDock
            //    this.SelectedTheme = Themes[0];
            //}
            //else
            //{
            //    // toggle the light theme for AvalonDock
            //    this.SelectedTheme = Themes[1];
            //}
        }

        public void Init()
        {
            // initiate the menu system
            MenuItems.Clear();
            MenuItems = new ObservableCollection<MenuItemViewModel>
            {
                new MenuItemViewModel { Header = "Layouts", Id="LayoutID", ViewModel=this, },
                new MenuItemViewModel { Header = "Windows", Id="WindowID", ViewModel=this,
                    MenuItems = new ObservableCollection<MenuItemViewModel>
                    {
                        new MenuItemViewModel { Header = "Alignment Tool",  Id="AlignmentToolID", ViewModel=this,},
                        new MenuItemViewModel { Header = "Biblical Terms",  Id="BiblicalTermsID", ViewModel=this,},
                        new MenuItemViewModel { Header = "Concordance Tool",  Id="ConcordanceToolID", ViewModel=this,},
                        new MenuItemViewModel { Header = "Dashboard",  Id="DashboardID", ViewModel=this,},
                        new MenuItemViewModel { Header = "Notes",  Id="NotesID", ViewModel=this,},
                        new MenuItemViewModel { Header = "PINS",  Id="PINSID", ViewModel=this,},
                        new MenuItemViewModel { Header = "Word Meanings",  Id="WordMeaningsID", ViewModel=this,},
                        new MenuItemViewModel { Header = "Source Context",  Id="SourceContextID", ViewModel=this,},
                        new MenuItemViewModel { Header = "Start Page",  Id="StartPageID", ViewModel=this,},
                        new MenuItemViewModel { Header = "Target Context",  Id="TargetContextID", ViewModel=this,},
                        new MenuItemViewModel { Header = "Text Collection",  Id="TextCollectionID", ViewModel=this,},
                    }
                },
                new MenuItemViewModel { Header = "Help",  Id="HelpID", ViewModel=this,}
            };


            // add in the document panes
            _files.Clear();

            Debug.WriteLine(DashboardProject.Name);
            _dashboardViewModel = new DashboardViewModel();
            _files.Add(_dashboardViewModel);

            _files.Add(new ConcordanceViewModel());
            _files.Add(new StartPageViewModel());
            _files.Add(new AlignmentToolViewModel());
            // trigger property changed event
            Files.Add(new TreeDownViewModel());


            // add in the tool panes
            _tools.Clear();
            _tools.Add(new BiblicalTermsViewModel());
            _tools.Add(new WordMeaningsViewModel());
            _tools.Add(new SourceContextViewModel());
            _tools.Add(new TargetContextViewModel());
            _tools.Add(new NotesViewModel());
            _tools.Add(new PinsViewModel());
            // trigger property changed event
            Tools.Add(new TextCollectionViewModel());




        }

        #endregion //Constructor

        #region Methods
        public void LoadLayout(XmlLayoutSerializer layoutSerializer)
        {
            // Here I've implemented the LayoutSerializationCallback just to show
            //  a way to feed layout desarialization with content loaded at runtime
            // Actually I could in this case let AvalonDock to attach the contents
            // from current layout using the content ids
            // LayoutSerializationCallback should anyway be handled to attach contents
            // not currently loaded
            layoutSerializer.LayoutSerializationCallback += (s, e) =>
            {
                // Debug.WriteLine(e.Model?.ContentId?.ToString());
                switch (e.Model.ContentId.ToUpper())
                {
                    case "DASHBOARD":
                        if (_dashboardViewModel is null)
                        {
                            e.Content = new DashboardViewModel();
                        }
                        else
                        {
                            e.Content = _dashboardViewModel;
                        }
                        break;
                    case "CONCORDANCETOOL":
                        e.Content = new ConcordanceViewModel();
                        break;
                    case "BIBLICALTERMS":
                        e.Content = new BiblicalTermsViewModel();
                        break;
                    case "WORDMEANINGS":
                        e.Content = new WordMeaningsViewModel();
                        break;
                    case "SOURCECONTEXT":
                        e.Content = new SourceContextViewModel();
                        break;
                    case "TARGETCONTEXT":
                        e.Content = new TargetContextViewModel();
                        break;
                    case "NOTES":
                        e.Content = new NotesViewModel();
                        break;
                    case "PINS":
                        e.Content = new PinsViewModel();
                        break;
                    case "TEXTCOLLECTION":
                        e.Content = new TextCollectionViewModel();
                        break;
                    case "STARTPAGE":
                        e.Content = new StartPageViewModel();
                        break;
                    case "ALIGNMENTTOOL":
                        e.Content = new AlignmentToolViewModel();
                        break;
                    case "TREEDOWN":
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
                case "BIBLICALTERMS":
                    var vm = new BiblicalTermsViewModel();
                    return (vm, vm.Title, vm.DockSide);
                case "DASHBOARD":
                    var vm1 = new DashboardViewModel();
                    return (vm1, vm1.Title, vm1.DockSide);
                case "CONCORDANCETOOL":
                    var vm2 = new ConcordanceViewModel();
                    return (vm2, vm2.Title, vm2.DockSide);
                case "WORDMEANINGS":
                    var vm3 = new WordMeaningsViewModel();
                    return (vm3, vm3.Title, vm3.DockSide);
                case "SOURCECONTEXT":
                    var vm4 = new SourceContextViewModel();
                    return (vm4, vm4.Title, vm4.DockSide);
                case "TARGETCONTEXT":
                    var vm5 = new TargetContextViewModel();
                    return (vm5, vm5.Title, vm5.DockSide);
                case "NOTES":
                    var vm6 = new NotesViewModel();
                    return (vm6, vm6.Title, vm6.DockSide);
                case "PINS":
                    var vm7 = new PinsViewModel();
                    return (vm7, vm7.Title, vm7.DockSide);
                case "TEXTCOLLECTION":
                    var vm8 = new TextCollectionViewModel();
                    return (vm8, vm8.Title, vm8.DockSide);
                case "STARTPAGE":
                    var vm9 = new StartPageViewModel();
                    return (vm9, vm9.Title, vm9.DockSide);
                case "ALIGNMENTTOOL":
                    var vm10 = new AlignmentToolViewModel();
                    return (vm10, vm10.Title, vm10.DockSide);
                case "TREEDOWN":
                    var vm11 = new TreeDownViewModel();
                    return (vm11, vm11.Title, vm11.DockSide);
            }
            return (null, null, PaneViewModel.EDockSide.Bottom);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion // Methods
    }
}
