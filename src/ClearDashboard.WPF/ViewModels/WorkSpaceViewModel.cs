using ClearDashboard.Common;
using ClearDashboard.Common.Models;
using ClearDashboard.DAL;
using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AvalonDock.Layout.Serialization;
using AvalonDock.Themes;
using ClearDashboard.Wpf.Helpers;
using ClearDashboard.Wpf.Views;
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
        private static WorkSpaceViewModel _this;
        public static WorkSpaceViewModel This => _this;

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
            _this = this;

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

            // add in the document panes
            _files = new ObservableCollection<PaneViewModel>();
            _files.Add(new StartPageViewModel());
            _files.Add(new AlignmentToolViewModel());
            _files.Add(new TreeDownViewModel());

            // add in the tool panes
            _tools = new ObservableCollection<ToolViewModel>();
            _tools.Add(new BiblicalTermsViewModel("BIBLICAL TERMS"));
            _tools.Add(new WordMeaningsViewModel("WORD MEANINGS"));
            _tools.Add(new SourceContextViewModel("SOURCE CONTEXT"));
            _tools.Add(new TargetContextViewModel("TARGET CONTEXT"));
            _tools.Add(new NotesViewModel("NOTES"));
            _tools.Add(new PinsViewModel("PINS"));
            _tools.Add(new TextCollectionViewModel("TEXT COLLECTIONS"));
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
                Debug.WriteLine(e.Model?.ContentId?.ToString());


                switch (e.Model.ContentId)
                {
                    case "{BiblicalTerms_ContentId}":
                        e.Content = new BiblicalTermsViewModel("BIBLICAL TERMS");
                        break;
                    case "{WordMeanings_ContentId}":
                        e.Content = new WordMeaningsViewModel("WORD MEANINGS");
                        break;
                    case "{SourceContext_ContentId}":
                        e.Content = new SourceContextViewModel("SOURCE CONTEXT");
                        break;
                    case "{TargetContext_ContentId}":
                        e.Content = new TargetContextViewModel("TARGET CONTEXT");
                        break;
                    case "{Notes_ContentId}":
                        e.Content = new NotesViewModel("NOTES");
                        break;
                    case "{Pins_ContentId}":
                        e.Content = new PinsViewModel("PINS");
                        break;
                    case "{TextCollection_ContentId}":
                        e.Content = new TextCollectionViewModel("TEXT COLLECTION");
                        break;
                    case "{StartPage_ContentId}":
                        e.Content = new StartPageViewModel();
                        break;
                    case "{AlignmentTool_ContentId}":
                        e.Content = new AlignmentToolViewModel();
                        break;
                    case "{TreeDown_ContentId}":
                        e.Content = new TreeDownViewModel();
                        break;

                }
            };
            layoutSerializer.Deserialize(@".\AvalonDock.Layout.config");
        }



        #endregion // Methods
    }
}
