
using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models
{
    public class NoteUserSeenAssociation : IdentifiableEntity
    {
        public NoteUserSeenAssociation() : base()
        {
            // ReSharper disable VirtualMemberCallInConstructor
            // ReSharper restore VirtualMemberCallInConstructor
        }

        [ForeignKey("NoteId")]
        public Guid NoteId { get; set; }
        public virtual Note? Note { get; set; }

        [ForeignKey("UserId")]
        public Guid UserId { get; set; }
        public virtual User? User { get; set; }
    }
}
