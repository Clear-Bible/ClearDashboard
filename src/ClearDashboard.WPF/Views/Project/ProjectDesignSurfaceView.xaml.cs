using System;
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

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
           
        }

        public void AddControl(FrameworkElement control)
        {
            DesignSurfaceCanvas.Dispatcher.Invoke(() => { DesignSurfaceCanvas.Children.Add(control); });

        }
    }
}
