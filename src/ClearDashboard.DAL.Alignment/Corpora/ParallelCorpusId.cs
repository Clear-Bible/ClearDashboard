
namespace ClearDashboard.DAL.Alignment.Corpora
{
    public record ParallelCorpusId : BaseId
    {
        public ParallelCorpusId(Guid id) : base(id)
        {
        }

        public ParallelCorpusId(Guid id, TokenizedTextCorpusId sourceTokenizedCorpusId, TokenizedTextCorpusId targetTokenizedCorpusId, DateTimeOffset created, UserId userId) : base(id)
        {
            SourceTokenizedCorpusId = sourceTokenizedCorpusId;
            TargetTokenizedCorpusId = targetTokenizedCorpusId;
            Created = created;
            UserId = userId;
        }

        public TokenizedTextCorpusId? SourceTokenizedCorpusId { get; }
        public TokenizedTextCorpusId? TargetTokenizedCorpusId { get; }
        public DateTimeOffset? Created { get; }
        public UserId? UserId { get; }
        public virtual bool Equals(ParallelCorpusId? other)
        {
            if (other == null)
                return false;

            if (this.Id == other.Id)
                return true;
            else
                return false;
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
    }
}
