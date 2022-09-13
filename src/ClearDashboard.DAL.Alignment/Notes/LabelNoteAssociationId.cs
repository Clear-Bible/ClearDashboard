
using ClearDashboard.DAL.Alignment.Corpora;

namespace ClearDashboard.DAL.Alignment.Notes
{
    public record LabelNoteAssociationId : BaseId
    {
        public LabelNoteAssociationId(Guid id) : base(id)
        {
        }
    }
}
