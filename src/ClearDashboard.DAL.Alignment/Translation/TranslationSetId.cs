using ClearDashboard.DAL.Alignment.Corpora;

namespace ClearDashboard.DAL.Alignment.Translation
{
    public record TranslationSetId : BaseId
    {
        public TranslationSetId(Guid id) : base(id)
        {
        }
    }
}
