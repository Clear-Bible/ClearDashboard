using System;
using System.Windows;
using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Lexicon;

namespace ClearDashboard.Wpf.Application.Events.Lexicon
{
    public class TranslationEntryEventArgs : RoutedEventArgs
    {
        public TranslationViewModel? Translation { get; set; } = new();
        //public TranslationId TranslationId { get; set; } = new();
        //public string TranslationText { get; set; } = String.Empty;
        public MeaningViewModel Meaning { get; set; } = new();
        public LexemeViewModel Lexeme { get; set; } = new();
    }
}
