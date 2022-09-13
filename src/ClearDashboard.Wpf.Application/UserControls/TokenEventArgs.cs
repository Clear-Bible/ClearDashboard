using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using ClearDashboard.Wpf.Application.ViewModels.Display;

namespace ClearDashboard.Wpf.Application.UserControls
{
    public class TokenEventArgs : RoutedEventArgs
    {
        public TokenDisplay TokenDisplay { get; set; }
    }
}
