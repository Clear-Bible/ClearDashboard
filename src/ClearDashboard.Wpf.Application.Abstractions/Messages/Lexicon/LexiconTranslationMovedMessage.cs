using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Lexicon;

namespace ClearDashboard.Wpf.Application.Messages.Lexicon
{
    public record LexiconTranslationMovedMessage(LexiconTranslationViewModel Translation, MeaningViewModel NewMeaning);
}
