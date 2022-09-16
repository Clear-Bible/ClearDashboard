using ClearBible.Engine.Utils;

namespace ClearDashboard.DAL.Alignment.Corpora
{
    public class ParallelCorpusId : EntityId<ParallelCorpusId>, IEquatable<ParallelCorpusId>
    {
        public ParallelCorpusId(Guid id)
        {
            Id = id;
            Metadata = new Dictionary<string, object>();
        }

        public ParallelCorpusId(Guid id, TokenizedTextCorpusId sourceTokenizedCorpusId, TokenizedTextCorpusId targetTokenizedCorpusId, string? displayName, Dictionary<string, object> metadata, DateTimeOffset created, UserId userId)
        {
            Id = id;
            SourceTokenizedCorpusId = sourceTokenizedCorpusId;
            TargetTokenizedCorpusId = targetTokenizedCorpusId;
            DisplayName = displayName;
            Metadata = metadata;
            Created = created;
            UserId = userId;
        }
        public string? DisplayName { get; set; }
        public Dictionary<string, object> Metadata { get; set; }

        public TokenizedTextCorpusId? SourceTokenizedCorpusId { get; }
        public TokenizedTextCorpusId? TargetTokenizedCorpusId { get; }
        public DateTimeOffset? Created { get; }
        public UserId? UserId { get; }

        public override bool Equals(object? obj) => Equals(obj as ParallelCorpusId);
        public bool Equals(ParallelCorpusId? other)
        {
            if (other is null) return false;

            if (!IdEquals(other)) return false;

            if (SourceTokenizedCorpusId != other.SourceTokenizedCorpusId ||
                TargetTokenizedCorpusId != other.TargetTokenizedCorpusId || 
                DisplayName != other.DisplayName ||
                Created != other.Created ||
                UserId != other.UserId)
            {
                return false;
            }

            return Metadata.SequenceEqual(other.Metadata);
        }
        public override int GetHashCode()
        {
            var mhc = 0;
            foreach (var item in Metadata)
            {
                mhc ^= (item.Key, item.Value).GetHashCode();
            }

            return HashCode.Combine(Id, SourceTokenizedCorpusId, TargetTokenizedCorpusId, DisplayName, Created, UserId, mhc);
        }
        public static bool operator ==(ParallelCorpusId? e1, ParallelCorpusId? e2) => object.Equals(e1, e2);
        public static bool operator !=(ParallelCorpusId? e1, ParallelCorpusId? e2) => !(e1 == e2);
    }
}
