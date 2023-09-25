using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json.Serialization;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.Wpf.Application.Collections.Lexicon;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Lexicon
{
    public class MeaningViewModel : PropertyChangedBase
    {
        public Meaning Entity { get; }

        public MeaningId? MeaningId
        {
            get => Entity.MeaningId;
            set => Entity.MeaningId = value;
        }

        public string? Text
        {
            get => Entity.Text ?? string.Empty;
            set
            {
                if (value.Trim() == string.Empty) return;
                if (Equals(value, Entity.Text)) return;
                Entity.Text = value;
                NotifyOfPropertyChange();
            }
        }

        public string? Language
        {
            get => Entity.Language;
            set
            {
                if (Equals(value, Entity.Language)) return;
                Entity.Language = value;
                NotifyOfPropertyChange();
            }
        }

        private LexiconTranslationViewModelCollection? _translations;
        [JsonIgnore]
        public LexiconTranslationViewModelCollection Translations
        {
            get => _translations ??= new LexiconTranslationViewModelCollection(Entity.Translations, this);
            set
            {
                _translations = value;
                foreach (var translation in _translations)
                {
                    translation.Meaning = this;
                }
                Entity.Translations = new ObservableCollection<Translation>(value.Select(tvm => tvm.Entity));
                NotifyOfPropertyChange();
            }
        }

        private SemanticDomainCollection? _semanticDomains;
        public SemanticDomainCollection SemanticDomains
        {
            get => _semanticDomains ??= new SemanticDomainCollection(Entity.SemanticDomains);
            set
            {
                Entity.SemanticDomains = value;
                _semanticDomains ??= new SemanticDomainCollection(Entity.SemanticDomains);
                NotifyOfPropertyChange();
            }
        }

        public MeaningViewModel() : this(new Meaning())
        {
        }

        public MeaningViewModel(Meaning meaning)
        {
            Entity = meaning;
            _translations = new LexiconTranslationViewModelCollection(meaning.Translations, this);
        }
    }
}
