using ClearDashboard.DAL.Alignment.Corpora;

namespace ClearDashboard.DAL.Alignment.Lexicon
{
    public record SemanticDomainId : BaseId
    {
        public SemanticDomainId(Guid id) : base(id)
        {
        }
    }
}
