using Caliburn.Micro;
using ClearBible.Engine.Corpora;
using System.Collections.Generic;

namespace ClearDashboard.Wpf.Application.Collections
{
    /// <summary>
    /// A class containing bindable collection of tokens with their detokenized padding values.
    /// </summary>
    public class PaddedTokenCollection : BindableCollection<(Token token, string paddingBefore, string paddingAfter)>
    {
        /// <summary>
        /// Initializes a new instance of the collection.
        /// </summary>
        public PaddedTokenCollection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the collection, initializing it with a sequence of tokens.
        /// </summary>
        /// <param name = "collection">The collection from which the elements are copied.</param>
        public PaddedTokenCollection(IEnumerable<(Token token, string paddingBefore, string paddingAfter)> collection) : base(collection)
        {
        }
    }
}
