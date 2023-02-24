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
    public sealed class TranslationViewModelCollection : BindableCollection<TranslationViewModel>
    {
        /// <summary>
        /// Initializes a new instance of the collection.
        /// </summary>
        public TranslationViewModelCollection()
        {
        }

        public TranslationViewModelCollection(IEnumerable<Translation> translations)
        {
            AddRange(translations.Select(t => new TranslationViewModel(t)));
        }

        /// <summary>
        /// Initializes a new instance of the collection, initializing it with a sequence of translations.
        /// </summary>
        /// <param name="collection">The collection from which the elements are copied.</param>
        public TranslationViewModelCollection(IEnumerable<TranslationViewModel> collection) : base(collection)
        {
        }
    }
}
