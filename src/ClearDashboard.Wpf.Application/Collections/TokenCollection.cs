using Caliburn.Micro;
using ClearBible.Engine.Corpora;
using System.Collections.Generic;

namespace ClearDashboard.Wpf.Application.Collections
{
    /// <summary>
    /// A class containing bindable collection of tokens.
    /// </summary>
    public class TokenCollection : BindableCollection<Token>
    {
        /// <summary>
        /// Initializes a new instance of the collection.
        /// </summary>
        public TokenCollection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the collection, initializing it with a sequence of tokens.
        /// </summary>
        /// <param name = "collection">The collection from which the elements are copied.</param>
        public TokenCollection(IEnumerable<Token> collection) : base(collection)
        {
        }
    }
}
