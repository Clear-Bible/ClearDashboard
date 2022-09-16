using ClearBible.Engine.Utils;

namespace ClearDashboard.DAL.Alignment.Corpora
{
    public class ParallelTokenizedCorpusId : EntityId<ParallelTokenizedCorpusId>, IEquatable<ParallelTokenizedCorpusId>
    {
        public ParallelTokenizedCorpusId(Guid id)
        {
            Id = id;
        }

        public override bool Equals(object? obj) => IdEquals(obj);
        public bool Equals(ParallelTokenizedCorpusId? other) => IdEquals(other);
        public override int GetHashCode() => Id.GetHashCode();
        public static bool operator ==(ParallelTokenizedCorpusId? e1, ParallelTokenizedCorpusId? e2) => object.Equals(e1, e2);
        public static bool operator !=(ParallelTokenizedCorpusId? e1, ParallelTokenizedCorpusId? e2) => !(e1 == e2);
    }
}
