
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

        public string? DomainEntityIdString { get; set; }
        public string? DomainEntityIdTypeString { get; set; }
        public string? DomainSubEntityIdString { get; set; }
        public string? DomainSubEntityIdTypeString { get; set; }
    }
}
