
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

        [ForeignKey(nameof(LexemeId))]
        public Guid LexemeId { get; set; }
        public virtual Lexicon_Lexeme? Lexeme { get; set; }
    }
}
