
using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models
{
    public class Label : IdentifiableEntity
    {
        public Label()
        {
            // ReSharper disable VirtualMemberCallInConstructor
            Notes = new HashSet<Note>();
            LabelNoteAssociations = new HashSet<LabelNoteAssociation>();
            LabelGroups = new HashSet<LabelGroup>();
            LabelGroupAssociations = new HashSet<LabelGroupAssociation>();
            // ReSharper restore VirtualMemberCallInConstructor
        }

        public string? Text { get; set; }
        public string? TemplateText { get; set; }
        public ICollection<Note> Notes { get; set; }
        public ICollection<LabelNoteAssociation> LabelNoteAssociations { get; set; }
        public ICollection<LabelGroup> LabelGroups { get; set; }
        public ICollection<LabelGroupAssociation> LabelGroupAssociations { get; set; }
    }
}
