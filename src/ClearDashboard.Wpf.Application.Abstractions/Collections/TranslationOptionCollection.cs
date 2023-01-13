using System.Collections.Generic;
using Caliburn.Micro;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;

namespace ClearDashboard.Wpf.Application.Collections
{
    /// <summary>
    /// A bindable collection of <see cref="TranslationOption"/> instances.
    /// </summary>
    public class TranslationOptionCollection : BindableCollection<TranslationOption>
    {
        /// <summary>
        /// Initializes a new instance of the collection.
        /// </summary>
        public TranslationOptionCollection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the collection, initializing it with a sequence of translation options.
        /// </summary>
        /// <param name="collection">The collection from which the elements are copied.</param>
        public TranslationOptionCollection(IEnumerable<TranslationOption> collection) : base(collection)
        {
        }
    }
}
