using MahApps.Metro.Controls;
using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace ClearDashboard.Wpf.Application.Controls
{
    public class ApplicationWindow : MetroWindow
    {
        // The enum flag for DwmSetWindowAttribute's second parameter, which tells the function what attribute to set.
        // Copied from dwmapi.h
        public enum Dwmwindowattribute
        {
            DwmwaWindowCornerPreference = 33
        }

        // The DWM_WINDOW_CORNER_PREFERENCE enum for DwmSetWindowAttribute's third parameter, which tells the function
        // what value of the enum to set.
        // Copied from dwmapi.h
        public enum DwmWindowCornerPreference
        {
            DwmwcpDefault = 0,
            DwmwcpDoNotRound = 1,
            DwmwcpRound = 2,
            DwmwcpRoundSmall = 3
        }

        // Import dwmapi.dll and define DwmSetWindowAttribute in C# corresponding to the native function.
        [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        internal static extern void DwmSetWindowAttribute(IntPtr hwnd,
            Dwmwindowattribute attribute,
            ref DwmWindowCornerPreference pvAttribute,
            uint cbAttribute);

        protected void RoundCorners()
        {
            var window = GetWindow(this)!;
            var hWnd = new WindowInteropHelper(window).EnsureHandle();
            var preference = DwmWindowCornerPreference.DwmwcpRound;
            DwmSetWindowAttribute(hWnd, Dwmwindowattribute.DwmwaWindowCornerPreference, ref preference, sizeof(uint));
        }
    }


    public class DialogWindow : ApplicationWindow
    {
        protected void Prepare()
        {
            RoundCorners();
            ShowCloseButton = true;
            ShowInTaskbar = false;
            ShowMinButton = false;
            ShowMaxRestoreButton = false;
        }
    }
}
