using ClearDashboard.DAL.Alignment.Corpora;

namespace ClearDashboard.DAL.Alignment.Translation
{
    public record AlignmentSetId : BaseId
    {
        public AlignmentSetId(Guid id) : base(id)
        {
            Metadata = new Dictionary<string, object>();
        }
        public AlignmentSetId(Guid id, ParallelCorpusId parallelCorpusId, string? displayName, string? smtModel, bool isSyntaxTreeAlignerRefined, Dictionary<string, object> metadata, DateTimeOffset created, UserId userId) : base(id)
        {
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
        public virtual bool Equals(AlignmentSetId? other)
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
