using ClearDashboard.DAL.Alignment.Corpora;

namespace ClearDashboard.DAL.Alignment.Translation
{
    public record AlignmentSetId : BaseId
    {
        public AlignmentSetId(Guid id) : base(id)
        {
        }
        public AlignmentSetId(Guid id, ParallelCorpusId parallelCorpusId, DateTimeOffset created, UserId userId) : base(id)
        {
            ParallelCorpusId = parallelCorpusId;
            Created = created;
            UserId = userId;
        }

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
