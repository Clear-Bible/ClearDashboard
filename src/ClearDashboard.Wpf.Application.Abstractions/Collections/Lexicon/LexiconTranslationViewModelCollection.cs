using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Lexicon;

namespace ClearDashboard.Wpf.Application.Collections.Lexicon
{
    /// <summary>
    /// A bindable collection of <see cref="Translation"/> instances.
    /// </summary>
    public sealed class LexiconTranslationViewModelCollection : BindableCollection<LexiconTranslationViewModel>
    {
        /// <summary>
        /// Initializes a new instance of the collection.
        /// </summary>
        public LexiconTranslationViewModelCollection()
        {
        }

        public LexiconTranslationViewModelCollection(IEnumerable<Translation> translations, MeaningViewModel meaning)
        {
            AddRange(translations.Select(t => new LexiconTranslationViewModel(t, meaning)));
        }

        /// <summary>
        /// Initializes a new instance of the collection, initializing it with a sequence of translations.
        /// </summary>
        /// <param name="collection">The collection from which the elements are copied.</param>
        public LexiconTranslationViewModelCollection(IEnumerable<LexiconTranslationViewModel> collection) : base(collection)
        {
        }

        public void RemoveIfContainsId(TranslationId translationId)
        {
            var existing = this.FirstOrDefault(t => t.TranslationId.IsInDatabase && t.TranslationId.Equals(translationId));
            if (existing != null)
            {
                Remove(existing);
            }
        }        
        
        public bool ContainsText(string text)
        {
            return this.Any(t => t.Text != null && t.Text.Equals(text));
        }

        public void RemoveIfContainsText(string text)
        {
            var existing = this.FirstOrDefault(t => t.Text != null && t.Text.Equals(text));
            if (existing != null)
            {
                Remove(existing);
            }
        }        
        
        public LexiconTranslationViewModel? SelectIfContainsText(string text)
        {
            var existing = this.FirstOrDefault(t => t.Text != null && t.Text.Equals(text));
            if (existing != null)
            {
                existing.IsSelected = true;
                return existing;
            }

            return null;
        }
    }
}
