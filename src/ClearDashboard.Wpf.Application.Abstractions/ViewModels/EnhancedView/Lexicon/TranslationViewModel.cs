using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.Wpf.Application.Collections.Lexicon;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Lexicon
{
    public class TranslationViewModel : PropertyChangedBase
    {
        public Translation Entity { get; }

        public TranslationId? TranslationId => Entity.TranslationId;

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

        public TranslationViewModel() : this(new Translation())
        {
        }

        public TranslationViewModel(Translation translation)
        {
            Entity = translation;
        }
    }
}
