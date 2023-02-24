using System.Windows;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Lexicon;

namespace ClearDashboard.Wpf.Application.Events.Lexicon
{
    public class LexemeEventArgs : RoutedEventArgs
    {
        public LexemeViewModel Lexeme { get; set; } = new();
    }
}
