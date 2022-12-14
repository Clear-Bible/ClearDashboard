
using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models
{
    public class Lexicon_Form : IdentifiableEntity
    {
        public Lexicon_Form()
        {
            // ReSharper disable VirtualMemberCallInConstructor
            // ReSharper restore VirtualMemberCallInConstructor
        }

        public string? Text { get; set; }

        [ForeignKey(nameof(LexicalItemId))]
        public Guid LexicalItemId { get; set; }
        public virtual Lexicon_LexicalItem? LexicalItem { get; set; }
    }
}
