using System.Collections.Generic;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Lexicon;

namespace ClearDashboard.Wpf.Application.Collections.Lexicon
{
    public class SemanticDomainCollection : BindableCollection<SemanticDomain>
    {
        /// <summary>
        /// Initializes a new instance of the collection.
        /// </summary>
        public SemanticDomainCollection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the collection, initializing it with a sequence of semantic domains.
        /// </summary>
        /// <param name="collection">The collection from which the elements are copied.</param>
        public SemanticDomainCollection(IEnumerable<SemanticDomain> collection) : base(collection)
        {
        }
    }
}
