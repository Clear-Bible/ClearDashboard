
using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models
{
    public class Lexicon_Lexeme : SynchronizableTimestampedEntity
    {
        public Lexicon_Lexeme()
        {
            // ReSharper disable VirtualMemberCallInConstructor
            Meanings = new HashSet<Lexicon_Meaning>();
            Forms = new HashSet<Lexicon_Form>();
            // ReSharper restore VirtualMemberCallInConstructor
        }

        public string? Language { get; set; }
        public string? Lemma { get; set; }
        public string? Type { get; set; }
        public ICollection<Lexicon_Meaning> Meanings { get; set; }
        public ICollection<Lexicon_Form> Forms { get; set; }
    }
}
