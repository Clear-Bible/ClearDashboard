using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using ClearDashboard.Common.Models;
using ClearDashboard.Wpf.Helpers;
using ClearDashboard.Wpf.ViewModels;
using ClearDashboard.Wpf.ViewModels.Panes;

namespace ClearDashboard.Wpf.Views
{
    /// <summary>
    /// Interaction logic for WorkSpaceView.xaml
    /// </summary>
    public partial class WorkSpaceView : Page
    {
        private WorkSpaceViewModel _vm;
        private DashboardProject dashboardProject;

        #region Startup

        public WorkSpaceView()
        {
            InitializeComponent();

            //---------------------------------------------------------------------------------------------------------
            // NB;  GERFEN - moved the follwoing code to Page_Loaded so that CM can complete the DataContext wire up.

            //_vm = this.DataContext as WorkSpaceViewModel;

            //INotifyPropertyChanged viewModel = (INotifyPropertyChanged)this.DataContext;
            //viewModel.PropertyChanged += (sender, args) =>
            //{
            //    // listen for the menu changes
            //    if (args.PropertyName.Equals("WindowIDToLoad"))
            //        UnHideWindow(_vm.WindowIDToLoad);
            //        return;
            //};

            //----------------------------------------------------------------------------------------------------------


            //// Add in a toolpane
            //var leftAnchorGroup = dockManager.Layout.LeftSide.Children.FirstOrDefault();
            //if (leftAnchorGroup == null)
            //{
            //    leftAnchorGroup = new LayoutAnchorGroup();
            //    dockManager.Layout.LeftSide.Children.Add(leftAnchorGroup);
            //}

            //leftAnchorGroup.Children.Add(new LayoutAnchorable()
            //{
            //    ContentId = "BIBLICALTERMS",
            //    Title = "BIBLICAL TERMS",
            //    Content = new Button{ Content = "BIBLICAL TERMS", Margin=new Thickness(10, 0, 10, 0)},
            //    AutoHideMinWidth = 400,
            //});

            //leftAnchorGroup.Children.Add(new LayoutAnchorable()
            //{
            //    ContentId = "WORDMEANINGS",
            //    Title = "WORD MEANINGS",
            //    Content = new Button { Content = "WORD MEANINGS", Margin = new Thickness(10, 0, 10, 0) },
            //    AutoHideMinWidth = 400,
            //});

            //leftAnchorGroup.Children.Add(new LayoutAnchorable()
            //{
            //    ContentId = "SOURCECONTEXT",
            //    Title = "SOURCE CONTEXT",
            //    Content = new Button { Content = "SOURCE CONTEXT", Margin = new Thickness(10, 0, 10, 0) },
            //    AutoHideMinWidth = 400,
            //});

            //leftAnchorGroup.Children.Add(new LayoutAnchorable()
            //{
            //    ContentId = "TARGETCONTEXT",
            //    Title = "TARGET CONTEXT",
            //    Content = new Button { Content = "TARGET CONTEXT", Margin = new Thickness(10, 0, 10, 0) },
            //    AutoHideMinWidth = 400,
            //});

            //leftAnchorGroup.Children.Add(new LayoutAnchorable()
            //{
            //    ContentId = "NOTES",
            //    Title = "NOTES",
            //    Content = new Button { Content = "NOTES", Margin = new Thickness(10, 0, 10, 0) },
            //    AutoHideMinWidth = 400,
            //});

            //leftAnchorGroup.Children.Add(new LayoutAnchorable()
            //{
            //    ContentId = "PINS",
            //    Title = "PINS",
            //    Content = new Button { Content = "PINS", Margin = new Thickness(10, 0, 10, 0) },
            //    AutoHideMinWidth = 400,
            //});

            //leftAnchorGroup.Children.Add(new LayoutAnchorable()
            //{
            //    ContentId = "TEXTCOLLECTIONS",
            //    Title = "TEXT COLLECTIONS",
            //    Content = new Button { Content = "TEXT COLLECTIONS", Margin = new Thickness(10, 0, 10, 0) },
            //    AutoHideMinWidth = 400,
            //});

            //var firstDocumentPane = dockManager.Layout.Descendents().OfType<LayoutDocumentPane>().FirstOrDefault();
            //if (firstDocumentPane != null)
            //{
            //    LayoutDocument doc = new LayoutDocument
            //    {
            //        ContentId = "TREEDOWN",
            //        Title = "TREEDOWN"
            //    };
            //    firstDocumentPane.Children.Add(doc);

            //    LayoutDocument doc2 = new LayoutDocument
            //    {
            //        ContentId = "ALIGNMENT",
            //        Title = "ALIGNMENT"
            //    };
            //    firstDocumentPane.Children.Add(doc2);
            //}

        }

     

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {


            _vm = this.DataContext as WorkSpaceViewModel;

            INotifyPropertyChanged viewModel = (INotifyPropertyChanged)this.DataContext;
            viewModel.PropertyChanged += (sender, args) =>
            {
                // listen for the menu changes
                if (args.PropertyName.Equals("WindowIDToLoad"))
                    UnHideWindow(_vm.WindowIDToLoad);
                return;
            };

            //_vm.Init();

        }

        #endregion


        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem)
            {
                var menuItem = sender as MenuItem;
                switch (menuItem.Tag.ToString().ToUpper())
                {
                    case "LOAD":
                        // remove all existing windows
                        dockManager.AnchorablesSource = null;
                        dockManager.DocumentsSource = null;
                        var layoutSerializer = new XmlLayoutSerializer(dockManager);

                        _vm.LoadLayout(layoutSerializer);

                        break;
                    case "SAVE":
                        //TODO
                        layoutSerializer = new XmlLayoutSerializer(dockManager);
                        layoutSerializer.Serialize(@".\AvalonDock.Layout.config");
                        break;
                    case "BIBLICALTERMS":
                        UnHideWindow(menuItem.Tag.ToString().ToUpper());
                        break;
                }
            }
        }


        /// <summary>
        /// Unhide window
        /// </summary>
        /// <param name="windowTag"></param>
        private void UnHideWindow(string windowTag)
        {
            // find the pane in the dockmanager with this contentID
            var windowPane = dockManager.Layout.Descendents()
                .OfType<LayoutAnchorable>()
                .SingleOrDefault(a => a.ContentId.ToUpper() == windowTag.ToUpper());

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
                windowPane = new LayoutAnchorable();
                windowPane.ContentId = windowTag;

                // setup the right ViewModel for the pane
                var obj = _vm.LoadWindow(windowTag);
                windowPane.Content = obj.vm;
                windowPane.Title = obj.title;
                windowPane.IsActive = true;
                // set where it will doc on layout
                if (obj.dockSide == PaneViewModel.EDockSide.Bottom)
                {
                    windowPane.AddToLayout(dockManager, AnchorableShowStrategy.Bottom);
                } 
                else if (obj.dockSide == PaneViewModel.EDockSide.Left)
                {
                    windowPane.AddToLayout(dockManager, AnchorableShowStrategy.Left);
                }
            }
        }
    }
}
