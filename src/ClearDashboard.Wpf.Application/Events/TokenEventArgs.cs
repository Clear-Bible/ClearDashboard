using System.Windows;
using ClearDashboard.Wpf.Application.ViewModels.Display;

namespace ClearDashboard.Wpf.Application.Events
{
    public class TokenEventArgs : RoutedEventArgs
    {
        public TokenDisplay TokenDisplay { get; set; }
    }
}
