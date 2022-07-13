using System;
using System.Windows;
using System.Windows.Interop;

namespace ViewModels.Popups
{
    public partial class CustomWindow : Window
    {
        // Prep stuff needed to remove close button on window
        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public CustomWindow()
        {
            Loaded += CustomWindow_Loaded;
        }

        void CustomWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Code to remove close box from window
            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
        }

        protected override void OnActivated(EventArgs e)
        {
            Loaded += CustomWindow_Loaded;
        }

        protected override void OnDeactivated(EventArgs e)
        {
            Loaded -= CustomWindow_Loaded;
        }
    }
}
