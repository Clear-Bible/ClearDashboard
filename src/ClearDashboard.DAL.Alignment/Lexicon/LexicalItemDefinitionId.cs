using ClearDashboard.DAL.Alignment.Corpora;

namespace ClearDashboard.DAL.Alignment.Lexicon
{
    public record LexicalItemDefinitionId : BaseId
    {
        public LexicalItemDefinitionId(Guid id) : base(id)
        {
        }
    }
}
