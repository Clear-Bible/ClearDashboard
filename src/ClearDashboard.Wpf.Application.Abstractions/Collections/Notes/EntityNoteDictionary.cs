using System.Collections.Generic;
using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Notes;

namespace ClearDashboard.Wpf.Application.Collections.Notes
{
    /// <summary>
    /// A dictionary mapping entity IDs to their attached notes.
    /// </summary>
    /// <remarks>This dictionary compares entity IDs by their Guid ID values rather than the overloaded <see cref="EntityId{T}"/> equality operators.</remarks>
    public class EntityNoteDictionary : Dictionary<IId, NoteCollection>
    {
        public EntityNoteDictionary() : base(new IIdEqualityComparer())
        {
        }

        public EntityNoteDictionary(IDictionary<IId, NoteCollection> dictionary) : base(dictionary, new IIdEqualityComparer())
        {
        }
    }
}
