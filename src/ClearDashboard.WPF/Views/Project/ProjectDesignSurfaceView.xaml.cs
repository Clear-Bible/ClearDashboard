using System.Windows;
using System.Windows.Controls;

namespace ClearDashboard.Wpf.Views.Project
{
    /// <summary>
    /// Interaction logic for ProjectDesignSurfaceView.xaml
    /// </summary>
    public partial class ProjectDesignSurfaceView : UserControl
    {
        public ProjectDesignSurfaceView()
        {
            InitializeComponent();
        }

        public void AddControl(FrameworkElement control)
        {
            this.CanvasDesignSurface.Children.Add(control);
        }
    }
}
