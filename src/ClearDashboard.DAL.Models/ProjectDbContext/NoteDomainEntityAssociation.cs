
using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models
{
    public class NoteDomainEntityAssociation : IdentifiableEntity
    {
        public NoteDomainEntityAssociation() : base()
        {
            // ReSharper disable VirtualMemberCallInConstructor
            // ReSharper restore VirtualMemberCallInConstructor
        }

        [ForeignKey("NoteId")]
        public Guid NoteId { get; set; }
        public virtual Note? Note { get; set; }

        public Guid? DomainEntityIdGuid { get; set; }
        public string? DomainEntityIdName { get; set; }
    }
}
