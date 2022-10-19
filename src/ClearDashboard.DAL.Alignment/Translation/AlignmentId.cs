using ClearBible.Engine.Corpora;
using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Corpora;

namespace ClearDashboard.DAL.Alignment.Translation
{
    public class AlignmentId : EntityId<AlignmentId>, IEquatable<AlignmentId>
    {
        public AlignmentId(Guid id, string sourceTokenizedCorpusName, string targetTokenizedCorpusName, TokenId sourceTokenId)
        {
            Id = id;
            SourceTokenizedCorpusName = sourceTokenizedCorpusName;
            TargetTokenizedCorpusName = targetTokenizedCorpusName;
            SourceTokenId = sourceTokenId;
        }
        public string? SourceTokenizedCorpusName { get; private set; }
        public string? TargetTokenizedCorpusName { get; private set; }
        public TokenId? SourceTokenId { get; private set; }

        public override bool Equals(object? obj) => Equals(obj as AlignmentId);
        public bool Equals(AlignmentId? other)
        {
            if (other is null) return false;

            if (!IdEquals(other)) return false;

            if (SourceTokenId != other.SourceTokenId ||
                SourceTokenizedCorpusName != other.SourceTokenizedCorpusName ||
                TargetTokenizedCorpusName != other.TargetTokenizedCorpusName)
            {
                return false;
            }

            return true;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Id, SourceTokenId, SourceTokenizedCorpusName, TargetTokenizedCorpusName);
        }
        public static bool operator ==(AlignmentId? e1, AlignmentId? e2) => object.Equals(e1, e2);
        public static bool operator !=(AlignmentId? e1, AlignmentId? e2) => !(e1 == e2);
    }
}
