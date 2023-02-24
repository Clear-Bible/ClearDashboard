using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.Wpf.Application.Collections.Lexicon;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Lexicon
{
    public class MeaningViewModel : PropertyChangedBase
    {
        public Meaning Entity { get; }

        public MeaningId? MeaningId => Entity.MeaningId;

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

        private TranslationViewModelCollection? _translations;
        public TranslationViewModelCollection Translations
        {
            get => _translations ??= new TranslationViewModelCollection(Entity.Translations);
            set
            {
                _translations = value;
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
            _translations = new TranslationViewModelCollection(meaning.Translations);
        }
    }
}
