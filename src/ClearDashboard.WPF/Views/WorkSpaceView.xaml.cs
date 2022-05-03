using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using ClearDashboard.Common.Models;
using ClearDashboard.Wpf.ViewModels;
using ClearDashboard.Wpf.ViewModels.Panes;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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
        }

     

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _vm = this.DataContext as WorkSpaceViewModel;

            //INotifyPropertyChanged viewModel = (INotifyPropertyChanged)this.DataContext;
            //viewModel.PropertyChanged += (sender, args) =>
            //{
            //    // listen for the menu changes
            //    if (args.PropertyName.Equals("WindowIDToLoad"))
            //        //UnHideWindow(_vm.WindowIDToLoad);
            //    return;
            //};

        }

        #endregion


        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                switch (menuItem.Tag.ToString()?.ToUpper())
                {
                    case "LOAD":
                        // remove all existing windows
                        dockManager.AnchorablesSource = null;
                        dockManager.DocumentsSource = null;
                        var layoutSerializer = new XmlLayoutSerializer(dockManager);

                        //_vm.LoadLayout(layoutSerializer);

                        break;
                    case "SAVE":
                        //TODO
                        layoutSerializer = new XmlLayoutSerializer(dockManager);
                        layoutSerializer.Serialize(@".\AvalonDock.Layout.config");
                        break;
                    case "BIBLICALTERMS":
                        //UnHideWindow(menuItem.Tag.ToString().ToUpper());
                        break;
                }
            }
        }


        /// <summary>
        /// Unhide window
        /// </summary>
        /// <param name="windowTag"></param>
        //private void UnHideWindow(string windowTag)
        //{
        //    // find the pane in the dockmanager with this contentID
        //    var windowPane = dockManager.Layout.Descendents()
        //        .OfType<LayoutAnchorable>()
        //        .SingleOrDefault(a =>
        //        {
        //            Debug.WriteLine(a.Title);
        //            if (a.ContentId is not null)
        //            {
        //                return a.ContentId.ToUpper() == windowTag.ToUpper();
        //            }
        //            else
        //            {
        //                return false;
        //            }
                    
        //        });

        //    if (windowPane != null)
        //    {
        //        if (windowPane.IsAutoHidden)
        //        {
        //            windowPane.ToggleAutoHide();
        //        }
        //        else if (windowPane.IsHidden)
        //        {
        //            windowPane.Show();
        //        }
        //        else if (windowPane.IsVisible)
        //        {
        //            windowPane.IsActive = true;
        //        }
        //    }
        //    else
        //    {
        //        // window has been closed so reload it
        //        windowPane = new LayoutAnchorable
        //        {
        //            ContentId = windowTag
        //        };

        //        // setup the right ViewModel for the pane
        //        var obj = _vm.LoadWindow(windowTag);
        //        windowPane.Content = obj.vm;
        //        windowPane.Title = obj.title;
        //        windowPane.IsActive = true;
        //        // set where it will doc on layout
        //        if (obj.dockSide == PaneViewModel.EDockSide.Bottom)
        //        {
        //            windowPane.AddToLayout(dockManager, AnchorableShowStrategy.Bottom);
        //        } 
        //        else if (obj.dockSide == PaneViewModel.EDockSide.Left)
        //        {
        //            windowPane.AddToLayout(dockManager, AnchorableShowStrategy.Left);
        //        }
        //    }
        //}
    }
}
