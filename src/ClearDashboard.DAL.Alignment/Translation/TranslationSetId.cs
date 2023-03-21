using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Corpora;

namespace ClearDashboard.DAL.Alignment.Translation
{
    public class TranslationSetId : EntityId<TranslationSetId>, IEquatable<TranslationSetId>
    {
        public TranslationSetId(Guid id)
        {
            Id = id;
            Metadata = new Dictionary<string, object>();
        }
        public TranslationSetId(Guid id, ParallelCorpusId parallelCorpusId, string? displayName, Guid alignmentSetGuid, Dictionary<string, object> metadata, DateTimeOffset created, UserId userId)
        {
            Id = id;
            ParallelCorpusId = parallelCorpusId;
            DisplayName = displayName;
            AlignmentSetGuid = alignmentSetGuid;
            Metadata = metadata;
            Created = created;
            UserId = userId;
        }
        public string? DisplayName { get; set; }
        public Guid? AlignmentSetGuid { get; }
        public Dictionary<string, object> Metadata { get; set; }
        public ParallelCorpusId? ParallelCorpusId { get; }
        public DateTimeOffset? Created { get; }
        public UserId? UserId { get; }

        public override bool Equals(object? obj) => Equals(obj as TranslationSetId);
        public bool Equals(TranslationSetId? other)
        {
            if (other is null) return false;

            if (!IdEquals(other)) return false;

            if (ParallelCorpusId != other.ParallelCorpusId ||
                DisplayName != other.DisplayName ||
                AlignmentSetGuid != other.AlignmentSetGuid ||
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

            return HashCode.Combine(Id, ParallelCorpusId, DisplayName, AlignmentSetGuid, Created, UserId, mhc);
        }
        public static bool operator ==(TranslationSetId? e1, TranslationSetId? e2) => object.Equals(e1, e2);
        public static bool operator !=(TranslationSetId? e1, TranslationSetId? e2) => !(e1 == e2);
    }
}
