using System.Collections.Generic;
using System.Linq;
using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Notes;

namespace ClearDashboard.Wpf.Application.Collections
{
    /// <summary>
    /// A dictionary mapping entity IDs to their context dictionary.
    /// </summary>
    /// <remarks>This dictionary compares entity IDs by their Guid ID values rather than the overloaded <see cref="EntityId{T}"/> equality operators.</remarks>
    public class EntityContextDictionary : Dictionary<IId, Dictionary<string, string>>
    {
        public EntityContextDictionary() : base(new IIdEqualityComparer())
        {
        }

        public EntityContextDictionary(IDictionary<IId, Dictionary<string, string>> dictionary) : base(dictionary, new IIdEqualityComparer())
        {
        }
    }
}
