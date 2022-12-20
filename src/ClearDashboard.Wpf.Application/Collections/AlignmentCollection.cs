using System.Collections.Generic;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Translation;

namespace ClearDashboard.Wpf.Application.Collections
{
    /// <summary>
    /// A bindable collection of <see cref="Alignment"/> instances.
    /// </summary>
    public class AlignmentCollection : BindableCollection<Alignment>
    {
        /// <summary>
        /// Initializes a new instance of the collection.
        /// </summary>
        public AlignmentCollection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the collection, initializing it with a sequence of alignments.
        /// </summary>
        /// <param name="collection">The collection from which the elements are copied.</param>
        public AlignmentCollection(IEnumerable<Alignment> collection) : base(collection)
        {
        }
    }
}
