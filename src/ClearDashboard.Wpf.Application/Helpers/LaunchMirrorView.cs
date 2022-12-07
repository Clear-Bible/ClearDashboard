using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ClearDashboard.Wpf.Application.ViewModels.Marble;
using ClearDashboard.Wpf.Application.Views;

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
                WindowState = WindowState.Maximized
            };

            var mirroredView = new TView();

            // get the instance of the MirrorView's grid
            if (mirroredView.Content is not Grid mainGrid)
            {
                throw new NullReferenceException($"mirroredView.Content' is not a Grid - cannot display the MirrorView of {typeof(TView)}.");
            }


            // add this UserControl to the root element
            mirror.MirrorViewRoot.Children.Add(mirroredView);
            // set the view's datacontext to whatever we are passing in to mirror
            mirroredView.DataContext = datacontext;
            // force the MirrorView to show
            mirror.Show();
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
            

            // scale the view accordingly
            mirror.MirrorViewRoot.LayoutTransform = new ScaleTransform(widthZoom, heightZoom);

        }
    }
}
