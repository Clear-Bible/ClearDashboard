using System.Windows;
using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Lexicon;

namespace ClearDashboard.Wpf.Application.Events.Lexicon
{
    public class SemanticDomainEventArgs : RoutedEventArgs
    {
        public SemanticDomain SemanticDomain { get; set; } = new();
        public MeaningViewModel Meaning { get; set; } = new();
    }
}
