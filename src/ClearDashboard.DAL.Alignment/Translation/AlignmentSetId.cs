using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Corpora;

namespace ClearDashboard.DAL.Alignment.Translation
{
    public class AlignmentSetId : EntityId<AlignmentSetId>, IEquatable<AlignmentSetId>
    {
        public AlignmentSetId(Guid id)
        {
            Id = id;
            Metadata = new Dictionary<string, object>();
        }
        public AlignmentSetId(Guid id, ParallelCorpusId parallelCorpusId, string? displayName, string? smtModel, bool isSyntaxTreeAlignerRefined, Dictionary<string, object> metadata, DateTimeOffset created, UserId userId)
        {
            Id = id;
            ParallelCorpusId = parallelCorpusId;
            DisplayName = displayName;
            SmtModel = smtModel;
            IsSyntaxTreeAlignerRefined = isSyntaxTreeAlignerRefined;
            Metadata = metadata;
            Created = created;
            UserId = userId;
        }
        public string? DisplayName { get; set; }
        public string? SmtModel { get; }
        public bool IsSyntaxTreeAlignerRefined { get; }
        public Dictionary<string, object> Metadata { get; set; }

        public ParallelCorpusId? ParallelCorpusId { get; }
        public DateTimeOffset? Created { get; }
        public UserId? UserId { get; }

        public override bool Equals(object? obj) => Equals(obj as AlignmentSetId);
        public bool Equals(AlignmentSetId? other)
        {
            if (other is null) return false;

            if (!IdEquals(other)) return false;

            if (ParallelCorpusId != other.ParallelCorpusId ||
                DisplayName != other.DisplayName ||
                SmtModel != other.SmtModel ||
                IsSyntaxTreeAlignerRefined != other.IsSyntaxTreeAlignerRefined ||
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

            return HashCode.Combine(Id, ParallelCorpusId, DisplayName, SmtModel, IsSyntaxTreeAlignerRefined, Created, UserId, mhc);
        }
        public static bool operator ==(AlignmentSetId? e1, AlignmentSetId? e2) => object.Equals(e1, e2);
        public static bool operator !=(AlignmentSetId? e1, AlignmentSetId? e2) => !(e1 == e2);
    }
}
