using ClearBible.Engine.Utils;

namespace ClearDashboard.DAL.Alignment
{
    public class EmptyEntityId : EntityId<EmptyEntityId>, IEquatable<EmptyEntityId>
    {
        public EmptyEntityId()
        {
            Id = Guid.Empty;
        }

        public bool Equals(EmptyEntityId? other)
        {
            if (other is null) return false;
            return Id == Guid.Empty;
        }

        public override bool Equals(object? obj) => Equals(obj as EmptyEntityId);
        public override int GetHashCode() => Id.GetHashCode();
    }
}
