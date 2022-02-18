using System;
using System.Collections.Generic;
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

namespace ClearDashboard.Wpf.Views
{
    /// <summary>
    /// Interaction logic for WorkSpace.xaml
    /// </summary>
    public partial class WorkSpace : Page
    {
        private WorkSpaceViewModel _vm;
        private DashboardProject dashboardProject;

        #region Startup

        public WorkSpace()
        {
            InitializeComponent();

            _vm = this.DataContext as WorkSpaceViewModel;

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
            _vm.Init();
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
            var windowPane = dockManager.Layout.Descendents()
                .OfType<LayoutAnchorable>()
                .SingleOrDefault(a => a.ContentId == windowTag);

            if (windowPane != null)
            {
                            if (windowPane.IsAutoHidden)
            {
                windowPane.Hide(false);
            }

            if (windowPane.IsHidden)
                windowPane.Show();
            else if (windowPane.IsVisible)
                windowPane.IsActive = true;
            else
                windowPane.AddToLayout(dockManager, AnchorableShowStrategy.Bottom | AnchorableShowStrategy.Most);
            }

        }
    }
}
