using ClearDashboard.DAL.Alignment.Corpora;

namespace ClearDashboard.DAL.Alignment.Lexicon
{
    public record LexicalItemId : BaseId
    {
        public LexicalItemId(Guid id) : base(id)
        {
        }
    }
}
