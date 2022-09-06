
namespace ClearDashboard.DAL.Alignment.Corpora
{
    public record TokenizedTextCorpusId : BaseId
    {
        public TokenizedTextCorpusId(Guid id) : base(id)
        {
        }

        public TokenizedTextCorpusId(Guid id, string? friendlyName, string? tokenizationFunction, DateTimeOffset created, UserId userId) : base(id)
        {
            FriendlyName = friendlyName;
            TokenizationFunction = tokenizationFunction;
            Created = created;
            UserId = userId;
        }

        public string? FriendlyName { get; }
        public string? TokenizationFunction { get; }
        public DateTimeOffset? Created { get; }
        public UserId? UserId { get; }
        public virtual bool Equals(TokenizedTextCorpusId? other)
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
