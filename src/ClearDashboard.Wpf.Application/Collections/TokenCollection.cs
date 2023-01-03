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
        /// Determines whether the specified <see cref="TokenId"/> is the first in the collection.
        /// </summary>
        /// <param name="tokenId">The token ID to check.</param>
        /// <returns>True if the <see cref="tokenId"/> is the first in the collection; false otherwise.</returns>
        public bool IsFirst(TokenId tokenId)
        {
            return Count > 0 && Items[0].TokenId.IdEquals(tokenId);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Token"/> is the first in the collection.
        /// </summary>
        /// <param name="token">The token to check.</param>
        /// <returns>True if the <see cref="token"/> is the first in the collection; false otherwise.</returns>
        public bool IsFirst(Token token)
        {
            return IsFirst(token.TokenId);
        }

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
