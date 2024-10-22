﻿using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Properties;
using ClearDashboard.Wpf.Application.ViewModels.Marble;
using ClearDashboard.Wpf.Application.Views;
using ClearDashboard.Wpf.Application.Views.ParatextViews;
using Serilog.Core;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ClearDashboard.Wpf.Application.Helpers
{
    public static class LaunchMirrorView<TView> where TView : UserControl, new()
    {
        public static void Show(object datacontext, double actualWidth, double actualHeight, string title = "")
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

            mirror.WindowStartupLocation = WindowStartupLocation.Manual;

            // load in the monitor settings
            var differentMonitor = Settings.Default.DifferentMonitor;
            var thirdMonitor = Settings.Default.ThirdMonitor;
            // get the number and sizes of the monitors on the system
            var monitors = Monitor.AllMonitors.ToList();


            //var logger = IoC.Get<Logger>();
            //logger.Information($"differentMonitor: {differentMonitor}");
            //logger.Information($"thirdMonitor: {thirdMonitor}");
            //logger.Information($"monitors.Count: {monitors.Count}");
            //int i = 0;
            //foreach (var monitor in monitors)
            //{
            //    logger.Information($"Monitor {i}: monitor.Bounds.Left: {monitor.Bounds.Left}");
            //    logger.Information($"monitor.Bounds.Top: {monitor.Bounds.Top}");
            //    logger.Information($"monitor.Bounds.Width: {monitor.Bounds.Width}");
            //    logger.Information($"monitor.Bounds.Height: {monitor.Bounds.Height}");
            //    i++;
            //}

            // figure out which monitor the app is on
            var thisApp = App.Current.MainWindow;

            // get the monitor that the app is on
            var thisMonitor = monitors.FirstOrDefault();  // x => x.Bounds.Left >= thisApp.Left && x.Bounds.Left <= thisApp.Left + thisApp.Width

            foreach (var monitor in monitors)
            {
                if (Math.Abs(monitor.Bounds.Left - thisApp.Left) < Math.Abs(thisMonitor.Bounds.Left - thisApp.Left))
                {
                    thisMonitor = monitor;
                    //logger.Information($"thisMonitor.Bounds.Left: {thisMonitor.Bounds.Left}");
                }
            }

            // put on primary monitor
            if ((differentMonitor == false && thirdMonitor == false) || monitors.Count == 0)
            {
                mirror.Left = thisMonitor.Bounds.Left;
                mirror.Top = thisMonitor.Bounds.Top;

                //logger.Information($"PRIMARY MONITOR mirror.Left: {mirror.Left}");
            }
            else if (monitors.Count > 2 && thirdMonitor && differentMonitor)
            {
                // throw on third monitor
                mirror.Left = monitors[2].Bounds.Left;
                mirror.Top = monitors[2].Bounds.Top;

                //logger.Information($"THIRD MONITOR mirror.Left: {mirror.Left}");
            }
            else
            {
                if (differentMonitor)
                {
                    // remove the monitor that the app is on
                    monitors.Remove(thisMonitor);

                    var sortedMonitors = monitors.OrderBy(x => x.Bounds.Left).ToList();

                    mirror.Left = monitors[0].Bounds.Left;
                    mirror.Top = monitors[0].Bounds.Top;

                    //logger.Information($"FIRST MONITOR mirror.Left: {mirror.Left}");
                }
                else
                {
                    mirror.Left = thisMonitor.Bounds.Left;
                    mirror.Top = thisMonitor.Bounds.Top;

                    //logger.Information($"SECOND MONITOR mirror.Left: {mirror.Left}");
                }
            }

            // turn off the mirror's close button
            mirror.WindowStyle = WindowStyle.None;

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

            mirror.Title = $"{title} Expanded View";

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
