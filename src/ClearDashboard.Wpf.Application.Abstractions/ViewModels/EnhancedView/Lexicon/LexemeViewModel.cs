using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.Wpf.Application.Collections.Lexicon;
using SIL.Reporting;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Lexicon
{
    public class LexemeViewModel : PropertyChangedBase, IDisposable
    {
        public Lexeme Entity { get; }

        public LexemeId LexemeId
        {
            get => Entity.LexemeId;
            set
            {
                if (Equals(value, Entity.LexemeId)) return;
                Entity.LexemeId = value;
                NotifyOfPropertyChange();
            }
        }
        public string? Lemma
        {
            get => Entity.Lemma ?? string.Empty;
            set
            {
                if (Equals(value, Entity.Lemma)) return;
                Entity.Lemma = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(LemmaDisplay));
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
                NotifyOfPropertyChange(nameof(LemmaDisplay));
            }
        }

        public string LemmaDisplay => $"{Lemma} ({Language})";

        public string? Type
        {
            get => Entity.Type;
            set
            {
                if (Equals(value, Entity.Type)) return;
                Entity.Type = value;
                NotifyOfPropertyChange();
            }
        }

        private MeaningViewModelCollection? _meanings;
        public MeaningViewModelCollection Meanings
        {
            get => _meanings ??= new MeaningViewModelCollection(Entity.Meanings);
            set
            {
                _meanings = value;
                Entity.Meanings = new ObservableCollection<Meaning>(value.Select(mvm => mvm.Entity));
                NotifyOfPropertyChange();
            }
        }

        private LexemeFormCollection? _forms;
        private bool _hasForms;

        public bool HasForms
        {
            get => _hasForms;
            set => Set(ref _hasForms, value);
        }

        public LexemeFormCollection Forms
        {
            get => _forms ??= new LexemeFormCollection(Entity.Forms);
            set
            {
                _forms = value;
                Entity.Forms = value;
                NotifyOfPropertyChange();
            }
        }

        public bool ContainsTranslationText(string text)
        {
            return Meanings.SelectMany(meaning => meaning.Translations).Any(translation => translation.Text == text);
        }        
        
        public LexiconTranslationViewModel? SelectTranslationText(string text)
        {
            var match = Meanings.SelectMany(meaning => meaning.Translations).FirstOrDefault(translation => translation.Text == text);
            if (match != null)
            {
                match.IsSelected = true;
                return match;
            }

            return null;
        }

        public LexemeViewModel() : this(new Lexeme())
        {
        }

        public LexemeViewModel(Lexeme lexeme)
        {
            Entity = lexeme;
            _meanings = new MeaningViewModelCollection(lexeme.Meanings);

            Forms.CollectionChanged += OnCollectionChanged;
        }

        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            HasForms = Forms.Count > 0;
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Forms.CollectionChanged -= OnCollectionChanged;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
