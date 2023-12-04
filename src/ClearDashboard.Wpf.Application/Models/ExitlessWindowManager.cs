using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;

namespace ClearDashboard.Wpf.Application.Models
{
    public class NoExitWindowManager:WindowManager
    {
        protected override Window EnsureWindow(object model, object view, bool isDialog)
        {
            Window window = base.EnsureWindow(model, view, isDialog);
            window.WindowStyle = WindowStyle.None; // Hides the close button
            return window;
        }
    }
}
