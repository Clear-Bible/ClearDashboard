using System.Windows;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.Wpf.Application.ViewModels.Display;

namespace ClearDashboard.Wpf.Application.UserControls
{
    public class NoteEventArgs : RoutedEventArgs
    {
        public TokenDisplay TokenDisplay { get; set; }
    }
}
