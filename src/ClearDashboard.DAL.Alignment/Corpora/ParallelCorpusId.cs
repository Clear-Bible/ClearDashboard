
namespace ClearDashboard.DAL.Alignment.Corpora
{
    public record ParallelCorpusId : BaseId
    {
        public ParallelCorpusId(Guid id) : base(id)
        {
            Metadata = new Dictionary<string, object>();
        }

        public ParallelCorpusId(Guid id, TokenizedTextCorpusId sourceTokenizedCorpusId, TokenizedTextCorpusId targetTokenizedCorpusId, string? displayName, Dictionary<string, object> metadata, DateTimeOffset created, UserId userId) : base(id)
        {
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
