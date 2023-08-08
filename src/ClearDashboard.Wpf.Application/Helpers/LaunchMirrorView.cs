using System;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CefSharp.DevTools.Database;
using ClearDashboard.Wpf.Application.Properties;
using ClearDashboard.Wpf.Application.ViewModels.Marble;
using ClearDashboard.Wpf.Application.Views;
using ClearDashboard.Wpf.Application.Views.ParatextViews;
using MahApps.Metro.IconPacks.Converter;
using static System.Net.Mime.MediaTypeNames;

namespace ClearDashboard.Wpf.Application.Helpers
{
    public static class LaunchMirrorView<TView> where TView : UserControl, new()
    {
        public static void Show(object datacontext, double actualWidth, double actualHeight)
        {
            bool found = false;
            foreach (var window in App.Current.Windows)
            {
                if (window is MirrorView)
                {
                    (window as MirrorView).Close();
                    found = true;
                }
            }

            if (found)
            {
                return;
            }

            // create instance of MirrorView
            var mirror = new MirrorView
            {
                //WindowState = WindowState.Maximized
            };


            // load in the monitor settings
            var differentMonitor = Settings.Default.DifferentMonitor;
            var thirdMonitor = Settings.Default.ThirdMonitor;
            // get the number and sizes of the monitors on the system
            var monitors = Monitor.AllMonitors.ToList();

            if (monitors.Count > 2 && thirdMonitor)
            {
                // throw on third monitor
                mirror.WindowStartupLocation = WindowStartupLocation.Manual;
                mirror.Left = monitors[2].Bounds.Left;
                mirror.Top = monitors[2].Bounds.Top;
            }
            else
            {
                mirror.WindowStartupLocation = WindowStartupLocation.Manual;

                // get this applications position on the screen
                var thisApp = App.Current.MainWindow;

                if (differentMonitor)
                {
                    if (thisApp.Left < monitors[0].Bounds.Right)
                    {
                        // throw on second monitor
                        mirror.Left = monitors[1].Bounds.Left;
                        mirror.Top = monitors[1].Bounds.Top;
                    }
                    else
                    {
                        // throw on first monitor
                        mirror.Left = monitors[0].Bounds.Left;
                        mirror.Top = monitors[0].Bounds.Top;
                    }
                }
                else
                {
                    mirror.Left = thisApp.Left;
                    mirror.Top = thisApp.Top;
                }
            }

            var mirroredView = new TView();

            // get the instance of the MirrorView's grid
            if (mirroredView.Content is not Grid mainGrid)
            {
                throw new NullReferenceException($"mirroredView.Content' is not a Grid - cannot display the MirrorView of {typeof(TView)}.");
            }

            // add this UserControl to the root element
            Grid.SetColumn(mirroredView, 1);
            mirror.MirrorViewRoot.Children.Add(mirroredView);

            // set the view's datacontext to whatever we are passing in to mirror
            mirroredView.DataContext = datacontext;
            if (mirroredView is BiblicalTermsView biblicalTermsView)
            {
                biblicalTermsView.MainGrid.Tag = "True";
            }
            if (mirroredView is PinsView pinsViewModel)
            {
                pinsViewModel.MainGrid.Tag = "True";
            }
            // force the MirrorView to show
            mirror.Show();
            mirror.WindowState = WindowState.Maximized;


            // now that it is shown, we can get it's actual size
            var newWidth = mainGrid.ActualWidth;
            var newHeight = mainGrid.ActualHeight;

            // calculate new zoom ratios
            var widthZoom = (actualWidth + newWidth) / newWidth;
            var heightZoom = (actualHeight + newHeight) / newHeight;

            if (heightZoom < widthZoom)
            {
                heightZoom = widthZoom;
            }
            else
            {
                widthZoom = heightZoom;
            }

            // TODO
            switch (datacontext)
            {
                case MarbleViewModel:
                    Uri iconUri = new Uri("pack://application:,,,/Resources/donut_icon.ico", UriKind.RelativeOrAbsolute); 
                    mirror.Icon = BitmapFrame.Create(iconUri);
                    break;
            }
        }

        public static void Scale(MirrorView mirror, double widthZoom, double heightZoom, bool initialization=false)
        {
            var transform = new ScaleTransform(widthZoom, heightZoom);
            mirror.MirrorViewRoot.LayoutTransform = transform;

            Settings.Default.MirrorViewScaleValue = widthZoom;
            Settings.Default.Save();
        }
    }
}
