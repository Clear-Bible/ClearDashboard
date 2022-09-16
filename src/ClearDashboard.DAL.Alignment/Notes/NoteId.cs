using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Corpora;

namespace ClearDashboard.DAL.Alignment.Notes
{
    public class NoteId : EntityId<NoteId>, IEquatable<NoteId>
    {
        public NoteId(Guid id)
        {
            Id = id;
        }
        public NoteId(Guid id, DateTimeOffset created, DateTimeOffset? modified, UserId userId)
        {
            Id = id;
            Created = created;
            Modified = modified;
            UserId = userId;
        }

        public DateTimeOffset? Created { get; }
        public DateTimeOffset? Modified { get; }
        public UserId? UserId { get; }

        public override bool Equals(object? obj) => Equals(obj as NoteId);
        public bool Equals(NoteId? other)
        {
            if (other is null) return false;

            if (!IdEquals(other)) return false;

            if (Created != other.Created ||
                Modified != other.Modified ||
                UserId != other.UserId)
            {
                return false;
            }

            return true;
        }
        public override int GetHashCode() => HashCode.Combine(Id, Created, Modified, UserId);
        public static bool operator ==(NoteId? e1, NoteId? e2) => object.Equals(e1, e2);
        public static bool operator !=(NoteId? e1, NoteId? e2) => !(e1 == e2);
    }
}
