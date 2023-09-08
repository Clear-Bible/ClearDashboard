using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Lexicon;

namespace ClearDashboard.DAL.Alignment.Features
{
    public abstract class SimpleSynchronizableTimestampedEntityId<T> : EntityId<T>, IEquatable<T>
        where T : SimpleSynchronizableTimestampedEntityId<T>, new()
    {
        public static T Create(Guid id, DateTimeOffset created, UserId userId)
        {
            var current = new T
            {
                Id = id,
                Created = created,
                UserId = userId
            };
            return current;
        }

        public static T Create(Guid id, UserId userId)
        {
            var current = new T
            {
                Id = id,
                UserId = userId
            };
            return current;
        }

        public static T Create(Guid id)
        {
            var current = new T
            {
                Id = id
            };
            return current;
        }

        public DateTimeOffset? Created { get; protected set; }
        public UserId? UserId { get; protected set; }
        public bool IsInDatabase { get => Created is not null; }

        public override bool Equals(object? obj) => Equals(obj as T);
        public bool Equals(T? other)
        {
            if (other is null) return false;

            if (!IdEquals(other)) return false;

            if (Created != other.Created ||
                UserId != other.UserId)
            {
                return false;
            }

            return true;
        }
        public override int GetHashCode() => HashCode.Combine(Id, Created, UserId);
        public static bool operator ==(SimpleSynchronizableTimestampedEntityId<T>? e1, SimpleSynchronizableTimestampedEntityId<T>? e2) => Equals(e1, e2);
        public static bool operator !=(SimpleSynchronizableTimestampedEntityId<T>? e1, SimpleSynchronizableTimestampedEntityId<T>? e2) => !(e1 == e2);
    }
}
