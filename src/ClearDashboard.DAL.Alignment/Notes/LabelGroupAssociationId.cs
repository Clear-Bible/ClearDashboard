
using ClearDashboard.DAL.Alignment.Corpora;

namespace ClearDashboard.DAL.Alignment.Notes
{
    public record LabelGroupAssociationId : BaseId
    {
        public LabelGroupAssociationId(Guid id) : base(id)
        {
        }
    }
}
