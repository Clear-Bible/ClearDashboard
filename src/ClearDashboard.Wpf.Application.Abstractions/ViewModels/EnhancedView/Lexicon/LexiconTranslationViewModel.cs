using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Lexicon;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Lexicon
{
    public class LexiconTranslationViewModel : PropertyChangedBase
    {
        public Translation Entity { get; }

        public MeaningViewModel? Meaning { get; set; }

        public bool MeaningEquals(MeaningViewModel meaning)
        {
            return Meaning != null && Meaning.MeaningId != null && Meaning.MeaningId.IdEquals(meaning.MeaningId);
        }

        public TranslationId TranslationId
        {
            get => Entity.TranslationId;
//#if DEBUG
            set => Entity.TranslationId = value;
//#endif
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

        private bool _isSelected;
        public bool IsSelected 
        {
            get => _isSelected;
            set => Set(ref _isSelected, value);
        }

        public LexiconTranslationViewModel() : this(new Translation())
        {
        }

        public LexiconTranslationViewModel(string text, int count) : this(new Translation { Text = text })
        {
            Count = count;
        }

        public LexiconTranslationViewModel(Translation translation, MeaningViewModel? meaning = null)
        {
            Entity = translation;
            Meaning = meaning;
        }
    }
}
