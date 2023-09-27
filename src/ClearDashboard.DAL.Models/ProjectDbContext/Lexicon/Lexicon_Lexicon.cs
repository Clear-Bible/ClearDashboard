
using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models
{
    public class Lexicon_Lexicon
    {
        public Lexicon_Lexicon()
        {
            // ReSharper disable VirtualMemberCallInConstructor
            Lexemes = new HashSet<Lexicon_Lexeme>();
            // ReSharper restore VirtualMemberCallInConstructor
        }

        public ICollection<Lexicon_Lexeme> Lexemes { get; set; }
    }
}
