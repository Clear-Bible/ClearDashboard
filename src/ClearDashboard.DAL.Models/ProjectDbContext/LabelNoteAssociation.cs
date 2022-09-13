
using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models
{
    public class LabelNoteAssociation : IdentifiableEntity
    {
        public LabelNoteAssociation() : base()
        {
            // ReSharper disable VirtualMemberCallInConstructor
            // ReSharper restore VirtualMemberCallInConstructor
        }

        [ForeignKey("LabelId")]
        public Guid LabelId { get; set; }
        public virtual Label? Label { get; set; }

        [ForeignKey("NoteId")]
        public Guid NoteId { get; set; }
        public virtual Note? Note { get; set; }
    }
}
