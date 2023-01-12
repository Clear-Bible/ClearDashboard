using CefSharp.DevTools.Database;
using ClearDashboard.Wpf.Application.ViewModels.Marble;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Properties;
using SIL.Reporting;

namespace ClearDashboard.Wpf.Application.Views
{
    /// <summary>
    /// Interaction logic for MirrorView.xaml
    /// </summary>
    public partial class MirrorView : Window
    {
        public MirrorView()
        {
            var storedMirrorValue= Settings.Default.MirrorViewScaleValue;
            InitializeComponent();
            ZoomSlider.Value = storedMirrorValue;
        }

        private void ZoomSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            LaunchMirrorView<UserControl>.Scale(this, e.NewValue, e.NewValue);
            ZoomSlider.LayoutTransform = new ScaleTransform(1/e.NewValue, 1/e.NewValue);
        }
    }
}
