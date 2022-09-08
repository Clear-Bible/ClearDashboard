
namespace ClearDashboard.DAL.Alignment.Corpora
{
    public record TokenizedTextCorpusId : BaseId
    {
        public TokenizedTextCorpusId(Guid id) : base(id)
        {
            Metadata = new Dictionary<string, object>();
        }

        public TokenizedTextCorpusId(Guid id, string? displayName, string? tokenizationFunction, Dictionary<string, object> metadata, DateTimeOffset created, UserId userId) : base(id)
        {
            DisplayName = displayName;
            TokenizationFunction = tokenizationFunction;
            Metadata = metadata;
            Created = created;
            UserId = userId;
        }

        public string? DisplayName { get; set; }
        public string? TokenizationFunction { get; }
        public Dictionary<string, object> Metadata { get; set; }
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
