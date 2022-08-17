using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClearDashboard.Wpf.Views.Project
{
    /// <summary>
    /// Interaction logic for ProjectWorkspaceWithGridSplitter.xaml
    /// </summary>
    public partial class ProjectWorkspaceWithGridSplitterView : Page
    {
        public ProjectWorkspaceWithGridSplitterView()
        {
            InitializeComponent();


            Splitter.DragDelta += SplitterNameDragDelta;
        }

        private void SplitterNameDragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            var newWidth = new GridLength(ProjectGrid.ColumnDefinitions[0].ActualWidth + e.HorizontalChange);
            if (newWidth.Value < 200)
            {
                newWidth = new GridLength(200);
            }
            ProjectGrid.ColumnDefinitions[0].Width = newWidth;
        }
    }
}
