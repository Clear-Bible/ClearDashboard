using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Forms; // Make sure to reference System.Windows.Forms
using System.Windows.Interop;

namespace ClearDashboard.Wpf.Application.Helpers
{
    
    public class WpfScreen
    {
        private readonly Screen _screen;

        internal WpfScreen(Screen screen)
        {
            _screen = screen;
        }

        public Rect DeviceBounds => GetRect(_screen.Bounds);
        public Rect WorkingArea => GetRect(_screen.WorkingArea);
        public bool IsPrimary => _screen.Primary;
        public string DeviceName => _screen.DeviceName;

        private Rect GetRect(Rectangle value)
        {
            return new Rect
            {
                X = value.X,
                Y = value.Y,
                Width = value.Width,
                Height = value.Height
            };
        }

        // Additional properties or methods can be added as needed

        // Example method to get all screens
        public static IEnumerable<WpfScreen> AllScreens()
        {
            foreach (var screen in Screen.AllScreens)
            {
                yield return new WpfScreen(screen);
            }
        }

        // Example method to get a screen from a Window
        public static WpfScreen GetScreenFrom(Window window)
        {
            var windowInteropHelper = new WindowInteropHelper(window);
            var screen = Screen.FromHandle(windowInteropHelper.Handle);
            return new WpfScreen(screen);
        }
    }
}
