
using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models
{
    public class LabelGroup : IdentifiableEntity
    {
        public LabelGroup()
        {
            // ReSharper disable VirtualMemberCallInConstructor
            Labels = new HashSet<Label>();
            LabelGroupAssociations = new HashSet<LabelGroupAssociation>();
            // ReSharper restore VirtualMemberCallInConstructor
        }

        public string Name { get; set; } = string.Empty;
        public ICollection<Label> Labels { get; set; }
        public ICollection<LabelGroupAssociation> LabelGroupAssociations { get; set; }
    }
}
