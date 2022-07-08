using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Caliburn.Micro;
using ClearDashboard.Wpf.Views;

namespace Helpers
{
    public static class LaunchMirrorView<TView> where TView : UserControl, new()
    {
        public static void Show(object datacontext, double actualWidth, double actualHeight)
        {
            // create instance of MirrorView
            var mirror = new MirrorView();
            mirror.WindowState = WindowState.Maximized;

            var mirroredView = new TView();

            // get the instance of the MirrorView's grid
            var mainGrid = mirroredView.Content as Grid;
            // add this usercontrol to the root element
            mirror.MirrorViewRoot.Children.Add(mirroredView);
            // set the view's datacontext to whatever we are passing in to mirror
            mirroredView.DataContext = datacontext;
            // force the mirrorview to show
            mirror.Show();
            // now that it is shown, we can get it's actual size
            var newWidth = mainGrid.ActualWidth;
            var newHeight = mainGrid.ActualHeight;

            // calculate new zoom ratios
            double widthZoom = (actualWidth + newWidth) / newWidth;
            double heightZoom = (actualHeight + newHeight) / newHeight;

            if (heightZoom < widthZoom)
            {
                heightZoom = widthZoom;
            }
            else
            {
                widthZoom = heightZoom;
            }

            // scale the view accordingly
            mirror.MirrorViewRoot.LayoutTransform = new ScaleTransform(widthZoom, heightZoom);
        }
    }
}
