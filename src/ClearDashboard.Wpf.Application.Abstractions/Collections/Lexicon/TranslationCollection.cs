using System.Collections.Generic;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Lexicon;

namespace ClearDashboard.Wpf.Application.Collections.Lexicon
{
    /// <summary>
    /// A bindable collection of <see cref="Translation"/> instances.
    /// </summary>
    public class TranslationCollection : BindableCollection<Translation>
    {
        /// <summary>
        /// Initializes a new instance of the collection.
        /// </summary>
        public TranslationCollection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the collection, initializing it with a sequence of translations.
        /// </summary>
        /// <param name="collection">The collection from which the elements are copied.</param>
        public TranslationCollection(IEnumerable<Translation> collection) : base(collection)
        {
        }
    }
}
