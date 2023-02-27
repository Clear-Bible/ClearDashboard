using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.Wpf.Application.Collections.Lexicon;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Lexicon
{
    public class LexiconTranslationViewModel : PropertyChangedBase
    {
        public Translation Entity { get; }

        public TranslationId? TranslationId
        {
            get => Entity.TranslationId;
#if DEBUG
            set => Entity.TranslationId = value;
#endif
        }

        public string? Text
        {
            get => Entity.Text ?? string.Empty;
            set
            {
                if (Equals(value, Entity.Text)) return;
                Entity.Text = value;
                NotifyOfPropertyChange();
            }
        }

        public int Count { get; set; }

        public LexiconTranslationViewModel() : this(new Translation())
        {
        }

        public LexiconTranslationViewModel(Translation translation)
        {
            Entity = translation;
        }
    }
}
