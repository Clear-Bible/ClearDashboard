using AvalonDock.Layout.Serialization;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Wpf.Application.ViewModels.Main;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace ClearDashboard.Wpf.Application.Views.Main
{

    /// <summary>
    /// Interaction logic for WorkSpaceView.xaml
    /// </summary>
    public partial class MainView : Page
    {
        private MainViewModel _mainViewModel;
        private DashboardProject dashboardProject;

        #region Startup

        public MainView()
        {
            InitializeComponent();
        }



        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _mainViewModel = this.DataContext as MainViewModel;

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
                        DockManager.AnchorablesSource = null;
                        DockManager.DocumentsSource = null;
                        var layoutSerializer = new XmlLayoutSerializer(DockManager);

                        //_vm.LoadLayout(layoutSerializer);

                        break;
                    case "SAVE":
                        //TODO
                        layoutSerializer = new XmlLayoutSerializer(DockManager);
                        layoutSerializer.Serialize(@".\AvalonDock.Layout.config");
                        break;
                    case "BIBLICALTERMS":
                        //UnHideWindow(menuItem.Tag.ToString().ToUpper());
                        break;
                }
            }
        }


        private void ProjectDesignSurfaceExpander_OnExpanded(object sender, RoutedEventArgs e)
        {
            if(ProjectDesignSurfaceExpander != null && ProjectDesignSurfaceControl != null)
            {
                ProjectDesignSurfaceExpander.Width = 500;
                ProjectDesignSurfaceControl.Width = 500;

                ProjectDesignSurfaceColumn.Width = new GridLength(1, GridUnitType.Auto);
                ProjectDesignSurfaceSplitter.Visibility = Visibility.Visible;
            }

            
        }

        private void ProjectDesignSurfaceExpander_OnCollapsed(object sender, RoutedEventArgs e)
        {
            ProjectDesignSurfaceExpander.Width = 24;
            ProjectDesignSurfaceControl.Width = 24;

            ProjectDesignSurfaceColumn.Width = new GridLength(1, GridUnitType.Auto);

            ProjectDesignSurfaceSplitter.Visibility = Visibility.Collapsed;
        }

        private void ProjectDesignSurfaceSplitter_OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            if (ProjectDesignSurfaceColumn.ActualWidth < 200)
            {
                ProjectDesignSurfaceControl.PdsLabelWide.Visibility = Visibility.Collapsed;
                //ProjectDesignSurfaceControl.ProjectNameWide.Visibility = Visibility.Collapsed;
                ProjectDesignSurfaceControl.LabelsNarrow.Visibility = Visibility.Visible;
            }
            else
            {
                ProjectDesignSurfaceControl.PdsLabelWide.Visibility = Visibility.Visible;
                //ProjectDesignSurfaceControl.ProjectNameWide.Visibility = Visibility.Visible;
                ProjectDesignSurfaceControl.LabelsNarrow.Visibility = Visibility.Collapsed;
            }
            
            ProjectDesignSurfaceExpander.Width = ProjectDesignSurfaceColumn.ActualWidth;
            ProjectDesignSurfaceControl.Width = ProjectDesignSurfaceColumn.ActualWidth;
        }
    }


}
