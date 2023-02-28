using System.Windows;
using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Lexicon;

namespace ClearDashboard.Wpf.Application.Events.Lexicon
{
    public class MeaningEventArgs : RoutedEventArgs
    {
        public MeaningViewModel Meaning { get; set; } = new();
        public LexemeViewModel Lexeme { get; set; } = new();
    }
}
