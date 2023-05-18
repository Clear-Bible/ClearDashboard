
using ClearDashboard.DAL.Alignment.Corpora;

namespace ClearDashboard.DAL.Alignment.Notes
{
    public record LabelGroupId : BaseId
    {
        public LabelGroupId(Guid id) : base(id)
        {
        }
    }
}
