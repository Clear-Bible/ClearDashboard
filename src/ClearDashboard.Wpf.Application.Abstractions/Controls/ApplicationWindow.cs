using MahApps.Metro.Controls;
using Microsoft.Win32;
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
            try
            {
                var window = GetWindow(this)!;
                var hWnd = new WindowInteropHelper(window).EnsureHandle();
                var preference = DwmWindowCornerPreference.DwmwcpRound;
                DwmSetWindowAttribute(hWnd, Dwmwindowattribute.DwmwaWindowCornerPreference, ref preference,
                    sizeof(uint));
            }
            catch (Exception ex)
            {
                // we're not running on Windows11 - swallow the exception
            }
               

        }


        public bool IsCurrentOSContains(string name)
        {
            var reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            string productName = (string)reg.GetValue("ProductName");

            return productName.Contains(name);
        }

        /// Check if it's Windows 8.1
        public bool IsWindows8Dot1()
        {
            return IsCurrentOSContains("Windows 8.1");
        }

        /// Check if it's Windows 10
        public bool IsWindows10()
        {
            return IsCurrentOSContains("Windows 10");
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
