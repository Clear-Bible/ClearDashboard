using System.Collections.Generic;
using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Notes;

namespace ClearDashboard.Wpf.Application.ViewModels.Display
{
    /// <summary>
    /// A dictionary mapping entity IDs to their attached notes.
    /// </summary>
    /// <remarks>This dictionary compares entity IDs by their Guid ID values rather than the overloaded <see cref="EntityId{T}"/> equality operators.</remarks>
    public class EntityNoteDictionary : Dictionary<IId, IEnumerable<Note>>
    {
        public EntityNoteDictionary() : base(new IIdEquatableComparer())
        {
        }

        public EntityNoteDictionary(IDictionary<IId, IEnumerable<Note>> dictionary) : base(dictionary, new IIdEquatableComparer())
        {
        }
    }
}
