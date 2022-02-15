using ClearDashboard.Common;
using ClearDashboard.Common.Models;
using ClearDashboard.DAL;
using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AvalonDock.Layout.Serialization;
using AvalonDock.Themes;
using ClearDashboard.Wpf.Helpers;
using Serilog;


namespace ClearDashboard.Wpf.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class WorkSpaceViewModel : ObservableObject
    {
        #region Member Variables

        private readonly ILogger _logger;

        #endregion //Member Variables

        #region Public Properties

        private BiblicalTermsViewModel _biblicalTerms = null;
        public BiblicalTermsViewModel BiblicalTerms
        {
            get
            {
                if (_biblicalTerms == null)
                {
                    _biblicalTerms = new BiblicalTermsViewModel("BIBILICAL TERMS NEW");
                }

                return _biblicalTerms;
            }
        }


        //private ToolViewModel[] _tools = null;
        //public IEnumerable<ToolViewModel> Tools
        //{
        //    get
        //    {
        //        return _tools;
        //    }
        //}

        #endregion //Public Properties

        #region Observable Properties

        ObservableCollection<ToolViewModel> _tools = null;
        public ObservableCollection<ToolViewModel> Tools
        {
            get
            {
                return _tools;
            }
        }


        ObservableCollection<PaneViewModel> _files = null;
        public ObservableCollection<PaneViewModel> Files
        {
            get
            {
                return _files;
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
            // grab a copy of the current logger from the App.xaml.cs
            _logger = (Application.Current as ClearDashboard.Wpf.App)?._logger;

            this.Themes = new List<Tuple<string, Theme>>
            {
                new Tuple<string, Theme>(nameof(Vs2013DarkTheme),new Vs2013DarkTheme()),
                new Tuple<string, Theme>(nameof(AeroTheme),new AeroTheme()),
                //new Tuple<string, Theme>(nameof(Vs2013LightTheme),new Vs2013LightTheme()),
                //new Tuple<string, Theme>(nameof(Vs2013BlueTheme),new Vs2013BlueTheme()),
                //new Tuple<string, Theme>(nameof(GenericTheme), new GenericTheme()),
                //new Tuple<string, Theme>(nameof(ExpressionDarkTheme),new ExpressionDarkTheme()),
                //new Tuple<string, Theme>(nameof(ExpressionLightTheme),new ExpressionLightTheme()),
                //new Tuple<string, Theme>(nameof(MetroTheme),new MetroTheme()),
                //new Tuple<string, Theme>(nameof(VS2010Theme),new VS2010Theme()),
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


            // subscribe to change events in the parent's theme
            (Application.Current as ClearDashboard.Wpf.App).ThemeChanged += WorkSpaceViewModel_ThemeChanged;


            //if (_tools == null)
            //    _tools = new ToolViewModel[] { BiblicalTerms };

            _tools = new ObservableCollection<ToolViewModel>();
            _tools.Add(new BiblicalTermsViewModel("Biblical Terms"));

            _files = new ObservableCollection<PaneViewModel>();
            _files.Add(new StartPageViewModel());
            _files.Add(new AlignmentToolViewModel());
            _files.Add(new TreeDownViewModel());
        }

        private void WorkSpaceViewModel_ThemeChanged()
        {
            var newTheme = (Application.Current as ClearDashboard.Wpf.App).Theme;
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

        public void Init()
        {
            
        }

        #endregion //Constructor

        #region Methods




        #endregion // Methods
    }
}
