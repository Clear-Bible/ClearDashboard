using System;
using System.Windows;
using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Lexicon;

namespace ClearDashboard.Wpf.Application.Events.Lexicon
{
    public class LexiconTranslationEventArgs : RoutedEventArgs
    {
        public LexiconTranslationViewModel? Translation { get; set; } = new();
        public MeaningViewModel Meaning { get; set; } = new();
        public LexemeViewModel Lexeme { get; set; } = new();
    }
}
