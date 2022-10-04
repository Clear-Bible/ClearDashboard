using System.Windows;
using System.Windows.Input;
using ClearDashboard.Wpf.Application.ViewModels.Display;

namespace ClearDashboard.Wpf.Application.Events
{
    public class TokenEventArgs : RoutedEventArgs
    {
        public TokenDisplayViewModel TokenDisplayViewModel { get; set; }
        public ModifierKeys ModifierKeys { get; set; }
    }
}
