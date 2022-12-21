using Caliburn.Micro;
using ClearBible.Engine.Corpora;
using System.Collections.Generic;
using System.Linq;

namespace ClearDashboard.Wpf.Application.Collections
{
    /// <summary>
    /// A class containing bindable collection of tokens.
    /// </summary>
    public class TokenCollection : BindableCollection<Token>
    {
        /// <summary>
        /// Gets a <see cref="IEnumerable{TokenId}"/> of the tokens in this collection.
        /// </summary>
        public IEnumerable<TokenId> TokenIds => this.Select(t => t.TokenId);

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
