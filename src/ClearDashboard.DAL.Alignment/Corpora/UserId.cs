
using ClearBible.Engine.Utils;
using System.Text.Json.Serialization;

namespace ClearDashboard.DAL.Alignment.Corpora
{
    public class UserId : EntityId<UserId>, IEquatable<UserId>
    {
        public UserId(Guid id, string displayName)
        {
            Id = id;
            DisplayName = displayName;
        }

        [JsonConstructor]
        public UserId(string id, string displayName)
        {
            Id = Guid.Parse(id);
            DisplayName = displayName;
        }
        public string? DisplayName { get; set; }

        public override bool Equals(object? obj) => IdEquals(obj);
        public bool Equals(UserId? other)
        {
            if (other is null) return false;

            if (!IdEquals(other)) return false;

            return DisplayName == other.DisplayName;
        }
        public override int GetHashCode() => HashCode.Combine(Id, DisplayName);
        public static bool operator ==(UserId? e1, UserId? e2) => object.Equals(e1, e2);
        public static bool operator !=(UserId? e1, UserId? e2) => !(e1 == e2);
    }
}
