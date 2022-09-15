using System.Windows;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.Wpf.Application.ViewModels.Display;

namespace ClearDashboard.Wpf.Application.Events
{
    public class TranslationEventArgs : RoutedEventArgs
    {
        public TokenDisplay TokenDisplay { get; set; }

        public Translation Translation { get; set; }
    }
}
