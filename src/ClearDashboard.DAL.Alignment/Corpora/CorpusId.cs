using ClearBible.Engine.Utils;
using System.Text.Json.Serialization;

namespace ClearDashboard.DAL.Alignment.Corpora
{
    public class CorpusId : EntityId<CorpusId>, IEquatable<CorpusId>
    {
        public CorpusId(Guid id)
        {
            Id = id;
        }

        [JsonConstructor]
        public CorpusId(string id)
        {
            Id = Guid.Parse(id);
        }

        public override bool Equals(object? obj) => Equals(obj as CorpusId);
        public bool Equals(CorpusId? other) => IdEquals(other);
        public override int GetHashCode() => Id.GetHashCode();
        public static bool operator ==(CorpusId? e1, CorpusId? e2) => object.Equals(e1, e2);
        public static bool operator !=(CorpusId? e1, CorpusId? e2) => !(e1 == e2);

    }
}
