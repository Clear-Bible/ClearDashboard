using System.Collections.Generic;
using System.Linq;
using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Notes;

namespace ClearDashboard.Wpf.Application.Collections.Notes
{
    /// <summary>
    /// A dictionary mapping entity IDs to their attached note IDs.
    /// </summary>
    /// <remarks>This dictionary compares entity IDs by their Guid ID values rather than the overloaded <see cref="EntityId{T}"/> equality operators.</remarks>
    public class EntityNoteIdDictionary : Dictionary<IId, NoteIdCollection>
    {
        public EntityNoteIdDictionary() : base(new IIdEqualityComparer())
        {
        }

        public EntityNoteIdDictionary(IDictionary<IId, NoteIdCollection> dictionary) : base(dictionary, new IIdEqualityComparer())
        {
        }

        public EntityNoteIdDictionary(IDictionary<IId, IEnumerable<NoteId>> dictionary)
            : base(dictionary.Select(kvp => new KeyValuePair<IId, NoteIdCollection>(kvp.Key, new NoteIdCollection(kvp.Value))),
                new IIdEqualityComparer())
        {

        }
    }
}
