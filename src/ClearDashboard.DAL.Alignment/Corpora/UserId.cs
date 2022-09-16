
using ClearBible.Engine.Utils;
using System.Text.Json.Serialization;

namespace ClearDashboard.DAL.Alignment.Corpora
{
    public class UserId : EntityId<UserId>, IEquatable<UserId>
    {
        public UserId(Guid id)
        {
            Id = id;
        }

        [JsonConstructor]
        public UserId(string id)
        {
            Id = Guid.Parse(id);
        }

        public override bool Equals(object? obj) => IdEquals(obj);
        public bool Equals(UserId? other) => IdEquals(other);
        public override int GetHashCode() => Id.GetHashCode();
        public static bool operator ==(UserId? e1, UserId? e2) => object.Equals(e1, e2);
        public static bool operator !=(UserId? e1, UserId? e2) => !(e1 == e2);
    }
}
