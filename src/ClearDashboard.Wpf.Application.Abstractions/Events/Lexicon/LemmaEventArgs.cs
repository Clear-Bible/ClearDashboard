using System.Windows;
using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Lexicon;

namespace ClearDashboard.Wpf.Application.Events.Lexicon
{
    public class LemmaEventArgs : RoutedEventArgs
    {
        public LexemeViewModel Lexeme { get; set; } = new();
    }
}
