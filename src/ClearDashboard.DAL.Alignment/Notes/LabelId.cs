
using ClearDashboard.DAL.Alignment.Corpora;

namespace ClearDashboard.DAL.Alignment.Notes
{
    public record LabelId : BaseId
    {
        public LabelId(Guid id) : base(id)
        {
        }
    }
}
