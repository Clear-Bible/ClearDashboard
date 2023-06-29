
using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models
{
    public class LabelGroupAssociation : IdentifiableEntity
    {
        public LabelGroupAssociation() : base()
        {
            // ReSharper disable VirtualMemberCallInConstructor
            // ReSharper restore VirtualMemberCallInConstructor
        }

        [ForeignKey("LabelId")]
        public Guid LabelId { get; set; }
        public virtual Label? Label { get; set; }

        [ForeignKey("LabelGroupId")]
        public Guid LabelGroupId { get; set; }
        public virtual LabelGroup? LabelGroup { get; set; }
    }
}
