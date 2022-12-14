
using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models
{
    public class Lexicon_Lexeme : SynchronizableTimestampedEntity
    {
        public Lexicon_Lexeme()
        {
            // ReSharper disable VirtualMemberCallInConstructor
            Definitions = new HashSet<Lexicon_Definition>();
            Forms = new HashSet<Lexicon_Form>();
            // ReSharper restore VirtualMemberCallInConstructor
        }

        public string? Language { get; set; }
        public string? Lemma { get; set; }
        public ICollection<Lexicon_Definition> Definitions { get; set; }
        public ICollection<Lexicon_Form> Forms { get; set; }
    }
}
