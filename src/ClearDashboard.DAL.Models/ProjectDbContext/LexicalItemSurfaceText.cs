
using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models
{
    public class LexicalItemSurfaceText : IdentifiableEntity
    {
        public LexicalItemSurfaceText()
        {
            // ReSharper disable VirtualMemberCallInConstructor
            // ReSharper restore VirtualMemberCallInConstructor
        }

        public string? SurfaceText { get; set; }

        [ForeignKey(nameof(LexicalItemId))]
        public Guid LexicalItemId { get; set; }
        public virtual LexicalItem? LexicalItem { get; set; }
    }
}
