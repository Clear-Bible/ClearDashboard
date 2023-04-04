using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Lexicon;

namespace ClearDashboard.Wpf.Application.Messages.Lexicon
{
    public record LexiconTranslationAddedMessage(
        LexiconTranslationViewModel Translation,
        MeaningViewModel Meaning);
}
