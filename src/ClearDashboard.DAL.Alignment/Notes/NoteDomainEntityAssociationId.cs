
using ClearDashboard.DAL.Alignment.Corpora;

namespace ClearDashboard.DAL.Alignment.Notes
{
    public record NoteDomainEntityAssociationId : BaseId
    {
        public NoteDomainEntityAssociationId(Guid id) : base(id)
        {
        }
    }
}
